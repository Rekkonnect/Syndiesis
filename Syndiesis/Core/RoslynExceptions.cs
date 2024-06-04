using System;

namespace Syndiesis.Core;

public static class RoslynExceptions
{
    public static Exception ThrowInvalidLanguageArgument(string? languageName, string parameterName)
    {
        throw new ArgumentException(
            $"Invalid language name {languageName}",
            nameof(parameterName));
    }
}
