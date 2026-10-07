// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.

using Common;

namespace Data;

public class RangeIntValue(string[] values) : DynamicValue(values), IRangeIntValue
{
  public ValueRange<int>? Get(Functions f)
  {
    var value = GetValue(f);
    if (value == null)
      return null;
    if (!value.Contains(";"))
    {
      var min = Parse.IntNull(value);
      return min == null ? null : new ValueRange<int>(min.Value, 0);
    }

    var split = value.Split(';');
    if (split.Length < 2)
      throw new System.InvalidOperationException($"Invalid range format: {value}");
    var minValue = Parse.IntNull(split[0]);
    var maxValue = Parse.IntNull(split[1]);
    if (minValue == null || maxValue == null)
      return null;
    return new ValueRange<int>(minValue.Value, maxValue.Value);
  }
}

public class ConstantRangeIntValue(ValueRange<int> value) : IRangeIntValue
{
  private readonly ValueRange<int> Value = value;
  public ValueRange<int>? Get(Functions f) => Value;
}

public interface IRangeIntValue
{
  ValueRange<int>? Get(Functions f);
}
