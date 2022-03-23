using System.Collections.Generic;
using ServerDevcommands;

namespace WorldEditCommands {
  public class TerrainAutoComplete {
    private static List<string> BlockCheck = new List<string>{
      "inverse",
      "off",
      "on"
    };
    public List<string> NamedParameters = new List<string>(){
      "lower",
      "level",
      "raise",
      "reset",
      "paint",
      "blockcheck",
      "square",
      "radius",
      "smooth",
      "slope",
      "angle",
      "offset",
      "step",
      "refPos"
    };
    public TerrainAutoComplete() {
      NamedParameters.Sort();
      var paints = new List<string>() {
        "dirt", "paved", "cultivated", "grass"
      };
      paints.Sort();
      AutoComplete.Register("terrain", (int index) => NamedParameters, new Dictionary<string, System.Func<int, List<string>>>() {
        {
          "reset", (int index) => ParameterInfo.Flag("Reset")
        },
        {
          "blockcheck", (int index) => index == 0 ? ParameterInfo.Create("blockcheck=<color=yellow>inverse</color>/<color=yellow>off</color>/<color=yellow>on</color>", "When <color=yellow>on</color>, excludes terrain under structures. When <color=yellow>inverse</color>, only includes terrain under structures.") : null
        },
        {
          "square", (int index) => ParameterInfo.Flag("Square")
        },
        {
          "angle", (int index) => index == 0 ? ParameterInfo.Create("angle=<color=yellow>number</yellow>/<color=yellow>n</yellow>/<color=yellow>ne</yellow>/<color=yellow>e</yellow>/<color=yellow>se</yellow>/<color=yellow>s</yellow>/<color=yellow>sw</yellow>/<color=yellow>w</yellow>/<color=yellow>nw</yellow> (from 0 to 360)", "Direction of the slope operation.") : null
        },
        {
          "radius", (int index) => index == 0 ? ParameterInfo.Create("radius=<color=yellow>number</yellow> (from 0 to 64)", "Radius of the included terrain.") : null
        },
        {
          "offset", (int index) => ParameterInfo.XZY("offset", "Position of the center", index)
        },
        {
          "refPos", (int index) => ParameterInfo.XZY("refPos", "Overrides the player position", index)
        },
        {
          "step", (int index) => ParameterInfo.XZY("step", "Offset based on radius (forward, right, up)", index)
        },
        {
          "smooth", (int index) => index == 0 ? ParameterInfo.Create("smooth=<color=yellow>number</yellow> (from 0.0 to 1.0)", "Higher values smoothen the effect near edges.") : null
        },
        {
          "slope", (int index) => index == 0 ? ParameterInfo.Create("slope=<color=yellow>number</yellow>", "Creates a slope with altitude difference of a given amount.") : null
        },
        {
          "raise", (int index) => index == 0 ? ParameterInfo.Create("raise=<color=yellow>number</yellow>", "Raises the terrain by a given amount.") : null
        },
        {
          "lower", (int index) => index == 0 ? ParameterInfo.Create("lower=<color=yellow>number</yellow>", "Lowers the terrain by a given amount.") : null
        },
        {
          "level", (int index) => index == 0 ? ParameterInfo.Create("level or level=<color=yellow>number</yellow>", "Levels the terrain to a given altitude. Without parameters, levels to the terrain altitude below the player.") : null
        },
        {
          "paint", (int index) => index == 0 ? paints : null
        },
      });
    }
  }
}
