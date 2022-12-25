using System;
using System.Linq;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakCreatureCommand : TweakCommand
{
  protected override string DoOperation(ZNetView view, string operation, string? value)
  {
    if (operation == "name") return TweakActions.Name(view, value);
    if (operation == "faction") return TweakActions.Faction(view, value);
    throw new System.NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value)
  {
    if (operation == "health") return TweakActions.Health(view, value);
    if (operation == "damage") return Actions.Damage(view, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, int? value)
  {
    if (operation == "level") return TweakActions.Level(view, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value)
  {
    if (operation == "item") return TweakActions.Items(view, value);
    if (operation == "affix") return TweakActions.CLLC(view, value);
    if (operation == "resistance") return TweakActions.Resistances(view, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, bool? value)
  {
    if (operation == "boss") return TweakActions.Boss(view, value);
    if (operation == "hunt") return TweakActions.Hunt(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, long? value)
  {
    throw new NotImplementedException();
  }

  public TweakCreatureCommand()
  {
    Component = typeof(Character);
    ComponentName = "character";
    SupportedOperations.Add("damage", typeof(float));
    SupportedOperations.Add("faction", typeof(string));
    SupportedOperations.Add("boss", typeof(bool));
    SupportedOperations.Add("health", typeof(float));
    SupportedOperations.Add("level", typeof(int));
    SupportedOperations.Add("hunt", typeof(bool));
    SupportedOperations.Add("resistance", typeof(string[]));
    SupportedOperations.Add("name", typeof(string));
    SupportedOperations.Add("item", typeof(string[]));
    if (WorldEditCommands.IsCLLC)
      SupportedOperations.Add("affix", typeof(string[]));

    if (WorldEditCommands.IsCLLC)
      AutoComplete.Add("affix", (int index) => Enum.GetNames(typeof(BossAffix)).ToList());
    AutoComplete.Add("name", (int index) => index == 0 ? ParameterInfo.Create("name=<color=yellow>text</color>", "Display name. Use _ as the space. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("faction", (int index) => index == 0 ? Enum.GetNames(typeof(Character.Faction)).ToList() : ParameterInfo.None);
    AutoComplete.Add("boss", (int index) => ParameterInfo.Create("boss=<color=yellow>true/false</color> ", "Sets the boss health bar. No value to reset."));
    AutoComplete.Add("hunt", (int index) => ParameterInfo.Create("hunt=<color=yellow>true/false</color>", "Sets the extra aggressiveness. No value to toggle."));
    AutoComplete.Add("level", (int index) => ParameterInfo.Create("level=<color=yellow>number</color>", "Sets the level (level 1 = 0 star)"));
    AutoComplete.Add("health", (int index) => ParameterInfo.Create("health=<color=yellow>number</color>", "Sets the health."));
    AutoComplete.Add("damage", (int index) => ParameterInfo.Create("damage=<color=yellow>number</color>", "Sets the damage multiplier."));
    AutoComplete.Add("resistance", (int index) =>
    {
      if (index == 0) return Enum.GetNames(typeof(HitData.DamageType)).ToList();
      if (index == 1) return Enum.GetNames(typeof(HitData.DamageModifier)).ToList();
      return ParameterInfo.Create("For additional entries, add more <color>resistance=...</color> parameters.");
    });
    AutoComplete.Add("item", (int index) =>
    {
      if (index == 0) return ParameterInfo.ItemIds;
      if (index == 1) return ParameterInfo.Create("item=id,<color=yellow>chance</color>,minamount,maxamount,flag", "Drop chance.");
      if (index == 2) return ParameterInfo.Create("item=id,chance,<color=yellow>minamount</color>,maxamount,flag", "Minimum amount.");
      if (index == 3) return ParameterInfo.Create("item=id,chance,minamount,<color=yellow>maxamount</color>,flag", "Maximum amount.");
      if (index == 4) return ParameterInfo.Create("item=id,chance,minamount,maxamount,<color=yellow>flag</color>", "Sum up: 1 = star multiplier, 2 = one per player.");
      return ParameterInfo.Create("For additional entries, add more <color>item=...</color> parameters.");
    });
    Init("tweak_creature", "Modify creatures");
  }
}
