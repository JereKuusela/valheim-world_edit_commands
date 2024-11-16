using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using HarmonyLib;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;

public class UndoHelper
{
  // Dictionary is needed because there can be multiple edits on the same object.
  private static readonly Dictionary<ZDOID, EditData> EditedInfo = [];
  private static readonly List<RemoveData> RemovedInfo = [];
  // Collect actual ZDOs in case they get modified after being spawned.
  private static readonly List<ZDO> SpawnedInfo = [];
  public static void BeginAction()
  {
    SubStack = 0;
    AddedZDOs.Track = true;
    EditedInfo.Clear();
    RemovedInfo.Clear();
    SpawnedInfo.Clear();
  }
  public static List<ZDO> GetSpawned() => SpawnedInfo;
  // Bit of a hack in case of nested actions.
  // Normally using the stack should be avoided because exceptions would leave it in an inconsistent state.
  private static int SubStack = 0;
  public static void BeginSubAction()
  {
    if (SubStack == 0) BeginAction();
    SubStack++;
  }
  public static void EndSubAction()
  {
    SubStack--;
    if (SubStack == 0) EndAction();
  }
  public static void AddRemoveAction(ZDO zdo)
  {
    RemovedInfo.Add(new(zdo));
    EditedInfo.Remove(zdo.m_uid);
  }
  public static void AddSpawnAction(ZDO zdo)
  {
    SpawnedInfo.Add(zdo);
  }

  public static void AddEditAction(ZNetView view) => AddEditAction(view.GetZDO());

  public static void AddEditAction(ZDO zdo)
  {
    if (zdo.GetPrefab() == Hash.Player) return;
    if (EditedInfo.ContainsKey(zdo.m_uid)) return;
    EditedInfo[zdo.m_uid] = new EditData(zdo);
  }
  public static void RefreshEditAction(ZDOID original, ZDO zdo)
  {
    var entry = EditedInfo[original];
    entry.Original = zdo;
    EditedInfo[zdo.m_uid] = entry;
    EditedInfo.Remove(original);
  }
  public static void EndAction()
  {
    SubStack = 0;
    AddedZDOs.Track = false;
    if (EditedInfo.Count == 0 && RemovedInfo.Count == 0 && SpawnedInfo.Count == 0) return;
    foreach (var data in EditedInfo.Values)
      data.Update();
    // Someone reported null reference error related to spawn action.
    // So there is a check to not get blamed for it.
    UndoData[] undoData = [.. EditedInfo.Values, .. RemovedInfo, .. SpawnedInfo.Where(zdo => zdo != null && zdo.IsValid()).Select(s => new SpawnData(s))];
    if (undoData.Length == 0) return;
    UndoAction action = new(undoData);
    UndoManager.Add(action);
  }
  public static void Place(FakeZDO zdo)
  {
    var prefab = ZNetScene.instance.GetPrefab(zdo.Prefab);
    if (!prefab) throw new InvalidOperationException("Error: Prefab not found.");
    ZNetView.m_initZDO = zdo.Create();
    Regenerated[zdo.Id] = ZNetView.m_initZDO.m_uid;
    zdo.Id = ZNetView.m_initZDO.m_uid;
    UnityEngine.Object.Instantiate(prefab, ZNetView.m_initZDO.GetPosition(), ZNetView.m_initZDO.GetRotation());
  }
  // Redoing an object makes a new instance so have to keep track of the situation.
  private static readonly Dictionary<ZDOID, ZDOID> Regenerated = [];
  public static void Destroy(FakeZDO zdo)
  {
    while (Regenerated.ContainsKey(zdo.Id))
      zdo.Id = Regenerated[zdo.Id];
    zdo.Destroy();
  }

  public static string Name(int hash) => Utils.GetPrefabName(ZNetScene.instance.GetPrefab(hash));
  public static string Print(IEnumerable<FakeZDO> zdos) => Print(zdos.Select(zdo => zdo.Prefab));
  public static string Print(IEnumerable<int> data)
  {
    if (data.Count() == 1) return Name(data.First());
    var names = data.GroupBy(Name);
    if (names.Count() == 1) return $"{names.First().Key} {names.First().Count()}x";
    return $" objects {data.Count()}x";
  }
}


public class EditData : UndoData
{
  public EditData(ZDO zdo) : base(new(zdo))
  {
    Original = zdo;
    Previous = Current;
  }
  public void Update()
  {
    Current = new(Original);
  }
  public override void Undo()
  {
    UndoHelper.Destroy(Current);
    UndoHelper.Place(Previous);
  }
  public override void Redo()
  {
    UndoHelper.Destroy(Previous);
    UndoHelper.Place(Current);
  }

  public FakeZDO Previous;
  public ZDO Original;
}

public class RemoveData(ZDO zdo) : UndoData(new(zdo))
{
  public override void Undo()
  {
    UndoHelper.Place(Current);
  }
  public override void Redo()
  {
    UndoHelper.Destroy(Current);
  }
}
public class SpawnData(ZDO zdo) : UndoData(new(zdo))
{
  public override void Undo()
  {
    UndoHelper.Destroy(Current);
  }
  public override void Redo()
  {
    UndoHelper.Place(Current);
  }
}

public abstract class UndoData(FakeZDO zdo)
{
  public FakeZDO Current = zdo;
  public abstract void Undo();
  public abstract void Redo();
}

public class UndoAction(IEnumerable<UndoData> data) : MonoBehaviour, IUndoAction
{
  public readonly UndoData[] Data = data.ToArray();

  public string Undo()
  {
    var message = $"Changed {UndoHelper.Print(Data.Select(data => data.Current))}";
    foreach (var data in Data)
      data.Undo();
    return message;
  }

  public string Redo()
  {
    var message = $"Changed {UndoHelper.Print(Data.Select(data => data.Current))}";
    foreach (var data in Data)
      data.Redo();
    return message;
  }
}

///<summary>Catch new zdos for undo.</summary>
[HarmonyPatch(typeof(ZNetView), "Awake")]
public class AddedZDOs
{
  public static bool Track = false;
  public static void Postfix(ZNetView __instance)
  {
    if (Track) UndoHelper.AddSpawnAction(__instance.GetZDO());
  }
}
