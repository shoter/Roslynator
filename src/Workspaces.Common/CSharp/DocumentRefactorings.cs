﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Documentation;
using Roslynator.CSharp.Refactorings;
using Roslynator.Documentation;
using Roslynator.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp
{
    internal static class DocumentRefactorings
    {
        public static Task<Document> ChangeTypeAsync(
           Document document,
           TypeSyntax type,
           ITypeSymbol typeSymbol,
           CancellationToken cancellationToken = default)
        {
            if (type.IsVar
                && type.Parent is DeclarationExpressionSyntax declarationExpression
                && declarationExpression.Designation.IsKind(SyntaxKind.ParenthesizedVariableDesignation)
                && declarationExpression.Parent.IsKind(SyntaxKind.SimpleAssignmentExpression)
                && object.ReferenceEquals(declarationExpression, ((AssignmentExpressionSyntax)declarationExpression.Parent).Left))
            {
                TupleExpressionSyntax tupleExpression = CreateTupleExpression(typeSymbol)
                    .WithTriviaFrom(declarationExpression);

                return document.ReplaceNodeAsync(declarationExpression, tupleExpression, cancellationToken);
            }

            TypeSyntax newType = ChangeType(type, typeSymbol);

            return document.ReplaceNodeAsync(type, newType, cancellationToken);
        }

        private static TypeSyntax ChangeType(TypeSyntax type, ITypeSymbol typeSymbol)
        {
            TypeSyntax newType = typeSymbol
                .ToTypeSyntax()
                .WithTriviaFrom(type);

            if (newType is TupleTypeSyntax tupleType)
            {
                SeparatedSyntaxList<TupleElementSyntax> newElements = tupleType
                    .Elements
                    .Select(tupleElement => tupleElement.WithType(tupleElement.Type.WithSimplifierAnnotation()))
                    .ToSeparatedSyntaxList();

                return tupleType.WithElements(newElements);
            }
            else
            {
                return newType.WithSimplifierAnnotation();
            }
        }

        private static TupleExpressionSyntax CreateTupleExpression(ITypeSymbol typeSymbol)
        {
            if (!typeSymbol.SupportsExplicitDeclaration())
                throw new ArgumentException($"Type '{typeSymbol.ToDisplayString()}' does not support explicit declaration.", nameof(typeSymbol));

            var tupleExpression = (TupleExpressionSyntax)ParseExpression(typeSymbol.ToDisplayString(SymbolDisplayFormats.FullName));

            SeparatedSyntaxList<ArgumentSyntax> newArguments = tupleExpression
                .Arguments
                .Select(f =>
                {
                    if (f.Expression is DeclarationExpressionSyntax declarationExpression)
                        return f.WithExpression(declarationExpression.WithType(declarationExpression.Type.WithSimplifierAnnotation()));

                    Debug.Fail(f.Expression.Kind().ToString());

                    return f;
                })
                .ToSeparatedSyntaxList();

            return   tupleExpression.WithArguments(newArguments);
        }

        public static Task<Document> ChangeTypeToVarAsync(
            Document document,
            TypeSyntax type,
            CancellationToken cancellationToken = default)
        {
            IdentifierNameSyntax newType = VarType().WithTriviaFrom(type);

            return document.ReplaceNodeAsync(type, newType, cancellationToken);
        }

        public static Task<Document> ChangeTypeToVarAsync(
            Document document,
            TupleExpressionSyntax tupleExpression,
            CancellationToken cancellationToken = default)
        {
            var builder = new SyntaxNodeTextBuilder(tupleExpression);

            builder.AppendFullSpan(tupleExpression.OpenParenToken);

            SeparatedSyntaxList<ArgumentSyntax> arguments = tupleExpression.Arguments;

            for (int i = 0; i < arguments.Count; i++)
            {
                var declarationExpression = (DeclarationExpressionSyntax)arguments[i].Expression;

                builder.AppendFullSpan(declarationExpression.Designation);

                if (i < arguments.Count - 1)
                    builder.AppendFullSpan(arguments.GetSeparator(i));
            }

            builder.AppendFullSpan(tupleExpression.CloseParenToken);

            ExpressionSyntax expression = ParseExpression(builder.ToString());

            return document.ReplaceNodeAsync(tupleExpression, expression, cancellationToken);
        }

        public static Task<Document> ChangeTypeAndAddAwaitAsync(
            Document document,
            VariableDeclarationSyntax variableDeclaration,
            VariableDeclaratorSyntax variableDeclarator,
            SyntaxNode containingDeclaration,
            ITypeSymbol newTypeSymbol,
            CancellationToken cancellationToken)
        {
            TypeSyntax type = variableDeclaration.Type;

            ExpressionSyntax value = variableDeclarator.Initializer.Value;

            AwaitExpressionSyntax newValue = AwaitExpression(value.WithoutTrivia()).WithTriviaFrom(value);

            TypeSyntax newType = ChangeType(type, newTypeSymbol);

            VariableDeclarationSyntax newVariableDeclaration = variableDeclaration
                .ReplaceNode(value, newValue)
                .WithType(newType);

            if (!SyntaxInfo.ModifierListInfo(containingDeclaration).IsAsync)
            {
                SyntaxNode newDeclaration = containingDeclaration
                    .ReplaceNode(variableDeclaration, newVariableDeclaration)
                    .InsertModifier(SyntaxKind.AsyncKeyword);

                return document.ReplaceNodeAsync(containingDeclaration, newDeclaration, cancellationToken);
            }

            return document.ReplaceNodeAsync(variableDeclaration, newVariableDeclaration, cancellationToken);
        }

        public static Task<Document> AddCastExpressionAsync(
            Document document,
            ExpressionSyntax expression,
            ITypeSymbol destinationType,
            CancellationToken cancellationToken = default)
        {
            TypeSyntax type = destinationType.ToTypeSyntax().WithSimplifierAnnotation();

            return AddCastExpressionAsync(document, expression, type, cancellationToken);
        }

        public static Task<Document> AddCastExpressionAsync(
            Document document,
            ExpressionSyntax expression,
            TypeSyntax destinationType,
            CancellationToken cancellationToken = default)
        {
            ExpressionSyntax newExpression = expression
                .WithoutTrivia()
                .Parenthesize();

            ExpressionSyntax newNode = CastExpression(destinationType, newExpression)
                .WithTriviaFrom(expression)
                .Parenthesize();

            return document.ReplaceNodeAsync(expression, newNode, cancellationToken);
        }

        public static Task<Document> RemoveAsyncAwaitAsync(
            Document document,
            SyntaxToken asyncKeyword,
            CancellationToken cancellationToken = default)
        {
            return RemoveAsyncAwait.RefactorAsync(document, asyncKeyword, cancellationToken);
        }

        public static async Task<Document> AddNewDocumentationCommentsAsync(
            Document document,
            DocumentationCommentGeneratorSettings settings = null,
            bool skipNamespaceDeclaration = true,
            CancellationToken cancellationToken = default)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var rewriter = new AddNewDocumentationCommentRewriter(settings, skipNamespaceDeclaration);

            SyntaxNode newRoot = rewriter.Visit(root);

            return document.WithSyntaxRoot(newRoot);
        }

        public static async Task<Document> AddBaseOrNewDocumentationCommentsAsync(
            Document document,
            SemanticModel semanticModel,
            DocumentationCommentGeneratorSettings settings = null,
            bool skipNamespaceDeclaration = true,
            CancellationToken cancellationToken = default)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var rewriter = new AddBaseOrNewDocumentationCommentRewriter(semanticModel, settings, skipNamespaceDeclaration, cancellationToken);

            SyntaxNode newRoot = rewriter.Visit(root);

            return document.WithSyntaxRoot(newRoot);
        }

        public static Task<Document> RemoveParenthesesAsync(
            Document document,
            ParenthesizedExpressionSyntax parenthesizedExpression,
            CancellationToken cancellationToken = default)
        {
            ExpressionSyntax expression = parenthesizedExpression.Expression;

            SyntaxTriviaList leading = parenthesizedExpression.GetLeadingTrivia()
                .Concat(parenthesizedExpression.OpenParenToken.TrailingTrivia)
                .Concat(expression.GetLeadingTrivia())
                .ToSyntaxTriviaList();

            SyntaxTriviaList trailing = expression.GetTrailingTrivia()
                .Concat(parenthesizedExpression.CloseParenToken.LeadingTrivia)
                .Concat(parenthesizedExpression.GetTrailingTrivia())
                .ToSyntaxTriviaList();

            ExpressionSyntax newExpression = expression
                .WithLeadingTrivia(leading)
                .WithTrailingTrivia(trailing)
                .WithFormatterAnnotation();

            if (!leading.Any())
            {
                SyntaxNode parent = parenthesizedExpression.Parent;

                switch (parent.Kind())
                {
                    case SyntaxKind.ReturnStatement:
                        {
                            var returnStatement = (ReturnStatementSyntax)parent;

                            SyntaxToken returnKeyword = returnStatement.ReturnKeyword;

                            if (!returnKeyword.TrailingTrivia.Any())
                            {
                                ReturnStatementSyntax newNode = returnStatement.Update(returnKeyword.WithTrailingTrivia(Space), newExpression, returnStatement.SemicolonToken);

                                return document.ReplaceNodeAsync(returnStatement, newNode, cancellationToken);
                            }

                            break;
                        }
                    case SyntaxKind.YieldReturnStatement:
                        {
                            var yieldReturn = (YieldStatementSyntax)parent;

                            SyntaxToken returnKeyword = yieldReturn.ReturnOrBreakKeyword;

                            if (!returnKeyword.TrailingTrivia.Any())
                            {
                                YieldStatementSyntax newNode = yieldReturn.Update(yieldReturn.YieldKeyword, returnKeyword.WithTrailingTrivia(Space), newExpression, yieldReturn.SemicolonToken);

                                return document.ReplaceNodeAsync(yieldReturn, newNode, cancellationToken);
                            }

                            break;
                        }
                    case SyntaxKind.AwaitExpression:
                        {
                            var awaitExpression = (AwaitExpressionSyntax)parent;

                            SyntaxToken awaitKeyword = awaitExpression.AwaitKeyword;

                            if (!awaitKeyword.TrailingTrivia.Any())
                            {
                                AwaitExpressionSyntax newNode = awaitExpression.Update(awaitKeyword.WithTrailingTrivia(Space), newExpression);

                                return document.ReplaceNodeAsync(awaitExpression, newNode, cancellationToken);
                            }

                            break;
                        }
                }
            }

            return document.ReplaceNodeAsync(parenthesizedExpression, newExpression, cancellationToken);
        }
    }
}
