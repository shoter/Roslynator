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
    public class UseVarInForEachAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.UseVarInsteadOfExplicitTypeInForEach); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeForEachStatement(f), SyntaxKind.ForEachStatement);
        }

        private static void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (ForEachStatementSyntax)context.Node;

            TypeAnalysis analysis = CSharpTypeAnalysis.AnalyzeType(forEachStatement, context.SemanticModel);

            if (analysis.IsExplicit
                && analysis.SupportsImplicit)
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.UseVarInsteadOfExplicitTypeInForEach, forEachStatement.Type);
            }
        }
    }
}
