using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
namespace Service;

public class Range<T>
{
  public T Min;
  public T Max;
  public Range(T value)
  {
    Min = value;
    Max = value;
  }
  public Range(T min, T max)
  {
    Min = min;
    Max = max;
  }
  public bool Uniform = true;
}

///<summary>Contains functions for parsing arguments, etc.</summary>
public static class Parse
{
  public static List<string> ToList(string str, bool removeEmpty = true) => [.. Split(str, removeEmpty)];
  public static string[] ToArr(string str, bool removeEmpty = true) => [.. Split(str, removeEmpty)];
  public static Vector2i Vector2Int(string arg)
  {
    string[] array = SplitWithEmpty(arg);
    return new Vector2i(Int(array[0]), (array.Length > 1) ? Int(array[1]) : 0);
  }
  public static int Int(string arg, int defaultValue = 0)
  {
    if (!TryInt(arg, out var result))
      return defaultValue;
    return result;
  }
  public static uint UInt(string arg, uint defaultValue = 0)
  {
    if (!TryUInt(arg, out var result))
      return defaultValue;
    return result;
  }

  public static long Long(string arg, long defaultValue = 0)
  {
    if (!long.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }
  public static bool TryLong(string arg, out long result)
  {
    return long.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
  }
  public static int Int(string[] args, int index, int defaultValue = 0)
  {
    if (args.Length <= index) return defaultValue;
    return Int(args[index], defaultValue);
  }
  public static bool TryUInt(string arg, out uint result)
  {
    return uint.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
  }
  public static bool TryInt(string arg, out int result)
  {
    return int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
  }
  public static float Float(string arg, float defaultValue = 0f)
  {
    if (!TryFloat(arg, out var result))
      return defaultValue;
    return result;
  }
  public static float Float(string[] args, int index, float defaultValue = 0f)
  {
    if (args.Length <= index) return defaultValue;
    return Float(args[index], defaultValue);
  }
  public static bool TryFloat(string arg, out float result)
  {
    return float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
  }
  public static bool TryDistanceAngle(string arg, out Vector3 vector)
  {
    return TryDistanceAngle(Split(arg, true, ','), out vector);
  }
  public static bool TryDistanceAngle(string[] values, out Vector3 vector)
  {
    vector = Vector3.zero;
    if (values.Length < 2 || values.Length > 3) return false;
    if (values[1] == null || !HasAngleSuffix(values[1].Trim())) return false;
    if (!TryDistanceAngle(values[0], values[1], out vector)) return false;
    if (values.Length > 2 && !TryFloat(values[2], out vector.y)) return false;
    return true;
  }
  private static bool TryDistanceAngle(string distanceValue, string angleValue, out Vector3 vector)
  {
    vector = Vector3.zero;
    if (!TryFloat(distanceValue, out var distance)) return false;
    if (!TryAngleRadians(angleValue, out var radians)) return false;
    vector = new Vector3(Mathf.Cos(radians) * distance, 0f, Mathf.Sin(radians) * distance);
    return true;
  }
  private static bool HasAngleSuffix(string arg)
  {
    return arg.EndsWith("deg", StringComparison.OrdinalIgnoreCase) || arg.EndsWith("rad", StringComparison.OrdinalIgnoreCase);
  }
  public static bool TryAngleRadians(string arg, out float radians)
  {
    radians = 0f;
    if (arg == null) return false;

    var value = arg.Trim();
    if (value.EndsWith("deg", StringComparison.OrdinalIgnoreCase))
    {
      var degreesValue = value.Substring(0, value.Length - 3).Trim();
      if (!TryFloat(degreesValue, out var degrees)) return false;
      radians = degrees * Mathf.Deg2Rad;
      return true;
    }

    if (value.EndsWith("rad", StringComparison.OrdinalIgnoreCase))
    {
      var radiansValue = value.Substring(0, value.Length - 3).Trim();
      if (!TryFloat(radiansValue, out radians)) return false;
      return true;
    }

    return TryFloat(value, out radians);
  }
  public static bool TryAngleDegrees(string arg, out float degrees)
  {
    degrees = 0f;
    if (arg == null) return false;

    var value = arg.Trim();
    if (value.EndsWith("deg", StringComparison.OrdinalIgnoreCase))
    {
      var degreesValue = value.Substring(0, value.Length - 3).Trim();
      return TryFloat(degreesValue, out degrees);
    }

    if (value.EndsWith("rad", StringComparison.OrdinalIgnoreCase))
    {
      var radiansValue = value.Substring(0, value.Length - 3).Trim();
      if (!TryFloat(radiansValue, out var radians)) return false;
      degrees = radians * Mathf.Rad2Deg;
      return true;
    }

    return TryFloat(value, out degrees);
  }
  public static bool TryBoolean(string arg, out bool result)
  {
    result = false;
    if (arg.ToLowerInvariant() == "true")
    {
      result = true;
      return true;
    }
    if (arg.ToLowerInvariant() == "false")
    {
      return true;
    }
    return false;
  }

  public static Quaternion AngleYXZ(string arg) => AngleYXZ(Split(arg), 0, Vector3.zero);
  public static Quaternion AngleYXZ(string[] args, int index) => AngleYXZ(args, index, Vector3.zero);
  public static Quaternion AngleYXZ(string[] args, int index, Vector3 defaultValue)
  {
    var vector = Vector3.zero;
    vector.y = Float(args, index, defaultValue.y);
    vector.x = Float(args, index + 1, defaultValue.x);
    vector.z = Float(args, index + 2, defaultValue.z);
    return Quaternion.Euler(vector);
  }

  public static string[] Split(string arg, bool removeEmpty = true, char split = ',') =>
    removeEmpty ?
    [.. arg.Split(split).Select(s => s.Trim()).Where(s => s != "")]
    : [.. arg.Split(split).Select(s => s.Trim())];
  public static KeyValuePair<string, string> Kvp(string str, char separator = ',')
  {
    var index = str.IndexOf(separator);
    if (index < 0) return new(str, "");
    return new(str.Substring(0, index), str.Substring(index + 1).Trim());
  }
  public static bool TryKvp(string str, out KeyValuePair<string, string> kvp, char separator = ',')
  {
    kvp = Kvp(str, separator);
    return kvp.Value != "";
  }
  public static string[] SplitWithEmpty(string arg, char split = ',') => [.. arg.Split(split).Select(s => s.Trim())];
  public static string[] SplitWithEscape(string arg, char separator = ',')
  {
    var parts = new List<string>();
    var split = arg.Split(separator);
    for (var i = 0; i < split.Length; i++)
    {
      var part = split[i].TrimStart();
      // Escape should only work if at start/end of the string.
      if (part.StartsWith("\""))
      {
        split[i] = part.Substring(1);
        var j = i;
        for (; j < split.Length; j++)
        {
          part = split[j].TrimEnd();
          if (part.EndsWith("\""))
          {
            split[j] = part.Substring(0, part.Length - 1);
            break;
          }
        }
        parts.Add(string.Join(separator.ToString(), split.Skip(i).Take(j - i + 1)));
        i = j;
        continue;
      }
      parts.Add(split[i].Trim());
    }
    return [.. parts];
  }
  public static string Name(string arg) => arg.Split(':')[0];
  public static Vector3 VectorXZY(string arg) => VectorXZY(arg, Vector3.zero);
  public static Vector3 VectorXZY(string arg, Vector3 defaultValue)
  {
    var split = Split(arg);
    return TryDistanceAngle(split, out var polar) ? polar : VectorXZY(split, 0, defaultValue);
  }

  ///<summary>Parses YXZ vector starting at given index. Zero is used for missing values.</summary>
  public static Vector3 VectorXZY(string[] args, int index) => VectorXZY(args, index, Vector3.zero);
  ///<summary>Parses YXZ vector starting at given index. Default values is used for missing values.</summary>
  public static Vector3 VectorXZY(string[] args, int index, Vector3 defaultValue)
  {
    var vector = Vector3.zero;
    vector.x = Float(args, index, defaultValue.x);
    vector.z = Float(args, index + 1, defaultValue.z);
    vector.y = Float(args, index + 2, defaultValue.y);
    return vector;
  }
  public static Vector2 VectorXY(string arg)
  {
    var vector = Vector2.zero;
    var args = Split(arg);
    vector.x = Float(args, 0);
    vector.y = Float(args, 1);
    return vector;
  }
  public static Vector3 Scale(string args) => Scale(Split(args), 0);
  ///<summary>Parses scale starting at zero index. Includes a sanity check and giving a single value for all axis.</summary>
  public static Vector3 Scale(string[] args) => Scale(args, 0);
  ///<summary>Parses scale starting at given index. Includes a sanity check and giving a single value for all axis.</summary>
  public static Vector3 Scale(string[] args, int index) => SanityCheck(VectorXZY(args, index));
  private static Vector3 SanityCheck(Vector3 scale)
  {
    // Sanity check and also adds support for setting all values with a single number.
    if (scale.x == 0) scale.x = 1;
    if (scale.y == 0) scale.y = scale.x;
    if (scale.z == 0) scale.z = scale.x;
    return scale;
  }

  public static Range<string> StringRange(string arg)
  {
    var range = Split(arg, true, ';');
    if (range.Length > 1) return new(range[0], range[1]);
    else return new(range[0], range[0]);

  }
  public static Range<int> IntRange(string arg)
  {
    var range = StringRange(arg);
    return new(Int(range.Min), Int(range.Max));
  }
  public static Range<float> FloatRange(string arg)
  {
    var range = StringRange(arg);
    return new(Float(range.Min), Float(range.Max));
  }
  public static Range<long> LongRange(string arg)
  {
    var range = StringRange(arg);
    return new(Long(range.Min), Long(range.Max));
  }
  public static int? IntNull(string arg)
  {
    if (int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return result;
    return null;
  }
  public static int? IntNull(string[] args, int index)
  {
    if (args.Length <= index) return null;
    return IntNull(args[index]);
  }
  public static float? FloatNull(string arg)
  {
    if (float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
      return result;
    return null;
  }
  public static float? FloatNull(string[] args, int index)
  {
    if (args.Length <= index) return null;
    return FloatNull(args[index]);
  }
  public static long? LongNull(string[] args, int index)
  {
    if (args.Length <= index) return null;
    return LongNull(args[index]);
  }
  public static long? LongNull(string arg)
  {
    if (long.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return result;
    return null;
  }
  public static Vector3? VectorXZYNull(string? arg) => arg == null ? null : VectorXZYNull(Split(arg));
  public static Vector3? VectorXZYNull(string[] args)
  {
    if (TryDistanceAngle(args, out var polar)) return polar;
    var x = FloatNull(args, 0);
    var y = FloatNull(args, 2);
    var z = FloatNull(args, 1);
    if (x == null && y == null && z == null) return null;
    return new(x ?? 0f, y ?? 0f, z ?? 0f);
  }
  public static Quaternion? AngleYXZNull(string? arg) => arg == null ? null : AngleYXZNull(Split(arg));
  public static Quaternion? AngleYXZNull(string[] values)
  {
    if (values.Length >= 4)
    {
      var x = FloatNull(values, 0);
      var y = FloatNull(values, 1);
      var z = FloatNull(values, 2);
      var w = FloatNull(values, 3);
      if (x == null && y == null && z == null && w == null) return null;
      return new Quaternion(x ?? 0f, y ?? 0f, z ?? 0f, w ?? 0f);
    }
    else
    {
      var y = FloatNull(values, 0);
      var x = FloatNull(values, 1);
      var z = FloatNull(values, 2);
      if (y == null && x == null && z == null) return null;
      return Quaternion.Euler(new(x ?? 0f, y ?? 0f, z ?? 0f));
    }
  }
  public static string String(string[] args, int index) => args.Length > index ? args[index] : "";
  public static int Hash(string[] args, int index) => args.Length > index ? args[index].GetStableHashCode() : 0;
  public static bool Boolean(string[] args, int index) => args.Length > index && Boolean(args[index]);
  public static bool Boolean(string arg) => arg.ToLowerInvariant() == "true";
  public static bool BooleanTrue(string arg) => arg.ToLowerInvariant() == "false";
  public static ZDOID ZdoId(string arg)
  {
    if (string.IsNullOrWhiteSpace(arg)) return ZDOID.None;
    var normalized = arg.Trim().Trim('(', ')', '[', ']', '{', '}');
    var split = normalized.Split([':', ','], StringSplitOptions.RemoveEmptyEntries)
      .Select(value => value.Trim())
      .ToArray();
    if (split.Length < 2)
    {
      split = normalized.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
    }
    if (split.Length < 2 || !long.TryParse(split[0], out var owner) || !uint.TryParse(split[1], out var id))
      return ZDOID.None;
    return new ZDOID(owner, id);
  }
  public static HitData Hit(ZDO? zdo, string arg)
  {
    HitData hit = new()
    {
      m_point = zdo?.m_position ?? Vector3.zero,
    };
    var split = Split(arg, true, ' ');
    foreach (var s in split)
    {
      var kvp = Kvp(s, '=');
      var key = kvp.Key;
      var value = kvp.Value;
      if (key == "damage") hit.m_damage.m_damage = Float(value);
      if (key == "blunt") hit.m_damage.m_blunt = Float(value);
      if (key == "slash") hit.m_damage.m_slash = Float(value);
      if (key == "pierce") hit.m_damage.m_pierce = Float(value);
      if (key == "chop") hit.m_damage.m_chop = Float(value);
      if (key == "pickaxe") hit.m_damage.m_pickaxe = Float(value);
      if (key == "fire") hit.m_damage.m_fire = Float(value);
      if (key == "frost") hit.m_damage.m_frost = Float(value);
      if (key == "lightning") hit.m_damage.m_lightning = Float(value);
      if (key == "poison") hit.m_damage.m_poison = Float(value);
      if (key == "spirit") hit.m_damage.m_spirit = Float(value);
      if (key == "tier") hit.m_toolTier = (short)Int(value);
      if (key == "force") hit.m_pushForce = Float(value);
      if (key == "backstab") hit.m_backstabBonus = Float(value);
      if (key == "stagger") hit.m_staggerMultiplier = Float(value);
      if (key == "dodge") hit.m_dodgeable = Boolean(value);
      if (key == "block") hit.m_blockable = Boolean(value);
      if (key == "dir") hit.m_dir = VectorXZY(value);
      if (key == "ranged") hit.m_ranged = Boolean(value);
      if (key == "pvp") hit.m_ignorePVP = Boolean(value);
      if (key == "pos") hit.m_point = VectorXZY(value);
      if (key == "status") hit.m_statusEffectHash = value.GetStableHashCode();
      if (key == "attacker") hit.m_attacker = ZdoId(value);
      if (key == "skill") hit.m_skillLevel = Float(value);
      if (key == "level") hit.m_itemLevel = (short)Int(value);
      if (key == "world") hit.m_itemWorldLevel = (byte)Int(value);
      if (key == "type") hit.m_hitType = Enum.TryParse(value, true, out HitData.HitType type) ? type : HitData.HitType.Undefined;
      if (key == "spot") hit.m_weakSpot = (short)Int(value);
    }
    return hit;
  }
  public static int EnumMessage(string arg)
  {
    return Enum.TryParse(arg, true, out MessageHud.MessageType state) ? (int)state : Int(arg, 2);
  }
  public static int EnumReason(string arg)
  {
    return Enum.TryParse(arg, true, out BaseAI.AggravatedReason state) ? (int)state : Int(arg, 0);
  }
  public static int EnumTrap(string arg)
  {
    return Enum.TryParse(arg, true, out Trap.TrapState state) ? (int)state : Int(arg, 0);
  }
  public static int EnumDamageText(string arg)
  {
    return Enum.TryParse(arg, true, out DamageText.TextType state) ? (int)state : Int(arg, 0);
  }
  public static int EnumTerrainPaint(string arg)
  {
    return Enum.TryParse(arg, true, out TerrainModifier.PaintType state) ? (int)state : Int(arg, 0);
  }
  private static Dictionary<string, Color> Paints = new() {
    {"grass", UnityEngine.Color.black},
    {"patches", new(0f, 0.75f, 0f)},
    {"grass_dark", new(0.6f, 0.5f, 0f)},
    {"dirt", UnityEngine.Color.red},
    {"cultivated", UnityEngine.Color.green},
    {"paved", UnityEngine.Color.blue},
    {"paved_moss", new(0f, 0f, 0.5f)},
    {"paved_dirt", new(1f, 0f, 0.5f)},
    {"paved_dark", new(0f, 1f, 0.5f)},
  };
  public static Color? Color(string arg, float defaultAlpha)
  {
    var lower = arg.ToLowerInvariant();
    if (Paints.TryGetValue(lower, out var color)) return color;
    var split = Split(arg);
    if (split.Length < 3) return null;
    return new(Float(split[0]), Float(split[1]), Float(split[2]), split.Length > 3 ? Float(split[3]) : defaultAlpha);
  }
}
