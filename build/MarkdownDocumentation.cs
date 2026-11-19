using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using ReverseMarkdown;

internal static class MarkdownDocumentation
{
    // Convert the already-published article DOM, not repository source globs. This
    // keeps old DocFX/Sphinx versions and their public API selection unchanged.
    public static void Build(string repositoryDirectory, string htmlDirectory, string markdownDirectory,
        bool requireApi = false, bool requireManifest = true)
    {
        repositoryDirectory = Path.GetFullPath(repositoryDirectory);
        htmlDirectory = Path.GetFullPath(htmlDirectory);
        markdownDirectory = Path.GetFullPath(markdownDirectory);
        var archive = markdownDirectory + ".zip";
        if (File.Exists(archive))
            File.Delete(archive);
        if (Directory.Exists(markdownDirectory))
            Directory.Delete(markdownDirectory, true);

        var pages = new Dictionary<string, HtmlNode>(StringComparer.OrdinalIgnoreCase);
        var apiPages = 0;
        var files = Directory.GetFiles(htmlDirectory, "*.html", SearchOption.AllDirectories).AsEnumerable();
        var manifest = Path.Combine(htmlDirectory, "manifest.json");
        if (requireManifest && !File.Exists(manifest))
            throw new InvalidOperationException("The published DocFX manifest was not found.");
        if (File.Exists(manifest))
        {
            using (var reader = JsonReaderWriterFactory.CreateJsonReader(File.ReadAllBytes(manifest), XmlDictionaryReaderQuotas.Max))
                files = XDocument.Load(reader).Descendants("relative_path")
                    .Select(element => element.Value)
                    .Where(path => Path.GetExtension(path).Equals(".html", StringComparison.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(path => SafePath(htmlDirectory, path)
                        ?? throw new InvalidOperationException("A published page leaves the documentation directory."))
                    .ToList();
        }
        foreach (var file in files)
        {
            var document = new HtmlDocument();
            document.Load(file, Encoding.UTF8);
            var article = document.DocumentNode.SelectSingleNode("//article")
                ?? document.DocumentNode.SelectSingleNode("//*[@role='main']");
            if (article != null)
            {
                pages.Add(Relative(htmlDirectory, file), article);
                if ((!string.IsNullOrWhiteSpace(article.GetAttributeValue("data-uid", ""))
                        || document.DocumentNode.SelectSingleNode("//meta[@name='uid']") != null)
                    && article.SelectSingleNode(".//pre") != null)
                    apiPages++;
            }
        }
        if (pages.Count == 0)
            throw new InvalidOperationException("No published documentation articles were found.");
        if (requireApi && apiPages == 0)
            throw new InvalidOperationException("No public API declarations were found in the published documentation.");

        Directory.CreateDirectory(markdownDirectory);
        var converter = new Converter(new Config
        {
            GithubFlavored = true,
            UnknownTags = Config.UnknownTagsOption.Bypass,
            RemoveComments = true
        });
        foreach (var page in pages)
        {
            var article = page.Value;
            foreach (var node in article.SelectNodes(".//script|.//style|.//a[contains(@class,'anchorjs-link')]|.//*[contains(@class,'contribution')]")
                ?? Enumerable.Empty<HtmlNode>())
                node.Remove();

            var anchors = new Dictionary<string, string>();
            foreach (var node in (article.SelectNodes(".//*[@id]") ?? Enumerable.Empty<HtmlNode>()).ToList())
            {
                var token = "MARKDOWNANCHOR" + anchors.Count.ToString("D8");
                anchors.Add(token, "<a id=\"" + WebUtility.HtmlEncode(node.GetAttributeValue("id", "")) + "\"></a>");
                node.ParentNode.InsertBefore(HtmlNode.CreateNode("<p>" + token + "</p>"), node);
            }
            foreach (var node in article.SelectNodes(".//*[@href or @src]") ?? Enumerable.Empty<HtmlNode>())
            {
                foreach (var name in new[] { "href", "src" })
                {
                    var attribute = node.Attributes[name];
                    if (attribute != null)
                        attribute.Value = RewriteUrl(attribute.Value, page.Key, pages.Keys,
                            htmlDirectory, markdownDirectory);
                }
            }
            var markdown = converter.Convert(article.InnerHtml);
            foreach (var anchor in anchors)
                markdown = markdown.Replace(anchor.Key, anchor.Value);
            Write(markdownDirectory, Path.ChangeExtension(page.Key, ".md"), markdown);
        }

        // Keep the repository entry point, changelog and license text verbatim
        // apart from links to pages/assets inside the documentation bundle.
        foreach (var file in Directory.GetFiles(repositoryDirectory))
        {
            var name = Path.GetFileName(file);
            if (!Regex.IsMatch(name, @"^(README|CHANGELOG|LICENSE|THIRD_PARTY_LICENSES)([._-].*)?$", RegexOptions.IgnoreCase))
                continue;
            if (Path.GetExtension(file).Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                var markdown = Regex.Replace(File.ReadAllText(file), @"(!?\[[^\]]*\]\()([^\s)]+)([^)]*\))",
                    match => match.Groups[1].Value + RewriteSourceUrl(match.Groups[2].Value,
                        pages.Keys, repositoryDirectory, htmlDirectory, markdownDirectory) + match.Groups[3].Value);
                markdown = Regex.Replace(markdown, "(<(?:img|a)\\b[^>]*\\b(?:src|href)=[\"'])([^\"']+)([\"'])",
                    match => match.Groups[1].Value + RewriteSourceUrl(match.Groups[2].Value,
                        pages.Keys, repositoryDirectory, htmlDirectory, markdownDirectory) + match.Groups[3].Value,
                    RegexOptions.IgnoreCase);
                Write(markdownDirectory, name, markdown);
                if (name.Equals("README.md", StringComparison.OrdinalIgnoreCase))
                    Write(markdownDirectory, "index.md", markdown);
            }
            else
                File.Copy(file, Path.Combine(markdownDirectory, name), true);
        }

        var index = Path.Combine(markdownDirectory, "index.md");
        if (!File.Exists(index) || string.IsNullOrWhiteSpace(File.ReadAllText(index)))
            throw new InvalidOperationException("The Markdown documentation has no readable index.md.");
        Write(markdownDirectory, "toc.md", "# Documentation\n\n" + string.Join("\n",
            Directory.GetFiles(markdownDirectory, "*.md", SearchOption.AllDirectories)
                .Select(file => Relative(markdownDirectory, file)).OrderBy(file => file, StringComparer.Ordinal)
                .Select(file => "- [" + file + "](" + string.Join("/", file.Split('/').Select(Uri.EscapeDataString)) + ")")) + "\n");
        File.AppendAllText(index, "\n\n[Documentation contents](toc.md)\n", new UTF8Encoding(false));
        ZipFile.CreateFromDirectory(markdownDirectory, archive, CompressionLevel.Optimal, false);
    }

    public static void Publish(string baseUrl, string apiKey, string version, string packagePath,
        Action uploadHtml, bool skipForVersionConflicts = false)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("An explicit documentation version is required.", "version");
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("Documentation URL and API key are required.");
        using (var archive = ZipFile.OpenRead(packagePath))
        {
            if (archive.GetEntry("index.md") == null)
                throw new InvalidOperationException("The Markdown package has no root index.md.");
        }
        try
        {
            uploadHtml();
        }
        catch (Exception exception)
        {
            // Legacy WebDocu returns void, so let it throw on a version conflict
            // and preserve the existing skip policy here for both uploads.
            if (skipForVersionConflicts && exception.Message.StartsWith(
                "Upload failed with status code: Conflict", StringComparison.Ordinal))
                return;
            throw;
        }
        using (var client = new HttpClient())
        using (var request = new HttpRequestMessage(HttpMethod.Post,
            baseUrl.TrimEnd('/') + "/API/MarkdownDocuments/Upload"))
        using (var content = new MultipartFormDataContent())
        using (var stream = File.OpenRead(packagePath))
        {
            content.Add(new StringContent(apiKey), "ApiKey");
            content.Add(new StringContent(version), "Version");
            content.Add(new StreamContent(stream), "MarkdownPackage", Path.GetFileName(packagePath));
            request.Content = content;
            using (var response = client.SendAsync(request).GetAwaiter().GetResult())
            {
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException("Markdown upload failed with status code: " + response.StatusCode);
            }
        }
    }

    private static string RewriteUrl(string url, string page, IEnumerable<string> pages,
        string htmlDirectory, string markdownDirectory)
    {
        string path, suffix;
        if (!LocalUrl(url, out path, out suffix))
            return url;
        var target = Resolve(page, path);
        if (pages.Contains(target, StringComparer.OrdinalIgnoreCase))
            return RelativeLink(Path.ChangeExtension(page, ".md"), Path.ChangeExtension(target, ".md")) + suffix;
        var asset = SafePath(htmlDirectory, target);
        if (asset != null && File.Exists(asset))
        {
            Copy(htmlDirectory, markdownDirectory, target);
            return RelativeLink(page, target) + suffix;
        }
        return url;
    }

    private static string RewriteSourceUrl(string url, IEnumerable<string> pages,
        string repositoryDirectory, string htmlDirectory, string markdownDirectory)
    {
        string path, suffix;
        if (!LocalUrl(url, out path, out suffix))
            return url;
        path = path.TrimStart('/');
        var html = Path.ChangeExtension(path, ".html").Replace('\\', '/');
        var candidates = new[] { html, html.StartsWith("docs/", StringComparison.OrdinalIgnoreCase) ? html.Substring(5) : html };
        var page = candidates.FirstOrDefault(candidate => pages.Contains(candidate, StringComparer.OrdinalIgnoreCase));
        if (page != null)
            return Path.ChangeExtension(page, ".md").Replace('\\', '/') + suffix;
        var source = SafePath(repositoryDirectory, path);
        if (source != null && File.Exists(source)
            && Regex.IsMatch(Path.GetExtension(source), @"^\.(png|jpe?g|gif|svg|webp|ico|pdf)$", RegexOptions.IgnoreCase))
            Copy(repositoryDirectory, markdownDirectory, path);
        return url;
    }

    private static bool LocalUrl(string url, out string path, out string suffix)
    {
        path = "";
        suffix = "";
        if (string.IsNullOrWhiteSpace(url) || url[0] == '#' || url.StartsWith("//")
            || Regex.IsMatch(url, @"^[a-z][a-z0-9+.-]*:", RegexOptions.IgnoreCase))
            return false;
        var split = url.IndexOfAny(new[] { '#', '?' });
        path = Uri.UnescapeDataString(split < 0 ? url : url.Substring(0, split));
        suffix = split < 0 ? "" : url.Substring(split);
        return true;
    }

    private static string Resolve(string page, string target)
    {
        var root = new Uri("https://documentation.invalid/");
        return Uri.UnescapeDataString(root.MakeRelativeUri(new Uri(new Uri(root, page), target)).ToString()).Split('#')[0];
    }

    private static string RelativeLink(string page, string target)
    {
        var root = new Uri("https://documentation.invalid/");
        return new Uri(root, page).MakeRelativeUri(new Uri(root, target)).ToString();
    }

    private static string Relative(string root, string file)
    {
        return Uri.UnescapeDataString(new Uri(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar)
            .MakeRelativeUri(new Uri(Path.GetFullPath(file))).ToString()).Replace('\\', '/');
    }

    private static string SafePath(string root, string relative)
    {
        var prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(root, relative));
        return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? path : null;
    }

    private static void Copy(string sourceDirectory, string destinationDirectory, string relative)
    {
        var source = SafePath(sourceDirectory, relative);
        var destination = SafePath(destinationDirectory, relative);
        if (source == null || destination == null)
            throw new InvalidOperationException("A documentation asset leaves the documentation directory.");
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        File.Copy(source, destination, true);
    }

    private static void Write(string directory, string relative, string markdown)
    {
        var destination = SafePath(directory, relative);
        if (destination == null)
            throw new InvalidOperationException("A documentation page leaves the documentation directory.");
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        File.WriteAllText(destination, markdown, new UTF8Encoding(false));
    }
}
