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
      "smooth"
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
          "radius", (int index) => index == 0 ? ParameterInfo.Create("radius=<color=yellow>number</yellow> (from 0 to 64)", "Radius of the included terrain.") : null
        },
        {
          "smooth", (int index) => index == 0 ? ParameterInfo.Create("smooth=<color=yellow>number</yellow> (from 0.0 to 1.0)", "Higher values smoothen the effect near edges.") : null
        },
        {
          "raise", (int index) => index == 0 ? ParameterInfo.Create("raise=<color=yellow>number</yellow>", "Raises the terrain by given amount.") : null
        },
        {
          "lower", (int index) => index == 0 ? ParameterInfo.Create("lower=<color=yellow>number</yellow>", "Lowers the terrain by given amount.") : null
        },
        {
          "level", (int index) => index == 0 ? ParameterInfo.Create("level or level=<color=yellow>number</yellow>", "Levels the terrain to given altitude. Without parameters, levels to the terrain altitude below the player.") : null
        },
        {
          "paint", (int index) => index == 0 ? paints : null
        },
      });
    }
  }
}
