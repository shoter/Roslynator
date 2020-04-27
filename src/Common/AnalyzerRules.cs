// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Roslynator.Options;
using Roslynator.Configuration;

namespace Roslynator
{
    internal class AnalyzerRules
    {
        public static AnalyzerRules Default { get; } = Create();

        public ReportDiagnostic GeneralDiagnosticOption { get; }

        public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; }

        public ImmutableDictionary<string, bool> AnalyzerOptions { get; }

        public AnalyzerRules(
            ReportDiagnostic generalDiagnosticOption,
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions,
            IEnumerable<KeyValuePair<string, bool>> defaultAnalyzerOptions = null)
        {
            GeneralDiagnosticOption = generalDiagnosticOption;
            SpecificDiagnosticOptions = specificDiagnosticOptions;

            ImmutableDictionary<string, bool>.Builder analyzerOptions = ImmutableDictionary.CreateBuilder<string, bool>();

            if (defaultAnalyzerOptions != null)
            {
                foreach (KeyValuePair<string, bool> rule in defaultAnalyzerOptions)
                    analyzerOptions.Add(rule);
            }

            foreach (KeyValuePair<string, ReportDiagnostic> kvp in specificDiagnosticOptions)
            {
                string key = kvp.Key;

                if (key.StartsWith(AnalyzerOptionIdentifiers.Prefix))
                {
                    ReportDiagnostic value = kvp.Value;

                    if (value == ReportDiagnostic.Info)
                    {
                        analyzerOptions[key] = true;
                    }
                    else if (value == ReportDiagnostic.Suppress)
                    {
                        analyzerOptions[key] = false;
                    }
                    else
                    {
                        Debug.Fail($"Invalid code style value '{value}'.");
                    }
                }
            }

            AnalyzerOptions = analyzerOptions.ToImmutable();
        }

        public bool IsOptionEnabled(Compilation compilation, string id)
        {
            if (compilation
                .Options
                .SpecificDiagnosticOptions.TryGetValue(id, out ReportDiagnostic reportDiagnostic))
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

            return AnalyzerOptions.GetValueOrDefault(id);
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
            string path = typeof(AnalyzerRules).Assembly.Location;

            if (!string.IsNullOrEmpty(path))
                path = Path.Combine(Path.GetDirectoryName(path), RuleSetUtility.DefaultRuleSetName);

            RuleSet ruleSet = RuleSetUtility.Load(path, CodeAnalysisConfiguration.Current.RuleSets) ?? RuleSetUtility.EmptyRuleSet;

            ImmutableDictionary<string, bool> defaultAnalyzerOptions = typeof(AnalyzerOptionDescriptors)
                .GetRuntimeFields()
                .Where(f => f.IsPublic)
                .Select(f => (AnalyzerOptionDescriptor)f.GetValue(null))
                .ToImmutableDictionary(f => f.Id, f => f.IsEnabledByDefault);

            return new AnalyzerRules(
                ruleSet.GeneralDiagnosticOption,
                ruleSet.SpecificDiagnosticOptions,
                defaultAnalyzerOptions);
        }
    }
}
