using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Data;

// WEC only: finds the <parameters> a data entry needs, used for autocomplete.
public static class RequiredParameters
{
  public static HashSet<string> Get(DataEntry data)
  {
    HashSet<string> result = [];
    foreach (var dict in new IDictionary?[] { data.Strings, data.Floats, data.Ints, data.Bools, data.Hashes, data.Longs, data.Vecs, data.Quats, data.ByteArrays })
    {
      if (dict == null) continue;
      foreach (var value in dict.Values)
        Collect(value, result);
    }
    Collect(data.Persistent, result);
    Collect(data.Distant, result);
    Collect(data.Position, result);
    Collect(data.Rotation, result);
    Collect(data.ItemAmount, result);
    Collect(data.Item, result);
    if (data.Items != null)
      foreach (var item in data.Items)
        Collect(item, result);
    return result;
  }

  private static void Collect(object? value, HashSet<string> result)
  {
    if (value is DynamicValue any)
    {
      foreach (var raw in any.RawValues)
        Extract(raw, result);
    }
    else if (value is ItemValue)
    {
      foreach (var field in typeof(ItemValue).GetFields(BindingFlags.Instance | BindingFlags.Public))
      {
        var fieldValue = field.GetValue(value);
        if (fieldValue is IDictionary dict)
          foreach (var v in dict.Values)
            Collect(v, result);
        else
          Collect(fieldValue, result);
      }
    }
  }

  private static void Extract(string value, HashSet<string> result)
  {
    var split = value.Split('<', '>');
    for (var i = 1; i < split.Length; i += 2)
    {
      var name = split[i].Split('=')[0];
      if (name != "") result.Add(name);
    }
  }
}
