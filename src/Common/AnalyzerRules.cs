// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
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

        public ReportDiagnostic GeneralDiagnosticOption { get; }

        public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; }

        public ImmutableDictionary<string, bool> CodeStyleRules { get; }

        public AnalyzerRules(
            ReportDiagnostic generalDiagnosticOption,
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions,
            IEnumerable<KeyValuePair<string, bool>> defaultCodeStyleRules = null)
        {
            GeneralDiagnosticOption = generalDiagnosticOption;
            SpecificDiagnosticOptions = specificDiagnosticOptions;

            ImmutableDictionary<string, bool>.Builder codeStyleRules = ImmutableDictionary.CreateBuilder<string, bool>();

            if (defaultCodeStyleRules != null)
            {
                foreach (KeyValuePair<string, bool> rule in defaultCodeStyleRules)
                    codeStyleRules.Add(rule);
            }

            foreach (KeyValuePair<string, ReportDiagnostic> kvp in specificDiagnosticOptions)
            {
                string key = kvp.Key;

                if (key.StartsWith(AnalyzerRuleIdentifiers.Prefix))
                {
                    ReportDiagnostic value = kvp.Value;

                    if (value == ReportDiagnostic.Info)
                    {
                        codeStyleRules[key] = true;
                    }
                    else if (value == ReportDiagnostic.Suppress)
                    {
                        codeStyleRules[key] = false;
                    }
                    else
                    {
                        Debug.Fail($"Invalid code style value '{value}'.");
                    }
                }
            }

            CodeStyleRules = codeStyleRules.ToImmutable();
        }

        public bool IsRuleEnabled(SemanticModel semanticModel, string ruleId)
        {
            return IsRuleEnabled(semanticModel.Compilation, ruleId);
        }

        public bool IsRuleEnabled(Compilation compilation, string ruleId)
        {
            if (compilation
                .Options
                .SpecificDiagnosticOptions.TryGetValue(ruleId, out ReportDiagnostic reportDiagnostic))
            {
                switch (reportDiagnostic)
                {
                    case ReportDiagnostic.Info:
                        return true;
                    case ReportDiagnostic.Suppress:
                        return false;
                }

                Debug.Fail($"Invalid code style value '{reportDiagnostic}'.");
                return false;
            }

            return CodeStyleRules.GetValueOrDefault(ruleId);
        }

        public DiagnosticSeverity GetDiagnosticSeverityOrDefault(string id, DiagnosticSeverity defaultValue)
        {
            if (!SpecificDiagnosticOptions.TryGetValue(id, out ReportDiagnostic reportDiagnostic))
                reportDiagnostic = GeneralDiagnosticOption;

            if (reportDiagnostic != ReportDiagnostic.Default
                && reportDiagnostic != ReportDiagnostic.Suppress)
            {
                return reportDiagnostic.ToDiagnosticSeverity();
            }

            return defaultValue;
        }

        public bool IsDiagnosticEnabledOrDefault(string id, bool defaultValue)
        {
            if (SpecificDiagnosticOptions.TryGetValue(id, out ReportDiagnostic reportDiagnostic))
            {
                if (reportDiagnostic != ReportDiagnostic.Default)
                    return reportDiagnostic != ReportDiagnostic.Suppress;
            }
            else if (GeneralDiagnosticOption == ReportDiagnostic.Suppress)
            {
                return false;
            }

            return defaultValue;
        }

        private static AnalyzerRules Create()
        {
            ImmutableDictionary<string, bool>.Builder defaultRules = ImmutableDictionary.CreateBuilder<string, bool>();

            defaultRules.Add(AnalyzerRuleIdentifiers.UseElementAccessOnElementAccess, true);
            defaultRules.Add(AnalyzerRuleIdentifiers.UseElementAccessOnInvocation, true);

            string path = typeof(AnalyzerRules).Assembly.Location;

            if (!string.IsNullOrEmpty(path))
                path = Path.Combine(Path.GetDirectoryName(path), RuleSetUtility.DefaultRuleSetName);

            RuleSet ruleSet = RuleSetUtility.Load(path, CodeAnalysisConfiguration.Current.RuleSets) ?? RuleSetUtility.EmptyRuleSet;

            return new AnalyzerRules(ruleSet.GeneralDiagnosticOption, ruleSet.SpecificDiagnosticOptions, defaultRules);
        }
    }
}
