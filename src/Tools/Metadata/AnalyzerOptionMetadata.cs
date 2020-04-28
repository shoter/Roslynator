// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Roslynator.Metadata
{
    public class AnalyzerOptionMetadata
    {
        public AnalyzerOptionMetadata(
            string identifier,
            AnalyzerOptionKind kind,
            string title,
            string messageFormat,
            string summary,
            IEnumerable<SampleMetadata> samples,
            bool isObsolete)
        {
            Identifier = identifier;
            Kind = kind;
            Title = title;
            MessageFormat = messageFormat;
            Summary = summary;
            Samples = new ReadOnlyCollection<SampleMetadata>(samples?.ToArray() ?? Array.Empty<SampleMetadata>());
            IsObsolete = isObsolete;
        }

        public string Identifier { get; }

        public AnalyzerOptionKind Kind { get; }

        public string AnalyzerId { get; }

        public string Title { get; }

        public string MessageFormat { get; }

        public string Summary { get; }

        public IReadOnlyList<SampleMetadata> Samples { get; }

        public bool IsObsolete { get; }
    }
}
