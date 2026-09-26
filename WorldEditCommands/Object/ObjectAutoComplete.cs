using System.Collections.Generic;
using Data;
using ServerDevcommands;
namespace WorldEditCommands;
public class ObjectAutoComplete : SharedObjectAutoComplete
{
  public List<string> NamedParameters;
  public ObjectAutoComplete()
  {
    NamedParameters = WithSharedParameters([
      "wild",
      "info",
      "sleep",
      "id",
      "ignore",
      "move",
      "rotate",
      "remove",
      "origin",
      "visual",
      "fuel",
      "prefab",
      "center",
      "respawn",
      "from",
      "rect",
      "angle",
      "creator",
      "chance",
      "type",
      "connect",
      "status",
      "components",
      "match",
      "unmatch",
      "copy"
    ]);
    AutoComplete.Register(ObjectCommand.Name, index => NamedParameters, WithSharedFetchers(new() {
      {
        "status", index => {
          if (index == 0) return ParameterInfo.StatusEffects;
          if (index == 1) return ParameterInfo.Create("status=name,<color=yellow>duration</color>,intensity", "Duration in seconds.");
          if (index == 2) return ParameterInfo.Create("status=name,duration,<color=yellow>intensity</color>", "Strength of the effect.");
          return ParameterInfo.None;
        }
      },
      {
        "type", index => ParameterInfo.Components
      },
      {
        "connect", index => ParameterInfo.Flag("Connect")
      },
      {
        "mirror", index => ParameterInfo.Flag("Mirror")
      },
      {
        "components", index => ParameterInfo.Flag("Components")
      },
      {
        "respawn", index => ParameterInfo.Flag("Respawn")
      },
      {
        "wild", index => ParameterInfo.Flag("Wild")
      },
      {
        "remove", index => ParameterInfo.Flag("Remove")
      },
      {
        "sleep", index => ParameterInfo.Flag("Sleep")
      },
      {
        "info", index => ParameterInfo.Flag("info")
      },
      {
        "id", index => ParameterInfo.Ids
      },
      {
        "ignore", index => ParameterInfo.Ids
      },
      {
        "prefab", index => index == 0 ? ParameterInfo.ObjectIds : ParameterInfo.None
      },
      {
        "fuel", index => index == 0 ? ParameterInfo.Create("fuel", "number", "Sets or gets the fuel amount.") : ParameterInfo.None
      },
      {
        "move", index => ParameterInfo.FRU("move", "Movement offset based on the player rotation (unless origin is given)", index)
      },
      {
        "rotate", index => {
          var desc = "Rotation based on the player rotation (unless origin is given)";
          if (index == 0) return ParameterInfo.Create("rotate=<color=yellow>reset</color> or " + ParameterInfo.YawRollPitch("rotate", desc, index)[0].Substring(1));
          return ParameterInfo.YawRollPitch("rotate", desc, index);
        }
      },
      {
        "center",
        index => ParameterInfo.XZY("center", "Overrides the player position. For <color=yellow>rotate</color> sets also the rotation center point.", index)
      },
      {
        "from",
        index => ParameterInfo.XZY("center", "Overrides the player position. For <color=yellow>rotate</color> sets also the rotation center point.", index)
      },
      {
        "visual", index => VisualAutoComplete("visual", index)
      },
      {
        "origin", index => index == 0 ? ParameterInfo.Origin : ParameterInfo.None
      },
      {
        "rect",
        index => {
          if (index == 0) return ParameterInfo.Create("rect=<color=yellow>size</color> or rect=<color=yellow>width</color>,depth", "Area of affected objects.");
          if (index == 1) return ParameterInfo.Create("rect=width,<color=yellow>depth</color>", "Area of affected objects.");
          return ParameterInfo.None;
        }
      },
      {
        "angle",
        index => index == 0 ? ParameterInfo.Create("angle=<color=yellow>degrees</color>", "Direction of the rectangle when used with <color=yellow>rect</color>.") : ParameterInfo.None
      },
      {
        "circle",
        index => index == 0 ? ParameterInfo.Create("circle=<color=yellow>number</color>", "Radius of affected objects.") : ParameterInfo.None
      },
      {
        "radius",
        index => index == 0 ? ParameterInfo.Create("radius=<color=yellow>number</color>", "Radius of affected objects.") : ParameterInfo.None
      },
      {
        "creator",
        index => index == 0 ? ParameterInfo.Create("creator=<color=yellow>player ID</color>", "Sets creator of objects (0 for no creator).") : ParameterInfo.None
      },
      {
        "chance",
        index => index == 0 ? ParameterInfo.Create("chance=<color=yellow>number</color>", "Chance to affect the object (from 0.0 to 1.0).") : ParameterInfo.None
      },
      {
        "match", index => index == 0 ? DataLoading.DataKeys : ParameterInfo.None
      },
      {
        "unmatch", index => index == 0 ? DataLoading.DataKeys : ParameterInfo.None
      },
      {
        "copy", index => ParameterInfo.Flag("copy")
      }
    }));
  }
}
