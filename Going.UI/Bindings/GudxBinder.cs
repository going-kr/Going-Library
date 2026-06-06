using System.Diagnostics;
using Going.UI.Containers;
using Going.UI.Controls;

namespace Going.UI.Bindings;

/// <summary>
/// 컨트롤 트리를 재귀 순회하며 각 컨트롤의 보류 바인딩(<see cref="GoControl.PendingBindings"/>)을
/// root 기준 실제 바인딩으로 wire한다. 컴포넌트 인스턴스 경계에서는 root를 파라미터 스코프로 교체한다.
/// <see cref="Going.UI.Design.GoDesign.WireBindings"/>의 구현부.
/// </summary>
public static class GudxBinder
{
    private const int MaxDepth = 32;

    /// <summary>node와 그 자손(IGoContainer.Childrens)의 보류 바인딩을 root에 연결.</summary>
    public static void WireTree(IGoControl node, object root) => WireTree(node, root, 0);

    private static void WireTree(IGoControl node, object root, int depth)
    {
        if (depth > MaxDepth)
        {
            Debug.WriteLine("[GudxBinder] nesting depth > 32 — aborting (possible cyclic component).");
            return;
        }

        // 컴포넌트 경계: root를 인스턴스 파라미터 스코프로 교체 후 자식 wire
        if (node is GoComponentInstance ci)
        {
            var scope = GoComponentScope.Build(ci, root);
            foreach (var child in ci.Childrens) WireTree(child, scope, depth + 1);
            return;
        }

        if (node is GoControl gc && gc.PendingBindings is { } pend)
        {
            foreach (var kv in pend)
            {
                try
                {
                    if (root is GoComponentScope scope)
                        GudxBindingExpression.WireScoped(gc, kv.Key, kv.Value, scope);
                    else
                        GudxBindingExpression.Wire(gc, kv.Key, kv.Value, root);
                }
                catch (System.Exception ex)
                {
                    // wire 실패(멤버 없음/타입 불일치 등)는 해당 바인딩만 스킵 — 다른 바인딩·렌더 영향 없음
                    Debug.WriteLine($"[GudxBinder] wire failed for {gc.GetType().Name}.{kv.Key} = '{kv.Value}': {ex.Message}");
                }
            }
        }

        if (node is IGoContainer cont)
            foreach (var child in cont.Childrens)
                WireTree(child, root, depth + 1);
    }
}
