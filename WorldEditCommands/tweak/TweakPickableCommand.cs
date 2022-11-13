using ServerDevcommands;

namespace WorldEditCommands;

public class TweakPickableCommand : TweakCommand
{
  protected override string DoOperation(ZNetView view, string operation, string? value)
  {
    if (operation == "spawn")
      return TweakActions.Spawn(view, value);
    if (operation == "name")
      return TweakActions.Name(view, value);
    throw new System.NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value)
  {
    if (operation == "respawn")
      return TweakActions.Respawn(view, value);
    if (operation == "spawnoffset")
      return TweakActions.SpawnOffset(view, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, int? value)
  {
    if (operation == "amount")
      return TweakActions.Amount(view, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value)
  {
    if (operation == "useeffect")
      return TweakActions.UseEffect(view, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, bool? value)
  {
    throw new System.NotImplementedException();
  }

  public TweakPickableCommand()
  {
    Component = typeof(Pickable);
    ComponentName = "pickable";
    SupportedOperations.Add("amount", typeof(int));
    SupportedOperations.Add("respawn", typeof(float));
    SupportedOperations.Add("spawnoffset", typeof(float));
    SupportedOperations.Add("spawn", typeof(string));
    SupportedOperations.Add("useeffect", typeof(string[]));
    SupportedOperations.Add("name", typeof(string));

    AutoComplete.Add("amount", (int index) => index == 0 ? ParameterInfo.Create("amount=<color=yellow>number</color>", "Amount of dropped items. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("respawn", (int index) => index == 0 ? ParameterInfo.Create("respawn=<color=yellow>minutes</color>", "Respawn time. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawnoffset", (int index) => index == 0 ? ParameterInfo.Create("spawnoffset=<color=yellow>meters</color>", "Spawn distance from the ground.. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("name", (int index) => index == 0 ? ParameterInfo.Create("name=<color=yellow>text</color>", "Display name. Use _ as the space. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawn", (int index) => index == 0 ? ParameterInfo.ObjectIds : ParameterInfo.None);
    AutoComplete.Add("useeffect", (int index) => TweakAutoComplete.Effect("useeffect", index));

    Init("tweak_pickable", "Modify pickables");
  }
}
