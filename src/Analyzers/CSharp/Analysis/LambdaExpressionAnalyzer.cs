﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LambdaExpressionAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.SimplifyLambdaExpression,
                    DiagnosticDescriptors.SimplifyLambdaExpressionFadeOut);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (startContext.IsAnalyzerSuppressed(DiagnosticDescriptors.SimplifyLambdaExpression))
                    return;

                startContext.RegisterSyntaxNodeAction(f => AnalyzeLambdaExpression(f), SyntaxKind.SimpleLambdaExpression);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeLambdaExpression(f), SyntaxKind.ParenthesizedLambdaExpression);
            });
        }

        private static void AnalyzeLambdaExpression(SyntaxNodeAnalysisContext context)
        {
            var lambda = (LambdaExpressionSyntax)context.Node;

            if (lambda.ContainsDiagnostics)
                return;

            if (!SimplifyLambdaExpressionAnalysis.IsFixable(lambda))
                return;

            CSharpSyntaxNode body = lambda.Body;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.SimplifyLambdaExpression, body);

            var block = (BlockSyntax)body;

            CSharpDiagnosticHelpers.ReportBraces(context, DiagnosticDescriptors.SimplifyLambdaExpressionFadeOut, block);

            StatementSyntax statement = block.Statements[0];

            if (statement.Kind() == SyntaxKind.ReturnStatement)
                DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.SimplifyLambdaExpressionFadeOut, ((ReturnStatementSyntax)statement).ReturnKeyword);
        }
    }
}
