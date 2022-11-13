using System;
using System.Collections.Generic;
using ServerDevcommands;
using UnityEngine;

namespace WorldEditCommands;

public class TweakObjectCommand : TweakCommand
{
  protected override string DoOperation(ZNetView view, string operation, string? value)
  {
    if (operation == "wear") return TweakActions.Wear(view, value);
    if (operation == "growth") return TweakActions.Growth(view, value);
    if (operation == "component") return TweakActions.Component(view, value);
    if (operation == "status") return TweakActions.Status(view, value);
    if (operation == "event") return TweakActions.Event(view, value);
    if (operation == "effect") return TweakActions.Effect(view, value);
    if (operation == "weather") return TweakActions.Weather(view, value);
    if (operation == "water") return TweakActions.Water(view, value);
    if (operation == "fall") return TweakActions.Fall(view, value);
    throw new System.NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value)
  {
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, int? value)
  {
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value)
  {
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, bool? value)
  {
    if (operation == "show") return TweakActions.Render(view, value);
    if (operation == "interact") return TweakActions.Interact(view, value);
    if (operation == "collision") return TweakActions.Collision(view, value);
    if (operation == "restrict") return TweakActions.Restrict(view, value);
    if (operation == "unlock") return TweakActions.Unlock(view, value);
    throw new NotImplementedException();
  }

  protected override void Postprocess(GameObject obj)
  {
    if (obj.TryGetComponent<StaticPhysics>(out var sp))
    {
      sp.m_createTime = Time.time - 30f;
      sp.SUpdate();
    }
  }
  public static List<string> Wears = new() {
      "default",
      "broken",
      "damaged",
      "healthy"
  };
  public static List<string> Growths = new() {
      "big",
      "big_bad",
      "default",
      "small",
      "small_bad"
  };
  public static List<string> Smokes = new() {
      "off",
      "on",
      "ignore",
  };
  public static List<string> FallTypes = new() {
      "off",
      "solid",
      "terrain"
  };
  public static List<string> Waters = new() {
      "cave",
      "crypt",
  };
  public static List<string> Components = new() {
      "altar",
      "pickable",
      "spawner",
      "spawnpoint",
      "runestone",
      "chest"
  };
  public TweakObjectCommand()
  {
    SupportedOperations.Add("component", typeof(string));
    SupportedOperations.Add("status", typeof(string));
    SupportedOperations.Add("effect", typeof(string));
    SupportedOperations.Add("weather", typeof(string));
    SupportedOperations.Add("event", typeof(string));
    SupportedOperations.Add("collision", typeof(bool));
    SupportedOperations.Add("show", typeof(bool));
    SupportedOperations.Add("interact", typeof(bool));
    SupportedOperations.Add("fall", typeof(string));
    SupportedOperations.Add("wear", typeof(string));
    SupportedOperations.Add("growth", typeof(string));
    SupportedOperations.Add("water", typeof(string));

    AutoComplete.Add("component", (int index) => Components);
    AutoComplete.Add("status", (int index) => index == 0 ? ParameterInfo.Create("status=<color=yellow>radius</color>,id", "Adds status area.") : index == 1 ? ParameterInfo.StatusEffects : ParameterInfo.None);
    AutoComplete.Add("effect", (int index) => index == 0 ? ParameterInfo.Create("effect=<color=yellow>radius</color>,id", "Adds effect area.") : index == 1 ? ParameterInfo.EffectAreas : ParameterInfo.None);
    AutoComplete.Add("weather", (int index) => index == 0 ? ParameterInfo.Create("weather=<color=yellow>radius</color>,id,instant", "Adds weather area.") : index == 1 ? ParameterInfo.Environments : index == 2 ? ParameterInfo.Create("weather=radius,id,<color=yellow>instant</color>", "Adds weather area.") : ParameterInfo.None);
    AutoComplete.Add("event", (int index) => index == 0 ? ParameterInfo.Create("event=<color=yellow>radius</color>,id", "Adds event area.") : index == 1 ? ParameterInfo.Events : ParameterInfo.None);
    AutoComplete.Add("collision", (int index) => ParameterInfo.Create("collision=<color=yellow>true/false</color> or no value to toggle.", "Sets object collision."));
    AutoComplete.Add("fall", (int index) => index == 0 ? FallTypes : ParameterInfo.None);
    AutoComplete.Add("show", (int index) => ParameterInfo.Create("show =< color = yellow > true / false </ color > or no value to toggle.", "Sets object visibility."));
    AutoComplete.Add("interact", (int index) => ParameterInfo.Create("interact=<color=yellow>true/false</color> or no value to toggle.", "Sets object interactability."));
    AutoComplete.Add("wear", (int index) => index == 0 ? Wears : ParameterInfo.None);
    AutoComplete.Add("growth", (int index) => index == 0 ? Growths : ParameterInfo.None);
    AutoComplete.Add("water", (int index) => index == 0 ? Waters : ParameterInfo.XZY("water", "Scale", index - 1));
    Init("tweak_object", "Modify objects");
  }
}
