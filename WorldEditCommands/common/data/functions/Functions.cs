// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Common;
using UnityEngine;

namespace Data;

// Functions are technically just a key-value mapping.
// Proper class allows properly adding caching and other features.
// While also ensuring that all code is in one place.
public partial class Functions(string prefab, string[] args, Vector3 pos)
{
  protected const char Separator = '_';
  public static Func<string, string?> ExecuteCode = key => null!;
  public static Func<string, string, string?> ExecuteCodeWithValue = (key, value) => null!;

  // Hooks for features that only some hosts have, hosts without them don't implement these.
  partial void ResolveApiFunction(string key, ref string? result);
  partial void ResolveApiValueFunction(string key, string value, ref string? result);
  partial void GetStoredValue(string key, string defaultValue, ref string? result);
  partial void IncrementStoredValue(string key, long amount, ref string? result);
  partial void SetStoredValue(string key, string value);

  private string LoadStored(string key, string defaultValue)
  {
    string? result = null;
    GetStoredValue(key, defaultValue, ref result);
    return result ?? defaultValue;
  }
  private string IncrementStored(string key, long amount)
  {
    string? result = null;
    IncrementStoredValue(key, amount, ref result);
    return result ?? "";
  }

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
    string? value = null;
    ResolveApiFunction(key, ref value);
    if (value != null) return value;
    value = ExecuteCode(key);
    if (value != null) return value;
    value = GetGeneralFunction(key, defaultValue);
    if (value != null) return value;
    var keyArg = Parse.Kvp(key, Separator);
    if (keyArg.Value == "") return null;
    key = keyArg.Key;
    var arg = keyArg.Value;

    ResolveApiValueFunction(key, arg, ref value);
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
     "load" => LoadStored(value, defaultValue),
     "save" => SetValue(value),
     "save++" => IncrementStored(value, 1),
     "save--" => IncrementStored(value, -1),
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
     "key" => LoadStored(value, defaultValue),
     "globalkey" => ZoneSystem.instance.GetGlobalKey(value, out var globalKey) ? globalKey.ToString() : defaultValue,
     _ => null,
   };

  private string GetPokeCount(string value, string defaultValue)
  {
    if (value == "" || pokeTargets == null) return defaultValue;
    var counts = GetPokeCounts();
    if (counts.TryGetValue(value, out var exact)) return exact.ToString(CultureInfo.InvariantCulture);
    if (!Wildcard.IsPattern(value)) return defaultValue;
    var sum = counts.Where(kv => Wildcard.Match(kv.Key, value)).Sum(kv => kv.Value);
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

  private string GetArg(int index, string defaultValue = "")
  {
    return args.Length <= index || args[index] == "" ? defaultValue : args[index];
  }
}
