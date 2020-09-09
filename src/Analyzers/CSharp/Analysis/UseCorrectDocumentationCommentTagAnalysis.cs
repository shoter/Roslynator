// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Syntax;
using static Roslynator.DiagnosticHelpers;

namespace Roslynator.CSharp.Analysis
{
    public static class UseCorrectDocumentationCommentTagAnalysis
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, XmlElementInfo info)
        {
            if (info.IsEmptyElement)
                return;

            var element = (XmlElementSyntax)info.Element;

            foreach (XmlNodeSyntax node in element.Content)
            {
                XmlElementInfo info2 = SyntaxInfo.XmlElementInfo(node);

                if (info2.Success)
                {
                    switch (info2.GetTag())
                    {
                        case XmlTag.C:
                            {
                                AnalyzeCElement(context, info2);
                                break;
                            }
                        case XmlTag.Code:
                            {
                                AnalyzeCodeElement(context, info2);
                                break;
                            }
                        case XmlTag.List:
                            {
                                AnalyzeList(context, info2);
                                break;
                            }
                        case XmlTag.Para:
                        case XmlTag.ParamRef:
                        case XmlTag.See:
                        case XmlTag.TypeParamRef:
                            {
                                Analyze(context, info2);
                                break;
                            }
                        case XmlTag.Content:
                        case XmlTag.Example:
                        case XmlTag.Exception:
                        case XmlTag.Exclude:
                        case XmlTag.Include:
                        case XmlTag.InheritDoc:
                        case XmlTag.Param:
                        case XmlTag.Permission:
                        case XmlTag.Remarks:
                        case XmlTag.Returns:
                        case XmlTag.SeeAlso:
                        case XmlTag.Summary:
                        case XmlTag.TypeParam:
                        case XmlTag.Value:
                            {
                                break;
                            }
                        default:
                            {
                                Debug.Fail(info2.GetTag().ToString());
                                break;
                            }
                    }
                }
            }
        }

        private static void AnalyzeList(SyntaxNodeAnalysisContext context, XmlElementInfo info)
        {
            if (!info.IsEmptyElement)
            {
                var element = (XmlElementSyntax)info.Element;

                foreach (XmlNodeSyntax node in element.Content)
                {
                    XmlElementInfo info2 = SyntaxInfo.XmlElementInfo(node);

                    if (!info2.Success)
                        continue;

                    if (info2.IsEmptyElement)
                        continue;

                    if (string.Equals(info2.LocalName, "listheader", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(info2.LocalName, "item", StringComparison.OrdinalIgnoreCase))
                    {
                        var element2 = (XmlElementSyntax)info2.Element;

                        foreach (XmlNodeSyntax node2 in element2.Content)
                        {
                            XmlElementInfo info3 = SyntaxInfo.XmlElementInfo(node2);

                            if (!info3.Success)
                                continue;

                            if (info3.IsEmptyElement)
                                continue;

                            if (string.Equals(info3.LocalName, "term", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(info3.LocalName, "description", StringComparison.OrdinalIgnoreCase))
                            {
                                Analyze(context, info3);
                            }
                        }
                    }
                }
            }
        }

        private static void AnalyzeCElement(SyntaxNodeAnalysisContext context, XmlElementInfo info)
        {
            if (info.IsEmptyElement)
                return;

            var element = (XmlElementSyntax)info.Element;

            SyntaxList<XmlNodeSyntax> content = element.Content;

            if (!content.Any())
                return;

            int start = content.First().FullSpan.Start;
            int end = content.Last().FullSpan.End;

            if (context.Node.SyntaxTree.IsMultiLineSpan(TextSpan.FromBounds(start, end)))
                ReportDiagnostic(context, DiagnosticDescriptors.UseCorrectDocumentationCommentTag, info.Element);
        }

        private static void AnalyzeCodeElement(SyntaxNodeAnalysisContext context, XmlElementInfo info)
        {
            if (info.IsEmptyElement)
                return;

            var element = (XmlElementSyntax)info.Element;

            SyntaxList<XmlNodeSyntax> content = element.Content;

            if (!content.Any())
                return;

            int start = content.First().FullSpan.Start;
            int end = content.Last().FullSpan.End;

            if (context.Node.SyntaxTree.IsSingleLineSpan(TextSpan.FromBounds(start, end)))
                ReportDiagnostic(context, DiagnosticDescriptors.UseCorrectDocumentationCommentTag, info.Element);
        }
    }
}
