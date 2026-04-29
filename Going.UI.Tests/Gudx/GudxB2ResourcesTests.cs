using System.IO;
using System.Linq;
using Going.UI.Design;
using Going.UI.Gudx;
using SkiaSharp;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxB2ResourcesTests
{
    private static string CreateTempDir()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tmp);
        return tmp;
    }

    private static byte[] MakeTestPngBytes()
    {
        // Tiny 4x4 white image as PNG bytes
        using var bmp = new SKBitmap(4, 4);
        bmp.Erase(SKColors.White);
        using var img = SKImage.FromBitmap(bmp);
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    [Fact]
    public void SerializeGudx_extractsImageToResourcesFolder()
    {
        var tmp = CreateTempDir();
        try
        {
            var d = new GoDesign { Name = "T", DesignWidth = 800, DesignHeight = 600 };
            d.AddImage("logo", MakeTestPngBytes());

            var masterPath = Path.Combine(tmp, "Master.gudx");
            d.SerializeGudx(masterPath);

            // Image file extracted
            var imgPath = Path.Combine(tmp, "resources", "logo.png");
            Assert.True(File.Exists(imgPath));
            Assert.True(new FileInfo(imgPath).Length > 0);

            // Master XML references the image
            var master = File.ReadAllText(masterPath);
            Assert.Contains("<Image Name=\"logo\" File=\"resources/logo.png\"", master);
            Assert.DoesNotContain("base64", master.ToLower());
        }
        finally { Directory.Delete(tmp, recursive: true); }
    }

    [Fact]
    public void SerializeGudx_extractsFontToResourcesFontsFolder()
    {
        var tmp = CreateTempDir();
        try
        {
            var d = new GoDesign { Name = "T", DesignWidth = 800, DesignHeight = 600 };
            // Use any non-empty bytes — round-trip doesn't validate font format
            var fakeFont = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x10 };  // arbitrary
            d.AddFont("Pretendard-Medium", fakeFont);

            var masterPath = Path.Combine(tmp, "Master.gudx");
            d.SerializeGudx(masterPath);

            var fontPath = Path.Combine(tmp, "resources", "fonts", "Pretendard-Medium.ttf");
            Assert.True(File.Exists(fontPath));
            Assert.Equal(fakeFont, File.ReadAllBytes(fontPath));

            var master = File.ReadAllText(masterPath);
            Assert.Contains("<Font Name=\"Pretendard-Medium\" File=\"resources/fonts/Pretendard-Medium.ttf\"", master);
        }
        finally { Directory.Delete(tmp, recursive: true); }
    }

    [Fact]
    public void RoundTrip_preservesImagesAndFonts()
    {
        var tmp = CreateTempDir();
        try
        {
            var original = new GoDesign { Name = "T", DesignWidth = 800, DesignHeight = 600 };
            original.AddImage("logo", MakeTestPngBytes());
            var fontBytes = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD };
            original.AddFont("MyFont", fontBytes);

            var masterPath = Path.Combine(tmp, "Master.gudx");
            original.SerializeGudx(masterPath);

            var restored = GoDesign.DeserializeGudx(masterPath)!;

            // Images dict: re-populated from disk
            var restoredImages = restored.GetImages();
            Assert.Single(restoredImages);
            Assert.Equal("logo", restoredImages[0].name);
            Assert.NotEmpty(restoredImages[0].images);

            // Fonts dict: re-populated from disk
            var restoredFonts = restored.GetFonts();
            Assert.Single(restoredFonts);
            Assert.Equal("MyFont", restoredFonts[0].name);
            Assert.Equal(fontBytes, restoredFonts[0].fonts[0]);
        }
        finally { Directory.Delete(tmp, recursive: true); }
    }

    [Fact]
    public void RoundTrip_emptyResources_noFolder()
    {
        // No images/fonts -> no resources folder created
        var tmp = CreateTempDir();
        try
        {
            var d = new GoDesign { Name = "T", DesignWidth = 800, DesignHeight = 600 };
            var masterPath = Path.Combine(tmp, "Master.gudx");
            d.SerializeGudx(masterPath);

            Assert.False(Directory.Exists(Path.Combine(tmp, "resources")));

            var master = File.ReadAllText(masterPath);
            Assert.DoesNotContain("<Image", master);
            Assert.DoesNotContain("<Font", master);
        }
        finally { Directory.Delete(tmp, recursive: true); }
    }
}
