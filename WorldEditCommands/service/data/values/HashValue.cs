using System.Linq;

namespace Data;

public class HashValue(string[] values) : AnyValue(values), IHashValue
{
  public int? Get(Functions f)
  {
    var value = GetValue(f);
    if (value == null || value == "") return null;
    return value.GetStableHashCode();
  }
  public bool? Match(Functions f, int value)
  {
    var values = GetAllValues(f);
    if (values.Count == 0) return null;
    return values.Any(v => v.GetStableHashCode() == value);
  }
}
public class SimpleHashValue(string value) : IHashValue
{
  private readonly int Value = value.GetStableHashCode();

  public int? Get(Functions f) => Value;
  public bool? Match(Functions f, int value) => Value == value;
}
public interface IHashValue
{
  int? Get(Functions f);
  bool? Match(Functions f, int value);
}
