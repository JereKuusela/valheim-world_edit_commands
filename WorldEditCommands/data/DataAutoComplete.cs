using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using ServerDevcommands;
namespace WorldEditCommands;

using NamedOptionsFetchers = Dictionary<string, Func<int, List<string>>>;
public class DataAutoComplete
{
  public List<string> NamedParameters = [];
  public static List<string> GetOptions()
  {
    List<string> parameters = [
      "id",
      "ignore",
      "radius",
      "center",
      "from",
      "rect",
      "angle",
      "chance",
      "type",
      "connect",
      "match",
      "unmatch",
      "filter",
      "par",
      ..DataParameters.SupportedOperations.Keys
    ];
    return [.. parameters.Distinct().OrderBy(s => s)];
  }
  public static NamedOptionsFetchers GetNamedOptions()
  {
    List<string> dataTypes = [
      "array",
      "bool",
      "float",
      "hash",
      "int",
      "long",
      "quat",
      "string",
      "vec3",
    ];
    NamedOptionsFetchers baseFetchers = new() {
      {
        "type", index => ParameterInfo.Components
      },
      {
        "connect", index => ParameterInfo.Flag("Connect")
      },
      {
        "id", index => ParameterInfo.Ids
      },
      {
        "ignore", index => ParameterInfo.Ids
      },
      {
        "center",
        index => ParameterInfo.XZY("center", "Overrides the player position. For <color=yellow>rotate</color> sets also the rotation center point.", index)
      },
      {
        "from",
        index => ParameterInfo.XZY("center", "Overrides the player position. For <color=yellow>rotate</color> sets also the rotation center point.", index)
      },
      {
        "rect",
        index => {
          if (index == 0) return ParameterInfo.Create("rect=<color=yellow>size</color> or rect=<color=yellow>width</color>,depth", "Area of affected objects.");
          if (index == 1) return ParameterInfo.Create("rect=width,<color=yellow>depth</color>", "Area of affected objects.");
          return ParameterInfo.None;
        }
      },
      {
        "angle",
        index => index == 0 ? ParameterInfo.Create("angle=<color=yellow>degrees</color>", "Direction of the rectangle when used with <color=yellow>rect</color>.") : ParameterInfo.None
      },
      {
        "circle",
        index => index == 0 ? ParameterInfo.Create("circle=<color=yellow>number</color>", "Radius of affected objects.") : ParameterInfo.None
      },
      {
        "radius",
        index => index == 0 ? ParameterInfo.Create("radius=<color=yellow>number</color>", "Radius of affected objects.") : ParameterInfo.None
      },
      {
        "chance",
        index => index == 0 ? ParameterInfo.Create("chance=<color=yellow>number</color>", "Chance to affect the object (from 0.0 to 1.0).") : ParameterInfo.None
      },
      {
        "save", index => ParameterInfo.Create("save=<color=yellow>name</color>", "Saves the object data to the file.")
      },
      {
        "dump", index => ParameterInfo.Create("dump=<color=yellow>name</color>", "Saves the object data with default fields to the file.")
      },
      {
        "keep", index => GetKeys() ?? ParameterInfo.Create("keep=<color=yellow>key,key,key,...</color>", "Removes all data except given keys. Hover an object to list its keys.")
      },
      {
        "clear", index => ParameterInfo.Flag("Removes all data.")
      },
      {
        "print", index => ParameterInfo.Flag("Prints data.")
      },
      {
        "remove", index => GetKeys() ?? ParameterInfo.Create("remove=<color=yellow>key,key,key,...</color>", "Removes given keys. Hover an object to list its keys.")
      },
      {
        "set", index => index == 0 ? dataTypes :
          index == 1 ? ParameterInfo.Create("set=type,<color=yellow>key</color>,value", "Name of the key.") :
          index == 2 ? ParameterInfo.Create("set=type,key,<color=yellow>value</color>", "Value of the key.") :
          ParameterInfo.None
      },
      {
        "load", index => DataLoading.DataKeys
      },
      {
        "merge", index => DataLoading.DataKeys
      },
      {
        "match", index => index == 0 ? DataLoading.DataKeys : ParameterInfo.None
      },
      {
        "unmatch", index => index == 0 ? DataLoading.DataKeys : ParameterInfo.None
      },
      {
        "filter", Filter
      },
      {
        "par", index => index == 0 ? GetDataParameters() : ParameterInfo.Create("par=key,<color=yellow>value</color>", "Value of the parameter.")
      },
      {
        "copy_raw", index => {
          if (index == 0) return ParameterInfo.Create("copy_raw or copy_raw=<color=yellow>all</color> or copy_raw=<color=yellow>key1,key2,...</color>", "Prints and copies object data to clipboard. By default excludes some data for better results.");
          return ParameterInfo.None;
        }
      },
    };
    return baseFetchers;
  }

  public static List<string> GetDataParameters()
  {
    var command = GetInput();
    var ret = DataFromCommand(command);
    if (ret.Count == 0)
      return ParameterInfo.Create("par=<color=yellow>key</color>,value", "Name of the parameter.");
    return [.. ret];
  }
  public static List<string> GetRawDataParameters()
  {
    var command = GetInput();
    var split = command.Split(' ');
    if (split.Length < 2) return ParameterInfo.Create("par=<color=yellow>key</color>,value", "Name of the parameter."); ;
    if (DataLoading.Data.TryGetValue(ZDOKeys.Hash(split[1]), out var data))
      return [.. RequiredParameters.Get(data)];
    return ParameterInfo.Create("par=<color=yellow>key</color>,value", "Name of the parameter.");
  }

  private static readonly List<string> FilterTypes = [
    "bool",
    "float",
    "hash",
    "int",
    "long",
    "quat",
    "string",
    "vec3",
  ];
  public static List<string> Filter(int index) => index == 0 ? FilterTypes :
    index == 1 ? GetKeys() ?? ParameterInfo.Create("filter=type,<color=yellow>key</color>,value", "Name of the key.") :
    index == 2 ? ParameterInfo.Create("filter=type,key,<color=yellow>value</color>", "Value or range of the key.") :
    ParameterInfo.None;
  private static List<string>? GetKeys()
  {
    ObjectSelection selection = new();
    foreach (var arg in GetInput().Split(' '))
    {
      var split = arg.Split(['='], 2);
      var name = split[0].ToLower();
      if (split.Length == 2 && (name == "id" || name == "ignore" || name == "type"))
        selection.ParseSelectionArg(name, split[1]);
    }
    var keys = selection.IncludedIds.Select(id => id.GetStableHashCode()).Where(ZNetScene.instance.m_namedPrefabs.ContainsKey).SelectMany(FieldAutoComplete.GetZdoKeys);
    var view = GetHovered(selection);
    if (view != null)
      keys = keys.Concat(DataHelper.GetKeys(view.GetZDO()).Select(ZDOKeys.Convert));
    List<string> list = [.. keys.Where(key => !int.TryParse(key, out _) && !key.StartsWith("HasFields", StringComparison.Ordinal)).Distinct().OrderBy(key => key)];
    return list.Count == 0 ? null : list;
  }
  private static ZNetView? GetHovered(ObjectSelection selection)
  {
    try
    {
      return selection.GetHovered();
    }
    catch (InvalidOperationException)
    {
      return null;
    }
  }
  private static HashSet<string> DataFromCommand(string command)
  {
    var split = command.Split(' ');
    var dataNames = split
      .Where(s => s.StartsWith("data=", StringComparison.Ordinal) || s.StartsWith("merge=", StringComparison.Ordinal) || s.StartsWith("load=", StringComparison.Ordinal))
      .SelectMany(s => Parse.Split(s.Split('=')[1])).ToArray();
    HashSet<string> parameters = [];
    foreach (var name in dataNames)
    {
      if (DataLoading.Data.TryGetValue(ZDOKeys.Hash(name), out var data))
        foreach (var oar in RequiredParameters.Get(data))
          parameters.Add(oar);
    }
    return parameters;
  }
  private static string GetInput()
  {
    Aliasing.RestoreAlias(Console.m_instance.m_input);
    var text = Aliasing.Plain(Console.m_instance.m_input.text);
    Aliasing.RemoveAlias(Console.m_instance.m_input);
    return text;
  }
}
