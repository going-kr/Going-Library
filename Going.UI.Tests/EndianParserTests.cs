using Going.Basis.Utils;

namespace Going.UI.Tests;

public class EndianParserTests
{
    [Theory]
    [InlineData(EndianOrder.ABCD, 0x1234, 0x5678)]
    [InlineData(EndianOrder.BADC, 0x3412, 0x7856)]
    [InlineData(EndianOrder.CDAB, 0x5678, 0x1234)]
    [InlineData(EndianOrder.DCBA, 0x7856, 0x3412)]
    public void ToWords_splits_int32_by_order(EndianOrder order, int word0, int word1)
    {
        var words = EndianParser.ToWords(0x12345678, order);

        Assert.Equal(new[] { word0, word1 }, words);
    }

    [Theory]
    [InlineData(EndianOrder.ABCD)]
    [InlineData(EndianOrder.BADC)]
    [InlineData(EndianOrder.CDAB)]
    [InlineData(EndianOrder.DCBA)]
    public void FromWords_round_trips_float(EndianOrder order)
    {
        const float value = 123.456f;
        var words = EndianParser.ToWords(value, order);

        var parsed = EndianParser.FromWords<float>(words, order);

        Assert.Equal(value, parsed);
    }

    [Fact]
    public void FromWords_reads_uint16_from_single_word()
    {
        var value = EndianParser.FromWords<ushort>(new[] { 0x1234 }, EndianOrder.DCBA);

        Assert.Equal((ushort)0x1234, value);
    }
}
