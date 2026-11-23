using System;
using System.Collections.Generic;
using Data;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;

public class SpawnObjectCommand
{
  public const string Name = "spawn_object";
  private static List<GameObject> SpawnObject(SpawnObjectParameters pars, GameObject prefab, int count, int amount)
  {
    List<GameObject> spawned = [];
    var data = pars.BuildSpawnData(prefab);
    var distantView = pars.Distant.HasValue ? prefab.GetComponent<ZNetView>() : null;
    for (int i = 0; i < count; i++)
    {
      var spawnPosition = pars.GetSpawnPosition(i, count);
      if (pars.Snap)
      {
        // TODO: Use World Gen if no zone.
        ZoneSystem.instance.FindFloor(spawnPosition, out var height);
        // Fixes spawning below terrain.
        if (height == 0f)
        {
          var higher = spawnPosition;
          higher.y += 100f;
          ZoneSystem.instance.FindFloor(higher, out height);
        }
        spawnPosition.y = height;
      }
      var rotation = pars.BaseRotation * Quaternion.Euler(Helper.RandomValue(pars.Rotation));
      Vector3? scale = pars.Scale == null ? null : Helper.RandomValue(pars.Scale);
      var originalDistant = distantView != null && distantView.m_distant;
      try
      {
        var zdo = DataHelper.Init(prefab, spawnPosition, rotation, scale, data.Entry, data.Parameters, data.WriteOverrides != null);
        var stack = zdo == null ? 0 : data.WriteOverrides?.Invoke(zdo, amount) ?? 0;
        if (distantView != null && zdo != null)
          distantView.m_distant = zdo.Distant;
        var obj = UnityEngine.Object.Instantiate(prefab, spawnPosition, rotation);
        amount -= stack;
        spawned.Add(obj);
        if (!ZNet.instance.IsServer())
          ZDOMan.instance.ClientChanged(obj.GetComponent<ZNetView>().GetZDO().m_uid);
      }
      catch (Exception e)
      {
        Debug.LogError(e);
      }
      finally
      {
        if (distantView != null)
          distantView.m_distant = originalDistant;
        DataHelper.CleanUp();
      }
    }
    return spawned;
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
      var amount = pars.Amount == null ? 1 : Helper.RandomValue(pars.Amount);
      var count = amount;
      if (itemDrop)
        count = (int)Math.Ceiling((double)count / itemDrop.m_itemData.m_shared.m_maxStackSize);

      List<GameObject> spawned = [];
      UndoHelper.Run(() => spawned = SpawnObject(pars, prefab, count, amount));
      Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, "Spawning object " + prefabName, spawned.Count, null);
      args.Context.AddString("Spawned: " + prefabName + " at " + Helper.PrintVectorXZY(pars.GetPosition()));
    });
  }
}
