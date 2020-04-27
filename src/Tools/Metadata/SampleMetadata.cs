// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Roslynator.Metadata
{
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    public readonly struct SampleMetadata
    {
        public SampleMetadata(string before, string after, string beforeOption, string afterOption)
        {
            Before = before;
            After = after;
            BeforeOption = beforeOption;
            AfterOption = afterOption;
        }

        public string Before { get; }

        public string After { get; }

        public string BeforeOption { get; }

        public string AfterOption { get; }

        public SampleMetadata WithBefore(string before)
        {
            return new SampleMetadata(
                before: before,
                after: After,
                beforeOption: BeforeOption,
                afterOption: AfterOption);
        }
    }
}
