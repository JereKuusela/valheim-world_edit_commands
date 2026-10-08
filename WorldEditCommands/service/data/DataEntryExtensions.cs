using System.Collections.Generic;

namespace Data;

// WEC only: convenience API for other mods (Infinity Hammer). Not part of the shared common/ data code.
public static class DataEntryExtensions
{
  public static void Set(this DataEntry data, int key, string value)
  {
    data.Strings ??= [];
    data.Strings[key] = DataValue.Constant(value);
  }
  public static void Set(this DataEntry data, int key, float value)
  {
    data.Floats ??= [];
    data.Floats[key] = DataValue.Constant(value);
  }
  public static void Set(this DataEntry data, int key, int value)
  {
    data.Ints ??= [];
    data.Ints[key] = DataValue.Constant(value);
  }
  public static void Set(this DataEntry data, int key, long value)
  {
    data.Longs ??= [];
    data.Longs[key] = DataValue.Constant(value);
  }
  public static void Set(this DataEntry data, int key, bool value)
  {
    data.Bools ??= [];
    data.Bools[key] = new ConstantBoolValue(value);
  }

  public static bool TryGetString(this DataEntry data, Dictionary<string, string> pars, int key, out string value)
  {
    value = "";
    if (data.Strings == null || !data.Strings.TryGetValue(key, out var val)) return false;
    var v = val.Get(new ParFunctions(pars, null));
    if (v == null) return false;
    value = v;
    return true;
  }
  public static bool TryGetFloat(this DataEntry data, Dictionary<string, string> pars, int key, out float value)
  {
    value = 0;
    if (data.Floats == null || !data.Floats.TryGetValue(key, out var val)) return false;
    var v = val.Get(new ParFunctions(pars, null));
    if (!v.HasValue) return false;
    value = v.Value;
    return true;
  }
  public static bool TryGetInt(this DataEntry data, Dictionary<string, string> pars, int key, out int value)
  {
    value = 0;
    if (data.Ints == null || !data.Ints.TryGetValue(key, out var val)) return false;
    var v = val.Get(new ParFunctions(pars, null));
    if (!v.HasValue) return false;
    value = v.Value;
    return true;
  }
  public static bool TryGetHash(this DataEntry data, Dictionary<string, string> pars, int key, out int value)
  {
    value = 0;
    if (data.Hashes == null || !data.Hashes.TryGetValue(key, out var val)) return false;
    var v = val.Get(new ParFunctions(pars, null));
    if (!v.HasValue) return false;
    value = v.Value;
    return true;
  }

  public static string GetBase64(this DataEntry data, Dictionary<string, string> pars) =>
    DataHelper.Resolve(data, pars, null).GetBase64();
}
