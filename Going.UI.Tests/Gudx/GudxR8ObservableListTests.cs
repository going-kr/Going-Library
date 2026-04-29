using System.Linq;
using System.Xml.Linq;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxR8ObservableListTests
{
    [Fact]
    public void GoListBox_serializesItemsAsObservableListChildren()
    {
        // Verifies ObservableList<T> works under [GoChildWrappers] dispatch
        // (List<T> path is already covered by P4 tests for GoSwitchPanel.Pages)
        var lb = new GoListBox { Name = "lb" };
        lb.Items.Add(new GoListItem { Text = "Apple" });
        lb.Items.Add(new GoListItem { Text = "Banana" });

        var xml = GoGudxConverter.SerializeControl(lb);
        var root = XElement.Parse(xml);

        var items = root.Elements("GoListItem").ToList();
        Assert.Equal(2, items.Count);
        Assert.Equal("Apple", items[0].Attribute("Text")?.Value);
        Assert.Equal("Banana", items[1].Attribute("Text")?.Value);
    }

    [Fact]
    public void GoListBox_roundTripsItemsViaObservableList()
    {
        var original = new GoListBox { Name = "lb" };
        original.Items.Add(new GoListItem { Text = "Apple" });
        original.Items.Add(new GoListItem { Text = "Banana" });

        var xml = GoGudxConverter.SerializeControl(original);
        var restored = (GoListBox)GoGudxConverter.DeserializeControl(xml);

        // ObservableList<T> deserialize was the InvalidCastException risk before R8 fix.
        Assert.Equal(2, restored.Items.Count);
        Assert.Equal("Apple", restored.Items[0].Text);
        Assert.Equal("Banana", restored.Items[1].Text);
    }
}
