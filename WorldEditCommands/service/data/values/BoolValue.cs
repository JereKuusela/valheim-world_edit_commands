
using System.Collections.Generic;

namespace Data;

public class BoolValue(string[] values) : AnyValue(values), IBoolValue
{
  public int? GetInt(Dictionary<string, string> pars)
  {
    var value = GetValue(pars);
    if (value == null) return null;
    return value == "true" ? 1 : 0;
  }
  public bool? GetBool(Dictionary<string, string> pars)
  {
    var value = GetValue(pars);
    if (value == null) return null;
    return value == "true";
  }
  public bool? Match(Dictionary<string, string> pars, bool value)
  {

    // If all values are null, default to a match.
    var allNull = true;
    foreach (var rawValue in Values)
    {
      var v = ReplaceParameters(rawValue, pars);
      if (v == null) continue;
      allNull = false;
      var truthy = v == "true";
      if (truthy == value)
        return true;
    }
    return allNull ? null : false;
  }
}

public class SimpleBoolValue(bool value) : IBoolValue
{
  private readonly bool Value = value;

  public int? GetInt(Dictionary<string, string> pars) => Value ? 1 : 0;
  public bool? GetBool(Dictionary<string, string> pars) => Value;
  public bool? Match(Dictionary<string, string> pars, bool value) => Value == value;
}

public interface IBoolValue
{
  int? GetInt(Dictionary<string, string> pars);
  bool? GetBool(Dictionary<string, string> pars);
  bool? Match(Dictionary<string, string> pars, bool value);
}
