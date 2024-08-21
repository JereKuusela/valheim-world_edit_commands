using System;
using System.Linq;
using Data;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakAltarCommand : TweakCommand
{
  protected override string DoOperation(ZNetView view, string operation, string? value)
  {
    if (operation == "spawndata")
      return TweakActions.SpawnData(view, value);
    if (operation == "spawn")
      return TweakActions.Spawn(view, Hash.Spawn, value);
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
    if (operation == "faction")
      return TweakActions.Faction(view, value);
    if (operation == "command")
      return TweakActions.Command(view, value);
    throw new NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value)
  {
    if (operation == "levelchance")
      return TweakActions.LevelChance(view, value);
    if (operation == "respawn")
      return TweakActions.Respawn(view, Hash.Respawn, value);
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

  protected override string DoOperation(ZNetView view, string operation, int? value)
  {
    if (operation == "minlevel")
      return TweakActions.MinLevel(view, value);
    if (operation == "maxlevel")
      return TweakActions.MaxLevel(view, value);
    if (operation == "amount")
      return TweakActions.Amount(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value)
  {
    if (operation == "starteffect")
      return TweakActions.StartEffect(view, value);
    if (operation == "spawneffect")
      return TweakActions.SpawnEffect(view, value);
    if (operation == "useeffect")
      return TweakActions.UseEffect(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, bool? value)
  {
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, long? value)
  {
    throw new NotImplementedException();
  }

  public TweakAltarCommand()
  {
    Component = typeof(OfferingBowl);
    ComponentName = "altar";
    SupportedOperations.Add("minlevel", typeof(int));
    SupportedOperations.Add("maxlevel", typeof(int));
    SupportedOperations.Add("delay", typeof(float));
    SupportedOperations.Add("spawnradius", typeof(float));
    SupportedOperations.Add("amount", typeof(int));
    SupportedOperations.Add("spawnoffset", typeof(float));
    SupportedOperations.Add("levelchance", typeof(float));
    SupportedOperations.Add("spawnmaxy", typeof(float));
    SupportedOperations.Add("spawn", typeof(string));
    SupportedOperations.Add("spawnitem", typeof(string));
    SupportedOperations.Add("starteffect", typeof(string[]));
    SupportedOperations.Add("useeffect", typeof(string[]));
    SupportedOperations.Add("spawneffect", typeof(string[]));
    SupportedOperations.Add("spawnhealth", typeof(float));
    SupportedOperations.Add("respawn", typeof(float));
    SupportedOperations.Add("name", typeof(string));
    SupportedOperations.Add("text", typeof(string));
    SupportedOperations.Add("itemoffset", typeof(string));
    SupportedOperations.Add("itemstandprefix", typeof(string));
    SupportedOperations.Add("globalkey", typeof(string));
    SupportedOperations.Add("faction", typeof(string));
    SupportedOperations.Add("spawndata", typeof(string));
    SupportedOperations.Add("command", typeof(string));

    AutoComplete.Add("minlevel", (int index) => index == 0 ? ParameterInfo.Create("minlevel=<color=yellow>number</color>", "Minimum level (level 1 = no star). No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("maxlevel", (int index) => index == 0 ? ParameterInfo.Create("maxlevel=<color=yellow>number</color>", "Maximum level (level 1 = no star). No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("amount", (int index) => index == 0 ? ParameterInfo.Create("amount=<color=yellow>number</color>", "Amount of needed items. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawnmaxy", (int index) => index == 0 ? ParameterInfo.Create("spawnmaxy=<color=yellow>number</color>", "Maximum height difference from the altar. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("delay", (int index) => index == 0 ? ParameterInfo.Create("delay=<color=yellow>seconds</color>", "Duration of the spawning. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawnoffset", (int index) => index == 0 ? ParameterInfo.Create("spawnoffset=<color=yellow>meters</color>", "Spawn distance from the ground. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawnradius", (int index) => index == 0 ? ParameterInfo.Create("spawnradius=<color=yellow>meters</color>", "Maximum spawn radius. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawnhealth", (int index) => index == 0 ? ParameterInfo.Create("spawnhealth=<color=yellow>number</color>", "Overrides the creature health. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("itemstandrange", (int index) => index == 0 ? ParameterInfo.Create("itemstandrange=<color=yellow>meters</color>", "Radius for included item stands. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("text", (int index) => index == 0 ? ParameterInfo.Create("text=<color=yellow>text</color>", "Use text. Use _ as the space. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("name", (int index) => index == 0 ? ParameterInfo.Create("name=<color=yellow>text</color>", "Display name. Use _ as the space. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("faction", (int index) => index == 0 ? [.. Enum.GetNames(typeof(Character.Faction))] : ParameterInfo.None);
    AutoComplete.Add("itemstandprefix", (int index) => index == 0 ? ParameterInfo.Create("itemstandprefix=<color=yellow>text</color>", "Prefix for included item stands. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("globalkey", (int index) => index == 0 ? ParameterInfo.Create("text=<color=yellow>key</color>", "Sets the global key when used. Start with - to remove the key. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawn", (int index) => index == 0 ? ParameterInfo.ObjectIds : ParameterInfo.None);
    AutoComplete.Add("spawnitem", (int index) => index == 0 ? ParameterInfo.ItemIds : ParameterInfo.None);
    AutoComplete.Add("respawn", (int index) => index == 0 ? ParameterInfo.Create("respawn=<color=yellow>minutes/false</color>", "Respawn time. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("levelchance", (int index) => index == 0 ? ParameterInfo.Create("levelchance=<color=yellow>percent</color>", "Level up chance (from 0 to 100). No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("command", (int index) => index == 0 ? ParameterInfo.Create("command=<color=yellow>text</color>", "Command to run when interacted. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawneffect", (int index) => TweakAutoComplete.Effect("spawneffect", index));
    AutoComplete.Add("starteffect", (int index) => TweakAutoComplete.Effect("starteffect", index));
    AutoComplete.Add("useeffect", (int index) => TweakAutoComplete.Effect("useeffect", index));
    AutoComplete.Add("itemoffset", (int index) => ParameterInfo.XZY("itemoffset", "Offset when spawning items. Also sets the <color=yellow>useeffect</color> position.", index));
    AutoComplete.Add("spawndata", (int index) => index == 0 ? DataLoading.DataKeys : ParameterInfo.None);
    Init("tweak_altar", "Modify altars");
  }
}

