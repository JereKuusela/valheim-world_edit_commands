using Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Data;

// Functions are technically just a key-value mapping.
// Proper class allows properly adding caching and other features.
// While also ensuring that all code is in one place.
public class Functions(string prefab, string[] args, Vector3 pos)
{
  protected const char Separator = '_';
  public static Func<string, string?> ExecuteCode = key => null!;
  public static Func<string, string, string?> ExecuteCodeWithValue = (key, value) => null!;

  // Hooks for features that only some hosts have.
  protected virtual string? ResolveApiFunction(string key) => null;
  protected virtual string? ResolveApiValueFunction(string key, string value) => null;
  protected virtual string GetStoredValue(string key, string defaultValue) => defaultValue;
  protected virtual string IncrementStoredValue(string key, long amount) => "";
  protected virtual void SetStoredValue(string key, string value) { }

  private readonly double time = ZNet.instance.GetTimeSeconds();

  public int Amount = 0;
  private List<ZDOID>? pokeTargets;
  private Dictionary<string, int>? pokeCounts;

  public void SetPokeTargets(List<ZDOID> targets)
  {
    pokeTargets = targets;
    pokeCounts = null;
    Amount = targets.Count;
  }
  public string Replace(string str) => Replace(str, false, false);
  public string Replace(string str, bool preventInjections, bool allValues)
  {
    StringBuilder parts = new();
    int nesting = 0;
    var start = 0;
    for (int i = 0; i < str.Length; i++)
    {
      if (str[i] == '<')
      {
        if (nesting == 0)
        {
          parts.Append(str.Substring(start, i - start));
          start = i;
        }
        nesting++;

      }
      if (str[i] == '>')
      {
        if (nesting == 1)
        {
          var key = str.Substring(start, i - start + 1);
          var resolved = ResolveFunctions(key, allValues);
          // Server Devcommands mod supports running commands separated by ';'.
          // This allows injection attacks when players can control function values.
          // For example with player name, chat messages or sign texts.
          if (preventInjections && resolved.Contains(";"))
            resolved = resolved.Replace(";", ",");
          parts.Append(resolved);
          start = i + 1;
        }
        if (nesting > 0)
          nesting--;
      }
    }
    if (start < str.Length)
      parts.Append(str.Substring(start));

    return parts.ToString();
  }
  private string ResolveFunctions(string str, bool allValues)
  {
    for (int i = 0; i < str.Length; i++)
    {
      var end = str.IndexOf(">", i);
      if (end == -1) break;
      i = end;
      var start = str.LastIndexOf("<", end);
      if (start == -1) continue;
      var length = end - start + 1;
      if (TryReplaceFunction(str.Substring(start, length), allValues, out var resolved))
      {
        str = str.Remove(start, length);
        str = str.Insert(start, resolved);
        // Resolved could contain functions, so need to recheck the same position.
        i = start - 1;
      }
      else
      {
        i = end;
      }
    }
    return str;
  }
  private bool TryReplaceFunction(string rawKey, bool allValues, out string? resolved)
  {
    var key = rawKey.Substring(1, rawKey.Length - 2);
    var keyDefault = Parse.Kvp(key, '=');
    var defaultValue = keyDefault.Value;
    // Ending with just '=' is probably a base64 encoded value.
    if (defaultValue.All(c => c == '='))
      defaultValue = "";
    else
      key = keyDefault.Key;

    resolved = GetFunction(key, defaultValue);
    if (resolved == null)
      resolved = allValues ? ResolveConditionValue(rawKey) : ResolveValue(rawKey);
    return resolved != rawKey;
  }

  protected virtual string? GetFunction(string key, string defaultValue)
  {
    var value = ResolveApiFunction(key);
    if (value != null) return value;
    value = ExecuteCode(key);
    if (value != null) return value;
    value = GetGeneralFunction(key, defaultValue);
    if (value != null) return value;
    var keyArg = Parse.Kvp(key, Separator);
    if (keyArg.Value == "") return null;
    key = keyArg.Key;
    var arg = keyArg.Value;

    value = ResolveApiValueFunction(key, arg);
    if (value != null) return value;
    value = ExecuteCodeWithValue(key, arg);
    if (value != null) return value;
    return GetValueFunction(key, arg, defaultValue);
  }

  private string? GetGeneralFunction(string key, string defaultValue) =>
    key switch
    {
      "prefab" => prefab,
      "safeprefab" => prefab.Replace(Separator, '-'),
      "par" => string.Join(" ", args),
      "par0" => GetArg(0, defaultValue),
      "par1" => GetArg(1, defaultValue),
      "par2" => GetArg(2, defaultValue),
      "par3" => GetArg(3, defaultValue),
      "par4" => GetArg(4, defaultValue),
      "par5" => GetArg(5, defaultValue),
      "par6" => GetArg(6, defaultValue),
      "par7" => GetArg(7, defaultValue),
      "par8" => GetArg(8, defaultValue),
      "par9" => GetArg(9, defaultValue),
      "day" => EnvMan.instance.GetDay(time).ToString(),
      "ticks" => ((long)(time * 10000000.0)).ToString(),
      "x" => Formatting.Format(pos.x),
      "y" => Formatting.Format(pos.y),
      "z" => Formatting.Format(pos.z),
      "snap" => Formatting.Format(WorldGenerator.instance.GetHeight(pos.x, pos.z)),
      "pokecount" => args.Length < 2 ? Amount.ToString() : null,
      "time" => Formatting.Format(time),
      "realtime" => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
      _ => null,
    };

  protected virtual string? GetValueFunction(string key, string value, string defaultValue) =>
   key switch
   {
     "pokecount" => GetPokeCount(value, defaultValue),
     "sqrt" => Parse.TryFloat(value, out var f) ? Mathf.Sqrt(f).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "round" => Parse.TryFloat(value, out var f) ? Mathf.Round(f).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "ceil" => Parse.TryFloat(value, out var f) ? Mathf.Ceil(f).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "floor" => Parse.TryFloat(value, out var f) ? Mathf.Floor(f).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "abs" => Parse.TryFloat(value, out var f) ? Mathf.Abs(f).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "sin" => Parse.TryAngleRadians(value, out var f) ? Mathf.Sin(f).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "cos" => Parse.TryAngleRadians(value, out var f) ? Mathf.Cos(f).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "tan" => Parse.TryAngleRadians(value, out var f) ? Mathf.Tan(f).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "asin" => Parse.TryFloat(value, out var f) ? Mathf.Asin(f).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "acos" => Parse.TryFloat(value, out var f) ? Mathf.Acos(f).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "rad2deg" => Rad2Deg(value) ?? defaultValue,
     "deg2rad" => Deg2Rad(value) ?? defaultValue,
     "rad2vec" => Rad2Vec(value) ?? defaultValue,
     "deg2vec" => Deg2Vec(value) ?? defaultValue,
     "vec2deg" => Vec2Deg(value) ?? defaultValue,
     "vec2rad" => Vec2Rad(value) ?? defaultValue,
     "angle" => HandleAngle(value, defaultValue),
     "distance" => HandleDistance(value, defaultValue),
     "dot" => HandleDot(value, defaultValue),
     "cross" => HandleCross(value, defaultValue),
     "normalize" => HandleNormalize(value, defaultValue),
     "magnitude" => HandleMagnitude(value, defaultValue),
     "sqrmagnitude" => HandleSqrMagnitude(value, defaultValue),
     "project" => HandleProject(value, defaultValue),
     "reflect" => HandleReflect(value, defaultValue),
     "lerp" => HandleLerp(value, defaultValue),
     "vecx" => HandleVecX(value, defaultValue),
     "vecy" => HandleVecY(value, defaultValue),
     "vecz" => HandleVecZ(value, defaultValue),
     "atan" => Atan(value, defaultValue),
     "pow" => Parse.TryKvp(value, out var kvp, Separator) && Parse.TryFloat(kvp.Key, out var f1) && Parse.TryFloat(kvp.Value, out var f2) ? Mathf.Pow(f1, f2).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "log" => Loga(value, defaultValue),
     "exp" => Parse.TryFloat(value, out var f) ? Mathf.Exp(f).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "min" => HandleMin(value, defaultValue),
     "max" => HandleMax(value, defaultValue),
     "add" => HandleAdd(value, defaultValue),
     "sub" => HandleSub(value, defaultValue),
     "mul" => HandleMul(value, defaultValue),
     "div" => HandleDiv(value, defaultValue),
     "mod" => HandleMod(value, defaultValue),
     "iter" => HandleIter(value, defaultValue),
     "iter2" => HandleIter2(value, defaultValue),
     "addlong" => HandleAddLong(value, defaultValue),
     "sublong" => HandleSubLong(value, defaultValue),
     "mullong" => HandleMulLong(value, defaultValue),
     "divlong" => HandleDivLong(value, defaultValue),
     "modlong" => HandleModLong(value, defaultValue),
     "randf" => Parse.TryKvp(value, out var kvp, Separator) && Parse.TryFloat(kvp.Key, out var f1) && Parse.TryFloat(kvp.Value, out var f2) ? UnityEngine.Random.Range(f1, f2).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "randi" => Parse.TryKvp(value, out var kvp, Separator) && Parse.TryInt(kvp.Key, out var i1) && Parse.TryInt(kvp.Value, out var i2) ? UnityEngine.Random.Range(i1, i2).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "random" => HandleRandom(value, defaultValue),
     "randomfloat" => Parse.TryKvp(value, out var kvp, Separator) && Parse.TryFloat(kvp.Key, out var f1) && Parse.TryFloat(kvp.Value, out var f2) ? UnityEngine.Random.Range(f1, f2).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "randomint" => Parse.TryKvp(value, out var kvp, Separator) && Parse.TryInt(kvp.Key, out var i1) && Parse.TryInt(kvp.Value, out var i2) ? UnityEngine.Random.Range(i1, i2).ToString(CultureInfo.InvariantCulture) : defaultValue,
     "hashof" => ZdoHelper.Hash(value).ToString(),
     "textof" => Parse.TryInt(value, out var hash) ? ZdoHelper.ReverseHash(hash) : defaultValue,
     "len" => value.Length.ToString(CultureInfo.InvariantCulture),
     "lower" => value.ToLowerInvariant(),
     "upper" => value.ToUpperInvariant(),
     "trim" => value.Trim(),
     "left" => HandleLeft(value, defaultValue),
     "right" => HandleRight(value, defaultValue),
     "mid" => HandleMid(value, defaultValue),
     "proper" => HandleProper(value, defaultValue),
     "search" => HandleSearch(value, defaultValue),
     "calcf" => Calculator.EvaluateFloat(value)?.ToString(CultureInfo.InvariantCulture) ?? defaultValue,
     "calci" => Calculator.EvaluateInt(value)?.ToString(CultureInfo.InvariantCulture) ?? defaultValue,
     "calcfloat" => Calculator.EvaluateFloat(value)?.ToString(CultureInfo.InvariantCulture) ?? defaultValue,
     "calcint" => Calculator.EvaluateInt(value)?.ToString(CultureInfo.InvariantCulture) ?? defaultValue,
     "calclong" => Calculator.EvaluateLong(value)?.ToString(CultureInfo.InvariantCulture) ?? defaultValue,
     "par" => Parse.TryInt(value, out var i) ? GetArg(i, defaultValue) : defaultValue,
     "rest" => Parse.TryInt(value, out var i) ? GetRest(i, defaultValue) : defaultValue,
     "load" => GetStoredValue(value, defaultValue),
     "save" => SetValue(value),
     "save++" => IncrementStoredValue(value, 1),
     "save--" => IncrementStoredValue(value, -1),
     "clear" => RemoveValue(value),
     "rank" => HandleRank(value, defaultValue),
     "small" => HandleSmall(value, defaultValue),
     "large" => HandleLarge(value, defaultValue),
     "eq" => HandleEqual(value, defaultValue),
     "ne" => HandleNotEqual(value, defaultValue),
     "gt" => HandleGreater(value, defaultValue),
     "ge" => HandleGreaterOrEqual(value, defaultValue),
     "lt" => HandleLess(value, defaultValue),
     "le" => HandleLessOrEqual(value, defaultValue),
     "even" => HandleEven(value, defaultValue),
     "odd" => HandleOdd(value, defaultValue),
     "findupper" => HandleFindUpper(value, defaultValue),
     "findlower" => HandleFindLower(value, defaultValue),
     "time" => HandleTime(value),
     "realtime" => HandleRealtime(value),
     "key" => GetStoredValue(value, defaultValue),
     "globalkey" => ZoneSystem.instance.GetGlobalKey(value, out var globalKey) ? globalKey.ToString() : defaultValue,
     _ => null,
   };

  private string GetPokeCount(string value, string defaultValue)
  {
    if (value == "" || pokeTargets == null) return defaultValue;
    var counts = GetPokeCounts();
    if (counts.TryGetValue(value, out var exact)) return exact.ToString(CultureInfo.InvariantCulture);
    if (!value.Contains('*')) return defaultValue;
    var sum = counts.Where(kv => SimpleStringValue.PatternMatch(kv.Key, value)).Sum(kv => kv.Value);
    return sum.ToString(CultureInfo.InvariantCulture);
  }

  private Dictionary<string, int> GetPokeCounts() => pokeCounts ??= GetPokeCounts(pokeTargets!);

  private static Dictionary<string, int> GetPokeCounts(List<ZDOID> targets)
  {
    Dictionary<string, int> counts = [];
    foreach (var target in targets)
    {
      var zdo = ZDOMan.instance.GetZDO(target);
      var name = zdo == null ? null : ZNetScene.instance.GetPrefab(zdo.GetPrefab())?.name;
      if (name == null) continue;
      counts.TryGetValue(name, out var current);
      counts[name] = current + 1;
    }
    return counts;
  }

  internal static string? Rad2Deg(string value)
  {
    if (!Parse.TryAngleRadians(value, out var radians)) return null;
    return (radians * Mathf.Rad2Deg).ToString(CultureInfo.InvariantCulture);
  }

  internal static string? Deg2Rad(string value)
  {
    if (!Parse.TryAngleDegrees(value, out var degrees)) return null;
    return (degrees * Mathf.Deg2Rad).ToString(CultureInfo.InvariantCulture);
  }

  internal static string? Rad2Vec(string value)
  {
    if (!Parse.TryAngleRadians(value, out var radians)) return null;
    return Formatting.FormatPos(new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)));
  }

  internal static string? Deg2Vec(string value)
  {
    if (!Parse.TryAngleDegrees(value, out var degrees)) return null;
    var radians = degrees * Mathf.Deg2Rad;
    return Formatting.FormatPos(new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)));
  }

  internal static string? Vec2Deg(string value)
  {
    if (!Parse.TryKvp(value, out var kvp, Separator)) return null;
    if (!Parse.TryFloat(kvp.Key, out var x) || !Parse.TryFloat(kvp.Value, out var z)) return null;
    return (Mathf.Atan2(z, x) * Mathf.Rad2Deg).ToString(CultureInfo.InvariantCulture);
  }

  internal static string? Vec2Rad(string value)
  {
    if (!Parse.TryKvp(value, out var kvp, Separator)) return null;
    if (!Parse.TryFloat(kvp.Key, out var x) || !Parse.TryFloat(kvp.Value, out var z)) return null;
    return Mathf.Atan2(z, x).ToString(CultureInfo.InvariantCulture);
  }

  private string HandleMin(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length == 0) return defaultValue;
    return values.Min(v => Parse.Float(v, float.MaxValue)).ToString(CultureInfo.InvariantCulture);
  }
  private string HandleMax(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length == 0) return defaultValue;
    return values.Max(v => Parse.Float(v, float.MinValue)).ToString(CultureInfo.InvariantCulture);
  }

  private string HandleIter(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length < 4) return defaultValue;
    var operation = values[0];
    if (!Parse.TryInt(values[1], out var minI)) return defaultValue;
    if (!Parse.TryInt(values[2], out var maxI)) return defaultValue;
    var template = BuildIteratorTemplate(string.Join(Separator.ToString(), values.Skip(3)), defaultValue);
    return BuildIteratorReduceExpression(operation, template, minI, maxI, null, null, defaultValue);
  }

  private string HandleIter2(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length < 6) return defaultValue;
    var operation = values[0];
    if (!Parse.TryInt(values[1], out var minI)) return defaultValue;
    if (!Parse.TryInt(values[2], out var maxI)) return defaultValue;
    if (!Parse.TryInt(values[3], out var minJ)) return defaultValue;
    if (!Parse.TryInt(values[4], out var maxJ)) return defaultValue;
    var template = BuildIteratorTemplate(string.Join(Separator.ToString(), values.Skip(5)), defaultValue);
    return BuildIteratorReduceExpression(operation, template, minI, maxI, minJ, maxJ, defaultValue);
  }

  private static string BuildIteratorTemplate(string template, string defaultValue)
  {
    if (defaultValue == "") return template;
    if (template.Contains("=")) return template;
    return $"{template}={defaultValue}";
  }

  private string BuildIteratorReduceExpression(string operation, string template, int minI, int maxI, int? minJ, int? maxJ, string defaultValue)
  {
    if (operation == "" || template == "") return defaultValue;
    if (minI > maxI) return defaultValue;
    if (minJ.HasValue && maxJ.HasValue && minJ.Value > maxJ.Value) return defaultValue;

    var values = new List<string>();
    if (minJ.HasValue && maxJ.HasValue)
    {
      for (var j = minJ.Value; j <= maxJ.Value; ++j)
      {
        for (var i = minI; i <= maxI; ++i)
        {
          values.Add(RenderIteratorTemplate(template, i, j));
        }
      }
    }
    else
    {
      for (var i = minI; i <= maxI; ++i)
      {
        values.Add(RenderIteratorTemplate(template, i, null));
      }
    }

    if (values.Count == 0) return defaultValue;
    if (values.Count == 1) return values[0];
    return $"<{operation}_{string.Join(Separator.ToString(), values)}>";
  }

  private static string RenderIteratorTemplate(string template, int i, int? j)
  {
    var value = ReplaceIteratorToken(template, "i", i.ToString(CultureInfo.InvariantCulture));
    if (j.HasValue)
      value = ReplaceIteratorToken(value, "j", j.Value.ToString(CultureInfo.InvariantCulture));
    if (value.StartsWith("<", StringComparison.Ordinal) && value.EndsWith(">", StringComparison.Ordinal))
      return value;
    if (IsIteratorLiteral(value))
      return value;
    return $"<{value}>";
  }

  private static bool IsIteratorLiteral(string value)
  {
    return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
  }

  private static string ReplaceIteratorToken(string input, string token, string replacement)
  {
    if (input == "") return input;

    var tokenLength = token.Length;
    var result = new StringBuilder(input.Length);
    var i = 0;
    while (i < input.Length)
    {
      if (i <= input.Length - tokenLength
        && string.CompareOrdinal(input, i, token, 0, tokenLength) == 0
        && (i == 0 || !char.IsLetterOrDigit(input[i - 1]))
        && (i + tokenLength == input.Length || !char.IsLetterOrDigit(input[i + tokenLength])))
      {
        result.Append(replacement);
        i += tokenLength;
      }
      else
      {
        result.Append(input[i]);
        ++i;
      }
    }
    return result.ToString();
  }

  private string SetValue(string value)
  {
    var kvp = Parse.Kvp(value, Separator);
    if (kvp.Value == "") return "";
    SetStoredValue(kvp.Key, kvp.Value);
    return kvp.Value;
  }
  private string RemoveValue(string value)
  {
    SetStoredValue(value, "");
    return "";
  }
  private string GetRest(int index, string defaultValue = "")
  {
    if (index < 0 || index >= args.Length) return defaultValue;
    return string.Join(" ", args, index, args.Length - index);
  }

  private string Atan(string value, string defaultValue)
  {
    var kvp = Parse.Kvp(value, Separator);
    if (!Parse.TryFloat(kvp.Key, out var f1)) return defaultValue;
    if (kvp.Value == "") return Mathf.Atan(f1).ToString(CultureInfo.InvariantCulture);
    if (!Parse.TryFloat(kvp.Value, out var f2)) return defaultValue;
    return Mathf.Atan2(f1, f2).ToString(CultureInfo.InvariantCulture);
  }

  private string HandleAngle(string value, string defaultValue)
  {
    if (!TryGetTwoVectors(value, out var from, out var to)) return defaultValue;
    return Vector3.Angle(from, to).ToString(CultureInfo.InvariantCulture);
  }

  private string HandleDistance(string value, string defaultValue)
  {
    if (!TryGetTwoVectors(value, out var from, out var to)) return defaultValue;
    return Vector3.Distance(from, to).ToString(CultureInfo.InvariantCulture);
  }

  private string HandleDot(string value, string defaultValue)
  {
    if (!TryGetTwoVectors(value, out var from, out var to)) return defaultValue;
    return Vector3.Dot(from, to).ToString(CultureInfo.InvariantCulture);
  }

  private string HandleCross(string value, string defaultValue)
  {
    if (!TryGetTwoVectors(value, out var from, out var to)) return defaultValue;
    return Formatting.FormatPos(Vector3.Cross(from, to));
  }

  private string HandleNormalize(string value, string defaultValue)
  {
    if (!TryEvaluateVector3(value, out var vector)) return defaultValue;
    return Formatting.FormatPos(vector.normalized);
  }

  private string HandleMagnitude(string value, string defaultValue)
  {
    if (!TryEvaluateVector3(value, out var vector)) return defaultValue;
    return vector.magnitude.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleSqrMagnitude(string value, string defaultValue)
  {
    if (!TryEvaluateVector3(value, out var vector)) return defaultValue;
    return vector.sqrMagnitude.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleProject(string value, string defaultValue)
  {
    if (!TryGetTwoVectors(value, out var vector, out var onNormal)) return defaultValue;
    return Formatting.FormatPos(Vector3.Project(vector, onNormal));
  }

  private string HandleReflect(string value, string defaultValue)
  {
    if (!TryGetTwoVectors(value, out var inDirection, out var inNormal)) return defaultValue;
    return Formatting.FormatPos(Vector3.Reflect(inDirection, inNormal));
  }

  private string HandleLerp(string value, string defaultValue)
  {
    var parts = value.Split(Separator);
    if (parts.Length != 3) return defaultValue;
    if (!TryEvaluateVector3(parts[0], out var from)) return defaultValue;
    if (!TryEvaluateVector3(parts[1], out var to)) return defaultValue;
    var t = Calculator.EvaluateFloat(parts[2]);
    if (t == null) return defaultValue;
    return Formatting.FormatPos(Vector3.LerpUnclamped(from, to, t.Value));
  }

  private string HandleVecX(string value, string defaultValue)
  {
    if (!TryEvaluateVector3(value, out var vector)) return defaultValue;
    return vector.x.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleVecY(string value, string defaultValue)
  {
    if (!TryEvaluateVector3(value, out var vector)) return defaultValue;
    return vector.y.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleVecZ(string value, string defaultValue)
  {
    if (!TryEvaluateVector3(value, out var vector)) return defaultValue;
    return vector.z.ToString(CultureInfo.InvariantCulture);
  }

  private bool TryGetTwoVectors(string value, out Vector3 first, out Vector3 second)
  {
    var kvp = Parse.Kvp(value, Separator);
    if (kvp.Value == "")
    {
      first = Vector3.zero;
      second = Vector3.zero;
      return false;
    }

    if (!TryEvaluateVector3(kvp.Key, out first))
    {
      second = Vector3.zero;
      return false;
    }
    return TryEvaluateVector3(kvp.Value, out second);
  }

  private static bool TryEvaluateVector3(string value, out Vector3 vector)
  {
    vector = Vector3.zero;
    if (value == "") return false;

    var values = Parse.Split(value.Replace(" ", ","));
    if (values.Length == 0 || values.Length > 3) return false;

    var x = Calculator.EvaluateFloat(values[0]);
    if (x == null) return false;
    vector.x = x.Value;

    if (values.Length > 1)
    {
      var z = Calculator.EvaluateFloat(values[1]);
      if (z == null) return false;
      vector.z = z.Value;
    }

    if (values.Length > 2)
    {
      var y = Calculator.EvaluateFloat(values[2]);
      if (y == null) return false;
      vector.y = y.Value;
    }

    return true;
  }

  private static bool TryEvaluateVector3Strict(string value, out Vector3 vector)
  {
    vector = Vector3.zero;
    if (value == "") return false;
    var values = Parse.Split(value);
    if (values.Length < 2 || values.Length > 3) return false;
    return TryEvaluateVector3(value, out vector);
  }

  private static bool TryGetStrictVectorOperands(string[] values, out Vector3?[] vectors)
  {
    vectors = new Vector3?[values.Length];
    var hasVector = false;
    for (var i = 0; i < values.Length; ++i)
    {
      if (!TryEvaluateVector3Strict(values[i], out var vector)) continue;
      vectors[i] = vector;
      hasVector = true;
    }
    return hasVector;
  }

  private static bool TryGetVectorMathOperand(string value, Vector3? parsedVector, out Vector3 operand)
  {
    if (parsedVector.HasValue)
    {
      operand = parsedVector.Value;
      return true;
    }

    var scalar = Calculator.EvaluateFloat(value);
    if (scalar == null)
    {
      operand = Vector3.zero;
      return false;
    }
    operand = new Vector3(scalar.Value, scalar.Value, scalar.Value);
    return true;
  }

  private string Loga(string value, string defaultValue)
  {
    var kvp = Parse.Kvp(value, Separator);
    if (!Parse.TryFloat(kvp.Key, out var f1)) return defaultValue;
    if (kvp.Value == "") return Mathf.Log(f1).ToString(CultureInfo.InvariantCulture);
    if (!Parse.TryFloat(kvp.Value, out var f2)) return defaultValue;
    return Mathf.Log(f1, f2).ToString(CultureInfo.InvariantCulture);
  }

  private string HandleAdd(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length == 0) return defaultValue;

    if (TryGetStrictVectorOperands(values, out var strictVectors))
    {
      var vectorResult = Vector3.zero;
      for (var i = 0; i < values.Length; ++i)
      {
        if (!TryGetVectorMathOperand(values[i], strictVectors[i], out var operand)) return defaultValue;
        vectorResult += operand;
      }
      return Formatting.FormatPos(vectorResult);
    }

    float result = 0f;
    foreach (var val in values)
    {
      result += Parse.Float(val, 0f);
    }
    return result.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleSub(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length == 0) return defaultValue;

    if (TryGetStrictVectorOperands(values, out var strictVectors))
    {
      if (!TryGetVectorMathOperand(values[0], strictVectors[0], out var vectorResult)) return defaultValue;
      for (int i = 1; i < values.Length; i++)
      {
        if (!TryGetVectorMathOperand(values[i], strictVectors[i], out var operand)) return defaultValue;
        vectorResult -= operand;
      }
      return Formatting.FormatPos(vectorResult);
    }

    float result = Parse.Float(values[0], 0f);
    for (int i = 1; i < values.Length; i++)
    {
      result -= Parse.Float(values[i], 0f);
    }
    return result.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleMul(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length == 0) return defaultValue;

    if (TryGetStrictVectorOperands(values, out var strictVectors))
    {
      if (!TryGetVectorMathOperand(values[0], strictVectors[0], out var vectorResult)) return defaultValue;
      for (int i = 1; i < values.Length; i++)
      {
        if (strictVectors[i].HasValue)
        {
          var vector = strictVectors[i]!.Value;
          vectorResult = Vector3.Scale(vectorResult, vector);
          continue;
        }

        var scalar = Calculator.EvaluateFloat(values[i]);
        if (scalar == null) return defaultValue;
        vectorResult *= scalar.Value;
      }
      return Formatting.FormatPos(vectorResult);
    }

    float result = 1f;
    foreach (var val in values)
    {
      result *= Parse.Float(val, 1f);
    }
    return result.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleDiv(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length == 0) return defaultValue;

    if (TryGetStrictVectorOperands(values, out var strictVectors))
    {
      if (!TryGetVectorMathOperand(values[0], strictVectors[0], out var vectorResult)) return defaultValue;
      for (int i = 1; i < values.Length; i++)
      {
        if (strictVectors[i].HasValue)
        {
          var vector = strictVectors[i]!.Value;
          if (vector.x == 0f || vector.y == 0f || vector.z == 0f) return defaultValue;
          vectorResult = new Vector3(vectorResult.x / vector.x, vectorResult.y / vector.y, vectorResult.z / vector.z);
          continue;
        }

        var scalar = Calculator.EvaluateFloat(values[i]);
        if (scalar == null || scalar.Value == 0f) return defaultValue;
        vectorResult /= scalar.Value;
      }
      return Formatting.FormatPos(vectorResult);
    }

    float result = Parse.Float(values[0], 0f);
    for (int i = 1; i < values.Length; i++)
    {
      var divisor = Parse.Float(values[i], 1f);
      if (divisor == 0f) return defaultValue;
      result /= divisor;
    }
    return result.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleMod(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length == 0) return defaultValue;

    float result = Parse.Float(values[0], 0f);
    for (int i = 1; i < values.Length; i++)
    {
      var divisor = Parse.Float(values[i], 1f);
      if (divisor == 0f) return defaultValue;
      result %= divisor;
    }
    return result.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleAddLong(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length == 0) return defaultValue;

    long result = 0L;
    foreach (var val in values)
    {
      result += Parse.Long(val, 0L);
    }
    return result.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleSubLong(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length == 0) return defaultValue;

    long result = Parse.Long(values[0], 0L);
    for (int i = 1; i < values.Length; i++)
    {
      result -= Parse.Long(values[i], 0L);
    }
    return result.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleMulLong(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length == 0) return defaultValue;

    long result = 1L;
    foreach (var val in values)
    {
      result *= Parse.Long(val, 1L);
    }
    return result.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleDivLong(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length == 0) return defaultValue;

    long result = Parse.Long(values[0], 0L);
    for (int i = 1; i < values.Length; i++)
    {
      var divisor = Parse.Long(values[i], 1L);
      if (divisor == 0L) return defaultValue;
      result /= divisor;
    }
    return result.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleModLong(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length == 0) return defaultValue;

    long result = Parse.Long(values[0], 0L);
    for (int i = 1; i < values.Length; i++)
    {
      var divisor = Parse.Long(values[i], 1L);
      if (divisor == 0L) return defaultValue;
      result %= divisor;
    }
    return result.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleRandom(string value, string defaultValue)
  {
    if (!Parse.TryKvp(value, out var kvp, Separator)) return defaultValue;

    if (HasFractionalMarker(kvp.Key) || HasFractionalMarker(kvp.Value))
    {
      if (!Parse.TryFloat(kvp.Key, out var minFloat) || !Parse.TryFloat(kvp.Value, out var maxFloat)) return defaultValue;
      return UnityEngine.Random.Range(minFloat, maxFloat).ToString(CultureInfo.InvariantCulture);
    }

    if (!Parse.TryInt(kvp.Key, out var minInt) || !Parse.TryInt(kvp.Value, out var maxInt)) return defaultValue;
    return UnityEngine.Random.Range(minInt, maxInt).ToString(CultureInfo.InvariantCulture);
  }

  private static bool HasFractionalMarker(string value) => value.Contains('.');

  private string HandleLeft(string value, string defaultValue)
  {
    var kvp = Parse.Kvp(value, Separator);
    var text = kvp.Key;
    var numChars = Parse.Int(kvp.Value, 1);

    if (text.Length == 0) return defaultValue;
    if (numChars <= 0) return "";
    if (numChars >= text.Length) return text;

    return text.Substring(0, numChars);
  }

  private string HandleRight(string value, string defaultValue)
  {
    var kvp = Parse.Kvp(value, Separator);
    var text = kvp.Key;
    var numChars = Parse.Int(kvp.Value, 1);

    if (text.Length == 0) return defaultValue;
    if (numChars <= 0) return "";
    if (numChars >= text.Length) return text;

    return text.Substring(text.Length - numChars);
  }

  private string HandleMid(string value, string defaultValue)
  {
    var parts = value.Split(Separator);
    if (parts.Length < 3) return defaultValue;

    var text = parts[0];
    if (!Parse.TryInt(parts[1], out var startNum) || !Parse.TryInt(parts[2], out var numChars))
      return defaultValue;

    if (text.Length == 0 || startNum >= text.Length || numChars <= 0)
      return "";

    var endPos = Math.Min(startNum + numChars, text.Length);
    return text.Substring(startNum, endPos - startNum);
  }

  private string HandleProper(string value, string defaultValue)
  {
    if (string.IsNullOrEmpty(value)) return defaultValue;

    var words = value.Split(' ');
    for (int i = 0; i < words.Length; i++)
    {
      if (words[i].Length > 0)
      {
        words[i] = char.ToUpper(words[i][0]) + (words[i].Length > 1 ? words[i].Substring(1).ToLower() : "");
      }
    }
    return string.Join(" ", words);
  }

  private string HandleSearch(string value, string defaultValue)
  {
    var parts = value.Split(Separator);
    if (parts.Length < 2) return defaultValue;

    var findText = parts[0];
    var withinText = parts[1];
    var startNum = parts.Length >= 3 ? Parse.Int(parts[2], 0) : 0;

    if (startNum >= withinText.Length) return defaultValue;

    var index = withinText.IndexOf(findText, startNum, StringComparison.OrdinalIgnoreCase);
    return index >= 0 ? index.ToString() : defaultValue;
  }

  private string GetArg(int index, string defaultValue = "")
  {
    return args.Length <= index || args[index] == "" ? defaultValue : args[index];
  }

  private string HandleRank(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length < 2) return defaultValue;

    if (!Parse.TryFloat(values[0], out var numberToRank)) return defaultValue;

    var numbers = values.Skip(1).Select(v => Parse.Float(v, float.MaxValue)).ToList();

    // Count how many numbers are greater than the number to rank
    int rank = 0;
    foreach (var num in numbers)
    {
      if (num > numberToRank)
        rank++;
    }

    return rank.ToString(CultureInfo.InvariantCulture);
  }

  private string HandleSmall(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length < 2) return defaultValue;

    if (!Parse.TryInt(values[0], out var index)) return defaultValue;

    var numbers = values.Skip(1).Select(v => Parse.Float(v, float.MaxValue)).ToList();
    numbers.Sort();
    if (index < 1) return numbers[0].ToString(CultureInfo.InvariantCulture);
    if (index > numbers.Count) return numbers[numbers.Count - 1].ToString(CultureInfo.InvariantCulture);
    return numbers[index - 1].ToString(CultureInfo.InvariantCulture);
  }

  private string HandleLarge(string value, string defaultValue)
  {
    var values = value.Split(Separator);
    if (values.Length < 2) return defaultValue;

    if (!Parse.TryInt(values[0], out var index)) return defaultValue;

    var numbers = values.Skip(1).Select(v => Parse.Float(v, float.MinValue)).ToList();
    numbers.Sort();
    if (index < 1) return numbers[numbers.Count - 1].ToString(CultureInfo.InvariantCulture);
    if (index > numbers.Count) return numbers[0].ToString(CultureInfo.InvariantCulture);
    return numbers[numbers.Count - index].ToString(CultureInfo.InvariantCulture);
  }

  private string HandleEqual(string value, string defaultValue)
  {
    var kvp = Parse.Kvp(value, Separator);
    if (kvp.Value == "") return defaultValue;

    // Try numeric comparison first
    if (Parse.TryFloat(kvp.Key, out var f1) && Parse.TryFloat(kvp.Value, out var f2))
      return (Math.Abs(f1 - f2) < float.Epsilon) ? "true" : "false";

    // Fall back to string comparison
    return string.Equals(kvp.Key, kvp.Value, StringComparison.OrdinalIgnoreCase) ? "true" : "false";
  }

  private string HandleNotEqual(string value, string defaultValue)
  {
    var kvp = Parse.Kvp(value, Separator);
    if (kvp.Value == "") return defaultValue;

    // Try numeric comparison first
    if (Parse.TryFloat(kvp.Key, out var f1) && Parse.TryFloat(kvp.Value, out var f2))
      return (Math.Abs(f1 - f2) >= float.Epsilon) ? "true" : "false";

    // Fall back to string comparison
    return !string.Equals(kvp.Key, kvp.Value, StringComparison.OrdinalIgnoreCase) ? "true" : "false";
  }

  private string HandleGreater(string value, string defaultValue)
  {
    var kvp = Parse.Kvp(value, Separator);
    if (kvp.Value == "" || !Parse.TryFloat(kvp.Key, out var f1) || !Parse.TryFloat(kvp.Value, out var f2))
      return defaultValue;

    return (f1 > f2) ? "true" : "false";
  }

  private string HandleGreaterOrEqual(string value, string defaultValue)
  {
    var kvp = Parse.Kvp(value, Separator);
    if (kvp.Value == "" || !Parse.TryFloat(kvp.Key, out var f1) || !Parse.TryFloat(kvp.Value, out var f2))
      return defaultValue;

    return (f1 >= f2) ? "true" : "false";
  }

  private string HandleLess(string value, string defaultValue)
  {
    var kvp = Parse.Kvp(value, Separator);
    if (kvp.Value == "" || !Parse.TryFloat(kvp.Key, out var f1) || !Parse.TryFloat(kvp.Value, out var f2))
      return defaultValue;

    return (f1 < f2) ? "true" : "false";
  }

  private string HandleLessOrEqual(string value, string defaultValue)
  {
    var kvp = Parse.Kvp(value, Separator);
    if (kvp.Value == "" || !Parse.TryFloat(kvp.Key, out var f1) || !Parse.TryFloat(kvp.Value, out var f2))
      return defaultValue;

    return (f1 <= f2) ? "true" : "false";
  }

  private string HandleEven(string value, string defaultValue)
  {
    if (!Parse.TryInt(value, out var number))
      return defaultValue;

    return (number % 2 == 0) ? "true" : "false";
  }

  private string HandleOdd(string value, string defaultValue)
  {
    if (!Parse.TryInt(value, out var number))
      return defaultValue;

    return (number % 2 != 0) ? "true" : "false";
  }

  private string HandleFindUpper(string value, string defaultValue)
  {
    if (string.IsNullOrEmpty(value)) return defaultValue;
    return new string([.. value.Where(char.IsUpper)]);
  }

  private string HandleFindLower(string value, string defaultValue)
  {
    if (string.IsNullOrEmpty(value)) return defaultValue;
    return new string([.. value.Where(char.IsLower)]);
  }

  private string HandleTime(string value)
  {
    var format = value;
    var dayLength = EnvMan.instance.m_dayLengthSec;
    var hourLength = dayLength / 24.0;
    var minuteLength = hourLength / 60.0;
    var day = (int)(time / dayLength);
    var hours = time - (day * dayLength);
    var hour = (int)(hours / hourLength);
    var minute = (int)((hours - (hour * hourLength)) / minuteLength);
    var second = (int)((hours - (hour * hourLength) - (minute * minuteLength)) / (minuteLength / 60.0));

    // Create a DateTimeOffset representing the game time
    var dt = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero)
      .AddDays(day)
      .AddHours(hour)
      .AddMinutes(minute)
      .AddSeconds(second);

    return dt.ToString(format, CultureInfo.InvariantCulture);
  }
  private string HandleRealtime(string value)
  {
    var parts = value.Split(Separator);
    var format = parts[0];
    var timezoneOffset = parts.Length > 1 ? Parse.Float(parts[1], 0f) : (float)TimeZoneInfo.Local.BaseUtcOffset.TotalHours;
    var utcNow = DateTimeOffset.UtcNow;
    var offsetTime = utcNow.AddHours(timezoneOffset);
    return offsetTime.ToString(format, CultureInfo.InvariantCulture);
  }

  // Function value could be a value group, so that has to be resolved.
  private static string ResolveValue(string value)
  {
    if (!value.StartsWith("<", StringComparison.OrdinalIgnoreCase)) return value;
    if (!value.EndsWith(">", StringComparison.OrdinalIgnoreCase)) return value;
    var sub = value.Substring(1, value.Length - 2);
    if (TryGetValueFromGroup(sub, out var valueFromGroup))
      return valueFromGroup;
    return value;
  }

  // Condition values can reference value groups, so these are expanded to all values.
  private static string ResolveConditionValue(string value)
  {
    if (!value.StartsWith("<", StringComparison.OrdinalIgnoreCase)) return value;
    if (!value.EndsWith(">", StringComparison.OrdinalIgnoreCase)) return value;
    var sub = value.Substring(1, value.Length - 2);
    if (TryGetValuesFromGroup(sub, out var valuesFromGroup))
      return string.Join(",", valuesFromGroup);
    return value;
  }

  private static bool TryGetValueFromGroup(string group, out string value)
  {
    var hash = group.ToLowerInvariant().GetStableHashCode();
    if (!DataLoading.ValueGroups.ContainsKey(hash))
    {
      value = group;
      return false;
    }
    var roll = UnityEngine.Random.Range(0, DataLoading.ValueGroups[hash].Count);
    // Value from group could be another group, so yet another resolve is needed.
    value = ResolveValue(DataLoading.ValueGroups[hash][roll]);
    return true;
  }

  private static bool TryGetValuesFromGroup(string group, out List<string> values)
  {
    values = [];
    HashSet<int> handled = [];
    AddGroupValues(group, values, handled);
    return values.Count > 0;
  }

  private static void AddGroupValues(string group, List<string> values, HashSet<int> handled)
  {
    var hash = group.ToLowerInvariant().GetStableHashCode();
    if (!handled.Add(hash)) return;
    if (!DataLoading.ValueGroups.TryGetValue(hash, out var groupValues)) return;
    foreach (var groupValue in groupValues)
    {
      if (groupValue.StartsWith("<", StringComparison.OrdinalIgnoreCase) && groupValue.EndsWith(">", StringComparison.OrdinalIgnoreCase))
      {
        var subGroup = groupValue.Substring(1, groupValue.Length - 2);
        AddGroupValues(subGroup, values, handled);
      }
      else
      {
        values.Add(groupValue);
      }
    }
  }
}
