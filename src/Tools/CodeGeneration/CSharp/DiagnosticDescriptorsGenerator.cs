// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using Roslynator.Documentation;
using Roslynator.Metadata;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeGeneration.CSharp
{
    public static class DiagnosticDescriptorsGenerator
    {
        public static CompilationUnitSyntax Generate(
            IEnumerable<AnalyzerMetadata> analyzers,
            bool obsolete,
            IComparer<string> comparer,
            string @namespace,
            string className,
            string identifiersClassName)
        {
            CompilationUnitSyntax compilationUnit = CompilationUnit(
                UsingDirectives("System", "Microsoft.CodeAnalysis"),
                NamespaceDeclaration(@namespace,
                    ClassDeclaration(
                        Modifiers.Public_Static_Partial(),
                        className,
                        List(
                            CreateMembers(
                                analyzers
                                    .Where(f => f.IsObsolete == obsolete)
                                    .OrderBy(f => f.Id, comparer),
                                identifiersClassName)))));

            compilationUnit = compilationUnit.NormalizeWhitespace();

            return (CompilationUnitSyntax)Rewriter.Instance.Visit(compilationUnit);
        }

        private static IEnumerable<MemberDeclarationSyntax> CreateMembers(IEnumerable<AnalyzerMetadata> analyzers, string identifiersClassName)
        {
            foreach (AnalyzerMetadata analyzer in analyzers)
            {
                string identifier = analyzer.Identifier;
                string title = analyzer.Title;
                string messageFormat = analyzer.MessageFormat;
                bool isEnabledByDefault = analyzer.IsEnabledByDefault;

                yield return CreateMember(
                    analyzer.Id,
                    analyzer.Identifier,
                    analyzer.Title,
                    analyzer.MessageFormat,
                    analyzer.IsEnabledByDefault,
                    analyzer.Category,
                    analyzer.DefaultSeverity,
                    (analyzer.SupportsFadeOut) ? WellKnownDiagnosticTags.Unnecessary : null,
                    identifiersClassName,
                    analyzer.IsObsolete);

                //TODO: del
                //FieldDeclarationSyntax fieldDeclaration = FieldDeclaration(
                //    (analyzer.IsObsolete) ? Modifiers.Internal_Static_ReadOnly() : Modifiers.Public_Static_ReadOnly(),
                //    IdentifierName("DiagnosticDescriptor"),
                //    identifier,
                //    SimpleMemberInvocationExpression(
                //        IdentifierName("Factory"),
                //        IdentifierName("Create"),
                //        ArgumentList(
                //            Argument(
                //                NameColon("id"),
                //                SimpleMemberAccessExpression(IdentifierName(identifiersClassName), IdentifierName(identifier))),
                //            Argument(
                //                NameColon("title"),
                //                StringLiteralExpression(title)),
                //            Argument(
                //                NameColon("messageFormat"),
                //                StringLiteralExpression(messageFormat)),
                //            Argument(
                //                NameColon("category"),
                //                SimpleMemberAccessExpression(IdentifierName("DiagnosticCategories"), IdentifierName(analyzer.Category))),
                //            Argument(
                //                NameColon("defaultSeverity"),
                //                SimpleMemberAccessExpression(IdentifierName("DiagnosticSeverity"), IdentifierName(analyzer.DefaultSeverity))),
                //            Argument(
                //                NameColon("isEnabledByDefault"),
                //                BooleanLiteralExpression(isEnabledByDefault)),
                //            Argument(
                //                NameColon("description"),
                //                NullLiteralExpression()),
                //            Argument(
                //                NameColon("helpLinkUri"),
                //                SimpleMemberAccessExpression(IdentifierName(identifiersClassName), IdentifierName(identifier))),
                //            Argument(
                //                NameColon("customTags"),
                //                (analyzer.SupportsFadeOut)
                //                    ? SimpleMemberAccessExpression(IdentifierName("WellKnownDiagnosticTags"), IdentifierName("Unnecessary"))
                //                    : ParseExpression("Array.Empty<string>()"))
                //            ))).AddObsoleteAttributeIf(analyzer.IsObsolete, error: true);

                //if (!analyzer.IsObsolete)
                //{
                //    var settings = new DocumentationCommentGeneratorSettings(
                //        summary: new string[] { analyzer.Id },
                //        indentation: "        ",
                //        singleLineSummary: true);

                //    fieldDeclaration = fieldDeclaration.WithNewSingleLineDocumentationComment(settings);
                //}

                if (analyzer.SupportsFadeOutAnalyzer)
                {
                    yield return FieldDeclaration(
                        Modifiers.Public_Static_ReadOnly(),
                        IdentifierName("DiagnosticDescriptor"),
                        identifier + "FadeOut",
                        SimpleMemberInvocationExpression(
                            IdentifierName("DiagnosticDescriptorFactory"),
                            IdentifierName("CreateFadeOut"),
                            ArgumentList(Argument(IdentifierName(identifier))))).AddObsoleteAttributeIf(analyzer.IsObsolete, error: true);
                }

                foreach (AnalyzerOptionMetadata option in analyzer.Options)
                {
                    yield return CreateMember(
                        analyzer.Id + "." + option.Identifier,
                        analyzer.Identifier + "." + option.Identifier,
                        option.Title,
                        option.MessageFormat,
                        false,
                        nameof(DiagnosticCategories.AnalyzerOption),
                        analyzer.DefaultSeverity,
                        null,
                        identifiersClassName,
                        analyzer.IsObsolete || option.IsObsolete);
                }
            }
        }

        private static MemberDeclarationSyntax CreateMember(
            string id,
            string identifier,
            string title,
            string messageFormat,
            bool isEnabledByDefault,
            string category,
            string defaultSeverity,
            string tag,
            string identifiersClassName,
            bool isObsolete)
        {
            FieldDeclarationSyntax fieldDeclaration = FieldDeclaration(
                (isObsolete) ? Modifiers.Internal_Static_ReadOnly() : Modifiers.Public_Static_ReadOnly(),
                IdentifierName("DiagnosticDescriptor"),
                identifier,
                SimpleMemberInvocationExpression(
                    IdentifierName("Factory"),
                    IdentifierName("Create"),
                    ArgumentList(
                        Argument(
                            NameColon("id"),
                            SimpleMemberAccessExpression(IdentifierName(identifiersClassName), IdentifierName(identifier))),
                        Argument(
                            NameColon("title"),
                            StringLiteralExpression(title)),
                        Argument(
                            NameColon("messageFormat"),
                            StringLiteralExpression(messageFormat)),
                        Argument(
                            NameColon("category"),
                            SimpleMemberAccessExpression(IdentifierName("DiagnosticCategories"), IdentifierName(category))),
                        Argument(
                            NameColon("defaultSeverity"),
                            SimpleMemberAccessExpression(IdentifierName("DiagnosticSeverity"), IdentifierName(defaultSeverity))),
                        Argument(
                            NameColon("isEnabledByDefault"),
                            BooleanLiteralExpression(isEnabledByDefault)),
                        Argument(
                            NameColon("description"),
                            NullLiteralExpression()),
                        Argument(
                            NameColon("helpLinkUri"),
                            SimpleMemberAccessExpression(IdentifierName(identifiersClassName), IdentifierName(identifier))),
                        Argument(
                            NameColon("customTags"),
                            (tag != null)
                                ? SimpleMemberAccessExpression(IdentifierName("WellKnownDiagnosticTags"), IdentifierName(tag))
                                : ParseExpression("Array.Empty<string>()"))
                        ))).AddObsoleteAttributeIf(isObsolete, error: true);

            if (!isObsolete)
            {
                var settings = new DocumentationCommentGeneratorSettings(
                    summary: new string[] { id },
                    indentation: "        ",
                    singleLineSummary: true);

                fieldDeclaration = fieldDeclaration.WithNewSingleLineDocumentationComment(settings);
            }

            return fieldDeclaration;
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
                if (node.NameColon != null)
                {
                    return node
                        .WithNameColon(node.NameColon.AppendToLeadingTrivia(TriviaList(NewLine(), Whitespace("            "))))
                        .WithExpression(node.Expression.PrependToLeadingTrivia(Whitespace(new string(' ', 18 - node.NameColon.Name.Identifier.ValueText.Length))));
                }

                return node;
            }

            public override SyntaxNode VisitAttribute(AttributeSyntax node)
            {
                return node;
            }
        }
    }
}
