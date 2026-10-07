// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data;

public class DynamicValue(string[] values)
{
  protected readonly string[] Values = values;
  public IReadOnlyList<string> RawValues => Values;

  private string? RollValue()
  {
    if (Values.Length == 1)
      return Values[0];
    return Values[Random.Range(0, Values.Length)];
  }
  protected string? GetValue(Functions f)
  {
    var value = RollValue();
    if (value == null || value == "<none>")
      return null;
    return f.Replace(value);
  }
  protected string? GetValue()
  {
    var value = RollValue();
    return value == null || value == "<none>" ? null : value;
  }
  protected List<string> GetAllValues(Functions f)
  {
    return [.. Values.Select(f.Replace).Where(v => v != null && v != "" && v != "<none>")];
  }
  protected string GetWholeValue(Functions f)
  {
    return string.Join(",", Values.Select(f.Replace));
  }
}
