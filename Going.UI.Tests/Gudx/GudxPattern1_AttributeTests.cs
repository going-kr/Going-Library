using Going.UI.Controls;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxPattern1_AttributeTests
{
    [Fact]
    public void GoButton_serializesScalarPropertiesAsAttributes()
    {
        var btn = new GoButton
        {
            Name = "btnSave",
            Text = "Save",
            FontSize = 14f,
            ButtonColor = "Good"
        };

        var xml = GoGudxConverter.SerializeControl(btn);

        Assert.Contains("Name=\"btnSave\"", xml);
        Assert.Contains("Text=\"Save\"", xml);
        Assert.Contains("FontSize=\"14\"", xml);
        Assert.Contains("ButtonColor=\"Good\"", xml);
        Assert.StartsWith("<GoButton", xml);

        // Negative assertions: [JsonIgnore] properties should NOT be emitted as standalone attributes
        Assert.DoesNotContain(" Width=\"", xml);
        Assert.DoesNotContain(" Tag=\"", xml);
    }

    [Fact]
    public void GoButton_roundTripsScalarProperties()
    {
        var original = new GoButton { Name = "btnA", Text = "A", FontSize = 18f, ButtonColor = "Danger" };
        var xml = GoGudxConverter.SerializeControl(original);
        var restored = (GoButton)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal("btnA", restored.Name);
        Assert.Equal("A", restored.Text);
        Assert.Equal(18f, restored.FontSize);
        Assert.Equal("Danger", restored.ButtonColor);
    }
}
