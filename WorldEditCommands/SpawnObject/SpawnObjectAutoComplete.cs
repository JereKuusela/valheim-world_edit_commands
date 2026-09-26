using System.Collections.Generic;
using Data;
using ServerDevcommands;
namespace WorldEditCommands;
public class SpawnObjectAutoComplete : SharedObjectAutoComplete
{
  public List<string> NamedParameters;
  public SpawnObjectAutoComplete()
  {
    NamedParameters = WithSharedParameters([
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
    ]);
    AutoComplete.Register(SpawnObjectCommand.Name, index => index == 0 ? ParameterInfo.ObjectIds : NamedParameters, WithSharedFetchers(new() {
      {
        "data",
        index => DataLoading.DataKeys
      },
      {
        "crafterId",
        index => index == 0 ? ParameterInfo.Create("crafterId=<color=yellow>number</color>", "Sets the crafter player ID.") : ParameterInfo.None
      },
      {
        "hunt",
        index => index == 0 ? ParameterInfo.Create("hunt=<color=yellow>true/false</color> or no value for true.", "Sets is the creature in aggressive mode.") : ParameterInfo.None
      },
      {
        "name",
        index => index == 0 ? ParameterInfo.Create("name", "string", "Name for tameable creatures.") : ParameterInfo.None
      },
      {
        "crafter",
        index => index == 0 ? ParameterInfo.Create("name", "string", "Crafter for items.") : ParameterInfo.None
      },
      {
        "variant",
        index => index == 0 ? ParameterInfo.CreateWithMinMax("variant", "integer", "Variant for items.") : ParameterInfo.None
      },
      {
        "amount",
        index => index == 0 ? ParameterInfo.CreateWithMinMax("amount", "integer", "Amount of spawned objects.") : ParameterInfo.None
      },
      {
        "pos",
        index => ParameterInfo.FRU("pos", "Offset from the player position", index)
      },
      {
        "to",
        index => ParameterInfo.XZY("to", "End position for multiple objects", index)
      },
      {
        "from",
        index => ParameterInfo.XZY("from", "Overrides the player position", index)
      },
      {
        "refPlayer",
        index => index == 0 ? ParameterInfo.PlayerNames : ParameterInfo.None
      },
      {
        "rot",
        index => ParameterInfo.YawRollPitch("rot", "Rotation from the player rotation", index)
      },
      {
        "refRot",
        index => ParameterInfo.YawRollPitch("refRot", "Overrides the player rotation", index)
      },
      {
        "circle",
        index => index == 0 ? ParameterInfo.Create("circle=<color=yellow>number</color>", "Maximum spawn distance when spawning multiple objects. Default is 0.5 meters.") : ParameterInfo.None
      },
      {
        "radius",
        index => index == 0 ? ParameterInfo.Create("radius=<color=yellow>number</color>", "Maximum spawn distance when spawning multiple objects. Default is 0.5 meters.") : ParameterInfo.None
      },
    }));
  }
}
