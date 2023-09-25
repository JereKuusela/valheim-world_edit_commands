using System.Collections.Generic;
using ServerDevcommands;
namespace WorldEditCommands;
public class ObjectAutoComplete : SharedObjectAutoComplete
{
  public List<string> NamedParameters;
  public static List<string> ObjectTypes = [
      "creature",
    "chest",
    "fireplace",
    "item",
    "structure",
    "spawner",
    "spawnpoint",
  ];
  public ObjectAutoComplete()
  {
    NamedParameters = WithSharedParameters([
      "wild",
      "copy",
      "info",
      "data",
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
      "status"
    ]);
    AutoComplete.Register(ObjectCommand.Name, (int index) => NamedParameters, WithSharedFetchers(new() {
      {
        "status", (int index) => {
          if (index == 0) return ParameterInfo.StatusEffects;
          if (index == 1) return ParameterInfo.Create("status=name,<color=yellow>duration</color>,intensity", "Duration in seconds.");
          if (index == 2) return ParameterInfo.Create("status=name,duration,<color=yellow>intensity</color>", "Strength of the effect.");
          return ParameterInfo.None;
        }
      },
      {
        "type", (int index) => index == 0 ? ObjectTypes : ParameterInfo.None
      },
      {
        "connect", (int index) => ParameterInfo.Flag("Connect")
      },
      {
        "mirror", (int index) => ParameterInfo.Flag("Mirror")
      },
      {
        "respawn", (int index) => ParameterInfo.Flag("Respawn")
      },
      {
        "wild", (int index) => ParameterInfo.Flag("Wild")
      },
      {
        "remove", (int index) => ParameterInfo.Flag("Remove")
      },
      {
        "sleep", (int index) => ParameterInfo.Flag("Sleep")
      },
      {
        "info", (int index) => ParameterInfo.Flag("info")
      },
      {
        "data", (int index) => {
          if (index == 0) return ParameterInfo.Create("data=<color=yellow>key</color> or data=<color=yellow>key,value</color>", "Prints data information. If value is given, sets the data.");
          if (index == 1) return ParameterInfo.Create("data=key,<color=yellow>value</color>", "Value to set the data.");
          return ParameterInfo.None;
        }
      },
      {
        "copy", (int index) => {
          if (index == 0) return ParameterInfo.Create("copy or copy=<color=yellow>all</color> or copy=<color=yellow>key1,key2,...</color>", "Prints and copies object data to clipboard. By default excludes some data for better results.");
          return ParameterInfo.None;
        }
      },
      {
        "id", (int index) => ParameterInfo.Ids
      },
      {
        "ignore", (int index) => ParameterInfo.Ids
      },
      {
        "prefab", (int index) => index == 0 ? ParameterInfo.ObjectIds : ParameterInfo.None
      },
      {
        "fuel", (int index) => index == 0 ? ParameterInfo.Create("fuel", "number", "Sets or gets the fuel amount.") : ParameterInfo.None
      },
      {
        "move", (int index) => ParameterInfo.FRU("move", "Movement offset based on the player rotation (unless origin is given)", index)
      },
      {
        "rotate", (int index) => {
          var desc = "Rotation based on the player rotation (unless origin is given)";
          if (index == 0) return ParameterInfo.Create("rotate=<color=yellow>reset</color> or " + ParameterInfo.YawRollPitch("rotate", desc, index)[0].Substring(1));
          return ParameterInfo.YawRollPitch("rotate", desc, index);
        }
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
        "visual", (int index) => VisualAutoComplete("visual", index)
      },
      {
        "origin", (int index) => index == 0 ? ParameterInfo.Origin : ParameterInfo.None
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
      },
      {
        "creator",
        (int index) => index == 0 ? ParameterInfo.Create("creator=<color=yellow>player ID</color>", "Sets creator of objects (0 for no creator).") : ParameterInfo.None
      },
      {
        "chance",
        (int index) => index == 0 ? ParameterInfo.Create("chance=<color=yellow>number</color>", "Chance to affect the object (from 0.0 to 1.0).") : ParameterInfo.None
      },
    }));
  }
}
