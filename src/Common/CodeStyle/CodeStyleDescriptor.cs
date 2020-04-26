// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CodeStyle
{
    public class CodeStyleDescriptor
    {
        public CodeStyleDescriptor(string id, bool isEnabledByDefault, string summary)
        {
            Id = id;
            IsEnabledByDefault = isEnabledByDefault;
            Summary = summary;
        }

        public string Id { get; }

        public bool IsEnabledByDefault { get; }

        public string Summary { get; }
    }
}
