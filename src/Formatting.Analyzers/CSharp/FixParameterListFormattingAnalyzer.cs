// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class FixParameterListFormattingAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.FixParameterListFormatting); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(AnalyzeParameterList, SyntaxKind.ParameterList);
            context.RegisterSyntaxNodeAction(AnalyzeBracketedParameterList, SyntaxKind.BracketedParameterList);
        }

        private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context)
        {
            var parameterList = (ParameterListSyntax)context.Node;

            Analyze(context, parameterList.OpenParenToken, parameterList.Parameters);
        }

        private static void AnalyzeBracketedParameterList(SyntaxNodeAnalysisContext context)
        {
            var parameterList = (BracketedParameterListSyntax)context.Node;

            Analyze(context, parameterList.OpenBracketToken, parameterList.Parameters);
        }

        private static void Analyze<TNode>(
            SyntaxNodeAnalysisContext context,
            SyntaxToken openToken,
            SeparatedSyntaxList<TNode> nodes) where TNode : SyntaxNode
        {
            if (!nodes.Any())
                return;

            TNode first = nodes.First();

            SyntaxTree syntaxTree = first.SyntaxTree;

            if (syntaxTree.IsSingleLineSpan(nodes.Span, context.CancellationToken))
            {
                SyntaxTriviaList trailing = openToken.TrailingTrivia;

                if (SyntaxTriviaAnalysis.IsOptionalWhitespaceThenOptionalSingleLineCommentThenEndOfLineTrivia(trailing))
                {
                    int indentationLength = GetIndentationLength();

                    if (indentationLength > 0)
                    {
                        SyntaxTriviaList leading = first.GetLeadingTrivia();

                        if (!leading.Any()
                            || !leading.Last().IsWhitespaceTrivia()
                            || leading.Last().Span.Length != indentationLength)
                        {
                            ReportDiagnostic();
                        }
                    }
                }
            }
            else
            {
                int indentationLength = GetIndentationLength();

                if (indentationLength > 0)
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        SyntaxTriviaList trailing = (i == 0)
                            ? openToken.TrailingTrivia
                            : nodes.GetSeparator(i - 1).TrailingTrivia;

                        if (!SyntaxTriviaAnalysis.IsOptionalWhitespaceThenOptionalSingleLineCommentThenEndOfLineTrivia(trailing))
                        {
                            ReportDiagnostic();
                            return;
                        }

                        SyntaxTriviaList leading = nodes[i].GetLeadingTrivia();

                        SyntaxTrivia last = (leading.Any() && leading.Last().IsWhitespaceTrivia())
                            ? leading.Last()
                            : default;

                        if (indentationLength != last.Span.Length)
                        {
                            ReportDiagnostic();
                            return;
                        }
                    }
                }
            }

            int GetIndentationLength()
            {
                IndentationAnalysis indentationAnalysis = SyntaxTriviaAnalysis.AnalyzeIndentation(openToken.Parent, context.CancellationToken);

                if (!indentationAnalysis.IsEmpty)
                {
                    return indentationAnalysis.Indentation.Span.Length + indentationAnalysis.IndentSize;
                }
                else
                {
                    return SyntaxTriviaAnalysis.DetermineIndentation(first, context.CancellationToken).Span.Length;
                }
            }

            void ReportDiagnostic()
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticDescriptors.FixParameterListFormatting,
                    Location.Create(syntaxTree, nodes.Span));
            }
        }
    }
}
