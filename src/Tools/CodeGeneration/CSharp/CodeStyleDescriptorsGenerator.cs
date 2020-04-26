// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeStyle;
using Roslynator.CSharp;
using Roslynator.Documentation;
using Roslynator.Metadata;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeGeneration.CSharp
{
    public static class CodeStyleDescriptorsGenerator
    {
        public static CompilationUnitSyntax Generate(
            IEnumerable<CodeStyleDescriptor> codeStyles,
            IComparer<string> comparer)
        {
            CompilationUnitSyntax compilationUnit = CompilationUnit(
                UsingDirectives(),
                NamespaceDeclaration("Roslynator.CodeStyle",
                    ClassDeclaration(
                        Modifiers.Internal_Static_Partial(),
                        "CodeStyleDescriptors",
                        List(
                            CreateMembers(
                                codeStyles
                                    .OrderBy(f => f.Id, comparer))))));

            compilationUnit = compilationUnit.NormalizeWhitespace();

            return (CompilationUnitSyntax)Rewriter.Instance.Visit(compilationUnit);
        }

        private static IEnumerable<MemberDeclarationSyntax> CreateMembers(IEnumerable<CodeStyleDescriptor> codeStyles)
        {
            foreach (CodeStyleDescriptor codeStyle in codeStyles)
            {
                var arguments = new List<ArgumentSyntax>()
                {
                    Argument(
                        NameColon("id"),
                        SimpleMemberAccessExpression(IdentifierName("CodeStyleIdentifiers"), IdentifierName(codeStyle.Id))),

                    Argument(
                        NameColon("isEnabledByDefault"),
                        BooleanLiteralExpression(true)),

                    Argument(
                        NameColon("summary"),
                        StringLiteralExpression(codeStyle.Summary ?? ""))
                };

                yield return FieldDeclaration(
                    Modifiers.Public_Static_ReadOnly(),
                    IdentifierName("CodeStyleDescriptor"),
                    codeStyle.Id,
                    ObjectCreationExpression(
                        IdentifierName("CodeStyleDescriptor"),
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
