using System.Collections.Generic;
using ServerDevcommands;
namespace WorldEditCommands;
public class SpawnObjectAutoComplete : SharedObjectAutoComplete {
  public List<string> NamedParameters;
  public SpawnObjectAutoComplete() {
    NamedParameters = WithSharedParameters(new() {
      "hunt",
      "durability",
      "name",
      "crafter",
      "variant",
      "amount",
      "pos",
      "rot",
      "refPlayer",
      "from",
      "refRot",
      "to",
      "data",
      "crafterId"
    });
    AutoComplete.Register(SpawnObjectCommand.Name, (int index) => {
      if (index == 0) return ParameterInfo.Ids;
      return NamedParameters;
    }, WithSharedFetchers(new() {
      {
        "data",
        (int index) => index == 0 ? ParameterInfo.Create("data=<color=yellow>base64 data/false</color>", "Sets ZDO data.") : ParameterInfo.None
      },
      {
        "crafterId",
        (int index) => index == 0 ? ParameterInfo.Create("crafterId=<color=yellow>number</color>", "Sets the crafter player ID.") : ParameterInfo.None
      },
      {
        "hunt",
        (int index) => index == 0 ? ParameterInfo.Create("hunt=<color=yellow>true/false</color> or no value for true.", "Sets is the creature in aggressive mode.") : ParameterInfo.None
      },
      {
        "name",
        (int index) => index == 0 ? ParameterInfo.Create("name", "string", "Name for tameable creatures.") : ParameterInfo.None
      },
      {
        "crafter",
        (int index) => index == 0 ? ParameterInfo.Create("name", "string", "Crafter for items.") : ParameterInfo.None
      },
      {
        "variant",
        (int index) => index == 0 ? ParameterInfo.CreateWithMinMax("variant", "integer", "Variant for items.") : ParameterInfo.None
      },
      {
        "amount",
        (int index) => index == 0 ? ParameterInfo.CreateWithMinMax("amount", "integer", "Amount of spawned objects.") : ParameterInfo.None
      },
      {
        "pos",
        (int index) => ParameterInfo.FRU("pos", "Offset from the player position", index)
      },
      {
        "to",
        (int index) => ParameterInfo.XZY("to", "End position for multiple objects", index)
      },
      {
        "from",
        (int index) => ParameterInfo.XZY("from", "Overrides the player position", index)
      },
      {
        "refPlayer",
        (int index) => index == 0 ? ParameterInfo.PlayerNames : ParameterInfo.None
      },
      {
        "rot",
        (int index) => ParameterInfo.YawRollPitch("rot", "Rotation from the player rotation", index)
      },
      {
        "refRot",
        (int index) => ParameterInfo.YawRollPitch("refRot", "Overrides the player rotation", index)
      },
      {
        "circle",
        (int index) => index == 0 ? ParameterInfo.Create("circle=<color=yellow>number</color>", "Maximum spawn distance when spawning multiple objects. Default is 0.5 meters.") : ParameterInfo.None
      },
      {
        "radius",
        (int index) => index == 0 ? ParameterInfo.Create("radius=<color=yellow>number</color>", "Maximum spawn distance when spawning multiple objects. Default is 0.5 meters.") : ParameterInfo.None
      },
    }));
  }
}
