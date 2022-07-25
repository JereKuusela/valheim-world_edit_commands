using System.Collections.Generic;
using ServerDevcommands;
namespace WorldEditCommands;
public class ObjectAutoComplete : SharedObjectAutoComplete {
  public List<string> NamedParameters;
  public static List<string> Wears = new() {
      "default",
      "broken",
      "damaged",
      "healthy"
  };
  public static List<string> Growths = new() {
      "big",
      "big_bad",
      "default",
      "small",
      "small_bad"
  };
  public ObjectAutoComplete() {
    NamedParameters = WithSharedParameters(new() {
      "baby",
      "wild",
      "info",
      "sleep",
      "id",
      "move",
      "rotate",
      "remove",
      "origin",
      "visual",
      "fuel",
      "prefab",
      "center",
      "respawn",
      "guide",
      "from",
      "rect",
      "angle",
      "creator",
      "growth",
      "wear",
      "chance",
      "collision",
      "show",
      "interact"
    });
    AutoComplete.Register(ObjectCommand.Name, (int index) => NamedParameters, WithSharedFetchers(new() {
      {
        "collision", (int index) => ParameterInfo.Create("collision=<color=yellow>true/false</color> or no value to toggle.", "Sets object collision.")
      },
      {
        "show", (int index) => ParameterInfo.Create("show=<color=yellow>true/false</color> or no value to toggle.", "Sets object visibility.")
      },
      {
        "interact", (int index) => ParameterInfo.Create("interact=<color=yellow>true/false</color> or no value to toggle.", "Sets object interactability.")
      },
      {
        "baby", (int index) => ParameterInfo.Flag("Baby")
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
        "info", (int index) => ParameterInfo.Flag("Info")
      },
      {
        "id", (int index) => index == 0 ? ParameterInfo.Ids : ParameterInfo.None
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
        "guide",
        (int index) => ParameterInfo.Flag("Guide", "Visualizes the affected area.")
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
      {
        "wear",
        (int index) => index == 0 ? Wears : ParameterInfo.None
      },
      {
        "growth",
        (int index) => index == 0 ? Growths : ParameterInfo.None
      },
    }));
  }
}
