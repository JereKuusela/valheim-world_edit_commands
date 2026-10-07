using System.Linq;
using Service;
using UnityEngine;

namespace Data;

public class QuaternionValue(string[] values) : AnyValue(values), IQuaternionValue
{
  public Quaternion? Get(Functions f)
  {
    var v = GetValue(f);
    return v == null ? null : Calculator.EvaluateQuaternion(v);
  }
  public bool? Match(Functions f, Quaternion value)
  {
    var values = GetAllValues(f);
    if (values.Count == 0) return null;
    return values.Any(v => Parse.AngleYXZNull(v) == value);
  }
}

public class SimpleQuaternionValue(Quaternion value) : IQuaternionValue
{
  private readonly Quaternion Value = value;
  public Quaternion? Get(Functions f) => Value;
  public bool? Match(Functions f, Quaternion value) => Value == value;
}

public interface IQuaternionValue
{
  Quaternion? Get(Functions f);
  bool? Match(Functions f, Quaternion value);
}
