using Syndiesis.Utilities;

namespace Syndiesis.Tests;

public sealed class BinaryIntegerWriterTests
{
    [Test]
    [MethodDataSource(nameof(WriterTestCasesSource))]
    public async Task Test(WriterTestCase @case)
    {
        var info = IntegerInfo.Create(@case.Value);
        var result = BinaryIntegerWriter.Write(info, @case.GroupLength);
        await Assert.That(result).IsEqualTo(@case.Expected);
    }

    public IReadOnlyList<WriterTestCase> WriterTestCasesSource()
    {
        return
        [
            // int
            new(0b1, 0, "1"),
            new(0b0001, 0, "1"),

            new(0b0001, 4, "1"),
            new(0b0010_0101_0001_0000, 4, "10_0101_0001_0000"),

            new(0b101010101, 2, "1_01_01_01_01"),

            new(0b1111111, 3, "1_111_111"),
        ];
    }

    public sealed record WriterTestCase(
        object Value, int GroupLength, string Expected);
}
