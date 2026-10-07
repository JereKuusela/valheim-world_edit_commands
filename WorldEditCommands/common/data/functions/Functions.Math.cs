// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Common;
using UnityEngine;

namespace Data;

public partial class Functions
{
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

  private string Atan(string value, string defaultValue)
  {
    var kvp = Parse.Kvp(value, Separator);
    if (!Parse.TryFloat(kvp.Key, out var f1)) return defaultValue;
    if (kvp.Value == "") return Mathf.Atan(f1).ToString(CultureInfo.InvariantCulture);
    if (!Parse.TryFloat(kvp.Value, out var f2)) return defaultValue;
    return Mathf.Atan2(f1, f2).ToString(CultureInfo.InvariantCulture);
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
}
