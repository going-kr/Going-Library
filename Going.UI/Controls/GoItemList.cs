using System.Collections;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Going.UI.Bindings;
using Going.UI.Collections;
using Going.UI.Containers;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Utils;

namespace Going.UI.Controls;

/// <summary>
/// 바인딩된 컬렉션을 ItemTemplate으로 반복 렌더하는 리스트. 각 행은 해당 아이템 객체에 바인딩된다.
/// 스크롤은 <see cref="GoScrollablePanel"/>에서 상속.
/// </summary>
public class GoItemList : GoScrollablePanel
{
    /// <summary>각 행의 높이.</summary>
    [GoProperty(PCategory.Control, 0)] public float ItemHeight { get; set; } = 30;

    /// <summary>바인딩으로 전달되는 컬렉션 참조(<c>Items="{Logs}"</c>). 펌프가 매 프레임 set.</summary>
    [JsonIgnore] public IEnumerable? ItemsSource { get; set; }

    /// <summary>마크업 attribute 이름(<c>Items</c>)용 별칭. 바인딩 전용(ItemsSource와 동일 백킹).</summary>
    [JsonIgnore] public IEnumerable? Items { get => ItemsSource; set => ItemsSource = value; }

    /// <summary>ItemTemplate 원본(행 복제 소스).</summary>
    [JsonIgnore] public XElement? ItemTemplateXml { get; set; }

    private object? lastSource;
    private int lastCount = -1;

    /// <inheritdoc/>
    protected override void OnUpdate()
    {
        if (NeedsRebuild()) RebuildRows();
        base.OnUpdate();   // 자식(행) 펌프
    }

    private bool NeedsRebuild()
    {
        var src = ItemsSource;
        if (!ReferenceEquals(src, lastSource)) return true;
        if (src == null) return false;
        if (src is IGoObservable obs && obs.Changed) return true;
        return CountOf(src) != lastCount;
    }

    /// <summary>현재 ItemsSource로 행을 전량 재생성한다.</summary>
    public void RebuildRows()
    {
        foreach (var c in Childrens) if (c is GoControl gc) gc.UnbindAll();
        Childrens.Clear();

        var src = ItemsSource;
        lastSource = src;
        if (src == null) { lastCount = 0; return; }

        int i = 0;
        foreach (var item in src)
        {
            if (ItemTemplateXml != null && item != null && GoGudxConverter.ReadElement(ItemTemplateXml) is GoControl row)
            {
                row.Bounds = Util.FromRect(0, i * ItemHeight, Width, ItemHeight);
                row.Parent = this;
                Childrens.Add(row);
                GudxBinder.WireTree(row, item);
                row.FireInit(Design);
            }
            i++;
        }
        lastCount = i;
        if (src is IGoObservable obs) obs.Changed = false;
    }

    private static int CountOf(IEnumerable src)
        => src is ICollection col ? col.Count : src.Cast<object>().Count();
}
