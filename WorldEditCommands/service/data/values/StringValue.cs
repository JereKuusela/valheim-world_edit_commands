using System.Linq;

namespace Data;

public class StringValue(string[] values) : AnyValue(values), IStringValue
{
  private readonly bool IsPattern = values.Any(v => v.Contains("*"));
  public string? Get(Functions f) => GetValue(f);
  public string? GetWhole(Functions f) => GetWholeValue(f);

  public bool? Match(Functions f, string value)
  {
    var values = GetAllValues(f);
    if (values.Count == 0) return null;
    return IsPattern ? values.Any(v => SimpleStringValue.PatternMatch(value, v)) : values.Contains(value);
  }
}
public class SimpleStringValue(string value) : IStringValue
{
  private readonly string Value = value;
  private readonly bool IsPattern = value.Contains("*");
  public string? Get(Functions f) => Value;
  public string? GetWhole(Functions f) => Value;
  public bool? Match(Functions f, string value) => IsPattern ? PatternMatch(value, Value) : Value == value;
  public static bool PatternMatch(string value, string pattern)
  {
    if (value == pattern) return true;
    if (pattern == "") return false;
    if (pattern[0] == '*' && pattern[pattern.Length - 1] == '*')
    {
      return value.Contains(pattern.Substring(1, pattern.Length - 2));
    }
    if (pattern[0] == '*')
    {
      return value.EndsWith(pattern.Substring(1), System.StringComparison.Ordinal);
    }
    if (pattern[pattern.Length - 1] == '*')
    {
      return value.StartsWith(pattern.Substring(0, pattern.Length - 1), System.StringComparison.Ordinal);
    }
    var wildIndex = pattern.IndexOf('*');
    if (wildIndex > 0 && wildIndex < pattern.Length - 1)
    {
      var prefix = pattern.Substring(0, wildIndex);
      var suffix = pattern.Substring(wildIndex + 1);
      return value.StartsWith(prefix, System.StringComparison.Ordinal) &&
             value.EndsWith(suffix, System.StringComparison.Ordinal);
    }
    return false;
  }
}
public interface IStringValue
{
  string? Get(Functions f);
  // GetWhole is needed when the value is passed as it is, and processed later.
  string? GetWhole(Functions f);
  bool? Match(Functions f, string value);
}