using Microsoft.CodeAnalysis;
using RoseLynn;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseCSharpTypeParameterSymbolExtraInlinesCreator<TSymbol>(
    BaseSymbolExtraInlinesCreatorContainer parentContainer)
    : BaseSymbolExtraInlinesCreator<TSymbol>(parentContainer)
    where TSymbol : class, ISymbol
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(TSymbol symbol)
    {
        var inlines = new ComplexGroupedRunInline.Builder();

        var typeParameters = symbol.GetTypeParameters();

        foreach (var parameter in typeParameters)
        {
            var inline = CreateTypeParameterInline(parameter);
            inlines.AddNonNullChild(inline);
        }

        return inlines;
    }

    private GroupedRunInline.IBuilder? CreateTypeParameterInline(ITypeParameterSymbol typeParameter)
    {
        // The inline will be structured as
        // \n  where TName : {constraints}

        var builder = new ComplexGroupedRunInline.Builder();

        if (typeParameter.HasUnmanagedTypeConstraint)
        {
            AddKeywordConstraint("unmanaged");
        }
        else if (typeParameter.HasValueTypeConstraint)
        {
            AddKeywordConstraint("struct");
        }
        else if (typeParameter.HasReferenceTypeConstraint)
        {
            var classRun = KeywordRun("class");
            var annotationInline = CreateNullableAnnotationInline(
                classRun,
                typeParameter.ReferenceTypeConstraintNullableAnnotation);
            AddConstraintRunOrGrouped(annotationInline);
        }

        if (typeParameter.HasNotNullConstraint)
        {
            AddKeywordConstraint("notnull");
        }

        var constraintTypes = typeParameter.ConstraintTypes;
        var constraintTypeNullableAnnotations = typeParameter.ConstraintNullableAnnotations;
        for (int i = 0; i < constraintTypes.Length; i++)
        {
            var constraintType = constraintTypes[i];
            var annotation = constraintTypeNullableAnnotations[i];

            var typeInlineCreator = ParentContainer.RootContainer.Commons.CreatorForSymbol(
                constraintType);
            // We should never encounter this
            if (typeInlineCreator is null)
                continue;

            var typeRun = typeInlineCreator.CreateSymbolInline(constraintType);
            var annotationInline = CreateNullableAnnotationInline(new(typeRun), annotation);
            AddConstraintRunOrGrouped(annotationInline);
        }

        if (typeParameter.HasConstructorConstraint)
        {
            AddConstraint(ConstructorConstraint());
        }

        if (typeParameter.AllowsRefLikeType)
        {
            AddKeywordConstraint("allows ref struct");
        }

        if (!builder.HasAny)
            return null;

        var whitespaceRun = Run("  ", CommonStyles.RawValueBrush);
        var whereRun = KeywordRun("where ");
        var nameRun = SingleRun(typeParameter.Name, ColorizationStyles.TypeParameterBrush);
        var whereSplitterRun = Run(" : ", CommonStyles.RawValueBrush);

        builder.Children!.InsertRange(0, [whitespaceRun, whereRun, nameRun, whereSplitterRun]);
        return builder;

        void AddKeywordConstraint(string text)
        {
            AddConstraint(SingleKeywordRun(text));
        }

        void AddSplitter()
        {
            if (!builder.HasAny)
            {
                return;
            }

            builder.AddChild(CreateArgumentSeparatorRun());
        }

        void AddConstraint(GroupedRunInline.IBuilder inline)
        {
            AddSplitter();
            builder.AddChild(inline);
        }

        void AddConstraintRunOrGrouped(RunOrGrouped inline)
        {
            AddSplitter();
            builder.AddChild(inline);
        }
    }

    private static RunOrGrouped CreateNullableAnnotationInline(
        RunOrGrouped inline,
        NullableAnnotation annotation)
    {
        if (annotation is not NullableAnnotation.Annotated)
            return inline;

        return new ComplexGroupedRunInline.Builder([inline, NullableAnnotationRun()]);
    }

    private static UIBuilder.Run NullableAnnotationRun()
    {
        return Run("?", CommonStyles.RawValueBrush);
    }

    private static GroupedRunInline.IBuilder ConstructorConstraint()
    {
        var newRun = KeywordRun("new");
        var parenthesesRun = Run("()", CommonStyles.RawValueBrush);
        return new SimpleGroupedRunInline.Builder([newRun, parenthesesRun]);
    }
}
