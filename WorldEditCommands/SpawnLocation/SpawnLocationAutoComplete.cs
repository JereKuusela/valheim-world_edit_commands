using System.Collections.Generic;
using ServerDevcommands;
namespace WorldEditCommands;
public class SpawnLocationAutoComplete
{
  public List<string> NamedParameters = [
    "seed",
    "dungeonSeed",
    "pos",
    "rot",
    "from",
    "refRot"
  ];
  public SpawnLocationAutoComplete()
  {
    NamedParameters.Sort();
    AutoComplete.Register(SpawnLocationCommand.Name, (int index) =>
    {
      if (index == 0) return ParameterInfo.LocationIds;
      return NamedParameters;
    }, new() {
      {
        "seed",
        (int index) => index == 0 ? ParameterInfo.Create("seed", "integer", "Sets the location appearance (if randomized).") : ParameterInfo.None
      },
      {
        "dungeonSeed",
        (int index) => index == 0 ? ParameterInfo.Create("dungeonSeed", "integer", "Sets the room layout for the next dungeon.") : ParameterInfo.None
      },
      {
        "pos",
        (int index) => ParameterInfo.XZY("pos", "Offset from the player / reference position", index)
      },
      {
        "from",
        (int index) => ParameterInfo.XZY("from", "Overrides the reference position (player's position)", index)
      },
      {
        "rot",
        (int index) => index == 0 ? ParameterInfo.Create("rot", "degrees", "Sets the location rotation. Randomized by default.") : ParameterInfo.None
      },
      {
        "refRot",
        (int index) => index == 0 ? ParameterInfo.Create("refRot", "degrees", "Overrides the reference rotation (player's rotation).") : ParameterInfo.None
      }
    });
  }
}

