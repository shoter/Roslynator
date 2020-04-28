# RCS1051\_NoParenthesesForSingleTokenExpression: Remove parentheses from condition of conditional expression

| Property | Value                                          |
| -------- | ---------------------------------------------- |
| Id       | RCS1051\_NoParenthesesForSingleTokenExpression |
| Category | AnalyzerOption                                 |
| Severity | None                                           |

## Example

### Code with Diagnostic

```csharp
x = (condition) ? "true" : "false"; // [|Id|]
```

### Code with Fix

```csharp
x = condition ? "true" : "false";
```

## See Also

* [How to Suppress a Diagnostic](../HowToConfigureAnalyzers.md#how-to-suppress-a-diagnostic)


*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*