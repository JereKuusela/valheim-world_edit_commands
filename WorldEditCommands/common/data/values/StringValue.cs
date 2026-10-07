// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System.Linq;
using Common;

namespace Data;

public class StringValue(string[] values) : DynamicValue(values), IStringValue
{
  private readonly bool IsPattern = values.Any(Wildcard.IsPattern);
  public string? Get(Functions f) => GetValue(f);
  public string? GetWhole(Functions f) => GetWholeValue(f);

  public bool? Match(Functions f, string value)
  {
    var anyValue = false;
    foreach (var rawValue in Values)
    {
      var v = f.Replace(rawValue);
      if (v == "" || v == "<none>") continue;
      anyValue = true;
      if (IsPattern ? Wildcard.Match(value, v) : v == value) return true;
    }
    return anyValue ? false : null;
  }
}
public class ConstantStringValue(string value) : IStringValue
{
  private readonly string Value = value;
  private readonly bool IsPattern = Wildcard.IsPattern(value);
  public string? Get(Functions f) => Value;
  public string? GetWhole(Functions f) => Value;
  public bool? Match(Functions f, string value) => IsPattern ? Wildcard.Match(value, Value) : Value == value;
}
public interface IStringValue
{
  string? Get(Functions f);
  // GetWhole is needed when the value is passed as it is, and processed later.
  string? GetWhole(Functions f);
  bool? Match(Functions f, string value);
}