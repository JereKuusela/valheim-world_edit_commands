using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace WorldEditCommands;

public abstract class TweakCommand {
  public static System.Random Random = new();
  public static bool Roll(float value) {
    if (value >= 1f) return true;
    return Random.NextDouble() < value;
  }
  private static Dictionary<ZDOID, EditInfo> EditedInfo = new();
  protected Type Component = typeof(int);
  protected string ComponentName = "";
  protected abstract string DoOperation(ZNetView view, string operation, string? value);
  protected abstract string DoOperation(ZNetView view, string operation, float? value);
  protected abstract string DoOperation(ZNetView view, string operation, string[] value);
  protected abstract string DoOperation(ZNetView view, string operation, int? value);
  protected abstract string DoOperation(ZNetView view, string operation, bool? value);
  protected virtual Dictionary<string, object?> Postprocess(ZNetView view, Dictionary<string, object?> operations) => operations;
  protected virtual void Postprocess(GameObject obj) { }

  private void Execute(Terminal context, float chance, bool force, Dictionary<string, object?> operations, ZNetView[] views) {
    var scene = ZNetScene.instance;
    Dictionary<ZDOID, long> oldOwner = new();
    views = views.Where(view => {
      if (!view || !view.GetZDO().IsValid()) {
        context.AddString($"Skipped: {view.name} is not loaded.");
        return false;
      }
      if (!Roll(chance)) {
        context.AddString($"Skipped: {view.name} (chance).");
        return false;
      }
      if (ComponentName != "" && !view.GetComponentInChildren(Component)) {
        if (force) {
          TweakActions.Component(view, ComponentName);
          return true;
        }
        context.AddString($"Skipped: {view.name} doesn't have the component.");
        return false;
      }
      return true;
    }).ToArray();
    EditedInfo.Clear();
    foreach (var view in views) {
      var zdo = view.GetZDO();
      oldOwner.Add(zdo.m_uid, zdo.m_owner);
      view.ClaimOwnership();
      EditedInfo[zdo.m_uid] = new EditInfo(zdo, true);
    }
    var count = views.Count();
    foreach (var view in views) {
      var operations2 = Postprocess(view, operations);
      foreach (var operation in operations2) {
        var type = SupportedOperations[operation.Key];
        var name = Utils.GetPrefabName(view.gameObject);
        var output = "";
        if (type == typeof(int))
          output = DoOperation(view, operation.Key, (int?)operation.Value);
        else if (type == typeof(float))
          output = DoOperation(view, operation.Key, (float?)operation.Value);
        else if (type == typeof(string[]))
          output = DoOperation(view, operation.Key, (string[])operation.Value!);
        else if (type == typeof(bool))
          output = DoOperation(view, operation.Key, (bool?)operation.Value!);
        else
          output = DoOperation(view, operation.Key, (string?)operation.Value);
        // No operation.
        if (output == "") continue;
        var message = output.Replace("¤", name);
        if (count == 1)
          Helper.AddMessage(context, message);
        else
          context.AddString(message);
      }
    }
    foreach (var view in views) {
      if (!view || view.GetZDO() == null || !view.GetZDO().IsValid() || !oldOwner.ContainsKey(view.GetZDO().m_uid)) continue;
      view.GetZDO().SetOwner(oldOwner[view.GetZDO().m_uid]);
      Postprocess(Actions.Refresh(view));
    }
    if (EditedInfo.Count > 0) {
      UndoEdit undo = new(EditedInfo.Select(info => info.Value.ToData()));
      UndoManager.Add(undo);
    }
  }
  public Dictionary<string, Type> SupportedOperations = new();
  public Dictionary<string, Func<int, List<string>>> AutoComplete = new();

  protected void Init(string name, string description) {
    var namedParameters = TweakAutoComplete.WithFilters(AutoComplete.Keys.ToList());
    ServerDevcommands.AutoComplete.Register(name, (int index) => namedParameters, TweakAutoComplete.WithFilters(AutoComplete));
    var fullDescription = CommandInfo.Create(description, null, namedParameters);
    Helper.Command(name, fullDescription, (args) => {
      TweakParameters pars = new(SupportedOperations, args);
      ZNetView[] views;
      if (pars.Connect) {
        var view = Selector.GetHovered(50f, null);
        if (view == null) return;
        views = Selector.GetConnected(view);
      } else if (pars.Radius.HasValue) {
        views = Selector.GetNearby(pars.Id, pars.ObjectType, position => Selector.Within(position, pars.Center ?? pars.From, pars.Radius.Value, pars.Height));
      } else if (pars.Width.HasValue && pars.Depth.HasValue) {
        views = Selector.GetNearby(pars.Id, pars.ObjectType, position => Selector.Within(position, pars.Center ?? pars.From, pars.Angle, pars.Width.Value, pars.Depth.Value, pars.Height));
      } else {
        var view = Selector.GetHovered(50f, null);
        if (view == null) return;
        if (!Selector.GetPrefabs(pars.Id).Contains(view.GetZDO().GetPrefab())) {
          Helper.AddMessage(args.Context, $"Skipped: {view.name} has invalid id.");
          return;
        }
        views = new[] { view };
      }
      Execute(args.Context, pars.Chance, pars.Force, pars.Operations, views);

    }, () => namedParameters);
  }
  public TweakCommand() {
  }
}
