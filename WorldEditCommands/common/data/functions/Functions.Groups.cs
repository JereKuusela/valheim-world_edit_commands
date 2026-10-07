// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System;
using System.Collections.Generic;

namespace Data;

public partial class Functions
{
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
    if (!ValueGroups.TryGetRandom(group, out value)) return false;
    // Value from group could be another group, so yet another resolve is needed.
    value = ResolveValue(value);
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
    if (!handled.Add(ValueGroups.Hash(group))) return;
    if (!ValueGroups.TryGet(group, out var groupValues)) return;
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
