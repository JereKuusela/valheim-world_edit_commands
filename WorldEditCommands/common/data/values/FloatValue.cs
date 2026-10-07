// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System.Globalization;
using UnityEngine;
using Common;

namespace Data;

public class FloatValue(string[] values) : DynamicValue(values), IFloatValue
{
  public float? Get(Functions f)
  {
    var value = GetValue(f);
    if (value == null)
      return null;
    if (!value.Contains(";"))
      return Parse.FloatNull(value);
    // Format for range is "start;end;step".
    var split = value.Split(';');
    if (split.Length < 2)
      throw new System.InvalidOperationException($"Invalid range format: {value}");
    var min = Parse.FloatNull(split[0]);
    var max = Parse.FloatNull(split[1]);
    if (min == null || max == null)
      return null;
    float? roll;
    if (split.Length < 3 || split[2] == "")
      roll = Random.Range(min.Value, max.Value);
    else
    {
      var step = Parse.FloatNull(split[2]);
      if (step == null || step == 0f)
        roll = Random.Range(min.Value, max.Value);
      else
      {
        var steps = (int)((max.Value - min.Value) / step.Value);
        var rollStep = Random.Range(0, steps + 1);
        roll = min + rollStep * step;
      }
    }
    return roll;
  }
  public bool TryGet(Functions f, out float value)
  {
    var v = Get(f);
    if (v.HasValue) value = v.Value;
    else value = 0;
    return v.HasValue;
  }
  public bool? Match(Functions f, float value)
  {
    // If all values are null, default to a match.
    var allNull = true;
    foreach (var rawValue in Values)
    {
      var v = f.Replace(rawValue);
      // Case 1: Simple value.
      if (!v.Contains(";"))
      {
        var parsed = Parse.FloatNull(v);
        if (parsed == null) continue;
        allNull = false;
        if (FloatCompare.Approx(parsed.Value, value))
          return true;
        continue;
      }
      var split = v.Split(';');
      if (split.Length < 2)
        throw new System.InvalidOperationException($"Invalid range format: {v}");
      var min = Parse.FloatNull(split[0]);
      var max = Parse.FloatNull(split[1]);
      if (min == null || max == null)
        continue;
      // Case 2: Range.
      if (split.Length < 3)
      {
        allNull = false;
        if (FloatCompare.ApproxBetween(value, min.Value, max.Value))
          return true;
      }
      // Case 3: Range with step.
      else
      {
        var step = Parse.FloatNull(split[2]);
        if (step == null)
          continue;
        allNull = false;
        if (step.Value == 0f)
        {
          if (FloatCompare.ApproxBetween(value, min.Value, max.Value))
            return true;
          continue;
        }
        var steps = (int)((max.Value - min.Value) / step.Value);
        for (var i = 0; i <= steps; ++i)
        {
          var roll = min.Value + i * step.Value;
          if (FloatCompare.Approx(roll, value))
            return true;
        }
      }
    }
    return allNull ? null : false;
  }
}

public class ConstantFloatValue(float value) : IFloatValue
{
  private readonly float Value = value;
  public float? Get(Functions f) => Value;
  public bool TryGet(Functions f, out float value)
  {
    value = Value;
    return true;
  }
  public bool? Match(Functions f, float value) => Value == value;
}
public interface IFloatValue
{
  float? Get(Functions f);
  bool TryGet(Functions f, out float value);
  bool? Match(Functions f, float value);
}