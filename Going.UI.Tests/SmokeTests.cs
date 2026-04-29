using Going.UI.Design;
using Xunit;

namespace Going.UI.Tests;

public class SmokeTests
{
    [Fact]
    public void GoDesign_canBeInstantiated()
    {
        var d = new GoDesign();
        Assert.NotNull(d);
    }
}
