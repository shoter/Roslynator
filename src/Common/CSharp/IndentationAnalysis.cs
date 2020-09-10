// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Roslynator.CSharp
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal readonly struct IndentationAnalysis
    {
        public static IndentationAnalysis Empty { get; } = new IndentationAnalysis(null, CSharpFactory.EmptyWhitespace(), CSharpFactory.EmptyWhitespace());

        public IndentationAnalysis(SyntaxNode node, SyntaxTrivia indentation, SyntaxTrivia indentation2)
        {
            Node = node;
            Indentation = indentation;
            Indentation2 = indentation2;
        }

        public SyntaxNode Node { get; }

        public SyntaxTrivia Indentation { get; }

        public SyntaxTrivia Indentation2 { get; }

        public bool IsEmpty
        {
            get
            {
                return Indentation.Span.Length == 0
                  && Indentation2.Span.Length == 0;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"Length1 = {Indentation.Span.Length} Length2 = {Indentation2.Span.Length}";

        public string GetSingleIndentation()
        {
            int length1 = Indentation.Span.Length;
            int length2 = Indentation2.Span.Length;

            if (length1 == length2)
                return "";

            return (length1 > length2)
                ? Indentation.ToString().Substring(length2)
                : Indentation2.ToString().Substring(length1);
        }

        public string GetIncreasedIndentation()
        {
            int length1 = Indentation.Span.Length;
            int length2 = Indentation2.Span.Length;

            if (length1 == length2)
                return "";

            if (length1 > length2)
            {
                string s = Indentation.ToString();
                return s + s.Substring(length2);
            }
            else
            {
                string s = Indentation2.ToString();
                return s + s.Substring(length1);
            }
        }

        public int GetIncreasedIndentationLength()
        {
            int length1 = Indentation.Span.Length;
            int length2 = Indentation2.Span.Length;

            if (length1 == length2)
                return 0;

            return (length1 > length2)
                ? length1 + length1 - length2
                : length2 + length2 - length1;
        }
    }
}
