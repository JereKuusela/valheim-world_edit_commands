using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
namespace WorldEditCommands;
using NamedOptionsFetchers = Dictionary<string, Func<int, List<string>>>;
public class SharedObjectAutoComplete {
  public static List<string> VisualAutoComplete(string name, int index) {
    if (index == 0) return ParameterInfo.ItemIds;
    if (index == 1) return ParameterInfo.Create($"${name}=id,<color=yellow>integer</color> | Item variant for items that have multiple variants.");
    return null;
  }
  public static List<string> WithSharedParameters(List<string> parameters) {
    List<string> namedParameters = new() {
      "durability",
      "tame",
      "left_hand",
      "right_hand",
      "helmet",
      "chest",
      "shoulders",
      "legs",
      "utility",
      "radius",
      "health",
      "stars",
      "level",
      "scale",
      "model"
    };
    parameters.AddRange(namedParameters);
    return parameters.Distinct().OrderBy(s => s).ToList();
  }
  public static NamedOptionsFetchers WithSharedFetchers(NamedOptionsFetchers fetchers) {
    NamedOptionsFetchers baseFetchers = new() {
      {
        "tame",
        (int index) => ParameterInfo.Flag("Tame")
      },
      {
        "left_hand",
        (int index) => VisualAutoComplete("left_hand", index)
      },
      {
        "right_hand",
        (int index) => VisualAutoComplete("right_hand", index)
      },
      {
        "helmet",
        (int index) => VisualAutoComplete("helmet", index)
      },
      {
        "chest",
        (int index) => VisualAutoComplete("chest", index)
      },
      {
        "shoulders",
        (int index) => VisualAutoComplete("shoulders", index)
      },
      {
        "legs",
        (int index) => VisualAutoComplete("legs", index)
      },
      {
        "utility",
        (int index) => VisualAutoComplete("utility", index)
      },
      {
        "radius",
        (int index) => index == 0 ? ParameterInfo.Create("radius", "number", "Maximum spawn distance when spawning multiple objects. Default is 0.5 meters.") : null
      },
      {
        "durability",
        (int index) => index == 0 ? ParameterInfo.Create("durability", "number", "Sets current durability/health (+ maximum health for creatures).") : null
      },
      {
        "health",
        (int index) => index == 0 ? ParameterInfo.CreateWithMinMax("health", "number", "Sets current durability/health (+ maximum health for creatures).") : null
      },
      {
        "stars",
        (int index) => index == 0 ? ParameterInfo.Create("stars", "integer", "Sets creature stars.") : null
      },
      {
        "level",
        (int index) => index == 0 ? ParameterInfo.Create("level", "integer", "Sets creature and item level.") : null
      },
      {
        "model",
        (int index) => index == 0 ? ParameterInfo.Create("model", "integer", "Sets the creature model (0 for male, 1 for female).") : null
      },
      {
        "scale",
        (int index) => ParameterInfo.Scale("scale", "Scaling for objects that support it", index)
      },
    };
    foreach (var kvp in fetchers) baseFetchers[kvp.Key] = kvp.Value;
    return baseFetchers;
  }
}
