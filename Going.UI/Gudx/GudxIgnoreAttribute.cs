using System;

namespace Going.UI.Gudx;

/// <summary>
/// Marks a property to be EXCLUDED from Gudx XML serialization.
/// Independent of `[System.Text.Json.Serialization.JsonIgnore]` — JSON behavior unchanged.
/// Use when a property:
///   - is layout-derived and reconstructed at runtime (e.g., GoControl.Bounds — computed from cells in v5)
///   - is runtime-only state that shouldn't persist (e.g., selection / hover)
///   - or otherwise should not appear in .gudx files even though it serializes correctly to JSON.
/// Both attribute filter (P1) and child filter (P2-P5/B1) honor this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class GudxIgnoreAttribute : Attribute
{
}
