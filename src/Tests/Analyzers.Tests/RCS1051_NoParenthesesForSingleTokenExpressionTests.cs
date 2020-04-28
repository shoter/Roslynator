// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.CodeFixes;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1051_NoParenthesesForSingleTokenExpressionTests : AbstractCSharpFixVerifier
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.ParenthesizeConditionOfConditionalExpression_NoParenthesesForSingleTokenExpression;

        public override DiagnosticAnalyzer Analyzer { get; } = new ParenthesizeConditionOfConditionalExpressionAnalyzer();

        public override CodeFixProvider FixProvider { get; } = new ParenthesizedExpressionCodeFixProvider();

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ParenthesizeConditionOfConditionalExpression)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool b = false;
        string s = [|(b)|] ? ""true"" : ""false"";
    }
}
", @"
class C
{
    void M()
    {
        bool b = false;
        string s = b ? ""true"" : ""false"";
    }
}
");
        }
    }
}
