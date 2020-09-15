// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.CodeFixes;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1176UseVarInsteadOfExplicitTypeWhenTypeIsNotObviousTests : AbstractCSharpFixVerifier
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseVarInsteadOfExplicitTypeWhenTypeIsNotObvious;

        public override DiagnosticAnalyzer Analyzer { get; } = new UseVarInsteadOfExplicitTypeWhenTypeIsNotObviousAnalyzer();

        public override CodeFixProvider FixProvider { get; } = new UseVarInsteadOfExplicitTypeCodeFixProvider();

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseVarInsteadOfExplicitTypeWhenTypeIsNotObvious)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object M()
    {
        [|var|] x = M();
    }
}
", @"
class C
{
    object M()
    {
        object x = M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseVarInsteadOfExplicitTypeWhenTypeIsNotObvious)]
        public async Task Test_Tuple()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    (object x, object y) M()
    {
        [|(object x, object y)|] = M();

        return default;
    }
}
", @"
class C
{
    (object x, object y) M()
    {
        var (x, y) = M();

        return default;
    }
}
");
        }
    }
}
