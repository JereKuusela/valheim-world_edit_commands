using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
namespace WorldEditCommands;
using NamedOptionsFetchers = Dictionary<string, Func<int, List<string>>>;
public class TweakAutoComplete {
  public List<string> NamedParameters = new();
  public static List<string> ObjectTypes = new() {
      "creature",
      "structure"
  };
  public static List<string> WithFilters(List<string> parameters) {
    List<string> namedParameters = new() {
      "id",
      "radius",
      "center",
      "from",
      "rect",
      "angle",
      "chance",
      "type",
      "connect",
    };
    parameters.AddRange(namedParameters);
    return parameters.Distinct().OrderBy(s => s).ToList();
  }
  public static NamedOptionsFetchers WithFilters(NamedOptionsFetchers fetchers) {
    NamedOptionsFetchers baseFetchers = new() {
      {
        "type", (int index) => index == 0 ? ObjectTypes : ParameterInfo.None
      },
      {
        "connect", (int index) => ParameterInfo.Flag("Connect")
      },
      {
        "id", (int index) => index == 0 ? ParameterInfo.Ids : ParameterInfo.None
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
      }
    };
    foreach (var kvp in fetchers) baseFetchers[kvp.Key] = kvp.Value;
    return baseFetchers;
  }

  public static List<string> Effect(int index) {
    if (index == 0) return ParameterInfo.Ids;
    if (index == 1) return ParameterInfo.Create("spawn=id,<color=yellow>flags</color>,variant,childTransform", "Sum up: 1 = inherit rotation, 2 = random rotation, 4 = allow scaling, 8 = inherit scale, 16 = attach to the provided object.");
    if (index == 2) return ParameterInfo.Create("spawn=id,flags,<color=yellow>variant</color>,childTransform", "Variant number (very rarely needed).");
    if (index == 3) return ParameterInfo.Create("spawn=id,flags,variant,<color=yellow>childTransform</color>", "Name of the transformation to attach.");
    return ParameterInfo.Create("For additional entries, add more <color>spawneffect=...</color> parameters.");
  }
}
