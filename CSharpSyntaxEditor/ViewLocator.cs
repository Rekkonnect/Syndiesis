using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CSharpSyntaxEditor.ViewModels;
using System;

namespace CSharpSyntaxEditor;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        var name = data?.GetType().FullName!.Replace("ViewModel", "View");
        var type = name is null ? null : Type.GetType(name);

        if (type is null)
        {
            return new TextBlock { Text = "Not Found: " + name };
        }

        return (Control)Activator.CreateInstance(type)!;
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}