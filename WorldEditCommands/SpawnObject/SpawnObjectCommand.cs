using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace WorldEditCommands;
public class SpawnObjectCommand
{
  public const string Name = "spawn_object";
  private static List<GameObject> SpawnObject(SpawnObjectParameters pars, GameObject prefab, int count)
  {
    List<GameObject> spawned = [];
    var defaultRadius = (count - 1) * 0.5f;
    for (int i = 0; i < count; i++)
    {
      Vector3 spawnPosition;
      if (pars.To.HasValue)
        spawnPosition = pars.GetPosition(i, count);
      else
      {
        spawnPosition = pars.GetPosition();
        var random = UnityEngine.Random.insideUnitCircle * (pars.Radius?.Max ?? defaultRadius);
        if (pars.Radius != null && pars.Radius.Min != pars.Radius.Max)
        {
          var angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
          random = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)).normalized * Helper.RandomValue(pars.Radius);
        }
        spawnPosition.x += random.x;
        spawnPosition.z += random.y;
      }
      if (pars.Snap)
      {
        // TODO: Use World Gen if no zone.
        ZoneSystem.instance.FindFloor(spawnPosition, out var height);
        // Fixes spawning below terrain.
        if (height == 0f)
        {
          var higher = spawnPosition;
          higher.y += 100f;
          ZoneSystem.instance.FindFloor(spawnPosition, out height);
        }
        spawnPosition.y = height;
      }
      var rotation = pars.BaseRotation * Quaternion.Euler(Helper.RandomValue(pars.Rotation));
      Vector3? scale = pars.Scale == null ? null : Helper.RandomValue(pars.Scale);
      DataHelper.Init(prefab, spawnPosition, rotation, scale, pars.Data, pars.DataParameters);
      try
      {
        var obj = UnityEngine.Object.Instantiate(prefab, spawnPosition, rotation);
        spawned.Add(obj);
        if (!ZNet.instance.IsServer())
          ZDOMan.instance.ClientChanged(obj.GetComponent<ZNetView>().GetZDO().m_uid);
      }
      catch (Exception e)
      {
        Debug.LogError(e);
      }
      DataHelper.CleanUp();
    }
    return spawned;
  }

  private static void Manipulate(IEnumerable<GameObject> spawned, SpawnObjectParameters pars, int total)
  {
    foreach (var obj in spawned)
    {
      var view = obj.GetComponent<ZNetView>();
      if (pars.Baby == true)
        Actions.SetBaby(obj);
      if (pars.Level != null)
        Actions.SetLevel(obj, Helper.RandomValue(pars.Level));
      if (pars.Damage != null)
        Actions.SetFloat(view, Helper.RandomValue(pars.Damage), Hash.Damage);
      if (pars.Ammo != null)
        Actions.SetInt(view, Helper.RandomValue(pars.Ammo), Hash.Ammo);
      if (pars.AmmoType != null)
        Actions.SetString(view, pars.AmmoType, Hash.AmmoType);
      if (pars.Health != null)
        Actions.SetHealth(obj, Helper.RandomValue(pars.Health), pars.isHealthPercentage);
      if (pars.Variant != null)
        Actions.SetVariant(obj, Helper.RandomValue(pars.Variant));
      if (pars.Name != null)
        Actions.SetName(obj, pars.Name);
      if (pars.CrafterId.HasValue)
        Actions.SetCrafterId(obj.GetComponent<ItemDrop>(), pars.CrafterId.Value);
      if (pars.Hunt.HasValue)
        Actions.SetHunt(obj, pars.Hunt.Value);
      if (pars.Tamed.HasValue)
        Actions.SetTame(obj, pars.Tamed.Value);
      total -= Actions.SetStack(obj, total);
      Actions.SetVisual(obj, VisSlot.Helmet, pars.Helmet);
      Actions.SetVisual(obj, VisSlot.Chest, pars.Chest);
      Actions.SetVisual(obj, VisSlot.Shoulder, pars.Shoulders);
      Actions.SetVisual(obj, VisSlot.Legs, pars.Legs);
      Actions.SetVisual(obj, VisSlot.Utility, pars.Utility);
      Actions.SetVisual(obj, VisSlot.HandLeft, pars.LeftHand);
      Actions.SetVisual(obj, VisSlot.HandRight, pars.RightHand);
      if (pars.Model != null)
        Actions.SetModel(obj, Helper.RandomValue(pars.Model));
      if (pars.Helmet != null || pars.Chest != null || pars.Shoulders != null || pars.Legs != null || pars.Utility != null || pars.LeftHand != null || pars.RightHand != null)
      {
        var zdo = obj.GetComponent<ZNetView>()?.GetZDO();
        // Temporarily losing the ownership prevents default items replacing the set items.
        zdo?.SetOwner(0);
      }
      if (pars.Fields.Count > 0)
        Actions.SetFields(obj, pars.Fields);
    }
  }
  public SpawnObjectCommand()
  {
    SpawnObjectAutoComplete autoComplete = new();
    Helper.Command(Name, "Spawns objects", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing object id.");
      var prefabName = args[1];
      var prefab = Helper.GetPrefab(prefabName);
      if (prefab == null) throw new InvalidOperationException("Unable to find the object.");

      SpawnObjectParameters pars = new(args);
      var itemDrop = prefab.GetComponent<ItemDrop>();
      var amount = Helper.RandomValue(pars.Amount);
      var count = amount;
      if (itemDrop)
        count = (int)Math.Ceiling((double)count / itemDrop.m_itemData.m_shared.m_maxStackSize);

      var spawned = SpawnObject(pars, prefab, count);
      Manipulate(spawned, pars, amount);
      Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, "Spawning object " + prefabName, spawned.Count, null);
      args.Context.AddString("Spawned: " + prefabName + " at " + Helper.PrintVectorXZY(pars.GetPosition()));
      var spawns = spawned.Select(obj => obj.GetComponent<ZNetView>()).Where(obj => obj != null).Select(obj => obj.GetZDO()).ToList();
      // from and refRot override the player based positioning (fixes undo position).
      var undoCommand = "spawn_object " + prefabName + " refRot=" + Helper.PrintAngleYXZ(pars.BaseRotation) + " from=" + Helper.PrintVectorXZY(pars.From) + " " + string.Join(" ", args.Args.Skip(2));
      UndoSpawn undo = new(spawns, undoCommand);
      UndoManager.Add(undo);
    });
  }
}
