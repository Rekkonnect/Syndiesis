using Avalonia.Controls;
using System;

namespace CSharpSyntaxEditor.Controls;

[Obsolete("This was just for seeing how to create a new user control without removing and rebuilding the existing controls")]
public partial class CodeEditorUser : UserControl
{
    public CodeEditorUser()
    {
        InitializeComponent();
    }
}
