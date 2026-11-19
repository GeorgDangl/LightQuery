using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using GlobExpressions;

internal static class MarkdownDocumentation
{
    public static void Create(string configFile, string outputDirectory)
    {
        var configDirectory = Path.GetDirectoryName(Path.GetFullPath(configFile));
        outputDirectory = Path.GetFullPath(outputDirectory);
        var artifactDirectory = Path.GetDirectoryName(outputDirectory);
        Directory.CreateDirectory(outputDirectory);
        using var config = JsonDocument.Parse(File.ReadAllText(configFile));
        var build = config.RootElement.GetProperty("build");

        foreach (var section in new[] { "content", "resource" })
        {
            if (!build.TryGetProperty(section, out var mappings))
            {
                continue;
            }

            foreach (var mapping in mappings.EnumerateArray())
            {
                var source = Path.GetFullPath(Path.Combine(configDirectory,
                    mapping.TryGetProperty("src", out var src) ? src.GetString() : "."));
                var destination = mapping.TryGetProperty("dest", out var dest) ? dest.GetString() : ".";
                var exclusions = Patterns(mapping, "exclude");
                var excluded = exclusions.Length == 0
                    ? Enumerable.Empty<string>()
                    : Files(source, exclusions);
                var files = Files(source, Patterns(mapping, "files")).Except(excluded);

                foreach (var file in files)
                {
                    if (Path.GetFullPath(file).StartsWith(artifactDirectory + Path.DirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var isMarkdown = Path.GetExtension(file).Equals(".md", StringComparison.OrdinalIgnoreCase);
                    var isOpenApi = section == "content"
                        && Path.GetExtension(file).Equals(".json", StringComparison.OrdinalIgnoreCase);
                    if (section == "content" && !isMarkdown && !isOpenApi && Path.GetFileName(file) != "toc.yml")
                    {
                        continue;
                    }

                    var target = Path.Combine(outputDirectory, destination, Path.GetRelativePath(source, file));
                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    if (isOpenApi)
                    {
                        using var definition = JsonDocument.Parse(File.ReadAllText(file));
                        if (!definition.RootElement.TryGetProperty("paths", out _))
                        {
                            throw new InvalidOperationException("Expected an OpenAPI definition: " + file);
                        }
                        File.WriteAllText(Path.ChangeExtension(target, ".md"),
                            "# REST API definition\n\n```json\n" + File.ReadAllText(file) + "\n```\n");
                    }
                    else if (isMarkdown)
                    {
                        var markdown = Regex.Replace(File.ReadAllText(file), @"\]\(([^)\s]+)\)", match =>
                        {
                            var link = match.Groups[1].Value;
                            if (link.StartsWith("#") || link.StartsWith("//")
                                || Regex.IsMatch(link, @"^[a-zA-Z][a-zA-Z0-9+.-]*:"))
                            {
                                return match.Value;
                            }

                            link = Regex.Replace(link, @"\.html(?=#|$)", ".md", RegexOptions.IgnoreCase);
                            if (link.StartsWith("/"))
                            {
                                link = Path.GetRelativePath(Path.GetDirectoryName(target),
                                    Path.Combine(outputDirectory, link.TrimStart('/'))).Replace('\\', '/');
                            }
                            return "](" + link + ")";
                        });
                        File.WriteAllText(target, markdown);
                    }
                    else
                    {
                        File.Copy(file, target, true);
                    }
                }
            }
        }

        if (!File.Exists(Path.Combine(outputDirectory, "index.md")))
        {
            throw new InvalidOperationException("The Markdown documentation must contain index.md.");
        }

        var archive = outputDirectory + ".zip";
        File.Delete(archive);
        ZipFile.CreateFromDirectory(outputDirectory, archive, CompressionLevel.Optimal, false);
    }

    private static string[] Patterns(JsonElement mapping, string name)
    {
        if (!mapping.TryGetProperty(name, out var patterns))
        {
            return Array.Empty<string>();
        }
        return patterns.ValueKind == JsonValueKind.String
            ? new[] { patterns.GetString() }
            : patterns.EnumerateArray().Select(pattern => pattern.GetString()).ToArray();
    }

    private static IEnumerable<string> Files(string directory, string[] patterns)
    {
        return patterns.SelectMany(pattern => Glob.Files(new DirectoryInfo(directory),
            pattern.Replace("**.", "**/*."), GlobOptions.CaseInsensitive)).Select(file => file.FullName);
    }
}
