
using System.Collections.Generic;
using System.Linq;

namespace Data;

public class StringValue(string[] values) : AnyValue(values), IStringValue
{
  public string? Get(Dictionary<string, string> pars) => GetValue(pars);

  public bool? Match(Dictionary<string, string> pars, string value)
  {
    var values = GetAllValues(pars);
    if (values.Length == 0) return null;
    return values.Contains(value);
  }
}
public class SimpleStringValue(string value) : IStringValue
{
  private readonly string Value = value;
  public string? Get(Dictionary<string, string> pars) => Value;
  public bool? Match(Dictionary<string, string> pars, string value) => Value == value;
}
public interface IStringValue
{
  string? Get(Dictionary<string, string> pars);
  bool? Match(Dictionary<string, string> pars, string value);
}