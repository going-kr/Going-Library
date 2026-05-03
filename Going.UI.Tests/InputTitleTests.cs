using System.Text.Json;
using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Json;
using Xunit;

namespace Going.UI.Tests;

public class InputTitleTests
{
    [Fact]
    public void GoValueString_TitleProps_Defaults()
    {
        var v = new GoValueString();
        Assert.Equal(12F, v.TitleFontSize);
        Assert.Equal(GoContentAlignment.MiddleCenter, v.TitleContentAlignment);
        Assert.Equal(GoAutoFontSize.NotUsed, v.AutoTitleFontSize);
    }

    [Fact]
    public void GoValueString_TitleProps_RoundTrip()
    {
        var src = new GoValueString
        {
            TitleFontSize = 16F,
            TitleContentAlignment = GoContentAlignment.MiddleLeft,
            AutoTitleFontSize = GoAutoFontSize.M,
        };
        var json = JsonSerializer.Serialize(src, GoJsonConverter.Options);
        var back = JsonSerializer.Deserialize<GoValueString>(json, GoJsonConverter.Options);

        Assert.NotNull(back);
        Assert.Equal(16F, back!.TitleFontSize);
        Assert.Equal(GoContentAlignment.MiddleLeft, back.TitleContentAlignment);
        Assert.Equal(GoAutoFontSize.M, back.AutoTitleFontSize);
    }

    [Fact]
    public void GoInputString_InheritsTitleProps_PolymorphicRoundTrip()
    {
        // GoInput 파생도 IGoControl polymorphic round-trip에서 동일 속성 노출 확인
        var t = new GoInputString
        {
            TitleFontSize = 14F,
            AutoTitleFontSize = GoAutoFontSize.L,
            TitleContentAlignment = GoContentAlignment.TopLeft,
        };
        var json = JsonSerializer.Serialize<IGoControl>(t, GoJsonConverter.Options);
        var back = JsonSerializer.Deserialize<IGoControl>(json, GoJsonConverter.Options) as GoInputString;

        Assert.NotNull(back);
        Assert.Equal(14F, back!.TitleFontSize);
        Assert.Equal(GoAutoFontSize.L, back.AutoTitleFontSize);
        Assert.Equal(GoContentAlignment.TopLeft, back.TitleContentAlignment);
    }
}
