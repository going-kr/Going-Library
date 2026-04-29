using System;
using System.Linq;
using System.Xml.Linq;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxR9PolymorphicWrapperTests
{
    [Fact]
    public void GoDataGrid_columnsRoundTrip_PolymorphicDerivedTypes()
    {
        // Verifies that a wrapper list with abstract base (GoDataGridColumn) can hold
        // and round-trip derived concrete types (GoDataGridLabelColumn, GoDataGridButtonColumn)
        // using the tag-name-as-class-name lookup.

        var grid = new GoDataGrid { Name = "grid" };
        var col1 = new GoDataGridLabelColumn
        {
            Name = "col_Label",
            HeaderText = "Header(GoDataGridLabelColumn)",
            Size = "100",
        };
        var col2 = new GoDataGridButtonColumn
        {
            Name = "col_Button",
            HeaderText = "Header(GoDataGridButtonColumn)",
            Size = "200",
        };
        grid.Columns.Add(col1);
        grid.Columns.Add(col2);

        var xml = GoGudxConverter.SerializeControl(grid);
        var root = XElement.Parse(xml);

        // v1.2.1: P4 wrappers grouped inside <Columns> property-name group element.
        var columnsGroup = root.Element("Columns");
        Assert.NotNull(columnsGroup);
        // Each child element should use the derived class name as tag
        Assert.NotNull(columnsGroup.Element("GoDataGridLabelColumn"));
        Assert.NotNull(columnsGroup.Element("GoDataGridButtonColumn"));

        // Round-trip
        var restored = (GoDataGrid)GoGudxConverter.DeserializeControl(xml);
        Assert.Equal(2, restored.Columns.Count);

        Assert.IsType<GoDataGridLabelColumn>(restored.Columns[0]);
        Assert.IsType<GoDataGridButtonColumn>(restored.Columns[1]);

        Assert.Equal("col_Label", restored.Columns[0].Name);
        Assert.Equal("Header(GoDataGridLabelColumn)", restored.Columns[0].HeaderText);
        Assert.Equal("100", restored.Columns[0].Size);

        Assert.Equal("col_Button", restored.Columns[1].Name);
        Assert.Equal("Header(GoDataGridButtonColumn)", restored.Columns[1].HeaderText);
        Assert.Equal("200", restored.Columns[1].Size);
    }

    [Fact]
    public void GoTreeNode_NodesRoundTrip_RecursiveNesting()
    {
        // Verifies GoTreeNode.Nodes with [GoChildWrappers] supports recursive nested structure
        // via GoTreeView.
        var tree = new GoTreeView { Name = "tree" };
        var rootNode = new GoTreeNode { Text = "root" };
        var child1 = new GoTreeNode { Text = "child1" };
        var child2 = new GoTreeNode { Text = "child2" };
        rootNode.Nodes.Add(child1);
        rootNode.Nodes.Add(child2);
        tree.Nodes.Add(rootNode);

        var xml = GoGudxConverter.SerializeControl(tree);
        var restored = (GoTreeView)GoGudxConverter.DeserializeControl(xml);

        Assert.Single(restored.Nodes);
        Assert.Equal("root", restored.Nodes[0].Text);
        Assert.Equal(2, restored.Nodes[0].Nodes.Count);
        Assert.Equal("child1", restored.Nodes[0].Nodes[0].Text);
        Assert.Equal("child2", restored.Nodes[0].Nodes[1].Text);
    }
}
