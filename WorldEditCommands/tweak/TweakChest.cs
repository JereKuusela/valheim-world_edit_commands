using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakChestCommand : TweakCommand
{
  protected override string DoOperation(ZNetView view, string operation, string? value)
  {
    if (operation == "name")
      return TweakActions.Name(view, value);
    throw new System.NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value)
  {
    if (operation == "respawn")
      return TweakActions.Respawn(view, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, int? value)
  {
    if (operation == "minamount")
      return TweakActions.MinAmount(view, value);
    if (operation == "maxamount")
      return TweakActions.MaxAmount(view, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value)
  {
    if (operation == "item")
      return TweakActions.Items(view, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, bool? value)
  {
    if (operation == "unlock")
      return TweakActions.Unlock(view, value);
    throw new NotImplementedException();
  }

  protected override Dictionary<string, object?> Postprocess(ZNetView view, Dictionary<string, object?> operations)
  {
    if (!operations.ContainsKey("respawn") || operations.ContainsKey("item")) return operations;
    var container = view.GetComponent<Container>();
    if (!container) return operations;
    var items = container.GetInventory().GetAllItems().Select(item => $"{item.m_dropPrefab.name},1,{item.m_stack}").ToArray();
    var newOperations = operations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    newOperations["item"] = items;
    if (!newOperations.ContainsKey("minamount"))
      newOperations["minamount"] = items.Count();
    if (!newOperations.ContainsKey("maxamount"))
      newOperations["maxamount"] = items.Count();
    return newOperations;
  }
  public TweakChestCommand()
  {
    Component = typeof(Container);
    ComponentName = "chest";
    SupportedOperations.Add("unlock", typeof(bool));
    SupportedOperations.Add("respawn", typeof(float));
    SupportedOperations.Add("name", typeof(string));
    SupportedOperations.Add("minamount", typeof(int));
    SupportedOperations.Add("maxamount", typeof(int));
    SupportedOperations.Add("item", typeof(string[]));

    AutoComplete.Add("unlock", (int index) => index == 0 ? ParameterInfo.Create("unlock=<color=yellow>true/false</color>", "Ignores wards. No value to toggle.") : ParameterInfo.None);
    AutoComplete.Add("name", (int index) => index == 0 ? ParameterInfo.Create("name=<color=yellow>text</color>", "Display name. Use _ as the space. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("respawn", (int index) => index == 0 ? ParameterInfo.Create("respawn=<color=yellow>minutes</color>", "Respawn time. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("minamount", (int index) => index == 0 ? ParameterInfo.Create("minamount=<color=yellow>number</color>", "Minimum amount of items. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("maxamount", (int index) => index == 0 ? ParameterInfo.Create("maxamount=<color=yellow>number</color>", "Maximum amount of items. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("item", (int index) =>
    {
      if (index == 0) return ParameterInfo.ItemIds;
      if (index == 1) return ParameterInfo.Create("item=id,<color=yellow>weight</color>,minamount,maxamount", "Chance relative to other items.");
      if (index == 2) return ParameterInfo.Create("item=id,weight,<color=yellow>minamount</color>,maxamount", "Minimum amount.");
      if (index == 3) return ParameterInfo.Create("item=id,weight,minamount,<color=yellow>maxamount</color>", "Maximum amount.");
      return ParameterInfo.Create("For additional entries, add more <color>item=...</color> parameters.");
    });
    Init("tweak_chest", "Modify chests");
  }
}
