// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System;

namespace Common;

/// <summary>Simple '*' wildcard matching: '*', '*x', 'x*', '*x*' and a single inner wildcard 'x*y'.</summary>
public static class Wildcard
{
  public static bool IsPattern(string value) => value.IndexOf('*') >= 0;

  public static bool Match(string value, string pattern, bool ignoreCase = false)
  {
    var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    if (string.Equals(value, pattern, comparison)) return true;
    if (pattern == "") return false;
    if (pattern == "*") return true;
    var starts = pattern[0] == '*';
    var ends = pattern[pattern.Length - 1] == '*';
    if (starts && ends) return value.IndexOf(pattern.Substring(1, pattern.Length - 2), comparison) >= 0;
    if (starts) return value.EndsWith(pattern.Substring(1), comparison);
    if (ends) return value.StartsWith(pattern.Substring(0, pattern.Length - 1), comparison);
    var wildIndex = pattern.IndexOf('*');
    if (wildIndex < 0) return false;
    // Prefix and suffix must not overlap.
    return value.Length >= pattern.Length - 1
      && value.StartsWith(pattern.Substring(0, wildIndex), comparison)
      && value.EndsWith(pattern.Substring(wildIndex + 1), comparison);
  }
}
