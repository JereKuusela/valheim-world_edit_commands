using System;
using System.Linq;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakDungeonCommand : TweakCommand {
  protected override string DoOperation(ZNetView view, string operation, string? value) {
    if (operation == "enter_text") return TweakActions.EnterText(view, value);
    if (operation == "enter_hover") return TweakActions.EnterHover(view, value);
    if (operation == "exit_text") return TweakActions.ExitText(view, value);
    if (operation == "exit_hover") return TweakActions.ExitHover(view, value);
    if (operation == "weather") return TweakActions.Weather(view, Hash.DungeonWeather, value);
    throw new System.NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value) {
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


  protected override string DoOperation(ZNetView view, string operation, long? value) {
    throw new NotImplementedException();
  }

  protected override ZNetView Preprocess(Terminal context, ZNetView view) {
    if (view.GetComponent<LocationProxy>()) return view;
    if (view.GetComponent<DungeonGenerator>()) {
      var zone = ZoneSystem.instance.GetZone(view.transform.position);
      var location = Location.m_allLocations.FirstOrDefault(l => ZoneSystem.instance.GetZone(l.transform.position) == zone);
      if (location)
        return location.GetComponentInParent<ZNetView>();
    }
    context.AddString($"Skipped: {view.name} is not a location.");
#nullable disable
    return null;
#nullable enable
  }

  public TweakDungeonCommand() {
    SupportedOperations.Add("enter_text", typeof(string));
    SupportedOperations.Add("enter_hover", typeof(string));
    SupportedOperations.Add("exit_text", typeof(string));
    SupportedOperations.Add("exit_hover", typeof(string));
    SupportedOperations.Add("weather", typeof(string));

    AutoComplete.Add("enter_text", (int index) => index == 0 ? ParameterInfo.Create("enter_text=<color=yellow>text</color>", "Text when entering the dungeon.") : ParameterInfo.None);
    AutoComplete.Add("enter_hover", (int index) => index == 0 ? ParameterInfo.Create("enter_hover=<color=yellow>text</color>", "Text when entering the dungeon.") : ParameterInfo.None);
    AutoComplete.Add("exit_text", (int index) => index == 0 ? ParameterInfo.Create("exit_text=<color=yellow>text</color>", "Text when entering the dungeon.") : ParameterInfo.None);
    AutoComplete.Add("exit_hover", (int index) => index == 0 ? ParameterInfo.Create("exit_hover=<color=yellow>text</color>", "Text when entering the dungeon.") : ParameterInfo.None);
    AutoComplete.Add("weather", (int index) => index == 0 ? ParameterInfo.Environments : ParameterInfo.None);
    Init("tweak_dungeon", "Modify dungeons");
  }
}
