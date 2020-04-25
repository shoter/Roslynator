// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Roslynator.Configuration;

namespace Roslynator
{
    internal sealed class AnalyzerSettings : CodeAnalysisSettings<string>
    {
        public static AnalyzerSettings Current { get; } = LoadSettings();

        private static AnalyzerSettings LoadSettings()
        {
            var settings = new AnalyzerSettings();

            settings.Reset();

            return settings;
        }

        public bool UseElementAccessOnInvocation { get; set; } = true;

        public bool UseElementAccessOnElementAccess { get; set; } = true;

        protected override void SetValues(CodeAnalysisConfiguration configuration)
        {
            if (configuration == null)
                return;

            UseElementAccessOnInvocation = true;
            UseElementAccessOnElementAccess = true;

            foreach (KeyValuePair<string, bool> kvp in CodeAnalysisConfiguration.Current.Analyzers)
            {
                if (string.Equals(kvp.Key, nameof(UseElementAccessOnInvocation), StringComparison.OrdinalIgnoreCase))
                {
                    UseElementAccessOnInvocation = kvp.Value;
                }
                else if (string.Equals(kvp.Key, nameof(UseElementAccessOnElementAccess), StringComparison.OrdinalIgnoreCase))
                {
                    UseElementAccessOnElementAccess = kvp.Value;
                }
                else
                {
                    Debug.Fail($"Unknown key '{kvp.Key}'");
                }
            }
        }
    }
}
