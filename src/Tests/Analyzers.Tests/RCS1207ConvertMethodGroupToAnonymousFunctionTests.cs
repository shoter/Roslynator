// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.CodeFixes;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1207ConvertMethodGroupToAnonymousFunctionTests : AbstractCSharpFixVerifier
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.ConvertAnonymousFunctionToMethodGroupOrViceVersa;

        public override DiagnosticAnalyzer Analyzer { get; } = new ConvertAnonymousFunctionToMethodGroupOrViceVersaAnalyzer();

        public override CodeFixProvider FixProvider { get; } = new ConvertAnonymousFunctionToMethodGroupOrViceVersaCodeFixProvider();

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Argument()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        M2([|M|]);
    }
}
", @"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        M2(() => M());
    }
}
", options: Options.WithEnabled(AnalyzerOptions.ConvertMethodGroupToAnonymousFunction));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Argument_OneParameter()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M(object p) => null;

    void M2(Func<object, object> p)
    {
        M2([|M|]);
    }
}
", @"
using System;

class C
{
    static object M(object p) => null;

    void M2(Func<object, object> p)
    {
        M2(f => M(f));
    }
}
", options: Options.WithEnabled(AnalyzerOptions.ConvertMethodGroupToAnonymousFunction));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Argument_TwoParameters()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M(object p1, object p2) => null;

    void M2(Func<object, object, object> p)
    {
        M2([|M|]);
    }
}
", @"
using System;

class C
{
    static object M(object p1, object p2) => null;

    void M2(Func<object, object, object> p)
    {
        M2((f, f2) => M(f, f2));
    }
}
", options: Options.WithEnabled(AnalyzerOptions.ConvertMethodGroupToAnonymousFunction));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Argument_WithClassName()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        M2([|C.M|]);
    }
}
", @"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        M2(() => C.M());

    }
}
", options: Options.WithEnabled(AnalyzerOptions.ConvertMethodGroupToAnonymousFunction));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Argument_WithNamespaceName()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        M2([|N.C.M|]);
    }
}
", @"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        M2(() => N.C.M());
    }
}
", options: Options.WithEnabled(AnalyzerOptions.ConvertMethodGroupToAnonymousFunction));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Argument_WithGlobalName()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        M2([|global::N.C.M|]);
    }
}
", @"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        M2(() => global::N.C.M());
    }
}
", options: Options.WithEnabled(AnalyzerOptions.ConvertMethodGroupToAnonymousFunction));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_LocalDeclaration()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        Func<object> x = [|M|];
    }
}
", @"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        Func<object> x = () => M();
    }
}
", options: Options.WithEnabled(AnalyzerOptions.ConvertMethodGroupToAnonymousFunction));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Assignment()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        Func<object> l = null;

        l = [|M|];
        l += [|M|];
        l -= [|M|];
        l ??= [|M|];
    }
}
", @"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        l = () => M();
        l += () => M();
        l -= () => M();
        l ??= () => M();
    }
}
", options: Options.WithEnabled(AnalyzerOptions.ConvertMethodGroupToAnonymousFunction));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_ObjectInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    Func<object> P { get; set; };

    static object M() => null;

    void M2(Func<object> p)
    {
        var c = new C() { P = [|M|] };
    }
}
", @"
using System;

class C
{
    Func<object> P { get; set; };

    static object M() => null;

    void M2(Func<object> p)
    {
        var c = new C() { P = () => M() };
    }
}
", options: Options.WithEnabled(AnalyzerOptions.ConvertMethodGroupToAnonymousFunction));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_PropertyInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    public Func<object> P { get; set; } = [|M|];

    static object M() => null;
}
", @"
using System;

class C
{
    public Func<object> P { get; set; } = () => M();

    static object M() => null;
}
", options: Options.WithEnabled(AnalyzerOptions.ConvertMethodGroupToAnonymousFunction));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_FieldInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    Func<object> F = [|M|];

    static object M() => null;
}
", @"
using System;

class C
{
    Func<object> F = () => M();

    static object M() => null;
}
", options: Options.WithEnabled(AnalyzerOptions.ConvertMethodGroupToAnonymousFunction));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task TestNoDiagnostic_NullReferenceException()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M2()
    {
    }
}
", options: Options.WithEnabled(AnalyzerOptions.ConvertMethodGroupToAnonymousFunction));
        }
    }
}
