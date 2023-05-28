using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakItemStandCommand : TweakCommand {
  protected override string DoOperation(ZNetView view, string operation, string? value) {
    if (operation == "item")
      return TweakActions.Item(view, value);
    if (operation == "name")
      return TweakActions.Name(view, value);
    throw new System.NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value) {
    if (operation == "respawn")
      return TweakActions.Respawn(view, Hash.Respawn, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, int? value) {
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value) {
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, bool? value) {
    throw new NotImplementedException();
  }

  protected override Dictionary<string, object?> Postprocess(ZNetView view, Dictionary<string, object?> operations) {
    if (!operations.ContainsKey("respawn") || operations.ContainsKey("item")) return operations;
    var itemstand = view.GetComponent<ItemStand>();
    if (!itemstand) return operations;
    var item = itemstand.m_visualName + "," + itemstand.m_visualVariant;
    var newOperations = operations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    newOperations["item"] = item;
    return newOperations;
  }

  protected override string DoOperation(ZNetView view, string operation, long? value) {
    throw new NotImplementedException();
  }

  public TweakItemStandCommand() {
    Component = typeof(ItemStand);
    ComponentName = "itemstand";
    SupportedOperations.Add("respawn", typeof(float));
    SupportedOperations.Add("name", typeof(string));
    SupportedOperations.Add("item", typeof(string));

    AutoComplete.Add("name", (int index) => index == 0 ? ParameterInfo.Create("name=<color=yellow>text</color>", "Display name. Use _ as the space. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("respawn", (int index) => index == 0 ? ParameterInfo.Create("respawn=<color=yellow>minutes</color>", "Respawn time. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("item", (int index) => {
      if (index == 0) return ParameterInfo.ItemIds;
      if (index == 1) return ParameterInfo.Create("item=id,<color=yellow>variant</color>", "Item variant (number).");
      return ParameterInfo.None;
    });
    Init("tweak_itemstand", "Modify item stands");
  }
}
