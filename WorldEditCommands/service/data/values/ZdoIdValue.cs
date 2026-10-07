using System.Linq;
using Service;

namespace Data;

public class ZdoIdValue(string[] values) : AnyValue(values), IZdoIdValue
{
  public ZDOID? Get(Functions f)
  {
    var value = GetValue(f);
    if (value == null) return null;
    return Parse.ZdoId(value);
  }
  public bool? Match(Functions f, ZDOID value)
  {
    var values = GetAllValues(f);
    if (values.Count == 0) return null;
    return values.Any(v => Parse.ZdoId(v) == value);
  }
}

public class SimpleZdoIdValue(ZDOID value) : IZdoIdValue
{
  private readonly ZDOID Value = value;
  public ZDOID? Get(Functions f) => Value;
  public bool? Match(Functions f, ZDOID value) => Value == value;
}

public interface IZdoIdValue
{
  ZDOID? Get(Functions f);
  bool? Match(Functions f, ZDOID value);
}
