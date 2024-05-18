using Avalonia.Collections;
using Avalonia.Controls.Documents;
using System;
using System.Collections.Generic;

namespace Syndiesis.Controls.Inlines;

public sealed class GroupedRunInlineCollection : AvaloniaList<object>
{
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    [Obsolete("Use the other overloads", error: true)]
    public override void Add(object item)
    {
        throw new InvalidOperationException("Use the other overloads");
    }

    [Obsolete("Use the other overloads", error: true)]
    public override void AddRange(IEnumerable<object> items)
    {
        throw new InvalidOperationException("Use the other overloads");
    }
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

    public GroupedRunInlineCollection() { }
    public GroupedRunInlineCollection(IEnumerable<RunOrGrouped> items) 
    {
        AddRange(items);
    }

    public void Add(RunOrGrouped run)
    {
        base.Add(run.AvailableObject!);
    }

    public void AddSingle(Run run)
    {
        Add(new SingleRunInline(run));
    }

    public void AddRange(IEnumerable<RunOrGrouped> runs)
    {
        foreach (var run in runs)
        {
            Add(run);
        }
    }

    public InlineCollection? AsInlineCollection()
    {
        var result = new InlineCollection();

        int count = Count;
        for (int i = 0; i < count; i++)
        {
            GroupedRunInline.AppendToInlines(result, this[i]);
        }

        return result;
    }

    public new RunOrGrouped this[int index]
    {
        get => RunOrGrouped.FromObject(base[index]);
        set => base[index] = value.AvailableObject!;
    }
}
