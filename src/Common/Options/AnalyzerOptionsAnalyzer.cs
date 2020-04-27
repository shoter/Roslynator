// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Options
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AnalyzerOptionsAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    AnalyzerOptionDescriptors.ParenthesizeSimpleConditionOfConditionalExpression,
                    AnalyzerOptionDescriptors.UseElementAccessOnElementAccess,
                    AnalyzerOptionDescriptors.UseElementAccessOnInvocation);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
        }
    }
}
