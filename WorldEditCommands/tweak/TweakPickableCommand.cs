using ServerDevcommands;

namespace WorldEditCommands;

public class TweakPickableCommand : TweakCommand {
  protected override string DoOperation(ZNetView view, string operation, string? value) {
    if (operation == "spawn")
      return TweakActions.Spawn(view, value);
    if (operation == "name")
      return TweakActions.Name(view, value);
    throw new System.NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value) {
    if (operation == "respawn")
      return TweakActions.Respawn(view, value);
    if (operation == "spawnoffset")
      return TweakActions.SpawnOffset(view, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, int? value) {
    if (operation == "amount")
      return TweakActions.Amount(view, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value) {
    if (operation == "useeffect")
      return TweakActions.UseEffect(view, value);
    throw new System.NotImplementedException();
  }

  public TweakPickableCommand() {
    Component = typeof(Pickable);
    SupportedOperations.Add("amount", typeof(int));
    SupportedOperations.Add("respawn", typeof(float));
    SupportedOperations.Add("spawnoffset", typeof(float));
    SupportedOperations.Add("spawn", typeof(string));
    SupportedOperations.Add("useeffect", typeof(string[]));
    SupportedOperations.Add("name", typeof(string));

    AutoComplete.Add("amount", (int index) => index == 0 ? ParameterInfo.Create("amount=<color=yellow>number</color>", "Sets the amount. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("respawn", (int index) => index == 0 ? ParameterInfo.Create("respawntime=<color=yellow>minutes</color>", "Sets the respawn time (no value reset).") : ParameterInfo.None);
    AutoComplete.Add("spawnoffset", (int index) => index == 0 ? ParameterInfo.Create("spawnoffset=<color=yellow>meters</color>", "Sets the spawn distance(no value reset).") : ParameterInfo.None);
    AutoComplete.Add("name", (int index) => index == 0 ? ParameterInfo.Create("name=<color=yellow>text</color>", "Sets name, use _ as the space (no value reset).") : ParameterInfo.None);
    AutoComplete.Add("spawn", (int index) => index == 0 ? ParameterInfo.ObjectIds : ParameterInfo.None);
    AutoComplete.Add("useeffect", TweakAutoComplete.Effect);

    Init("tweak_pickable", "Modify pickables");
  }
}
