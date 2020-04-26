// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CodeStyle;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ParenthesizeConditionInConditionalExpressionAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.ParenthesizeConditionInConditionalExpression); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(AnalyzeConditionalExpression, SyntaxKind.ConditionalExpression);
        }

        private static void AnalyzeConditionalExpression(SyntaxNodeAnalysisContext context)
        {
            var conditionalExpression = (ConditionalExpressionSyntax)context.Node;

            if (conditionalExpression.ContainsDiagnostics)
                return;

            ExpressionSyntax condition = conditionalExpression.Condition;

            if (condition == null)
                return;

            SyntaxKind kind = condition.Kind();

            if (kind == SyntaxKind.ParenthesizedExpression)
                return;

            if (CSharpFacts.IsSingleTokenExpression(kind)
                && !context.IsCodeStyleEnabled(CodeStyleIdentifiers.ParenthesizeSimpleConditionOfConditionalExpression))
            {
                return;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.ParenthesizeConditionInConditionalExpression, condition);
        }
    }
}
