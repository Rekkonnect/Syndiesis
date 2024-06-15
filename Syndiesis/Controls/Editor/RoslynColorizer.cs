using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Microsoft.CodeAnalysis;
using Syndiesis.Core;
using Syndiesis.InternalGenerators.Core;
using System.Text.Json.Serialization;

namespace Syndiesis.Controls.Editor;

public abstract partial class RoslynColorizer(ISingleTreeCompilationSource compilationSource)
    : DocumentColorizingTransformer
{
    public ISingleTreeCompilationSource CompilationSource { get; } = compilationSource;

    public bool Enabled = false;

    protected bool ShouldColorize()
    {
        return Enabled
            && AppSettings.Instance.EnableColorization;
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        if (ShouldColorize())
        {
            ColorizeLineEnabled(line);
        }
        else
        {
            ChangeLinePart(line.Offset, line.EndOffset, ResetLine);
        }
    }

    protected void ResetLine(VisualLineElement element)
    {
        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(Colors.White));
    }

    protected abstract void ColorizeLineEnabled(DocumentLine line);

    public readonly record struct SymbolTypeKind(SymbolKind SymbolKind, TypeKind TypeKind)
    {
        public static readonly SymbolTypeKind TypeParameter
            = new(SymbolKind.TypeParameter, TypeKind.TypeParameter);

        public static readonly SymbolTypeKind EnumField
            = new SymbolTypeKind() with
            {
                IsEnumField = true
            };

        public bool IsEnumField { get; init; }

        public static implicit operator SymbolTypeKind(SymbolKind kind)
            => new(kind, default);
        public static implicit operator SymbolTypeKind(TypeKind kind)
            => new(SymbolKind.NamedType, kind);
    }
}

partial class RoslynColorizer
{
    public static ColorizationStyles Styles
        => AppSettings.Instance.ColorizationPreferences.ColorizationStyles!;

    // Token kinds
    [SolidColor("Comment", 0xFF57A64A)]
    [SolidColor("Documentation", 0xFF608B4E)]
    [SolidColor("StringLiteral", 0xFFD69D85)]
    [SolidColor("PreprocessingStatement", 0xFF9B9B9B)]
    [SolidColor("DisabledText", 0xFF767676)]
    [SolidColor("NumericLiteral", 0xFFBB5E00)]
    [SolidColor("Keyword", 0xFF569CD6)]

    // Symbol kinds
    [SolidColor("Property", 0xFFFFC9B9)]
    [SolidColor("Field", 0xFFDCDCDC)]
    [SolidColor("Event", 0xFFFFB9EA)]
    [SolidColor("Method", 0xFFFFF4B9)]
    [SolidColor("Local", 0xFF88EAFF)]
    [SolidColor("Parameter", 0xFF88EAFF)]
    [SolidColor("Label", 0xFFDCDCDC)]
    [SolidColor("Constant", 0xFFC0B9FF)]
    [SolidColor("EnumField", 0xFFE9A0FA)]

    // Named type kinds
    [SolidColor("TypeParameter", 0xFFBFD39A)]
    [SolidColor("Module", 0xFF4EC9B0)]
    [SolidColor("Class", 0xFF4EC9B0)]
    [SolidColor("Struct", 0xFF4DCA85)]
    [SolidColor("Interface", 0xFFA2D080)]
    [SolidColor("Enum", 0xFFB8D7A3)]
    [SolidColor("Delegate", 0xFF4BCBC8)]

    // XML literal kinds
    [SolidColor("XmlText", 0xFFC2A186)]
    [SolidColor("XmlAttribute", 0xFF88EAFF)]
    [SolidColor("XmlTag", 0xFFA6BFFF)]
    [SolidColor("XmlEntityLiteral", 0xFF88EAFF)]
    [SolidColor("XmlCData", 0xFFFFF4B9)]
    public sealed partial class ColorizationStyles
    {
        // Custom gradient colorization for a premium feeling on rarer occurrences

        // Range variable

        public const uint RangeVariableStartDefaultInt = 0xFFDCDCDC;
        private Color _RangeVariableStartColor = Color.FromUInt32(RangeVariableStartDefaultInt);
        [JsonInclude]
        public Color RangeVariableStartColor
        {
            get => _RangeVariableStartColor;
            set => _RangeVariableStartColor = value;
        }
        private LazilyUpdatedGradientStop _RangeVariableStartStop = new(new(default, 0));
        public LazilyUpdatedGradientStop RangeVariableStartStop
        {
            get
            {
                _RangeVariableStartStop.Color = _RangeVariableStartColor;
                return _RangeVariableStartStop;
            }
        }

        public const uint RangeVariableEndDefaultInt = 0xFF88EAFF;
        private Color _RangeVariableEndColor = Color.FromUInt32(RangeVariableEndDefaultInt);
        [JsonInclude]
        public Color RangeVariableEndColor
        {
            get => _RangeVariableEndColor;
            set => _RangeVariableEndColor = value;
        }
        private LazilyUpdatedGradientStop _RangeVariableEndStop = new(new(default, 1));
        public LazilyUpdatedGradientStop RangeVariableEndStop
        {
            get
            {
                _RangeVariableEndStop.Color = _RangeVariableEndColor;
                return _RangeVariableEndStop;
            }
        }

        public readonly LazilyUpdatedGradientBrush RangeVariableBrush;

        // Preprocessing 

        public const uint PreprocessingStartDefaultInt = 0xFF9B9B9B;
        private Color _PreprocessingStartColor = Color.FromUInt32(PreprocessingStartDefaultInt);
        [JsonInclude]
        public Color PreprocessingStartColor
        {
            get => _PreprocessingStartColor;
            set => _PreprocessingStartColor = value;
        }
        private LazilyUpdatedGradientStop _PreprocessingStartStop = new(new(default, 0));
        public LazilyUpdatedGradientStop PreprocessingStartStop
        {
            get
            {
                _PreprocessingStartStop.Color = _PreprocessingStartColor;
                return _PreprocessingStartStop;
            }
        }

        public const uint PreprocessingEndDefaultInt = 0xFFCBCBCB;
        private Color _PreprocessingEndColor = Color.FromUInt32(PreprocessingEndDefaultInt);
        [JsonInclude]
        public Color PreprocessingEndColor
        {
            get => _PreprocessingEndColor;
            set => _PreprocessingEndColor = value;
        }
        private LazilyUpdatedGradientStop _PreprocessingEndStop = new(new(default, 1));
        public LazilyUpdatedGradientStop PreprocessingEndStop
        {
            get
            {
                _PreprocessingEndStop.Color = _PreprocessingEndColor;
                return _PreprocessingEndStop;
            }
        }

        public readonly LazilyUpdatedGradientBrush PreprocessingBrush;

        // Conflict markers 

        public const uint ConflictMarkerStartDefaultInt = 0xFFA699E6;
        private Color _ConflictMarkerStartColor = Color.FromUInt32(ConflictMarkerStartDefaultInt);
        [JsonInclude]
        public Color ConflictMarkerStartColor
        {
            get => _ConflictMarkerStartColor;
            set => _ConflictMarkerStartColor = value;
        }
        private LazilyUpdatedGradientStop _ConflictMarkerStartStop = new(new(default, 0));
        public LazilyUpdatedGradientStop ConflictMarkerStartStop
        {
            get
            {
                _ConflictMarkerStartStop.Color = _ConflictMarkerStartColor;
                return _ConflictMarkerStartStop;
            }
        }

        public const uint ConflictMarkerEndDefaultInt = 0xFFFF01C1;
        private Color _ConflictMarkerEndColor = Color.FromUInt32(ConflictMarkerEndDefaultInt);
        [JsonInclude]
        public Color ConflictMarkerEndColor
        {
            get => _ConflictMarkerEndColor;
            set => _ConflictMarkerEndColor = value;
        }
        private LazilyUpdatedGradientStop _ConflictMarkerEndStop = new(new(default, 1));
        public LazilyUpdatedGradientStop ConflictMarkerEndStop
        {
            get
            {
                _ConflictMarkerEndStop.Color = _ConflictMarkerEndColor;
                return _ConflictMarkerEndStop;
            }
        }

        public readonly LazilyUpdatedGradientBrush ConflictMarkerBrush;

        public ColorizationStyles()
        {
            RangeVariableBrush = DoubleColorGradientBrush(
                RangeVariableStartStop, RangeVariableEndStop);
            PreprocessingBrush = DoubleColorGradientBrush(
                PreprocessingStartStop, PreprocessingEndStop);
            ConflictMarkerBrush = DoubleColorGradientBrush(
                ConflictMarkerStartStop, ConflictMarkerEndStop);
        }

        private static LazilyUpdatedGradientBrush DoubleColorGradientBrush(
            LazilyUpdatedGradientStop startStop,
            LazilyUpdatedGradientStop endStop)
        {
            var brush = new LinearGradientBrush()
            {
                StartPoint = new(new(0, 0), RelativeUnit.Relative),
                EndPoint = new(new(0.2, 1), RelativeUnit.Relative),
                SpreadMethod = GradientSpreadMethod.Reflect,
            };
            return new LazilyUpdatedGradientBrush(brush, [startStop, endStop]);
        }
    }
}
