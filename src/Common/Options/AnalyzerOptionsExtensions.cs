// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Options
{
    internal static class AnalyzerOptionsExtensions
    {
        public static bool IsOptionEnabled(this SyntaxNodeAnalysisContext context, string id)
        {
            return AnalyzerRules.Default.IsOptionEnabled(context.SemanticModel.Compilation, id);
        }
    }
}
