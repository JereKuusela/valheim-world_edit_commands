namespace Data;

public class BoolValue(string[] values) : AnyValue(values), IBoolValue
{
  public int? GetInt(Functions f)
  {
    var value = GetValue(f);
    if (value == null) return null;
    return value == "true" ? 1 : 0;
  }
  public bool? GetBool(Functions f)
  {
    var value = GetValue(f);
    if (value == null) return null;
    return value == "true";
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
      var truthy = v == "true";
      if (truthy == value)
        return true;
    }
    return allNull ? null : false;
  }
}

public class SimpleBoolValue(bool value) : IBoolValue
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
