using System;
using System.Globalization;
using Service;
using UnityEngine;

namespace Data;

public class Calculator
{

  public static Vector3 EvaluateVector3(string expression)
  {
    var s = Parse.Split(expression);
    if (Parse.TryDistanceAngle(s, out var polar))
      return polar;

    var vector = Vector3.zero;
    vector.x = EvaluateFloat(s[0]) ?? 0f;
    if (s.Length > 1) vector.z = EvaluateFloat(s[1]) ?? 0f;
    if (s.Length > 2) vector.y = EvaluateFloat(s[2]) ?? 0f;
    return vector;
  }
  public static Quaternion EvaluateQuaternion(string expression)
  {
    var vector = Vector3.zero;
    var s = Parse.Split(expression);
    vector.y = EvaluateFloat(s[0]) ?? 0f;
    if (s.Length > 1) vector.x = EvaluateFloat(s[1]) ?? 0f;
    if (s.Length > 2) vector.z = EvaluateFloat(s[2]) ?? 0f;
    return Quaternion.Euler(vector);
  }
  public static int? EvaluateInt(string expression)
  {
    try
    {
      return (int?)EvaluateLong(expression);
    }
    catch
    {
      return null;
    }
  }
  public static float? EvaluateFloat(string expression)
  {
    try
    {
      return (float?)EvaluateDouble(expression);
    }
    catch
    {
      return null;
    }
  }
  private static double EvaluateDouble(string expression)
  {
    var parser = new DoubleParser(expression.Replace("**", "^"));
    return parser.Parse();
  }

  public static long? EvaluateLong(string expression)
  {
    try
    {
      return EvalLong(expression);
    }
    catch
    {
      return null;
    }
  }
  private static long EvalLong(string expression)
  {
    var parser = new LongParser(expression.Replace("**", "^"));
    return parser.Parse();
  }

  private sealed class DoubleParser(string expression)
  {
    private readonly string expression = expression;
    private int index = 0;

    public double Parse()
    {
      var value = ParseExpression();
      SkipWhiteSpace();
      if (index != expression.Length) throw new InvalidOperationException($"Failed to parse expression: {expression}");
      return value;
    }

    private double ParseExpression()
    {
      var value = ParseTerm();
      while (true)
      {
        SkipWhiteSpace();
        if (TryRead('+')) value += ParseTerm();
        else if (TryRead('-')) value -= ParseTerm();
        else return value;
      }
    }

    private double ParseTerm()
    {
      var value = ParseUnary();
      while (true)
      {
        SkipWhiteSpace();
        if (TryRead('*')) value *= ParseUnary();
        else if (TryRead('/')) value /= ParseUnary();
        else return value;
      }
    }

    private double ParseUnary()
    {
      SkipWhiteSpace();
      if (TryRead('+')) return ParseUnary();
      if (TryRead('-')) return -ParseUnary();
      return ParsePower();
    }

    private double ParsePower()
    {
      var value = ParsePrimary();
      SkipWhiteSpace();
      if (TryRead('^')) value = Math.Pow(value, ParseUnary());
      return value;
    }

    private double ParsePrimary()
    {
      SkipWhiteSpace();
      if (TryRead('('))
      {
        var value = ParseExpression();
        SkipWhiteSpace();
        if (!TryRead(')')) throw new InvalidOperationException($"Failed to parse expression: {expression}");
        return value;
      }
      return ParseNumber();
    }

    private double ParseNumber()
    {
      SkipWhiteSpace();
      var start = index;
      var hasDigits = false;
      while (index < expression.Length && char.IsDigit(expression[index]))
      {
        hasDigits = true;
        ++index;
      }
      if (index < expression.Length && expression[index] == '.')
      {
        ++index;
        while (index < expression.Length && char.IsDigit(expression[index]))
        {
          hasDigits = true;
          ++index;
        }
      }

      if (index < expression.Length && (expression[index] == 'e' || expression[index] == 'E'))
      {
        var exponentStart = index;
        ++index;
        if (index < expression.Length && (expression[index] == '+' || expression[index] == '-')) ++index;
        var exponentDigitsStart = index;
        while (index < expression.Length && char.IsDigit(expression[index])) ++index;
        if (exponentDigitsStart == index)
        {
          index = exponentStart;
        }
      }

      if (!hasDigits) throw new InvalidOperationException($"Failed to parse expression: {expression}");
      var value = expression.Substring(start, index - start);
      return double.Parse(value, NumberFormatInfo.InvariantInfo);
    }

    private bool TryRead(char c)
    {
      if (index >= expression.Length || expression[index] != c) return false;
      ++index;
      return true;
    }

    private void SkipWhiteSpace()
    {
      while (index < expression.Length && char.IsWhiteSpace(expression[index])) ++index;
    }
  }

  private sealed class LongParser(string expression)
  {
    private readonly string expression = expression;
    private int index = 0;

    public long Parse()
    {
      var value = ParseExpression();
      SkipWhiteSpace();
      if (index != expression.Length) throw new InvalidOperationException($"Failed to parse expression: {expression}");
      return value;
    }

    private long ParseExpression()
    {
      var value = ParseTerm();
      while (true)
      {
        SkipWhiteSpace();
        if (TryRead('+')) value += ParseTerm();
        else if (TryRead('-')) value -= ParseTerm();
        else return value;
      }
    }

    private long ParseTerm()
    {
      var value = ParseUnary();
      while (true)
      {
        SkipWhiteSpace();
        if (TryRead('*')) value *= ParseUnary();
        else if (TryRead('/')) value /= ParseUnary();
        else return value;
      }
    }

    private long ParseUnary()
    {
      SkipWhiteSpace();
      if (TryRead('+')) return ParseUnary();
      if (TryRead('-')) return -ParseUnary();
      return ParsePower();
    }

    private long ParsePower()
    {
      var value = ParsePrimary();
      SkipWhiteSpace();
      if (TryRead('^'))
      {
        var exponent = ParseUnary();
        if (exponent < 0) throw new InvalidOperationException($"Failed to parse expression: {expression}");
        value = Pow(value, exponent);
      }
      return value;
    }

    private long ParsePrimary()
    {
      SkipWhiteSpace();
      if (TryRead('('))
      {
        var value = ParseExpression();
        SkipWhiteSpace();
        if (!TryRead(')')) throw new InvalidOperationException($"Failed to parse expression: {expression}");
        return value;
      }
      return ParseNumber();
    }

    private long ParseNumber()
    {
      SkipWhiteSpace();
      var start = index;
      while (index < expression.Length && char.IsDigit(expression[index])) ++index;
      if (start == index) throw new InvalidOperationException($"Failed to parse expression: {expression}");
      var value = expression.Substring(start, index - start);
      return long.Parse(value, NumberFormatInfo.InvariantInfo);
    }

    private static long Pow(long value, long exponent)
    {
      checked
      {
        var result = 1L;
        while (exponent > 0)
        {
          if ((exponent & 1L) == 1L) result *= value;
          exponent >>= 1;
          if (exponent > 0) value *= value;
        }
        return result;
      }
    }

    private bool TryRead(char c)
    {
      if (index >= expression.Length || expression[index] != c) return false;
      ++index;
      return true;
    }

    private void SkipWhiteSpace()
    {
      while (index < expression.Length && char.IsWhiteSpace(expression[index])) ++index;
    }
  }

}