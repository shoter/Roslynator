// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CodeStyle
{
    internal static class CodeStyleExtensions
    {
        internal static bool IsCodeStyleEnabled(this SyntaxNodeAnalysisContext context, string id)
        {
            return AnalyzerRules.Default.IsCodeStyleEnabled(context.SemanticModel, id);
        }
    }
}
