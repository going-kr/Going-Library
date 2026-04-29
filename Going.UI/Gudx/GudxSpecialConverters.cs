using System;
using System.Globalization;
using SkiaSharp;

namespace Going.UI.Gudx;

/// <summary>
/// Special-type converters for SKColor and SKRect scalar serialization in Gudx.
/// These handle format/parse for non-primitive types that are treated as scalars
/// in the Gudx XML schema.
///
/// Design Decision: Public API (Phase 0 PoC).
/// Although internal to Gudx serialization, the class is public for testability.
/// Not externally documented; reserved for future Task 12+ refinements.
/// </summary>
public static class GudxSpecialConverters
{
    /// <summary>
    /// Formats an SKColor as #AARRGGBB hex string (8 hex digits, alpha first).
    /// Example: SKColor(0x1A, 0x2B, 0x3C, 0xFF) → "#FF1A2B3C"
    /// </summary>
    public static string FormatSKColor(SKColor c) =>
        $"#{c.Alpha:X2}{c.Red:X2}{c.Green:X2}{c.Blue:X2}";

    /// <summary>
    /// Parses an SKColor from hex string.
    /// Accepts both #AARRGGBB (8 digits) and #RRGGBB (6 digits, assumes opaque alpha=0xFF).
    /// </summary>
    public static SKColor ParseSKColor(string s)
    {
        if (string.IsNullOrEmpty(s) || s[0] != '#')
            throw new FormatException($"Invalid SKColor: '{s}'");
        var hex = s.Substring(1);
        // Accept #AARRGGBB or #RRGGBB
        if (hex.Length == 6) hex = "FF" + hex;
        if (hex.Length != 8) throw new FormatException($"Invalid SKColor hex length: '{s}'");
        var a = Convert.ToByte(hex.Substring(0, 2), 16);
        var r = Convert.ToByte(hex.Substring(2, 2), 16);
        var g = Convert.ToByte(hex.Substring(4, 2), 16);
        var b = Convert.ToByte(hex.Substring(6, 2), 16);
        return new SKColor(r, g, b, a);
    }

    /// <summary>
    /// Formats an SKRect as comma-separated "Left,Top,Right,Bottom" string.
    /// Uses InvariantCulture for float parsing.
    /// Example: SKRect(1, 2, 3, 4) → "1,2,3,4"
    /// </summary>
    public static string FormatSKRect(SKRect r) =>
        string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", r.Left, r.Top, r.Right, r.Bottom);

    /// <summary>
    /// Parses an SKRect from comma-separated string "Left,Top,Right,Bottom".
    /// Uses InvariantCulture for float parsing.
    /// </summary>
    public static SKRect ParseSKRect(string s)
    {
        var parts = s.Split(',');
        if (parts.Length != 4) throw new FormatException($"Invalid SKRect: '{s}'");
        return new SKRect(
            float.Parse(parts[0], CultureInfo.InvariantCulture),
            float.Parse(parts[1], CultureInfo.InvariantCulture),
            float.Parse(parts[2], CultureInfo.InvariantCulture),
            float.Parse(parts[3], CultureInfo.InvariantCulture));
    }
}
