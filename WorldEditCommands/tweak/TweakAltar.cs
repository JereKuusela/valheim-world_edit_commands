using System;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakAltarCommand : TweakCommand {
  protected override string DoOperation(ZNetView view, string operation, string? value) {
    if (operation == "spawn")
      return TweakActions.Spawn(view, value);
    if (operation == "spawnitem")
      return TweakActions.SpawnItem(view, value);
    if (operation == "name")
      return TweakActions.Name(view, value);
    if (operation == "text")
      return TweakActions.Text(view, value);
    if (operation == "globalkey")
      return TweakActions.GlobalKey(view, value);
    if (operation == "itemstandprefix")
      return TweakActions.ItemStandPrefix(view, value);
    if (operation == "itemoffset")
      return TweakActions.ItemOffset(view, value);
    throw new NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value) {
    if (operation == "spawnhealth")
      return TweakActions.SpawnHealth(view, value);
    if (operation == "delay")
      return TweakActions.Delay(view, value);
    if (operation == "spawnoffset")
      return TweakActions.SpawnOffset(view, value);
    if (operation == "spawnradius")
      return TweakActions.SpawnRadius(view, value);
    if (operation == "spawnmaxy")
      return TweakActions.SpawnMaxY(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, int? value) {
    if (operation == "minlevel")
      return TweakActions.MinLevel(view, value);
    if (operation == "maxlevel")
      return TweakActions.MaxLevel(view, value);
    if (operation == "amount")
      return TweakActions.Amount(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value) {
    if (operation == "starteffect")
      return TweakActions.StartEffect(view, value);
    if (operation == "spawneffect")
      return TweakActions.SpawnEffect(view, value);
    if (operation == "useeffect")
      return TweakActions.UseEffect(view, value);
    throw new NotImplementedException();
  }

  public TweakAltarCommand() {
    Component = typeof(OfferingBowl);
    SupportedOperations.Add("minlevel", typeof(int));
    SupportedOperations.Add("maxlevel", typeof(int));
    SupportedOperations.Add("spawnhealth", typeof(float));
    SupportedOperations.Add("delay", typeof(float));
    SupportedOperations.Add("spawnradius", typeof(float));
    SupportedOperations.Add("amount", typeof(int));
    SupportedOperations.Add("spawnoffset", typeof(float));
    SupportedOperations.Add("spawnmaxy", typeof(float));
    SupportedOperations.Add("spawn", typeof(string));
    SupportedOperations.Add("spawnitem", typeof(string));
    SupportedOperations.Add("starteffect", typeof(string[]));
    SupportedOperations.Add("useeffect", typeof(string[]));
    SupportedOperations.Add("spawneffect", typeof(string[]));
    SupportedOperations.Add("name", typeof(string));
    SupportedOperations.Add("text", typeof(string));
    SupportedOperations.Add("itemoffset", typeof(string));
    SupportedOperations.Add("itemstandprefix", typeof(string));
    SupportedOperations.Add("globalkey", typeof(string));

    AutoComplete.Add("minlevel", (int index) => index == 0 ? ParameterInfo.Create("minlevel=<color=yellow>number</color>", "Sets the minimum level. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("maxlevel", (int index) => index == 0 ? ParameterInfo.Create("maxlevel=<color=yellow>number</color>", "Sets the maximum level. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("amount", (int index) => index == 0 ? ParameterInfo.Create("amount=<color=yellow>number</color>", "Sets the amount of needed items. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawnhealth", (int index) => index == 0 ? ParameterInfo.Create("spawnhealth=<color=yellow>number</color>", "Sets the creature health. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawnmaxy", (int index) => index == 0 ? ParameterInfo.Create("spawnmaxy=<color=yellow>number</color>", "Sets the maximum height difference from the altar. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("delay", (int index) => index == 0 ? ParameterInfo.Create("delay=<color=yellow>seconds</color>", "Sets the spawn delay. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawnoffset", (int index) => index == 0 ? ParameterInfo.Create("spawnoffset=<color=yellow>meters</color>", "Sets the spawn distance from the ground. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawnradius", (int index) => index == 0 ? ParameterInfo.Create("spawnradius=<color=yellow>meters</color>", "Sets the spawn radius. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("itemstandrange", (int index) => index == 0 ? ParameterInfo.Create("itemstandrange=<color=yellow>meters</color>", "Sets the radius for included item stands. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("text", (int index) => index == 0 ? ParameterInfo.Create("text=<color=yellow>text</color>", "Sets the use text. Use _ as the space. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("name", (int index) => index == 0 ? ParameterInfo.Create("name=<color=yellow>text</color>", "Sets the name. Use _ as the space. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("itemstandprefix", (int index) => index == 0 ? ParameterInfo.Create("itemstandprefix=<color=yellow>text</color>", "Sets the prefix for included item stands. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("globalkey", (int index) => index == 0 ? ParameterInfo.Create("text=<color=yellow>key</color>", "Sets the global key when used, Start with - to remove the key. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawn", (int index) => index == 0 ? ParameterInfo.ObjectIds : ParameterInfo.None);
    AutoComplete.Add("spawnitem", (int index) => index == 0 ? ParameterInfo.ItemIds : ParameterInfo.None);
    AutoComplete.Add("starteffect", TweakAutoComplete.Effect);
    AutoComplete.Add("useeffect", TweakAutoComplete.Effect);
    AutoComplete.Add("spawneffect", TweakAutoComplete.Effect);
    AutoComplete.Add("itemoffset", (int index) => ParameterInfo.XZY("itemoffset", "Offset for the item position", index));
    Init("tweak_altar", "Modify altars");
  }
}

