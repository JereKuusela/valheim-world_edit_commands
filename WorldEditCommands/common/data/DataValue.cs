// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System.Collections.Generic;
using System.Linq;
using Common;
using Service;
using UnityEngine;

namespace Data;

public class DataValue
{

  public static IZdoIdValue ZdoId(string values)
  {
    var split = SplitWithValues(values);
    var zdo = Parse.ZdoId(split[0]);
    if (split.Length == 1 && zdo != ZDOID.None)
      return new ConstantZdoIdValue(zdo);
    return new ZdoIdValue(split);
  }
  // Different function name because string would be ambiguous.
  public static IIntValue Constant(int value) => new ConstantIntValue(value);
  public static IStringValue Constant(string value) => new ConstantStringValue(value);
  public static IFloatValue Constant(float value) => new ConstantFloatValue(value);
  public static ILongValue Constant(long value) => new ConstantLongValue(value);
  public static IVector3Value Constant(Vector3 value) => new ConstantVector3Value(value);
  public static IQuaternionValue Constant(Quaternion value) => new ConstantQuaternionValue(value);
  public static IBytesValue Constant(byte[]? value) => new ConstantBytesValue(value);

  public static IIntValue Int(string values)
  {
    var split = SplitWithValues(values);
    if (split.Length == 1 && Parse.TryInt(split[0], out var result))
      return new ConstantIntValue(result);
    return new IntValue(split);
  }

  public static IRangeIntValue RangeInt(string values)
  {
    var split = SplitWithValues(values);
    if (split.Length == 1 && !HasFunctions(split[0]))
    {
      if (Parse.TryInt(split[0], out var result))
        return new ConstantRangeIntValue(new ValueRange<int>(result, 0));
      var range = Parse.IntRange(split[0]);
      return new ConstantRangeIntValue(range);
    }
    return new RangeIntValue(split);
  }

  public static IFloatValue Float(string values)
  {
    var split = SplitWithValues(values);
    if (split.Length == 1 && Parse.TryFloat(split[0], out var result))
      return new ConstantFloatValue(result);
    return new FloatValue(split);
  }

  public static ILongValue Long(string values)
  {
    var split = SplitWithValues(values);
    if (split.Length == 1 && Parse.TryLong(split[0], out var result))
      return new ConstantLongValue(result);
    return new LongValue(split);
  }

  public static IStringValue String(string values)
  {
    // Quick hack for quoted strings.
    if (values.Length > 2 && values[0] == '"' && values[values.Length - 1] == '"')
      return new ConstantStringValue(values.Substring(1, values.Length - 2));
    var split = SplitWithValues(values);
    if (split.Length == 1 && !HasFunctions(split[0]))
      return new ConstantStringValue(split[0]);
    return new StringValue(split);
  }
  public static IBoolValue Bool(string values)
  {
    var split = SplitWithValues(values);
    if (split.Length == 1 && bool.TryParse(split[0], out var result))
      return new ConstantBoolValue(result);
    return new BoolValue(split);
  }

  public static IBytesValue Bytes(string values)
  {
    var split = SplitWithValues(values);
    if (split.Length == 1 && !HasFunctions(split[0]))
    {
      if (string.IsNullOrEmpty(split[0]))
        return new ConstantBytesValue(null);
      try
      {
        var bytes = System.Convert.FromBase64String(split[0]);
        return new ConstantBytesValue(bytes);
      }
      catch (System.FormatException)
      {
        // If not valid base64, treat as a dynamic value
      }
    }
    return new BytesValue(split);
  }

  public static IHashValue Hash(string values)
  {
    var split = SplitWithValues(values);
    if (split.Length == 1 && !HasFunctions(split[0]))
      return new ConstantHashValue(split[0]);
    return new HashValue(split);
  }
  public static IPrefabValue Prefab(string values)
  {
    if (HasFunctions(values))
      return new PrefabValue(SplitWithValues(values));
    var prefabs = PrefabHelper.GetPrefabs(values, "");
    if (prefabs.Count == 0) return new ConstantPrefabValue(null);
    if (prefabs.Count == 1) return new ConstantPrefabValue(prefabs[0]);
    return new ConstantPrefabListValue(prefabs);
  }

  public static IVector3Value Vector3(string values)
  {
    var split = SplitWithValues(values);
    if (HasFunctions(values) || split.Length > 3)
    {
      // Vectors are trickly to handle because the coordinate separator is same as value separator.
      List<string> combined = [];
      for (var i = 0; i < split.Length; i += 3)
      {
        // Last vector can be partial.
        var v = i + 3 < split.Length ? split.Skip(i).Take(3).ToArray() : [.. split.Skip(i)];
        combined.Add(string.Join(",", v));
      }
      return new Vector3Value([.. combined]);
    }
    if (Parse.TryDistanceAngle(split, out var polar))
      return new ConstantVector3Value(polar);
    var parsed = Parse.VectorXZYNull(split);
    return new ConstantVector3Value(parsed.HasValue ? parsed.Value : UnityEngine.Vector3.zero);
  }
  public static IQuaternionValue Quaternion(string values)
  {
    var split = SplitWithValues(values);
    if (HasFunctions(values) || split.Length > 3)
    {
      List<string> combined = [];
      for (var i = 0; i < split.Length; i += 3)
      {
        var v = i + 3 < split.Length ? split.Skip(i).Take(3).ToArray() : [.. split.Skip(i)];
        combined.Add(string.Join(",", v));
      }
      return new QuaternionValue([.. combined]);
    }
    var parsed = Parse.AngleYXZNull(split);
    return new ConstantQuaternionValue(parsed.HasValue ? parsed.Value : UnityEngine.Quaternion.identity);
  }

  private static bool HasFunctions(string value) => value.Contains("<") && value.Contains(">");

  private static string[] SplitWithValues(string str)
  {
    List<string> result = [];
    var split = Parse.SplitWithEmpty(str);
    foreach (var value in split)
    {
      if (!HasFunctions(value))
      {
        result.Add(value);
        continue;
      }
      var parSplit = value.Split('<', '>');
      List<string> parameters = [];
      List<int> hashes = [];
      for (var i = 1; i < parSplit.Length; i += 2)
      {
        var hash = ValueGroups.Hash(parSplit[i]);
        // Value groups should work same as using the value directly.
        // So it makes sense to resolve them on load.
        // This way other code doesn't have to worry about resolving them and performance is better.
        if (ValueGroups.Contains(hash))
        {
          parameters.Add($"<{parSplit[i]}>");
          hashes.Add(hash);
        }
      }
      if (parameters.Count == 0)
      {
        result.Add(value);
        // Early exit because preloading too many values wastes memory.
        if (result.Count > 1000)
          break;
      }
      else
        SubstituteValues(result, value, parameters, hashes, 0);

    }
    if (result.Count > 1000)
      Log.Warning("Too many values loaded for " + str);
    return result.Count > 1000 ? [str] : [.. result];
  }

  // Recursion needed because there can be multiple value groups.
  private static void SubstituteValues(List<string> result, string format, List<string> parameters, List<int> hashes, int index)
  {
    if (!ValueGroups.TryGet(hashes[index], out var groups)) return;
    foreach (var group in groups)
    {
      var newFormat = format.Replace(parameters[index], group);
      if (index == parameters.Count - 1)
      {
        result.Add(newFormat);
        // Early exit because preloading too many values wastes memory.
        if (result.Count > 1000)
          break;
      }
      else
        SubstituteValues(result, newFormat, parameters, hashes, index + 1);
    }
  }
}
