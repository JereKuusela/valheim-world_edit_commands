// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System.Globalization;
using Common;
using UnityEngine;

namespace Data;

public partial class Functions
{
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
}
