using System.Collections.Generic;
using ServerDevcommands;

namespace WorldEditCommands {
  public class SpawnLocationAutoComplete {
    public SpawnLocationAutoComplete() {
      var namedParameters = new List<string>() {
        "seed",
        "dungeonSeed",
        "pos",
        "rot",
        "refPos",
        "refRot"
      };
      namedParameters.Sort();
      AutoComplete.Register("spawn_location", (int index) => {
        if (index == 0) return ParameterInfo.LocationIds;
        return namedParameters;
      }, new Dictionary<string, System.Func<int, List<string>>>() {
        {
          "seed", (int index) => index == 0 ? ParameterInfo.Create("Location seed", "an integer") : null
        },
        {
          "dungeonSeed", (int index) => index == 0 ? ParameterInfo.Create("Dungeon seed", "an integer") : null
        },
        {
          "pos", (int index) => ParameterInfo.XZY("pos", "Offset from the player / reference position", index)
        },
        {
          "refPos", (int index) => ParameterInfo.XZY("refPos", "Overrides the reference position (player's position)", index)
        },
        {
          "rot", (int index) => index == 0 ? ParameterInfo.Create("Rotation", "a number") : null
        },
        {
          "refRot", (int index) => index == 0 ? ParameterInfo.Create("Reference rotation", "a number") : null
        }
      });
    }
  }
}
