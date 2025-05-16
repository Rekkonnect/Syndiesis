using Syndiesis.Utilities;

namespace Syndiesis.Tests;

public sealed class HexIntegerWriterTests
{
    [Test]
    [MethodDataSource(nameof(WriterTestCasesSource))]
    public async Task Test(WriterTestCase @case)
    {
        var info = IntegerInfo.Create(@case.Value);
        var result = HexIntegerWriter.Write(info, @case.GroupLength);
        await Assert.That(result).IsEqualTo(@case.Expected);
    }

    public IReadOnlyList<WriterTestCase> WriterTestCasesSource()
    {
        return
        [
            new(0x00000123, 0, "00000123"),
            
            new(0x0000_0123, 4, "0000_0123"),
            new(0x1021_3201, 4, "1021_3201"),

            new(0x01_23_45_67, 2, "01_23_45_67"),

            new(0x01234567, 3, "01_234_567"),
        ];
    }

    public sealed record WriterTestCase(
        object Value, int GroupLength, string Expected);
}
