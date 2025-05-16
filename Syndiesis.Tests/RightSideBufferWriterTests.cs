using Syndiesis.Utilities;

namespace Syndiesis.Tests;

public sealed class RightSideBufferWriterTests
{
    [Test]
    public async Task UseCase0()
    {
        const int capacity = 10;
        Span<char> s = stackalloc char[capacity];
        var writer = new RightSideBufferWriter<char>(s);
        writer.Write("312");
        writer.Write('4');
        writer.Write("01");

        var finalized = writer.GetFinalized();
        await Assert.That(finalized.ToString()).IsEqualTo("014312");
    }

    [Test]
    public async Task TestOverflowWithChar()
    {
        const int capacity = 1;
        Span<char> s = stackalloc char[capacity];
        var writer = new RightSideBufferWriter<char>(s);
        writer.Write('4');

        // Can't use writer inside a lambda; hence the manual try block
        try
        {
            writer.Write('0');
            Assert.Fail("Expected an exception");
        }
        catch { }

        var finalized = writer.GetFinalized();
        await Assert.That(finalized.ToString()).IsEqualTo("4");
    }

    [Test]
    public async Task TestOverflowWithString0()
    {
        const int capacity = 1;
        Span<char> s = stackalloc char[capacity];
        var writer = new RightSideBufferWriter<char>(s);
        writer.Write('4');

        // Can't use writer inside a lambda; hence the manual try block
        try
        {
            writer.Write("0");
            Assert.Fail("Expected an exception");
        }
        catch { }

        try
        {
            writer.Write("041");
            Assert.Fail("Expected an exception");
        }
        catch { }

        // Expect no exception with an empty string
        writer.Write(string.Empty);

        var finalized = writer.GetFinalized();
        await Assert.That(finalized.ToString()).IsEqualTo("4");
    }

    [Test]
    public async Task TestOverflowWithString1()
    {
        const int capacity = 2;
        Span<char> s = stackalloc char[capacity];
        var writer = new RightSideBufferWriter<char>(s);
        writer.Write('4');

        try
        {
            writer.Write("041");
            Assert.Fail("Expected an exception");
        }
        catch { }

        try
        {
            writer.Write("01");
            Assert.Fail("Expected an exception");
        }
        catch { }

        // Expect no exception with an empty string
        writer.Write(string.Empty);

        // Expect the buffer to have one more char of remaining space
        writer.Write("0");

        // Now the buffer should be full
        try
        {
            writer.Write("0");
            Assert.Fail("Expected an exception");
        }
        catch { }

        var finalized = writer.GetFinalized();
        await Assert.That(finalized.ToString()).IsEqualTo("04");
    }

    [Test]
    public async Task TestEmpty()
    {
        Span<char> s = [];
        var writer = new RightSideBufferWriter<char>(s);
        var finalized = writer.GetFinalized();
        await Assert.That(finalized.ToString()).IsEqualTo(string.Empty);
    }
}
