// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.Options;
using Roslynator.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeGeneration.CSharp
{
    public static class AnalyzerOptionIdentifiersGenerator
    {
        public static CompilationUnitSyntax Generate(IEnumerable<AnalyzerOptionDescriptor> analyzerOptions, IComparer<string> comparer)
        {
            return CompilationUnit(
                UsingDirectives(),
                NamespaceDeclaration(
                    "Roslynator.Options",
                    ClassDeclaration(
                        Modifiers.Internal_Static_Partial(),
                        "AnalyzerOptionIdentifiers",
                        analyzerOptions
                            .OrderBy(f => f.Id, comparer)
                            .Select(f =>
                            {
                                return FieldDeclaration(
                                   Modifiers.Public_Const(),
                                   PredefinedStringType(),
                                   f.Id,
                                   StringLiteralExpression(AnalyzerOptionIdentifiers.Prefix + f.Id));
                            })
                            .ToSyntaxList<MemberDeclarationSyntax>())));
        }
    }
}
