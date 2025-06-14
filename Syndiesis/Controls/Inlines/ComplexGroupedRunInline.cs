﻿using Avalonia.Controls.Documents;

namespace Syndiesis.Controls.Inlines;

public sealed class ComplexGroupedRunInline : GroupedRunInline
{
    public List<RunOrGrouped> Children { get; set; } = new();

    public override IEnumerable<object> InlineObjects => Children.Select(s => s.AvailableObject)!;

    public ComplexGroupedRunInline() { }

    public ComplexGroupedRunInline(IEnumerable<RunOrGrouped> children)
    {
        Children.AddRange(children);
    }

    protected override int CalculatedTextLength()
    {
        int length = 0;
        foreach (var run in Children)
        {
            length += GetTextLength(run);
        }
        return length;
    }

    protected override void CalculateText(StringBuilder builder)
    {
        foreach (var value in Children)
        {
            AppendText(builder, value);
        }
    }

    public override void AppendToInlines(InlineCollection inlines)
    {
        foreach (var value in Children)
        {
            AppendToInlines(inlines, value);
        }
    }

    public sealed class Builder(
        List<RunOrGrouped>? children = null)
        : Builder<ComplexGroupedRunInline>
    {
        public List<RunOrGrouped>? Children { get; set; } = children;

        public bool HasAny => Children is { Count: > 0 };

        public void AddNonNullChild(IBuilder? builder)
        {
            if (builder is null)
                return;

            AddChild(builder);
        }

        public void AddChild(IBuilder builder)
        {
            AddChild(new RunOrGrouped(builder));
        }
        
        public void AddChild(RunOrGrouped runOrGrouped)
        {
            Add(runOrGrouped);
        }
        
        public void Add(RunOrGrouped runOrGrouped)
        {
            Children ??= new();
            Children.Add(runOrGrouped);
        }

        public override ComplexGroupedRunInline Build()
        {
            if (Children is null)
                return new();

            var built = Children.Select(s => s.Build());
            return new(built);
        }
    }
}
