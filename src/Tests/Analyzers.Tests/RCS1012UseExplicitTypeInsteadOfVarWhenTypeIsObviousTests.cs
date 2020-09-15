// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.CodeFixes;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1012UseExplicitTypeInsteadOfVarWhenTypeIsObviousTests : AbstractCSharpFixVerifier
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseExplicitTypeInsteadOfVarWhenTypeIsObvious;

        public override DiagnosticAnalyzer Analyzer { get; } = new UseExplicitTypeInsteadOfVarWhenTypeIsObviousAnalyzer();

        public override CodeFixProvider FixProvider { get; } = new UseExplicitTypeInsteadOfVarCodeFixProvider();

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsObvious)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        [|var|] x = new List<string>();
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        List<string> x = new List<string>();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsObvious)]
        public async Task Test_Tuple()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        [|var|] (x, y) = default((object, System.DateTime));
    }
}
", @"
class C
{
    void M()
    {
        (object x, System.DateTime y) = default((object, System.DateTime));
    }
}
");
        }
    }
}
