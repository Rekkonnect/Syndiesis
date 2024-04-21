using CSharpSyntaxEditor.Utilities;

namespace CSharpSyntaxEditor.Tests;

public class MultilineStringEditorTests
{
    private const string codeText = """
        abc
        def
        string void Hello() { }
        void NewMethod()
        {

        }

        """;

    [Test]
    public void RoundTrip()
    {
        var editor = new MultilineStringEditor();
        editor.SetText(codeText);
        var converted = editor.FullString();
        Assert.That(converted, Is.EqualTo(codeText));
    }

    [Test]
    public void InsertText()
    {
        var editor = new MultilineStringEditor();
        editor.SetText(codeText);

        editor.InsertAt(5, 0, ' ');
        editor.InsertAt(5, 1, "  ");
        editor.InsertAt(5, 2, " ");

        editor.InsertAt(5, 4, """Console.WriteLine("this is within the test");""");

        var converted = editor.FullString();
        const string expected = """
            abc
            def
            string void Hello() { }
            void NewMethod()
            {
                Console.WriteLine("this is within the test");
            }
            
            """;
        Assert.That(converted, Is.EqualTo(expected));
    }

    [Test]
    public void RemoveLines()
    {
        var editor = new MultilineStringEditor();
        editor.SetText(codeText);

        editor.RemoveLineRange(0, 1);

        var converted = editor.FullString();
        const string expected = """
            string void Hello() { }
            void NewMethod()
            {
            
            }
            
            """;
        Assert.That(converted, Is.EqualTo(expected));
    }

    [Test]
    public void BreakLine()
    {
        var editor = new MultilineStringEditor();
        editor.SetText(codeText);

        editor.BreakLineAt(2, 22);
        editor.BreakLineAt(2, 20);

        var converted = editor.FullString();
        const string expected = """
            abc
            def
            string void Hello() 
            { 
            }
            void NewMethod()
            {
            
            }
            
            """;
        Assert.That(converted, Is.EqualTo(expected));
    }
}
