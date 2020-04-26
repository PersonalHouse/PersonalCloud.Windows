// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2000", Justification = "Disposables are disposed elsewhere.", Scope = "module")]
[assembly: SuppressMessage("Design", "CA1062", Justification = "Null-checks are performed elsewhere.", Scope = "module")]
