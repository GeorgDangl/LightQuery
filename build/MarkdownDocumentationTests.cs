using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

internal static class MarkdownDocumentationTests
{
    public static void Run(string root)
    {
        root = Path.GetFullPath(root);
        var source = Path.Combine(root, "source");
        var html = Path.Combine(root, "html");
        var output = Path.Combine(root, "markdown");
        Directory.CreateDirectory(source);
        Directory.CreateDirectory(Path.Combine(html, "api"));
        Directory.CreateDirectory(Path.Combine(html, "guides"));
        Directory.CreateDirectory(Path.Combine(html, "images"));
        File.WriteAllText(Path.Combine(source, "README.md"), "# Source README\n\n[API](/api/Example.html#Run)\n");
        File.WriteAllText(Path.Combine(source, "LICENSE"), "Example license");
        File.WriteAllText(Path.Combine(html, "index.html"), "<html><body><nav>NO NAV</nav><article><h1>Home</h1></article></body></html>");
        File.WriteAllText(Path.Combine(html, "api", "Example.html"),
            "<html><head></head><body><article data-uid='Example'><h1 id='Example'>Example API</h1>"
            + "<pre><code class='lang-csharp'>public class Example</code></pre>"
            + "<h4 id='Run'>Run</h4><p>Runs the example.</p><a href='../guides/start.html#hello'>Guide</a>"
            + "<a href='https://example.com/remote.html'>External</a><script>NO SCRIPT</script></article></body></html>");
        File.WriteAllText(Path.Combine(html, "guides", "start.html"),
            "<article><h1 id='hello'>Getting started</h1><p>Use this API.</p><img src='../images/example.svg'>"
            + "<a href='/api/Example.html#Run'>Run example</a></article>");
        File.WriteAllText(Path.Combine(html, "images", "example.svg"), "<svg xmlns='http://www.w3.org/2000/svg'/>");
        File.WriteAllText(Path.Combine(html, "unpublished.html"), "<article><h1>Unpublished project</h1></article>");
        File.WriteAllText(Path.Combine(html, "manifest.json"),
            "{\"files\":[{\"output\":{\".html\":{\"relative_path\":\"index.html\"}}},"
            + "{\"output\":{\".html\":{\"relative_path\":\"api/Example.html\"}}},"
            + "{\"output\":{\".html\":{\"relative_path\":\"guides/start.html\"}}}]}");
        var before = File.ReadAllText(Path.Combine(source, "README.md"));
        MarkdownDocumentation.Build(source, html, output, true);
        var api = File.ReadAllText(Path.Combine(output, "api", "Example.md"));
        Assert(api.Contains("public class Example") && api.Contains("Runs the example."), "API content");
        Assert(api.Contains("id=\"Run\"") && api.Contains("../guides/start.md#hello"), "API anchors/links");
        Assert(api.Contains("https://example.com/remote.html") && !api.Contains("NO SCRIPT"), "external links/scripts");
        Assert(File.ReadAllText(Path.Combine(output, "index.md")).Contains("# Source README"), "source README index");
        Assert(File.ReadAllText(Path.Combine(source, "README.md")) == before, "source unchanged");
        using (var zip = ZipFile.OpenRead(output + ".zip"))
        {
            Assert(zip.GetEntry("index.md") != null && zip.GetEntry("api/Example.md") != null, "ZIP root/API");
            Assert(zip.GetEntry("images/example.svg") != null && zip.GetEntry("LICENSE") != null, "ZIP resources/license");
            Assert(!zip.Entries.Any(entry => entry.FullName.Contains("markdown/")), "no directory wrapper");
            Assert(zip.GetEntry("unpublished.md") == null, "preserve public documentation boundaries");
        }
        File.WriteAllText(Path.Combine(output, "stale.md"), "stale");
        MarkdownDocumentation.Build(source, html, output, true);
        Assert(!File.Exists(Path.Combine(output, "stale.md")), "clean regeneration");
        var sphinx = Path.Combine(root, "sphinx");
        Directory.CreateDirectory(sphinx);
        File.WriteAllText(Path.Combine(sphinx, "index.html"), "<div role='main' data-uid=''><h1>Sphinx article</h1><p>Sphinx body.</p><pre><code>not an API</code></pre></div>");
        MarkdownDocumentation.Build(source, sphinx, output + "-sphinx", requireManifest: false);
        Assert(File.ReadAllText(Path.Combine(output + "-sphinx", "index.md")).Contains("# Source README"), "Sphinx conversion");
        ExpectFailure(() => MarkdownDocumentation.Build(source, sphinx, output + "-missing-api", true, false), "missing API");
        Assert(!File.Exists(output + "-missing-api.zip"), "missing API cannot leave a publishable archive");
        ExpectFailure(() => MarkdownDocumentation.Build(source, sphinx, output + "-missing-manifest"), "missing manifest");
        var callbackCount = 0;
        ExpectFailure(() => MarkdownDocumentation.Publish("http://127.0.0.1:1", "dummy", "1.2.3", output + ".zip",
            () => { callbackCount++; throw new Exception("HTML failed"); }), "HTML failure");
        Assert(callbackCount == 1, "HTML is called once");
        MarkdownDocumentation.Publish("http://127.0.0.1:1", "dummy", "1.2.3", output + ".zip",
            () => { throw new Exception("Upload failed with status code: Conflict\nalready exists"); }, true);
        ExpectFailure(() => MarkdownDocumentation.Publish("http://127.0.0.1:1", "dummy", "1.2.3", output + ".zip",
            () => { throw new Exception("Upload failed with status code: Conflict"); }), "strict HTML conflict");
        ExpectFailure(() => MarkdownDocumentation.Publish("http://127.0.0.1:1", "dummy", "", output + ".zip",
            () => { throw new Exception("must not run"); }), "explicit version");
        TestUpload(output + ".zip", 200);
        TestUpload(output + ".zip", 500);
        Console.WriteLine("PASS: generation, API content, source README, assets, anchors, clean ZIP, Sphinx, fail-closed publishing, conflict policy and multipart upload.");
    }

    private static void TestUpload(string package, int status)
    {
        var socket = new TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        var port = ((IPEndPoint)socket.LocalEndpoint).Port;
        var url = "http://127.0.0.1:" + port + "/";
        try
        {
            var htmlCompleted = false;
            var received = Task.Run(() =>
            {
                using (var client = socket.AcceptTcpClient())
                using (var stream = client.GetStream())
                {
                    stream.ReadTimeout = 10000;
                    stream.WriteTimeout = 10000;
                    var header = new StringBuilder();
                    while (!header.ToString().EndsWith("\r\n\r\n", StringComparison.Ordinal))
                    {
                        var next = stream.ReadByte();
                        Assert(next >= 0 && header.Length < 65536, "HTTP request headers");
                        header.Append((char)next);
                    }
                    Assert(htmlCompleted, "HTML completes before Markdown upload");
                    Assert(header.ToString().StartsWith("POST /API/MarkdownDocuments/Upload HTTP/1.1", StringComparison.Ordinal), "upload route");
                    var length = int.Parse(header.ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .Single(line => line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)).Split(':')[1].Trim());
                    var bytes = new byte[length];
                    var offset = 0;
                    while (offset < bytes.Length)
                    {
                        var count = stream.Read(bytes, offset, bytes.Length - offset);
                        Assert(count > 0, "HTTP request body");
                        offset += count;
                    }
                    var body = Encoding.UTF8.GetString(bytes);
                    Assert(body.Contains("ApiKey") && body.Contains("dummy-test-key"), "API key field");
                    Assert(body.Contains("Version") && body.Contains("1.2.3-beta+sha"), "exact version field");
                    Assert(body.Contains("MarkdownPackage") && body.Contains("markdown.zip"), "package field");
                    var response = Encoding.ASCII.GetBytes("HTTP/1.1 " + status + " Test\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
                    stream.Write(response, 0, response.Length);
                }
            });
            Action publish = () => MarkdownDocumentation.Publish(url, "dummy-test-key", "1.2.3-beta+sha", package,
                () => htmlCompleted = true);
            if (status == 200)
                publish();
            else
                ExpectFailure(publish, "Markdown HTTP failure");
            received.GetAwaiter().GetResult();
        }
        finally { socket.Stop(); }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new Exception("Assertion failed: " + message);
    }

    private static void ExpectFailure(Action action, string message)
    {
        try { action(); }
        catch { return; }
        throw new Exception("Expected failure: " + message);
    }
}
