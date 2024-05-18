using Syndiesis.Controls.AnalysisVisualization;
using System;
using System.Collections.Generic;

namespace Syndiesis.Core.DisplayAnalysis;

public static class TypeExtensions
{
    public static bool IsOrImplements(this Type interfaceType, Type target)
    {
        if (interfaceType == target)
            return true;

        if (interfaceType.GetInterface(target.Name) is not null)
            return true;

        return false;
    }
}
