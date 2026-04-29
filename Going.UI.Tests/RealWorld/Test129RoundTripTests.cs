using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Going.UI.Design;
using Xunit;

namespace Going.UI.Tests.RealWorld;

public class Test129RoundTripTests
{
    private const string SourceGud = @"C:\Users\hy752\OneDrive\문서\Senvas\Test129\ui\Test129.gud";
    private const string GudxDir   = @"C:\Users\hy752\OneDrive\문서\Senvas\Test129\ui\gudx";
    private const string ResultGud = @"C:\Users\hy752\OneDrive\문서\Senvas\Test129\ui\Test129-gudx-result.gud";

    [Fact]
    public void Test129_RoundTrip_GudToGudxToGud()
    {
        if (!File.Exists(SourceGud))
        {
            Console.WriteLine($"[SKIP] Source file not found: {SourceGud}");
            return;
        }

        Console.WriteLine($"=== Step 1: {SourceGud} => {GudxDir} ===");
        var sourceJson = File.ReadAllText(SourceGud);
        Console.WriteLine($"  Source size: {sourceJson.Length} chars");

        var design = GoDesign.JsonDeserialize(sourceJson);
        Assert.NotNull(design);
        Console.WriteLine($"  Loaded GoDesign: Name={design!.Name}, Pages={design.Pages.Count}, Windows={design.Windows.Count}");

        // Clear gudx folder
        if (Directory.Exists(GudxDir))
            Directory.Delete(GudxDir, recursive: true);
        Directory.CreateDirectory(GudxDir);

        var masterPath = Path.Combine(GudxDir, "Master.gudx");
        design.SerializeGudx(masterPath);

        var gudxFiles = Directory.GetFiles(GudxDir, "*.gudx", SearchOption.AllDirectories);
        Console.WriteLine($"  Wrote {gudxFiles.Length} .gudx files:");
        foreach (var f in gudxFiles.OrderBy(x => x))
            Console.WriteLine($"    {Path.GetRelativePath(GudxDir, f)} ({new FileInfo(f).Length} bytes)");

        Console.WriteLine();
        Console.WriteLine($"=== Step 2: {GudxDir} => {ResultGud} ===");
        var restored = GoDesign.DeserializeGudx(masterPath);
        Assert.NotNull(restored);
        Console.WriteLine($"  Loaded GoDesign: Name={restored!.Name}, Pages={restored.Pages.Count}, Windows={restored.Windows.Count}");

        var resultJson = restored.JsonSerialize();
        File.WriteAllText(ResultGud, resultJson);
        Console.WriteLine($"  Wrote result: {resultJson.Length} chars");

        Console.WriteLine();
        Console.WriteLine($"=== Step 3: diff {Path.GetFileName(SourceGud)} <-> {Path.GetFileName(ResultGud)} ===");

        if (sourceJson == resultJson)
        {
            Console.WriteLine("  BYTE-IDENTICAL");
            return;
        }

        Console.WriteLine($"  Source: {sourceJson.Length} chars");
        Console.WriteLine($"  Result: {resultJson.Length} chars");
        Console.WriteLine($"  Delta size: {resultJson.Length - sourceJson.Length:+#;-#;0} chars");

        // Structural JSON diff (best effort)
        try
        {
            using var sDoc = JsonDocument.Parse(sourceJson);
            using var rDoc = JsonDocument.Parse(resultJson);
            var diffs = new System.Collections.Generic.List<string>();
            DiffJson(sDoc.RootElement, rDoc.RootElement, "$", diffs, maxDiffs: 50);

            Console.WriteLine($"  Structural differences ({diffs.Count}):");
            foreach (var d in diffs.Take(50)) Console.WriteLine($"    {d}");
            if (diffs.Count > 50) Console.WriteLine($"    ... (+{diffs.Count - 50} more)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  JSON diff failed: {ex.Message}");
        }

        // Don't fail the test -- this is a research run, not a regression check.
        // (User wants to see the result, not block the suite.)
    }

    private static void DiffJson(JsonElement a, JsonElement b, string path, System.Collections.Generic.List<string> diffs, int maxDiffs)
    {
        if (diffs.Count >= maxDiffs) return;

        if (a.ValueKind != b.ValueKind)
        {
            diffs.Add($"{path}: kind {a.ValueKind} vs {b.ValueKind}");
            return;
        }

        switch (a.ValueKind)
        {
            case JsonValueKind.Object:
                var aProps = a.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                var bProps = b.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                foreach (var key in aProps.Keys.Union(bProps.Keys).OrderBy(k => k))
                {
                    if (diffs.Count >= maxDiffs) return;
                    if (!aProps.ContainsKey(key)) { diffs.Add($"{path}.{key}: missing in source"); continue; }
                    if (!bProps.ContainsKey(key)) { diffs.Add($"{path}.{key}: missing in result"); continue; }
                    DiffJson(aProps[key], bProps[key], $"{path}.{key}", diffs, maxDiffs);
                }
                break;
            case JsonValueKind.Array:
                if (a.GetArrayLength() != b.GetArrayLength())
                {
                    diffs.Add($"{path}: array length {a.GetArrayLength()} vs {b.GetArrayLength()}");
                    return;
                }
                for (int i = 0; i < a.GetArrayLength(); i++)
                {
                    if (diffs.Count >= maxDiffs) return;
                    DiffJson(a[i], b[i], $"{path}[{i}]", diffs, maxDiffs);
                }
                break;
            case JsonValueKind.String:
                if (a.GetString() != b.GetString()) diffs.Add($"{path}: \"{Truncate(a.GetString())}\" vs \"{Truncate(b.GetString())}\"");
                break;
            case JsonValueKind.Number:
                if (a.GetRawText() != b.GetRawText()) diffs.Add($"{path}: {a.GetRawText()} vs {b.GetRawText()}");
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                // already same kind
                break;
        }
    }

    private static string Truncate(string? s, int max = 60) =>
        s == null ? "<null>" : (s.Length > max ? s.Substring(0, max) + "..." : s);
}
