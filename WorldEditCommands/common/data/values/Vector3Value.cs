// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System.Linq;
using UnityEngine;
using Common;

namespace Data;

public class Vector3Value(string[] values) : DynamicValue(values), IVector3Value
{
  public Vector3? Get(Functions f)
  {
    var v = GetValue(f);
    return v == null ? null : Calculator.EvaluateVector3(v);
  }
  public bool? Match(Functions f, Vector3 value)
  {
    var values = GetAllValues(f);
    if (values.Count == 0) return null;
    return values.Any(v => Parse.VectorXZYNull(v) == value);
  }
}

public class ConstantVector3Value(Vector3 value) : IVector3Value
{
  private readonly Vector3 Value = value;
  public Vector3? Get(Functions f) => Value;
  public bool? Match(Functions f, Vector3 value) => Value == value;
}

public interface IVector3Value
{
  Vector3? Get(Functions f);
  bool? Match(Functions f, Vector3 value);
}