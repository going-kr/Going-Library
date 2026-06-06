using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Going.UI.Collections;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Gudx;
using Going.UI.Utils;
using Xunit;

namespace Going.UI.Tests.Controls;

public class GoItemListTests
{
    public sealed class LogVM { public string Message { get; set; } = ""; public int N { get; set; } }

    private const string TemplateBox = "<GoBoxPanel><Childrens><GoLabel Text='{Message}'/></Childrens></GoBoxPanel>";

    [Fact]
    public void ObservableList_ImplementsIGoObservable_ChangedFlag()
    {
        var list = new ObservableList<int>();
        IGoObservable obs = list;          // 비제네릭 접근
        list.Add(1);
        Assert.True(obs.Changed);
        obs.Changed = false;
        Assert.False(list.Changed);
    }

    [Fact]
    public void Rebuild_GeneratesRow_PerItem_WithBounds()
    {
        var il = new GoItemList { ItemHeight = 20 };
        il.Bounds = Util.FromRect(0, 0, 200, 100);
        il.ItemTemplateXml = XElement.Parse(TemplateBox);
        il.ItemsSource = new List<LogVM>
        {
            new() { Message = "a" }, new() { Message = "b" }, new() { Message = "c" },
        };

        il.RebuildRows();

        Assert.Equal(3, il.Childrens.Count);
        Assert.Equal(0, il.Childrens[0].Bounds.Top);
        Assert.Equal(20, il.Childrens[1].Bounds.Top);
        Assert.Equal(40, il.Childrens[2].Bounds.Top);
    }

    [Fact]
    public void Rebuild_RowBindsToItem()
    {
        var il = new GoItemList { ItemHeight = 20 };
        il.Bounds = Util.FromRect(0, 0, 200, 100);
        il.ItemTemplateXml = XElement.Parse(TemplateBox);
        il.ItemsSource = new List<LogVM> { new() { Message = "hello" } };

        il.RebuildRows();

        var box = (GoBoxPanel)il.Childrens[0];
        var lbl = (GoLabel)box.Childrens[0];
        lbl.FireUpdate();
        Assert.Equal("hello", lbl.Text);
    }

    [Fact]
    public void Rebuild_DistinctItems_DistinctRows()
    {
        var il = new GoItemList { ItemHeight = 20 };
        il.Bounds = Util.FromRect(0, 0, 200, 100);
        il.ItemTemplateXml = XElement.Parse(TemplateBox);
        il.ItemsSource = new List<LogVM> { new() { Message = "x" }, new() { Message = "y" } };

        il.RebuildRows();

        var lbl0 = (GoLabel)((GoBoxPanel)il.Childrens[0]).Childrens[0];
        var lbl1 = (GoLabel)((GoBoxPanel)il.Childrens[1]).Childrens[0];
        lbl0.FireUpdate(); lbl1.FireUpdate();
        Assert.Equal("x", lbl0.Text);
        Assert.Equal("y", lbl1.Text);
    }

    [Fact]
    public void Deserialize_CapturesItemTemplate_AndItemsBinding()
    {
        var xml = "<GoItemList ItemHeight='28' Items='{Logs}'><ItemTemplate>" + TemplateBox + "</ItemTemplate></GoItemList>";
        var il = (GoItemList)GoGudxConverter.ReadElement(XElement.Parse(xml));

        Assert.Equal(28, il.ItemHeight);
        Assert.NotNull(il.ItemTemplateXml);
        Assert.Equal("GoBoxPanel", il.ItemTemplateXml!.Name.LocalName);
        Assert.Equal("{Logs}", il.PendingBindings!["Items"]);   // 컬렉션 바인딩 보류
    }

    [Fact]
    public void RoundTrip_ItemList_PreservesItemsAndTemplate()
    {
        var il = (GoItemList)GoGudxConverter.ReadElement(XElement.Parse(
            "<GoItemList ItemHeight='28' Items='{Logs}'><ItemTemplate><GoLabel Text='{Message}'/></ItemTemplate></GoItemList>"));
        var e = GoGudxConverter.WriteAny(il);

        Assert.Equal("{Logs}", e.Attribute("Items")?.Value);            // D6: 비스칼라 바인딩 보존
        var tmpl = e.Element("ItemTemplate");
        Assert.NotNull(tmpl);
        Assert.Equal("GoLabel", tmpl!.Elements().First().Name.LocalName);
        Assert.Null(e.Element("Childrens"));                            // 생성 행 미출력
    }

    [Fact]
    public void Update_AddItem_AddsRow()
    {
        var il = new GoItemList { ItemHeight = 20 };
        il.Bounds = Util.FromRect(0, 0, 200, 100);
        il.ItemTemplateXml = XElement.Parse("<GoLabel Text='{Message}'/>");
        var data = new ObservableList<LogVM> { new() { Message = "a" } };
        il.ItemsSource = data;

        il.RebuildRows();
        Assert.Single(il.Childrens);

        data.Add(new LogVM { Message = "b" });
        il.FireUpdate();   // OnUpdate가 Changed 감지 → 재생성
        Assert.Equal(2, il.Childrens.Count);
    }
}
