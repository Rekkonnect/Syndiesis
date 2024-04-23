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

    [Test]
    public void DeleteBackwards_Full()
    {
        const string markup = """
            using System;

            public static void {|}Main()
            {
                Console.WriteLine("this is a test");
            }
            """;

        var editor = FromSource(markup, out int cursorLine, out int cursorColumn);
        editor.RemoveBackwardsAt(cursorLine, cursorColumn, int.MaxValue);

        var removed = editor.FullString();
        const string expected = """
            Main()
            {
                Console.WriteLine("this is a test");
            }
            """;
        Assert.That(removed, Is.EqualTo(expected));
    }

    [Test]
    public void DeleteBackwards_WithinLine()
    {
        const string markup = """
            using System;

            public static void {|}Main()
            {
                Console.WriteLine("this is a test");
            }
            """;

        var editor = FromSource(markup, out int cursorLine, out int cursorColumn);
        editor.RemoveBackwardsAt(cursorLine, cursorColumn, 15);

        var removed = editor.FullString();
        const string expected = """
            using System;
            
            publMain()
            {
                Console.WriteLine("this is a test");
            }
            """;
        Assert.That(removed, Is.EqualTo(expected));
    }

    [Test]
    [Ignore("This is not yet implemented -- and is out of the scope of the editor")]
    public void DeleteBackwards_WithSomeLines()
    {
        const string markup = """
            using System;

            public static void {|}Main()
            {
                Console.WriteLine("this is a test");
            }
            """;

        var editor = FromSource(markup, out int cursorLine, out int cursorColumn);
        editor.RemoveBackwardsAt(cursorLine, cursorColumn, 25);

        var removed = editor.FullString();
        const string expected = """
            using SystMain()
            {
                Console.WriteLine("this is a test");
            }
            """;
        Assert.That(removed, Is.EqualTo(expected));
    }

    [Test]
    public void DeleteForwards_WithinLine()
    {
        const string markup = """
            using System;

            public static void {|}Main()
            {
                Console.WriteLine("this is a test");
            }
            """;

        var editor = FromSource(markup, out int cursorLine, out int cursorColumn);
        editor.RemoveForwardsAt(cursorLine, cursorColumn, 5);

        var removed = editor.FullString();
        const string expected = """
            using System;
            
            public static void )
            {
                Console.WriteLine("this is a test");
            }
            """;
        Assert.That(removed, Is.EqualTo(expected));
    }

    private const string cursorIndicator = "{|}";

    private static MultilineStringEditor FromSourceWithoutMarkup(string source)
    {
        var editor = new MultilineStringEditor();
        editor.SetText(source);
        return editor;
    }

    private static MultilineStringEditor FromSource(
        string markupSource,
        out int cursorLine,
        out int cursorColumn)
    {
        cursorLine = 0;
        cursorColumn = -1;
        foreach (var enumeratedLine in markupSource.AsSpan().EnumerateLines())
        {
            int cursorIndex = enumeratedLine.IndexOf(cursorIndicator);
            if (cursorIndex > -1)
            {
                cursorColumn = cursorIndex;
                break;
            }

            cursorLine++;
        }

        var source = markupSource;
        if (cursorColumn >= 0)
        {
            source = markupSource.Replace(cursorIndicator, null);
        }

        var editor = new MultilineStringEditor();
        editor.SetText(source);
        return editor;
    }
}
