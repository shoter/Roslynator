// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator
{
    internal static class AnalyzerRuleIdentifiers
    {
        public const string Prefix = "Roslynator.";

        public const string UseElementAccessOnInvocation = Prefix + nameof(UseElementAccessOnInvocation);

        public const string UseElementAccessOnElementAccess = Prefix + nameof(UseElementAccessOnElementAccess);
    }
}
