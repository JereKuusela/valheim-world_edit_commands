// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System;
using System.Globalization;
using System.Linq;
using Common;

namespace Data;

public partial class Functions
{
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
}
