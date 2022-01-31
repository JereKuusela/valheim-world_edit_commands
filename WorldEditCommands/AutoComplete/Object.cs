using System.Collections.Generic;
using System.Linq;
using DEV;

namespace WorldEditCommands {
  public class ObjectAutoComplete {
    private static List<string> VisualAutoComplete(int index) {
      if (index == 0) return ParameterInfo.ItemIds;
      if (index == 1) return ParameterInfo.Create("Visual", "number (0 or more)");
      return null;
    }
    public ObjectAutoComplete() {
      var parameters = ObjectCommand.Operations.Concat(ObjectCommand.Params).ToList();
      parameters.Sort();
      AutoComplete.Register("object", (int index) => parameters, new Dictionary<string, System.Func<int, List<string>>>() {
        {
          "baby", (int index) => ParameterInfo.Flag("Baby")
        },
        {
          "tame", (int index) => ParameterInfo.Flag("Tame")
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
          "id", (int index) => index == 0 ? ParameterInfo.Ids : null
        },
        {
          "left_hand", VisualAutoComplete
        },
        {
          "right_hand", VisualAutoComplete
        },
        {
          "helmet", VisualAutoComplete
        },
        {
          "chest", VisualAutoComplete
        },
        {
          "shoulders", VisualAutoComplete
        },
        {
          "legs", VisualAutoComplete
        },
        {
          "utility", VisualAutoComplete
        },
        {
          "radius", (int index) => index == 0 ? ParameterInfo.Create("Radius", "a number") : null
        },
        {
          "health", (int index) => index == 0 ? ParameterInfo.Create("Health", "a number") : null
        },
        {
          "stars", (int index) => index == 0 ? ParameterInfo.Create("Stars", "an integer") : null
        },
        {
          "move", (int index) => index == 3 ? ParameterInfo.Origin : ParameterInfo.XZY(index)
        },
        {
          "rotate", (int index) => {
            if (index == 0) return ParameterInfo.Create("Y", "number or reset");
            if (index == 3) return ParameterInfo.Origin;
            return ParameterInfo.YXZ(index);
          }
        },
        {
          "scale", ParameterInfo.Scale
        },
      });
    }
  }
}
