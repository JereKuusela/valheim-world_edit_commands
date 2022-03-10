using System.Collections.Generic;
using ServerDevcommands;

namespace WorldEditCommands {
  public class SpawnLocationAutoComplete {
    public List<string> NamedParameters = new List<string>() {
        "seed",
        "dungeonSeed",
        "pos",
        "rot",
        "refPos",
        "refRot"
      };
    public SpawnLocationAutoComplete() {
      NamedParameters.Sort();
      AutoComplete.Register("spawn_location", (int index) => {
        if (index == 0) return ParameterInfo.LocationIds;
        return NamedParameters;
      }, new Dictionary<string, System.Func<int, List<string>>>() {
        {
          "seed", (int index) => index == 0 ? ParameterInfo.Create("seed", "integer", "Sets the location appearance (if randomized).") : null
        },
        {
          "dungeonSeed", (int index) => index == 0 ? ParameterInfo.Create("dungeonSeed", "integer", "Sets the room layout for the next dungeon.") : null
        },
        {
          "pos", (int index) => ParameterInfo.XZY("pos", "Offset from the player / reference position", index)
        },
        {
          "refPos", (int index) => ParameterInfo.XZY("refPos", "Overrides the reference position (player's position)", index)
        },
        {
          "rot", (int index) => index == 0 ? ParameterInfo.Create("rot", "degrees", "Sets the location rotation. Randomized by default.") : null
        },
        {
          "refRot", (int index) => index == 0 ? ParameterInfo.Create("refRot", "degrees", "Overrides the reference rotation (player's rotation).") : null
        }
      });
    }
  }
}
