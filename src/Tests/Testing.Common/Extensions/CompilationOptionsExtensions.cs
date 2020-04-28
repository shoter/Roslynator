// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal static class CompilationOptionsExtensions
    {
        public static CompilationOptions EnsureEnabled(this CompilationOptions compilationOptions, DiagnosticDescriptor descriptor)
        {
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = compilationOptions.SpecificDiagnosticOptions;

            specificDiagnosticOptions = specificDiagnosticOptions.SetItem(
                descriptor.Id,
                descriptor.DefaultSeverity.ToReportDiagnostic());

            return compilationOptions.WithSpecificDiagnosticOptions(specificDiagnosticOptions);
        }
    }
}
