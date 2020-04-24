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

        public bool RCS1246_UseElementAccess_SupportInvocationExpression { get; set; }

        public bool RCS1246_UseElementAccess_SupportElementAccess { get; set; }

        protected override void SetValues(CodeAnalysisConfiguration configuration)
        {
            if (configuration == null)
                return;

            RCS1246_UseElementAccess_SupportInvocationExpression = false;
            RCS1246_UseElementAccess_SupportElementAccess = false;

            foreach (KeyValuePair<string, string> kvp in CodeAnalysisConfiguration.Default.Analyzers)
            {
                switch (kvp.Key)
                {
                    case "RCS1246": // UseElementAccess
                        {
                            if (string.Equals(kvp.Value, "SupportInvocationExpression", StringComparison.OrdinalIgnoreCase))
                            {
                                RCS1246_UseElementAccess_SupportInvocationExpression = true;
                            }
                            else if (string.Equals(kvp.Value, "SupportElementAccessExpression", StringComparison.OrdinalIgnoreCase))
                            {
                                RCS1246_UseElementAccess_SupportElementAccess = true;
                            }

                            break;
                        }
                    default:
                        {
                            Debug.Fail($"Unknown analyzer id '{kvp.Key}'");
                            break;
                        }
                }
            }
        }
    }
}
