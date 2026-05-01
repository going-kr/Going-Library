using System;

namespace Going.UI.Gudx;

/// <summary>
/// 1-arity generic wrapper class 에 박제하여 numeric type alias 자동 등록을 명시한다.
/// 박제 시 byte/sbyte/short/ushort/int/uint/long/ulong/float/double/decimal 11종으로
/// close generic instance 자동 생성 + alias (e.g., Foo_double → Foo&lt;double&gt;) 등록.
/// 박제 안 된 1-arity generic 는 alias 시도 X (constraint 위반 시 ArgumentException 회피).
/// </summary>
/// <example>
/// [GoNumericAlias]
/// public class GoDataGridNumberColumn&lt;T&gt; : GoDataGridColumn where T : struct, INumber&lt;T&gt;
/// { ... }
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class GoNumericAliasAttribute : Attribute
{
}
