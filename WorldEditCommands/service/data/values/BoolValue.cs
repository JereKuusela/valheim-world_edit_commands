
using System.Collections.Generic;
using ServerDevcommands;

namespace Data;

public class BoolValue(string[] values) : AnyValue(values), IBoolValue
{
  public int? Get(Dictionary<string, string> pars)
  {
    var value = Parse.Boolean(GetValue(pars));
    if (value == null) return null;
    return value.Value ? 1 : 0;
  }
  public bool? Match(Dictionary<string, string> pars, bool value)
  {

    // If all values are null, default to a match.
    var allNull = true;
    foreach (var rawValue in Values)
    {
      var v = Parse.Boolean(ReplaceParameters(rawValue, pars));
      if (v == null) continue;
      allNull = false;
      if (value == v)
        return true;
    }
    return allNull ? null : false;
  }
}

public class SimpleBoolValue(bool value) : IBoolValue
{
  private readonly bool Value = value;

  public int? Get(Dictionary<string, string> pars) => Value ? 1 : 0;
  public bool? Match(Dictionary<string, string> pars, bool value) => Value == value;
}

public interface IBoolValue
{
  int? Get(Dictionary<string, string> pars);
  bool? Match(Dictionary<string, string> pars, bool value);
}
