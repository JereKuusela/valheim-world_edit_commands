using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
namespace WorldEditCommands;
using NamedOptionsFetchers = Dictionary<string, Func<int, List<string>>>;
public class TweakAutoComplete
{
  public List<string> NamedParameters = [];
  public static List<string> WithFilters(List<string> parameters)
  {
    List<string> namedParameters = [
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
      "force"
    ];
    parameters.AddRange(namedParameters);
    return parameters.Distinct().OrderBy(s => s).ToList();
  }
  public static NamedOptionsFetchers WithFilters(NamedOptionsFetchers fetchers)
  {
    NamedOptionsFetchers baseFetchers = new() {
      {
        "type", (int index) => ParameterInfo.Components
      },
      {
        "connect", (int index) => ParameterInfo.Flag("Connect")
      },
      {
        "force", (int index) => ParameterInfo.Flag("Force", "Sets the component if missing.")
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
    };
    foreach (var kvp in fetchers) baseFetchers[kvp.Key] = kvp.Value;
    return baseFetchers;
  }

  public static List<string> Effect(string name, int index)
  {
    if (index == 0) return ParameterInfo.Ids;
    if (index == 1) return ParameterInfo.Create($"{name}=id,<color=yellow>flag</color>", "1 = random rotation");
    return ParameterInfo.Create($"For additional entries, add more <color>{name}=...</color> parameters.");
  }
  public static List<string> EffectFull(string name, int index)
  {
    if (index == 0) return ParameterInfo.Ids;
    if (index == 1) return ParameterInfo.Create($"{name}=id,<color=yellow>flag</color>,variant,childTransform", "Sum up: 1 = random rotation, 2 = inherit rotation, 4 = allow scaling, 8 = inherit scale, 16 = attach to the provided object.");
    if (index == 2) return ParameterInfo.Create($"{name}t=id,flag,<color=yellow>variant</color>,childTransform", "Variant number (very rarely needed).");
    if (index == 3) return ParameterInfo.Create($"{name}=id,flag,variant,<color=yellow>childTransform</color>", "Name of the transformation to attach.");
    return ParameterInfo.Create($"For additional entries, add more <color>{name}=...</color> parameters.");
  }
}
