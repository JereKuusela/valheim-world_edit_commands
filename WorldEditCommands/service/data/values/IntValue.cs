using Service;
using UnityEngine;

namespace Data;

public class IntValue(string[] values) : AnyValue(values), IIntValue
{
  public int? Get(Functions f)
  {
    var value = GetValue(f);
    if (value == null)
      return null;
    if (!value.Contains(";"))
      return Parse.IntNull(value);
    // Format for range is "start;end;step;statement".
    var split = value.Split(';');
    if (split.Length < 2)
      throw new System.InvalidOperationException($"Invalid range format: {value}");
    var min = Parse.IntNull(split[0]);
    var max = Parse.IntNull(split[1]);
    if (min == null || max == null)
      return null;
    int? roll;
    if (split.Length < 3 || split[2] == "")
      roll = Random.Range(min.Value, max.Value + 1);
    else
    {
      var step = Parse.IntNull(split[2]);
      if (step == null)
        roll = Random.Range(min.Value, max.Value + 1);
      else
      {
        var steps = (max - min) / step;
        var rollStep = Random.Range(0, steps.Value + 1);
        roll = min + rollStep * step;
      }
    }
    return roll;
  }
  public bool? Match(Functions f, int value)
  {
    // If all values are null, default to a match.
    var allNull = true;
    foreach (var rawValue in Values)
    {
      var v = f.Replace(rawValue);
      // Case 1: Simple value.
      if (!v.Contains(";"))
      {
        var parsed = Parse.IntNull(v);
        if (parsed == null) continue;
        allNull = false;
        if (parsed.Value == value)
          return true;
        continue;
      }
      var split = v.Split(';');
      if (split.Length < 2)
        throw new System.InvalidOperationException($"Invalid range format: {v}");
      var min = Parse.IntNull(split[0]);
      var max = Parse.IntNull(split[1]);
      if (min == null || max == null)
        continue;
      // Case 2: Range.
      if (split.Length < 3)
      {
        allNull = false;
        if (value >= min.Value && value <= max.Value)
          return true;
      }
      // Case 3: Range with step.
      else if (split.Length < 4)
      {
        var step = Parse.IntNull(split[2]);
        if (step == null)
          continue;
        allNull = false;
        var steps = (max.Value - min.Value) / step.Value;
        for (var i = 0; i <= steps; ++i)
        {
          var roll = min.Value + i * step.Value;
          if (roll == value)
            return true;
        }
      }
    }
    return allNull ? null : false;
  }
}

public class SimpleIntValue(int value) : IIntValue
{
  private readonly int Value = value;
  public int? Get(Functions f) => Value;
  public bool? Match(Functions f, int value) => Value == value;
}
public interface IIntValue
{
  int? Get(Functions f);
  bool? Match(Functions f, int value);
}