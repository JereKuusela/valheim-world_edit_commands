
using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;

namespace Service;
public enum ObjectType {
  All,
  Character,
  Structure,
  Fireplace,
  Item,
  Chest,
  Spawner,
  SpawnPoint
}
public class Hovered {
  public ZNetView Obj;
  public int Index;
  public Hovered(ZNetView obj, int index) {
    Obj = obj;
    Index = index;
  }
}
public static class Selector {
  ///<summary>Helper to check object validity.</summary>
  public static bool IsValid(ZNetView view) => view && IsValid(view.GetZDO());
  ///<summary>Helper to check object validity.</summary>
  public static bool IsValid(ZDO zdo) => zdo != null && zdo.IsValid();

  ///<summary>Returns the hovered object.</summary>
  public static ZNetView? GetHovered(float range, string[] excluded) {
    var hovered = GetHovered(Player.m_localPlayer, range, excluded);
    if (hovered == null) return null;
    return hovered.Obj;
  }
  public static int GetPrefabFromHit(RaycastHit hit) => hit.collider.GetComponentInParent<ZNetView>().GetZDO().GetPrefab();
  public static Hovered? GetHovered(Player obj, float maxDistance, string[] excluded, bool allowOtherPlayers = false) {
    var excludedPrefabs = GetExcludedPrefabs(excluded);
    var raycast = Math.Max(maxDistance + 5f, 50f);
    var mask = LayerMask.GetMask(new string[]
    {
      "item",
      "piece",
      "piece_nonsolid",
      "Default",
      "static_solid",
      "Default_small",
      "character",
      "character_net",
      "terrain",
      "vehicle",
      "character_trigger" // Added to remove spawners with ESP mod.
    });
    var hits = Physics.RaycastAll(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, raycast, mask);
    Array.Sort<RaycastHit>(hits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
    foreach (var hit in hits) {
      if (Vector3.Distance(hit.point, obj.m_eye.position) >= maxDistance) continue;
      var netView = hit.collider.GetComponentInParent<ZNetView>();
      if (!IsValid(netView)) continue;
      if (excludedPrefabs.Contains(netView.GetZDO().GetPrefab())) continue;
      if (hit.collider.GetComponent<EffectArea>()) continue;
      var player = netView.GetComponentInChildren<Player>();
      if (player == obj) continue;
      if (!allowOtherPlayers && player) continue;
      var mineRock = netView.GetComponent<MineRock5>();
      var index = 0;
      if (mineRock)
        index = mineRock.GetAreaIndex(hit.collider);
      return new(netView, index);
    }
    return null;
  }
  private static float GetX(float x, float y, float angle) => Mathf.Cos(angle) * x - Mathf.Sin(angle) * y;
  private static float GetY(float x, float y, float angle) => Mathf.Sin(angle) * x + Mathf.Cos(angle) * y;
  public static bool Within(Vector3 position, Vector3 center, float angle, Range<float> width, Range<float> depth, float height) {
    var dx = position.x - center.x;
    var dz = position.z - center.z;
    var distanceX = GetX(dx, dz, angle);
    var distanceZ = GetY(dx, dz, angle);
    if (center.y - position.y > 1000f) return false;
    if (position.y - center.y > (height == 0f ? 1000f : height)) return false;
    return Helper.Within(width, depth, Mathf.Abs(distanceX), Mathf.Abs(distanceZ));
  }
  public static bool Within(Vector3 position, Vector3 center, Range<float> radius, float height) {
    return Helper.Within(radius, Utils.DistanceXZ(position, center)) && center.y - position.y < 1000f && position.y - center.y <= (height == 0f ? 1000f : height);
  }
  private static bool IsIncluded(string id, string name) {
    if (id.StartsWith("*", StringComparison.Ordinal) && id.EndsWith("*", StringComparison.Ordinal))
      return name.Contains(id.Substring(1, id.Length - 3));
    if (id.StartsWith("*", StringComparison.Ordinal)) return name.EndsWith(id.Substring(1), StringComparison.Ordinal);
    if (id.EndsWith("*", StringComparison.Ordinal)) return name.StartsWith(id.Substring(0, id.Length - 2), StringComparison.Ordinal);
    return id == name;
  }
  public static HashSet<int> GetExcludedPrefabs(string[] ids) {
    HashSet<int> prefabs = new();
    foreach (var id in ids)
      prefabs.UnionWith(GetExcludedPrefabs(id));
    return prefabs;
  }
  private static HashSet<int> GetExcludedPrefabs(string id) {
    if (id == "") return new();
    id = id.ToLower();
    return ZNetScene.instance.m_namedPrefabs.Values
      .Where(prefab => IsIncluded(id, prefab.name.ToLower()))
      .Select(prefab => prefab.name.GetStableHashCode())
      .ToHashSet();
  }
  public static HashSet<int> GetPrefabs(string[] ids) {
    if (ids.Length == 0) return GetPrefabs("*");
    HashSet<int> prefabs = new();
    foreach (var id in ids)
      prefabs.UnionWith(GetPrefabs(id));
    return prefabs;
  }
  private static HashSet<int> GetPrefabs(string id) {
    id = id.ToLower();
    IEnumerable<GameObject> values = ZNetScene.instance.m_namedPrefabs.Values;
    values = values.Where(prefab => prefab.name != "Player");
    if (id == "*" || id == "")
      values = values.Where(prefab => !prefab.name.StartsWith("_", StringComparison.Ordinal));
    else if (id.Contains("*"))
      values = values.Where(prefab => IsIncluded(id, prefab.name.ToLower()));
    else
      values = values.Where(prefab => prefab.name.ToLower() == id);
    return values.Select(prefab => prefab.name.GetStableHashCode()).ToHashSet();
  }
  public static ZNetView[] GetNearby(string[] included, ObjectType type, string[] excluded, Vector3 center, Range<float> radius, float height) {
    var includedPrefabs = GetPrefabs(included);
    var excludedPrefabs = GetExcludedPrefabs(excluded);
    var checker = (Vector3 pos) => Within(pos, center, radius, height);
    return GetNearby(includedPrefabs, type, excludedPrefabs, checker);
  }
  public static ZNetView[] GetNearby(string[] included, ObjectType type, string[] excluded, Vector3 center, float angle, Range<float> width, Range<float> depth, float height) {
    var includedPrefabs = GetPrefabs(included);
    var excludedPrefabs = GetExcludedPrefabs(excluded);
    var checker = (Vector3 pos) => Within(pos, center, angle, width, depth, height);
    return GetNearby(includedPrefabs, type, excludedPrefabs, checker);
  }
  public static ZNetView[] GetNearby(HashSet<int> included, ObjectType type, HashSet<int> excluded, Func<Vector3, bool> checker) {
    var scene = ZNetScene.instance;
    var objects = ZNetScene.instance.m_instances.Values;
    var views = objects.Where(IsValid);
    if (included.Count > 0)
      views = views.Where(view => included.Contains(view.GetZDO().GetPrefab()));
    if (excluded.Count > 0)
      views = views.Where(view => !excluded.Contains(view.GetZDO().GetPrefab()));
    views = views.Where(view => checker(view.GetZDO().GetPosition()));
    if (type == ObjectType.Structure)
      views = views.Where(view => view.GetComponent<Piece>());
    if (type == ObjectType.Character)
      views = views.Where(view => view.GetComponent<Character>());
    if (type == ObjectType.Fireplace)
      views = views.Where(view => view.GetComponent<Fireplace>());
    if (type == ObjectType.Item)
      views = views.Where(view => view.GetComponent<ItemDrop>());
    if (type == ObjectType.Chest)
      views = views.Where(view => view.GetComponent<Container>());
    if (type == ObjectType.Spawner)
      views = views.Where(view => view.GetComponent<SpawnArea>());
    if (type == ObjectType.SpawnPoint)
      views = views.Where(view => view.GetComponent<CreatureSpawner>());
    var objs = views.ToArray();
    if (objs.Length == 0) throw new InvalidOperationException("Nothing is nearby.");
    return objs;
  }
  private static KeyValuePair<int, int> RaftParent = ZDO.GetHashZDOID("MBParent");

  public static ZNetView[] GetConnectedRaft(ZNetView baseView, HashSet<int> excluded) {
    var id = baseView.GetZDO().GetZDOID(RaftParent);
    var instances = ZNetScene.instance.m_instances.Values;
    return instances
      .Where(IsValid)
      .Where(view => !excluded.Contains(view.GetZDO().m_prefab))
      .Where(view => view.GetZDO().m_uid == id || view.GetZDO().GetZDOID(RaftParent) == id)
      .ToArray();
  }
  ///<summary>Returns connected WearNTear objects.</summary>
  public static ZNetView[] GetConnected(ZNetView baseView, string[] excluded) {
    var excludedPrefabs = GetExcludedPrefabs(excluded);
    if (baseView.GetZDO().GetZDOID(RaftParent) != ZDOID.None) return GetConnectedRaft(baseView, excludedPrefabs);
    var baseWear = baseView.GetComponent<WearNTear>() ?? throw new InvalidOperationException("Connected doesn't work for this object.");
    HashSet<ZNetView> views = new() { baseView };
    Queue<WearNTear> todo = new();
    todo.Enqueue(baseWear);
    while (todo.Count > 0) {
      var wear = todo.Dequeue();
      if (wear.m_colliders == null) wear.SetupColliders();
      foreach (var boundData in wear.m_bounds) {
        var boxes = Physics.OverlapBoxNonAlloc(boundData.m_pos, boundData.m_size, WearNTear.m_tempColliders, boundData.m_rot, WearNTear.m_rayMask);
        for (int i = 0; i < boxes; i++) {
          var collider = WearNTear.m_tempColliders[i];
          if (collider.isTrigger || collider.attachedRigidbody != null || wear.m_colliders.Contains(collider)) continue;
          var wear2 = collider.GetComponentInParent<WearNTear>();
          if (!wear2 || !IsValid(wear2.m_nview)) continue;
          if (excludedPrefabs.Contains(wear2.m_nview.GetZDO().GetPrefab())) continue;
          if (views.Contains(wear2.m_nview)) continue;
          views.Add(wear2.m_nview);
          todo.Enqueue(wear2);
        }
      }
    }
    return views.ToArray();
  }
}