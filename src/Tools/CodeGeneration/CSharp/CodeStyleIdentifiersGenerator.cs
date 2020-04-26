// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeStyle;
using Roslynator.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeGeneration.CSharp
{
    public static class CodeStyleIdentifiersGenerator
    {
        public static CompilationUnitSyntax Generate(IEnumerable<CodeStyleDescriptor> codeStyles, IComparer<string> comparer)
        {
            return CompilationUnit(
                UsingDirectives(),
                NamespaceDeclaration(
                    "Roslynator.CodeStyle",
                    ClassDeclaration(
                        Modifiers.Internal_Static_Partial(),
                        "CodeStyleIdentifiers",
                        codeStyles
                            .OrderBy(f => f.Id, comparer)
                            .Select(f =>
                            {
                                return FieldDeclaration(
                                   Modifiers.Public_Const(),
                                   PredefinedStringType(),
                                   f.Id,
                                   StringLiteralExpression(CodeStyleIdentifiers.Prefix + f.Id));
                            })
                            .ToSyntaxList<MemberDeclarationSyntax>())));
        }
    }
}
