# Analyzer Options

Each analyzer option is represented by an analyzer that is is not stand-alone analyzer but rather a modification of its parent analyzer.

## Properties

* "Analyzer Option" has ID that is derived from its parent analyzer (RCS0000 will have option RCS0000a, RCS0000b etc.)
* "Analyzer Option" is displayed in section "Options" in documentation of its parent analyzer.
* "Analyzer Option" requires its parent analyzer to be enabled.
* "Analyzer Option" is never be reported as a diagnostic. When you enable some analyzer option you will effectively
change behavior of its parent analyzer.

## Example

Analyzer [analyzers/RCS1051.md](RCS1051) suggest to parenthesize each condition of conditional expression.
It can make code more readable. But that depends on programmer's preference so this analyzer is not enabled by default.

Long after this analyzer was introduced it was [proposed](https://github.com/JosefPihrt/Roslynator/issues/169) to keep condition without parentheses if it is a single token.
This is reasonable proposal. It could be solved just by adding new analyzer that would have ID let's say RCS1234. But this would be very confusing for the user
because no one expects that there should be some connection between RCS1051 and RCS1234.

This one of the reasons why Roslynator come up with "Analyzer Options".

Solution to this proposal it to add new "analyzer option" [analyzers/RCS1051a.md](RCS1051a) which does following:

* suggest to parenthesize condition only in cases where expression is not a single token.
* suggest to remove parentheses from condition if it is a single token.

## Types of Analyzer Options

### "Enable" Option 

This option will add additional case(s) where parent analyzer is reported. Example is [analyzers/RCS1036a.md](RCS1036a).

### "Disable" Options

This option will cause some case(s) not to be reported by its parent analyzer. Example is [analyzers/RCS1246a.md](RCS1246a).

This option will 

### "Change" Option

This option is combination of "Enable" and "Disable" option. Example is [analyzers/RCS1051a.md](RCS1051a).

### "Invert" Option

This option inverts behavior of its parent analyzer. For example instead of add some kind of a syntax
it will suggest to remove the syntax.
