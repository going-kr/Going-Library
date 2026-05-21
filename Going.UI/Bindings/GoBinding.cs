using System;
using System.Reflection;
using Going.UI.Controls;

namespace Going.UI.Bindings;

/// <summary>
/// 단일 binding 인스턴스. 한 컨트롤 속성과 한 소스 식 간의 동기화 상태를 보관한다.
/// </summary>
internal sealed class GoBinding
{
    public required PropertyInfo CtrlProperty { get; init; }
    public required Func<GoControl, object?> CtrlGet { get; init; }
    public required Action<GoControl, object?> CtrlSet { get; init; }
    public required Func<object?> SourceGet { get; init; }
    public Action<object?>? SourceSet { get; init; }

    public object? LastSrcValue;
    public object? LastCtrlValue;
    public bool PendingFlush;
    public bool Initialized;

    public bool IsCommand;
    public int CommandTimeout;
    public object? PendingCommandValue;
    public long PendingCommandTick;
    public bool HasPendingCommand;
}
