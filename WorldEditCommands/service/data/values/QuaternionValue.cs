using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;

namespace Data;

public class QuaternionValue(string[] values) : AnyValue(values), IQuaternionValue
{
  public Quaternion? Get(Dictionary<string, string> pars)
  {
    var v = GetValue(pars);
    return v == null ? null : Parse.AngleYXZNull(v);
  }
  public bool? Match(Dictionary<string, string> pars, Quaternion value)
  {
    var values = GetAllValues(pars);
    if (values.Length == 0) return null;
    return values.Any(v => Parse.AngleYXZNull(v) == value);
  }
}

public class SimpleQuaternionValue(Quaternion value) : IQuaternionValue
{
  private readonly Quaternion Value = value;
  public Quaternion? Get(Dictionary<string, string> pars) => Value;
  public bool? Match(Dictionary<string, string> pars, Quaternion value) => Value == value;
}

public interface IQuaternionValue
{
  Quaternion? Get(Dictionary<string, string> pars);
  bool? Match(Dictionary<string, string> pars, Quaternion value);
}
