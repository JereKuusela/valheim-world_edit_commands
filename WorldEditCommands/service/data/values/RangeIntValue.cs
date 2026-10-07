using Service;

namespace Data;

public class RangeIntValue(string[] values) : AnyValue(values), IRangeIntValue
{
  public Range<int>? Get(Functions f)
  {
    var value = GetValue(f);
    if (value == null)
      return null;
    if (!value.Contains(";"))
    {
      var min = Parse.IntNull(value);
      return min == null ? null : new Range<int>(min.Value, 0);
    }

    var split = value.Split(';');
    if (split.Length < 2)
      throw new System.InvalidOperationException($"Invalid range format: {value}");
    var minValue = Parse.IntNull(split[0]);
    var maxValue = Parse.IntNull(split[1]);
    if (minValue == null || maxValue == null)
      return null;
    return new Range<int>(minValue.Value, maxValue.Value);
  }
}

public class SimpleRangeIntValue(Range<int> value) : IRangeIntValue
{
  private readonly Range<int> Value = value;
  public Range<int>? Get(Functions f) => Value;
}

public interface IRangeIntValue
{
  Range<int>? Get(Functions f);
}
