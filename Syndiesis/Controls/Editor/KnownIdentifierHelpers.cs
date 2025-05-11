using Microsoft.CodeAnalysis;
using System;

namespace Syndiesis.Controls.Editor;

public static class KnownIdentifierHelpers
{
    public static class CallingConventions
    {
        public const string ThisCall = "Thiscall";
        public const string FastCall = "Fastcall";
        public const string StdCall = "Stdcall";
        public const string CDecl = "Cdecl";

        public static bool IsKnownCallingConventionAttributeName(string name)
        {
            return name
                is ThisCall
                or FastCall
                or StdCall
                or CDecl
                ;
        }
    }
    
    // ReSharper disable UnusedMember.Global
    private interface ILanguageSpecificIdentifierProvider
    {
        public static abstract string? GetTypeAlias(TypeCode typeCode);
        public static abstract string? GetTypeAlias(SpecialType type);
    }
    // ReSharper restore UnusedMember.Global
    
    public abstract class CSharp : ILanguageSpecificIdentifierProvider
    {
        public static string? GetTypeAlias(TypeCode typeCode)
        {
            return typeCode switch
            {
                TypeCode.Boolean => "bool",
                TypeCode.Char => "char",
                TypeCode.String => "string",
                TypeCode.Object => "object",
                
                _ => throw new NotImplementedException("Not implemented yet due to lack of interest"),
            };
        }

        public static string? GetTypeAlias(SpecialType type)
        {
            return type switch
            {
                SpecialType.System_Void => "void",
                SpecialType.System_Boolean => "bool",
                SpecialType.System_Char => "char",
                SpecialType.System_String => "string",
                SpecialType.System_Object => "object",
                
                SpecialType.System_SByte => "sbyte",
                SpecialType.System_Int16 => "short",
                SpecialType.System_Int32 => "int",
                SpecialType.System_Int64 => "long",
                
                SpecialType.System_Byte => "byte",
                SpecialType.System_UInt16 => "ushort",
                SpecialType.System_UInt32 => "uint",
                SpecialType.System_UInt64 => "ulong",
                
                SpecialType.System_IntPtr => "nint",
                SpecialType.System_UIntPtr => "nuint",
                
                SpecialType.System_Single => "float",
                SpecialType.System_Double => "double",
                SpecialType.System_Decimal => "decimal",
                
                _ => null,
            };
        }
    }
    
    public abstract class VisualBasic : ILanguageSpecificIdentifierProvider
    {
        public static string? GetTypeAlias(TypeCode typeCode)
        {
            return typeCode switch
            {
                TypeCode.Boolean => "Boolean",
                TypeCode.Char => "Char",
                TypeCode.String => "String",
                TypeCode.Object => "Object",
                
                _ => throw new NotImplementedException("Not implemented yet due to lack of interest"),
            };
        }

        public static string? GetTypeAlias(SpecialType type)
        {
            return type switch
            {
                SpecialType.System_Boolean => "Boolean",
                SpecialType.System_Char => "Char",
                SpecialType.System_String => "String",
                SpecialType.System_Object => "Object",
                
                SpecialType.System_SByte => "SByte",
                SpecialType.System_Int16 => "Short",
                SpecialType.System_Int32 => "Integer",
                SpecialType.System_Int64 => "Long",
                
                SpecialType.System_Byte => "Byte",
                SpecialType.System_UInt16 => "UShort",
                SpecialType.System_UInt32 => "UInteger",
                SpecialType.System_UInt64 => "ULong",
                
                SpecialType.System_Single => "Single",
                SpecialType.System_Double => "Double",
                SpecialType.System_Decimal => "Decimal",
                SpecialType.System_DateTime => "Date",
                
                _ => null,
            };
        }
    }
}
