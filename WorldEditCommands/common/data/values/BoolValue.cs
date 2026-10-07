// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
namespace Data;

public class BoolValue(string[] values) : DynamicValue(values), IBoolValue
{
  private static bool IsTrue(string value) => string.Equals(value, "true", System.StringComparison.OrdinalIgnoreCase);

  public int? GetInt(Functions f)
  {
    var value = GetValue(f);
    if (value == null) return null;
    return IsTrue(value) ? 1 : 0;
  }
  public bool? GetBool(Functions f)
  {
    var value = GetValue(f);
    if (value == null) return null;
    return IsTrue(value);
  }
  public bool? Match(Functions f, bool value)
  {
    // If all values are null, default to a match.
    var allNull = true;
    foreach (var rawValue in Values)
    {
      var v = f.Replace(rawValue);
      if (v == null) continue;
      allNull = false;
      var truthy = IsTrue(v);
      if (truthy == value)
        return true;
    }
    return allNull ? null : false;
  }
}

public class ConstantBoolValue(bool value) : IBoolValue
{
  private readonly bool Value = value;

  public int? GetInt(Functions f) => Value ? 1 : 0;
  public bool? GetBool(Functions f) => Value;
  public bool? Match(Functions f, bool value) => Value == value;
}

public interface IBoolValue
{
  int? GetInt(Functions f);
  bool? GetBool(Functions f);
  bool? Match(Functions f, bool value);
}
