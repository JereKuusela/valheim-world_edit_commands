
using System.Collections.Generic;
using System.Globalization;
using ServerDevcommands;
using UnityEngine;

namespace Data;

public class FloatValue(string[] values) : AnyValue(values), IFloatValue
{
  public float? Get(Dictionary<string, string> pars)
  {
    var value = GetValue(pars);
    if (value == null)
      return null;
    if (!value.Contains(";"))
      return Calculator.EvaluateFloat(value);
    // Format for range is "start;end;step;statement".
    var split = value.Split(';');
    if (split.Length < 2)
      throw new System.InvalidOperationException($"Invalid range format: {value}");
    var min = Calculator.EvaluateFloat(split[0]);
    var max = Calculator.EvaluateFloat(split[1]);
    if (min == null || max == null)
      return null;
    float? roll;
    if (split.Length < 3 || split[2] == "")
      roll = Random.Range(min.Value, max.Value);
    else
    {
      var step = Calculator.EvaluateFloat(split[2]);
      if (step == null)
        roll = Random.Range(min.Value, max.Value);
      else
      {
        var steps = (int)((max.Value - min.Value) / step.Value);
        var rollStep = Random.Range(0, steps + 1);
        roll = min + rollStep * step;
      }
    }
    if (split.Length < 4)
      return roll;
    return Calculator.EvaluateFloat(split[3].Replace("<value>", roll?.ToString(CultureInfo.InvariantCulture)));
  }
  public bool? Match(Dictionary<string, string> pars, float value)
  {
    // If all values are null, default to a match.
    var allNull = true;
    foreach (var rawValue in Values)
    {
      var v = ReplaceParameters(rawValue, pars);
      // Case 1: Simple value.
      if (!v.Contains(";"))
      {
        var parsed = Calculator.EvaluateFloat(v);
        if (parsed == null) continue;
        allNull = false;
        if (Helper.Approx(parsed.Value, value))
          return true;
        continue;
      }
      var split = v.Split(';');
      if (split.Length < 2)
        throw new System.InvalidOperationException($"Invalid range format: {v}");
      var min = Calculator.EvaluateFloat(split[0]);
      var max = Calculator.EvaluateFloat(split[1]);
      if (min == null || max == null)
        continue;
      // Case 2: Range.
      if (split.Length < 3)
      {
        allNull = false;
        if (Helper.ApproxBetween(value, min.Value, max.Value))
          return true;
      }
      // Case 3: Range with step.
      else if (split.Length < 4)
      {
        var step = Calculator.EvaluateFloat(split[2]);
        if (step == null)
          continue;
        allNull = false;
        var steps = (int)((max.Value - min.Value) / step.Value);
        for (var i = 0; i <= steps; ++i)
        {
          var roll = min.Value + i * step.Value;
          if (Helper.Approx(roll, value))
            return true;
        }
      }
      else
      {
        // Case 4: Range with statement.
        if (split[2] == "")
        {
          var minValue = Calculator.EvaluateFloat(split[3].Replace("<value>", min?.ToString(CultureInfo.InvariantCulture)));
          var maxValue = Calculator.EvaluateFloat(split[3].Replace("<value>", max?.ToString(CultureInfo.InvariantCulture)));
          if (minValue == null || maxValue == null)
            continue;
          allNull = false;
          if (Helper.ApproxBetween(value, minValue.Value, maxValue.Value))
            return true;
        }
        else
        {
          // Case 5: Range with step and statement.
          var step = Calculator.EvaluateFloat(split[2]);
          if (step == null)
            continue;
          allNull = false;
          var steps = (int)((max.Value - min.Value) / step.Value);
          for (var i = 0; i <= steps; ++i)
          {
            var roll = min + i * step;
            var parsed = Calculator.EvaluateFloat(split[3].Replace("<value>", roll?.ToString(CultureInfo.InvariantCulture)));
            if (parsed == null) continue;
            if (Helper.Approx(parsed.Value, value))
              return true;
          }
        }
      }
    }
    return allNull ? null : false;
  }
}

public class SimpleFloatValue(float value) : IFloatValue
{
  private readonly float Value = value;
  public float? Get(Dictionary<string, string> pars) => Value;
  public bool? Match(Dictionary<string, string> pars, float value) => Value == value;
}
public interface IFloatValue
{
  float? Get(Dictionary<string, string> pars);
  bool? Match(Dictionary<string, string> pars, float value);
}