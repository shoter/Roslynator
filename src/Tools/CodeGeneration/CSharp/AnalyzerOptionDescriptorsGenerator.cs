// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.Options;
using Roslynator.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeGeneration.CSharp
{
    public static class AnalyzerOptionDescriptorsGenerator
    {
        public static CompilationUnitSyntax Generate(
            IEnumerable<AnalyzerOptionDescriptor> analyzerOptions,
            IComparer<string> comparer)
        {
            CompilationUnitSyntax compilationUnit = CompilationUnit(
                UsingDirectives(),
                NamespaceDeclaration("Roslynator.Options",
                    ClassDeclaration(
                        Modifiers.Internal_Static_Partial(),
                        "AnalyzerOptionDescriptors",
                        List(
                            CreateMembers(
                                analyzerOptions
                                    .OrderBy(f => f.Id, comparer))))));

            compilationUnit = compilationUnit.NormalizeWhitespace();

            return (CompilationUnitSyntax)Rewriter.Instance.Visit(compilationUnit);
        }

        private static IEnumerable<MemberDeclarationSyntax> CreateMembers(IEnumerable<AnalyzerOptionDescriptor> analyzerOptions)
        {
            foreach (AnalyzerOptionDescriptor analyzerOption in analyzerOptions)
            {
                var arguments = new List<ArgumentSyntax>()
                {
                    Argument(
                        NameColon("id"),
                        SimpleMemberAccessExpression(IdentifierName("AnalyzerOptionIdentifiers"), IdentifierName(analyzerOption.Id))),

                    Argument(
                        NameColon("title"),
                        StringLiteralExpression(analyzerOption.Title ?? "")),

                    Argument(
                        NameColon("isEnabledByDefault"),
                        BooleanLiteralExpression(true)),

                    Argument(
                        NameColon("summary"),
                        StringLiteralExpression(analyzerOption.Summary ?? ""))
                };

                yield return FieldDeclaration(
                    Modifiers.Public_Static_ReadOnly(),
                    IdentifierName("AnalyzerOptionDescriptor"),
                    analyzerOption.Id,
                    ObjectCreationExpression(
                        IdentifierName("AnalyzerOptionDescriptor"),
                        ArgumentList(arguments.ToSeparatedSyntaxList())));
            }
        }

        private class Rewriter : CSharpSyntaxRewriter
        {
            public static Rewriter Instance { get; } = new Rewriter();

            public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                node = (FieldDeclarationSyntax)base.VisitFieldDeclaration(node);

                return node.AppendToTrailingTrivia(NewLine());
            }

            public override SyntaxNode VisitArgument(ArgumentSyntax node)
            {
                ExpressionSyntax newExpression = node.Expression.PrependToLeadingTrivia(Whitespace(new string(' ', 18 - node.NameColon?.Name.Identifier.ValueText.Length ?? 0)));

                if (node.NameColon != null)
                {
                    node = node.WithNameColon(node.NameColon.AppendToLeadingTrivia(TriviaList(NewLine(), Whitespace("            "))));
                }
                else
                {
                    newExpression = newExpression.AppendToLeadingTrivia(TriviaList(NewLine(), Whitespace("            ")));
                }

                return node.WithExpression(newExpression);
            }

            public override SyntaxNode VisitAttribute(AttributeSyntax node)
            {
                return node;
            }
        }
    }
}
