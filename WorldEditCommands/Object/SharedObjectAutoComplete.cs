using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;

namespace WorldEditCommands {
  using NamedOptionsFetchers = Dictionary<string, Func<int, List<string>>>;
  public class SharedObjectAutoComplete {
    public static List<string> VisualAutoComplete(int index) {
      if (index == 0) return ParameterInfo.ItemIds;
      if (index == 1) return ParameterInfo.Create("Visual", "number (0 or more)");
      return null;
    }
    public static List<string> MergeDefault(List<string> parameters) {
      var namedParameters = new List<string>() {
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
        "scale"
      };
      parameters.AddRange(namedParameters);
      return parameters.Distinct().OrderBy(s => s).ToList();
    }
    public static NamedOptionsFetchers MergeDefault(NamedOptionsFetchers fetchers) {
      var baseFetchers = new NamedOptionsFetchers() {
        {
          "tame", (int index) => ParameterInfo.Flag("Tame")
        },
        {
          "left_hand", VisualAutoComplete
        },
        {
          "right_hand", VisualAutoComplete
        },
        {
          "helmet", VisualAutoComplete
        },
        {
          "chest", VisualAutoComplete
        },
        {
          "shoulders", VisualAutoComplete
        },
        {
          "legs", VisualAutoComplete
        },
        {
          "utility", VisualAutoComplete
        },
        {
          "radius", (int index) => index == 0 ? ParameterInfo.Create("Radius", "a number range") : null
        },
        {
          "durability", (int index) => index == 0 ? ParameterInfo.Create("Durability", "a number range") : null
        },
        {
          "health", (int index) => index == 0 ? ParameterInfo.Create("Health", "a number range") : null
        },
        {
          "stars", (int index) => index == 0 ? ParameterInfo.Create("Stars", "an integer range") : null
        },
        {
          "level", (int index) => index == 0 ? ParameterInfo.Create("Level", "an integer range") : null
        },
        {
          "scale", ParameterInfo.Scale
        },
      };
      foreach (var kvp in fetchers) baseFetchers[kvp.Key] = kvp.Value;
      return baseFetchers;
    }
  }
}
