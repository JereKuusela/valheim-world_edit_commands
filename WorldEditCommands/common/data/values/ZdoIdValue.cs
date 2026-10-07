// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System.Linq;
using Common;

namespace Data;

public class ZdoIdValue(string[] values) : DynamicValue(values), IZdoIdValue
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

public class ConstantZdoIdValue(ZDOID value) : IZdoIdValue
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
