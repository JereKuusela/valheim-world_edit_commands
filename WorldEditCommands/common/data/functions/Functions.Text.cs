// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System;
using System.Linq;
using Common;

namespace Data;

public partial class Functions
{
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
}
