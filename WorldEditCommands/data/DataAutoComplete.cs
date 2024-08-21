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
        "type", (int index) => ParameterInfo.Components
      },
      {
        "connect", (int index) => ParameterInfo.Flag("Connect")
      },
      {
        "id", (int index) => ParameterInfo.Ids
      },
      {
        "ignore", (int index) => ParameterInfo.Ids
      },
      {
        "center",
        (int index) => ParameterInfo.XZY("center", "Overrides the player position. For <color=yellow>rotate</color> sets also the rotation center point.", index)
      },
      {
        "from",
        (int index) => ParameterInfo.XZY("center", "Overrides the player position. For <color=yellow>rotate</color> sets also the rotation center point.", index)
      },
      {
        "rect",
        (int index) => {
          if (index == 0) return ParameterInfo.Create("rect=<color=yellow>size</color> or rect=<color=yellow>width</color>,depth", "Area of affected objects.");
          if (index == 1) return ParameterInfo.Create("rect=width,<color=yellow>depth</color>", "Area of affected objects.");
          return ParameterInfo.None;
        }
      },
      {
        "angle",
        (int index) => index == 0 ? ParameterInfo.Create("angle=<color=yellow>degrees</color>", "Direction of the rectangle when used with <color=yellow>rect</color>.") : ParameterInfo.None
      },
      {
        "circle",
        (int index) => index == 0 ? ParameterInfo.Create("circle=<color=yellow>number</color>", "Radius of affected objects.") : ParameterInfo.None
      },
      {
        "radius",
        (int index) => index == 0 ? ParameterInfo.Create("radius=<color=yellow>number</color>", "Radius of affected objects.") : ParameterInfo.None
      },
      {
        "chance",
        (int index) => index == 0 ? ParameterInfo.Create("chance=<color=yellow>number</color>", "Chance to affect the object (from 0.0 to 1.0).") : ParameterInfo.None
      },
      {
        "save", (int index) => ParameterInfo.Create("save=<color=yellow>name</color>", "Saves the object data to the file.")
      },
      {
        "keep", (int index) => ParameterInfo.Create("keep=<color=yellow>key,key,key,...</color>", "Removes all data except given keys.")
      },
      {
        "clear", (int index) => ParameterInfo.Flag("Removes all data.")
      },
      {
        "print", (int index) => ParameterInfo.Flag("Prints data.")
      },
      {
        "remove", (int index) => ParameterInfo.Create("remove=<color=yellow>key,key,key,...</color>", "Removes given keys.")
      },
      {
        "set", (int index) =>
          index == 0 ? dataTypes :
          index == 1 ? ParameterInfo.Create("set=type,<color=yellow>key</color>,value", "Name of the key.") :
          index == 2 ? ParameterInfo.Create("set=type,key,<color=yellow>value</color>", "Value of the key.") :
          ParameterInfo.None
      },
      {
        "load", (int index) => DataLoading.DataKeys
      },
      {
        "merge", (int index) => DataLoading.DataKeys
      },
      {
        "match", (int index) => index == 0 ? DataLoading.DataKeys : ParameterInfo.None
      },
      {
        "unmatch", (int index) => index == 0 ? DataLoading.DataKeys : ParameterInfo.None
      },
      {
        "par", (int index) => index == 0 ? GetDataParameters() : ParameterInfo.Create("par=key,<color=yellow>value</color>", "Value of the parameter.")
      },
      {
        "copy_raw", (int index) => {
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
    if (DataLoading.Data.TryGetValue(split[1].GetStableHashCode(), out var data))
      return [.. data.RequiredParameters];
    return ParameterInfo.Create("par=<color=yellow>key</color>,value", "Name of the parameter.");
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
      if (DataLoading.Data.TryGetValue(name.GetStableHashCode(), out var data))
        foreach (var oar in data.RequiredParameters)
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
