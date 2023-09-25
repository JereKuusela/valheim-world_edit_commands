using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
namespace WorldEditCommands;
using NamedOptionsFetchers = Dictionary<string, Func<int, List<string>>>;
public class SharedObjectAutoComplete
{
  public static List<string> VisualAutoComplete(string name, int index)
  {
    if (index == 0) return ParameterInfo.ItemIds;
    if (index == 1) return ParameterInfo.Create($"${name}=id,<color=yellow>integer</color> | Item variant for items that have multiple variants.");
    return ParameterInfo.None;
  }

  public static List<string> WithSharedParameters(List<string> parameters)
  {
    List<string> namedParameters = [
      "baby",
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
      "model",
      "damage",
      "ammo",
      "ammoType",
      "field",
      "f"
    ];
    parameters.AddRange(namedParameters);
    return parameters.Distinct().OrderBy(s => s).ToList();
  }
  public static NamedOptionsFetchers WithSharedFetchers(NamedOptionsFetchers fetchers)
  {
    NamedOptionsFetchers baseFetchers = new() {
      {
        "tame",
        (int index) => index == 0 ? ParameterInfo.Create("tame=<color=yellow>true/false</color> or no value for default.", "Sets is the creature tamed.") : ParameterInfo.None
      },
      {
        "baby", (int index) => ParameterInfo.Flag("Baby")
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
        "durability",
        (int index) => index == 0 ? ParameterInfo.Create("durability", "number", "Sets current durability/health (+ maximum health for creatures).") : ParameterInfo.None
      },
      {
        "health",
        (int index) => index == 0 ? ParameterInfo.CreateWithMinMax("health", "number", "Sets current durability/health (+ maximum health for creatures).") : ParameterInfo.None
      },
      {
        "damage",
        (int index) => index == 0 ? ParameterInfo.CreateWithMinMax("damage", "number", "Sets the damage multiplier.") : ParameterInfo.None
      },
      {
        "ammo",
        (int index) => index == 0 ? ParameterInfo.CreateWithMinMax("ammo", "number", "Sets the amount of ammo.") : ParameterInfo.None
      },
      {
        "ammoType",
        (int index) => index == 0 ? ParameterInfo.ItemIds : ParameterInfo.None
      },
      {
        "stars",
        (int index) => index == 0 ? ParameterInfo.Create("stars", "integer", "Sets creature stars.") : ParameterInfo.None
      },
      {
        "level",
        (int index) => index == 0 ? ParameterInfo.Create("level", "integer", "Sets creature and item level.") : ParameterInfo.None
      },
      {
        "model",
        (int index) => index == 0 ? ParameterInfo.Create("model", "integer", "Sets the creature model (0 for male, 1 for female).") : ParameterInfo.None
      },
      {
        "scale",
        (int index) => ParameterInfo.Scale("scale", "Scaling for objects that support it", index)
      },
      {
        "field",
        (int index) => index == 0 ? DataAutoComplete.GetComponents() : index == 1 ? DataAutoComplete.GetFields() : DataAutoComplete.GetTypes(index - 2)
      },
      {
        "f",
        (int index) => index == 0 ? DataAutoComplete.GetComponents() : index == 1 ? DataAutoComplete.GetFields() : DataAutoComplete.GetTypes(index - 2)
      },
    };
    foreach (var kvp in fetchers) baseFetchers[kvp.Key] = kvp.Value;
    return baseFetchers;
  }
}
