// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis;
using Roslynator.Configuration;

namespace Roslynator
{
    internal class AnalyzerRules
    {
        public static AnalyzerRules Current { get; } = Create();

        public static bool UseElementAccessOnInvocation { get; } = true;

        public static bool UseElementAccessOnElementAccess { get; } = true;

        public RuleSet RuleSet { get; }

        public ImmutableDictionary<string, bool> DefaultRules { get; }

        public AnalyzerRules(RuleSet ruleSet, ImmutableDictionary<string, bool> defaultRules = null)
        {
            RuleSet = ruleSet;
            DefaultRules = defaultRules ?? ImmutableDictionary<string, bool>.Empty;
        }

        public bool IsRuleEnabled(SemanticModel semanticModel, string ruleId, bool defaultValue)
        {
            return IsRuleEnabled(semanticModel.Compilation, ruleId, defaultValue);
        }

        public bool IsRuleEnabled(Compilation compilation, string ruleId, bool defaultValue)
        {
            if (!compilation
                .Options
                .SpecificDiagnosticOptions.TryGetValue(ruleId, out ReportDiagnostic reportDiagnostic))
            {
                reportDiagnostic = RuleSet.SpecificDiagnosticOptions.GetValueOrDefault(ruleId);
            }

            switch (reportDiagnostic)
            {
                case ReportDiagnostic.Default:
                    return DefaultRules.GetValueOrDefault(ruleId);
                case ReportDiagnostic.Suppress:
                    return false;
                case ReportDiagnostic.Info:
                    return true;
            }

            Debug.Fail($"Invalid value '{reportDiagnostic}'.");
            return false;
        }

        private static AnalyzerRules Create()
        {
            var defaultRules = ImmutableDictionary.CreateBuilder<string, bool>();

            defaultRules.Add(AnalyzerRuleIdentifiers.UseElementAccessOnElementAccess, true);
            defaultRules.Add(AnalyzerRuleIdentifiers.UseElementAccessOnInvocation, true);

            return CreateFromAssemblyLocation(typeof(AnalyzerRules).Assembly.Location, defaultRules.ToImmutableDictionary());
        }

        internal static AnalyzerRules CreateFromAssemblyLocation(string assemblyLocation, ImmutableDictionary<string, bool> defaultValues = null)
        {
            string path = null;

            if (!string.IsNullOrEmpty(assemblyLocation))
                path = Path.Combine(Path.GetDirectoryName(assemblyLocation), RuleSetUtility.DefaultRuleSetName);

            RuleSet ruleSet = RuleSetUtility.Load(path, CodeAnalysisConfiguration.Current.RuleSets) ?? RuleSetUtility.EmptyRuleSet;

            return new AnalyzerRules(ruleSet);
        }
    }
}
