// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.Formatting.CodeFixes.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0052FixParameterListAlignmentTests : AbstractCSharpFixVerifier
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.FixParameterListAlignment;

        public override DiagnosticAnalyzer Analyzer { get; } = new FixParameterListAlignmentAnalyzer();

        public override CodeFixProvider FixProvider { get; } = new BaseParameterListCodeFixProvider();

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixParameterListAlignment)]
        public async Task Test_Singleline_AlignedToOpenParenthesis()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(
           [|object p1, object p2|]) 
    {
    }
}
", @"
class C
{
    void M(
        object p1, object p2) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixParameterListAlignment)]
        public async Task Test_Singleline_WithoutIndentation()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(
[|object p1, object p2|]) 
    {
    }
}
", @"
class C
{
    void M(
        object p1, object p2) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixParameterListAlignment)]
        public async Task Test_Singleline_EmptyLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(

[|object p1, object p2|]) 
    {
    }
}
", @"
class C
{
    void M(
        object p1, object p2) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixParameterListAlignment)]
        public async Task Test_Singleline_EmptyLine_Comment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(
// x
[|object p1, object p2|]) 
    {
    }
}
", @"
class C
{
    void M(
// x
        object p1, object p2) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixParameterListAlignment)]
        public async Task Test_Multiline()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
    }

    [Obsolete]
    void M2(
           [|object p1,
           object p2|]) 
    {
    }
}
", @"
using System;

class C
{
    void M()
    {
    }

    [Obsolete]
    void M2(
        object p1,
        object p2) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixParameterListAlignment)]
        public async Task Test_Multiline2()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    class C
    {
        void M([|object p1,
            object p2|]) 
        {
        }
    }
}
", @"
namespace N
{
    class C
    {
        void M(
            object p1,
            object p2) 
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixParameterListAlignment)]
        public async Task Test_Multiline3()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M( [|object p1,
           object p2|]) 
    {
    }
}
", @"
class C
{
    void M(
        object p1,
        object p2) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixParameterListAlignment)]
        public async Task Test_Multiline4()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(
        [|object p1,
        object p2,object p3|]) 
    {
    }
}
", @"
class C
{
    void M(
        object p1,
        object p2,
        object p3) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixParameterListAlignment)]
        public async Task TestNoDiagnostic_Singleline()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(object p1, object p2, object p3) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixParameterListAlignment)]
        public async Task TestNoDiagnostic_Singleline2()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(
        object p1, object p2, object p3) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixParameterListAlignment)]
        public async Task TestNoDiagnostic_Multiline()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(
        object p1,
        object p2,
        object p3) 
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixParameterListAlignment)]
        public async Task TestNoDiagnostic_Multiline2()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
void M(
    object p1,
    object p2,
    object p3) 
{
}
}
");
        }
    }
}
