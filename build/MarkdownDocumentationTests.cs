using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Fallout.Common;

internal static class MarkdownDocumentationTests
{
    public static void Run(string fixtureDirectory)
    {
        fixtureDirectory = Path.GetFullPath(fixtureDirectory);
        if (Directory.Exists(fixtureDirectory))
        {
            Directory.Delete(fixtureDirectory, true);
        }

        var source = Path.Combine(fixtureDirectory, "source");
        var artifacts = Path.Combine(source, "output");
        var bundle = Path.Combine(artifacts, "docs-markdown");
        try
        {
            Write(source, "index.md",
                "# Documentation\n[Guide](/guide/page.html#topic)\n[External](https://example.test/page.html)\n");
            Write(source, "guide/page.md", "# Guide\n[Home](/index.html)\n");
            Write(source, "guide/excluded.md", "Excluded article");
            Write(source, "toc.yml", "- name: Home\n  href: index.md\n");
            Write(source, "api/Legacy.yml", "uid: Legacy\n");
            Write(artifacts, "private/index.md", "Private documentation");
            Write(artifacts, "docs/index.html", "<h1>HTML site</h1>");
            Write(bundle, "api/Example.md", "# Native API reference\n");
            var image = new byte[] { 137, 80, 78, 71 };
            var imagePath = Path.Combine(source, "images", "nested", "DIAGRAM.PNG");
            Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
            File.WriteAllBytes(imagePath, image);

            var configFile = Path.Combine(source, "docfx.json");
            File.WriteAllText(configFile, """
                {
                  "build": {
                    "content": [
                      {
                        "files": ["**/*.md", "**/*.yml"],
                        "exclude": ["guide/excluded.md"]
                      }
                    ],
                    "resource": [{"files": ["**.png"]}]
                  }
                }
                """);

            var originalIndex = File.ReadAllText(Path.Combine(source, "index.md"));
            MarkdownDocumentation.Create(configFile, bundle);
            MarkdownDocumentation.Create(configFile, bundle);

            Assert.True(File.ReadAllText(Path.Combine(source, "index.md")) == originalIndex,
                "Packaging must not modify source articles.");
            Assert.True(File.ReadAllText(Path.Combine(bundle, "index.md")).Contains("](guide/page.md#topic)"),
                "Root-relative HTML links must point to Markdown articles.");
            Assert.True(File.ReadAllText(Path.Combine(bundle, "guide/page.md")).Contains("](../index.md)"),
                "Nested articles must resolve root-relative links at their own depth.");
            Assert.True(File.ReadAllText(Path.Combine(bundle, "index.md")).Contains("https://example.test/page.html"),
                "External links must remain unchanged.");
            Assert.True(File.ReadAllBytes(Path.Combine(bundle, "images/nested/DIAGRAM.PNG")).SequenceEqual(image),
                "DocFX recursive image globs must include nested mixed-case resources.");
            Assert.True(File.ReadAllText(Path.Combine(bundle, "api/Example.md")) == "# Native API reference\n",
                "Packaging must preserve generated native API Markdown.");

            using (var archive = ZipFile.OpenRead(bundle + ".zip"))
            {
                Assert.True(archive.GetEntry("index.md") != null && archive.GetEntry("api/Example.md") != null,
                    "The ZIP must contain root-level articles and native API pages without a wrapper directory.");
                Assert.True(archive.GetEntry("guide/excluded.md") == null && archive.GetEntry("api/Legacy.yml") == null,
                    "Excluded articles and intermediate DocFX API YAML must not be published.");
                Assert.True(archive.Entries.All(entry => !entry.FullName.StartsWith("output/")),
                    "HTML output and sibling private documentation must not enter the Markdown archive.");
            }

            File.Delete(Path.Combine(source, "index.md"));
            var rejectedMissingIndex = false;
            try
            {
                MarkdownDocumentation.Create(configFile, Path.Combine(artifacts, "missing-index"));
            }
            catch (InvalidOperationException)
            {
                rejectedMissingIndex = true;
            }
            Assert.True(rejectedMissingIndex, "Packaging must fail when the documentation entry point is missing.");
        }
        finally
        {
            Directory.Delete(fixtureDirectory, true);
        }
    }

    private static void Write(string directory, string relativePath, string content)
    {
        var file = Path.Combine(directory, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(file));
        File.WriteAllText(file, content);
    }
}
