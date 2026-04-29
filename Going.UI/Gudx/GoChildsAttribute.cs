using System;

namespace Going.UI.Gudx;

/// <summary>
/// Marks a property as a Gudx child node container.
/// Used by GoGudxConverter reflection algorithm to identify which collection holds
/// XML child elements during serialization/deserialization. The property type
/// determines the pattern (homogeneous list, cell-indexed collection, dictionary,
/// single object, wrapper-list-of-wrapper-types).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class GoChildsAttribute : Attribute
{
}
