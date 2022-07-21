using System;
using UnityEngine;
namespace WorldEditCommands;
public static class Actions {
  public static void SetTame(GameObject obj, bool tame) {
    SetTame(obj.GetComponent<Character>(), tame);
  }
  public static void SetTame(Character obj, bool tame) {
    if (!obj) return;
    obj.SetTamed(tame); // Ok to use (action sent to the owner).
    var AI = obj.GetComponent<BaseAI>();
    if (AI) {
      AI.SetAlerted(false);
      if (tame) {
        SetHunt(AI, false);
        AI.SetPatrolPoint();
      }
      AI.SetTargetInfo(ZDOID.None);
      var monster = obj.GetComponent<MonsterAI>();
      if (monster) {
        monster.m_targetCreature = null;
        monster.m_targetStatic = null;
        if (tame) {
          monster.SetDespawnInDay(false);
          monster.SetEventCreature(false);
        }
      }
      var animal = obj.GetComponent<AnimalAI>();
      if (animal) {
        animal.m_target = null;
      }
    }
  }
  public static void SetHunt(GameObject obj, bool hunt) {
    SetHunt(obj.GetComponent<BaseAI>(), hunt);
  }
  public static bool SetPrefab(GameObject obj, string prefab) {
    return SetPrefab(obj.GetComponent<ZNetView>(), prefab);
  }
  public static bool SetPrefab(ZNetView obj, string prefab) {
    if (!obj) return false;
    var zdo = obj.GetZDO();
    if (zdo == null || !zdo.IsValid()) return false;
    var previous = zdo.GetPrefab();
    zdo.SetPrefab(prefab.GetStableHashCode());
    var newObj = ZNetScene.instance.CreateObject(zdo);
    if (!newObj) {
      zdo.SetPrefab(previous);
      return false;
    }
    UnityEngine.Object.Destroy(obj.gameObject);
    ZNetScene.instance.m_instances[zdo] = newObj.GetComponent<ZNetView>();
    return true;
  }
  private static int HashHuntplayer = "huntplayer".GetStableHashCode();
  public static void SetHunt(BaseAI obj, bool hunt) {
    if (!obj) return;
    obj.m_huntPlayer = hunt;
    obj.m_nview.GetZDO().Set(HashHuntplayer, hunt);
  }
  public static void SetFuel(GameObject obj, float amount) {
    SetFuel(obj.GetComponent<Fireplace>(), amount);
  }
  private static int HashFuel = "fuel".GetStableHashCode();
  public static void SetFuel(Fireplace obj, float amount) {
    if (!obj) return;
    obj.m_nview.GetZDO().Set(HashFuel, amount);
  }
  public static void SetSleeping(GameObject obj, bool sleep) {
    SetSleeping(obj.GetComponent<MonsterAI>(), sleep);
  }
  private static int HashSleeping = "sleeping".GetStableHashCode();
  public static void SetSleeping(MonsterAI obj, bool sleep) {
    if (!obj) return;
    obj.m_sleeping = sleep;
    obj.m_nview.GetZDO().Set(HashSleeping, sleep);
  }
  public static void SetBaby(GameObject obj) {
    SetBaby(obj.GetComponent<Growup>());
  }
  private static int HashSpawntime = "spawntime".GetStableHashCode();
  public static void SetBaby(Growup obj) {
    if (!obj) return;
    obj.m_nview.GetZDO().Set(HashSpawntime, DateTime.MaxValue.Ticks);
  }
  public static void SetLevel(GameObject obj, int level) {
    if (level < 1) return;
    SetLevel(obj.GetComponent<ItemDrop>(), level);
    SetLevel(obj.GetComponent<Character>(), level);
  }
  public static void SetLevel(Character obj, int level) {
    if (!obj) return;
    if (obj.GetLevel() != level)
      obj.SetLevel(level); // Ok to use (no owner check).
  }
  private static int HashQuality = "quality".GetStableHashCode();
  public static void SetLevel(ItemDrop obj, int level) {
    if (!obj) return;
    obj.m_itemData.m_quality = level;
    obj.m_nview.GetZDO().Set(HashQuality, level);
  }
  private static int HashHealth = "health".GetStableHashCode();
  public static float GetHealth(ZNetView obj) {
    var zdo = obj.GetZDO();
    var itemDrop = obj.GetComponent<ItemDrop>();
    if (itemDrop) return itemDrop.m_itemData.m_durability;
    var character = obj.GetComponent<Character>();
    if (character) return character.GetHealth();
    var wearNTear = obj.GetComponent<WearNTear>();
    if (wearNTear) return zdo.GetFloat(HashHealth, wearNTear.m_health);
    var destructible = obj.GetComponent<Destructible>();
    if (destructible) return zdo.GetFloat(HashHealth, destructible.m_health);
    var treeLog = obj.GetComponent<TreeLog>();
    if (treeLog) return zdo.GetFloat(HashHealth, treeLog.m_health);
    var treeBase = obj.GetComponent<TreeBase>();
    if (treeBase) return zdo.GetFloat(HashHealth, treeBase.m_health);
    return -1;
  }
  public static float SetHealth(GameObject obj, float health) {
    var itemDrop = obj.GetComponent<ItemDrop>();
    if (itemDrop) return SetHealth(itemDrop, health);
    var character = obj.GetComponent<Character>();
    if (character) return SetHealth(character, health);
    var wearNTear = obj.GetComponent<WearNTear>();
    if (wearNTear) return SetHealth(wearNTear, health);
    var treeLog = obj.GetComponent<TreeLog>();
    if (treeLog) return SetHealth(treeLog, health);
    var treeBase = obj.GetComponent<TreeBase>();
    if (treeBase) return SetHealth(treeBase, health);
    var destructible = obj.GetComponent<Destructible>();
    if (destructible) return SetHealth(destructible, health);
    return 0;
  }
  public static float SetHealth(Character obj, float health) {
    if (!obj) return 0f;
    var previous = obj.GetMaxHealth();
    if (health == 0) {
      obj.SetupMaxHealth();
      return previous;
    }
    obj.SetMaxHealth(health);
    // Max health resets on awake if health is equal to max.
    obj.SetHealth(health * 1.000001f);
    return previous;
  }
  public static float SetHealth(WearNTear obj, float health) {
    if (!obj) return 0f;
    if (health == 0) health = obj.m_health;
    var previous = obj.m_nview.GetZDO().GetFloat(HashHealth, obj.m_health);
    obj.m_nview.GetZDO().Set(HashHealth, health);
    return previous;
  }
  public static float SetHealth(TreeLog obj, float health) {
    if (!obj) return 0f;
    if (health == 0) health = obj.m_health;
    var previous = obj.m_nview.GetZDO().GetFloat(HashHealth, obj.m_health);
    obj.m_nview.GetZDO().Set(HashHealth, health);
    return previous;
  }
  public static float SetHealth(Destructible obj, float health) {
    if (!obj) return 0f;
    if (health == 0) health = obj.m_health;
    var previous = obj.m_nview.GetZDO().GetFloat(HashHealth, obj.m_health);
    obj.m_nview.GetZDO().Set(HashHealth, health);
    return previous;
  }
  public static float SetHealth(TreeBase obj, float health) {
    if (!obj) return 0f;
    if (health == 0) health = obj.m_health;
    var previous = obj.m_nview.GetZDO().GetFloat(HashHealth, obj.m_health);
    obj.m_nview.GetZDO().Set(HashHealth, health);
    return previous;
  }
  private static int HashDurability = "durability".GetStableHashCode();
  public static float SetHealth(ItemDrop obj, float health) {
    if (!obj) return 0f;
    if (health == 0) health = obj.m_itemData.GetMaxDurability();
    var previous = obj.m_itemData.m_durability;
    obj.m_itemData.m_durability = health;
    obj.m_nview.GetZDO().Set(HashDurability, obj.m_itemData.m_durability);
    return previous;
  }

  public static float SetCreator(GameObject obj, long creator) {
    if (obj.GetComponent<Piece>() is { } piece) return SetCreator(piece, creator);
    return 0;
  }
  private static int HashCreator = "creator".GetStableHashCode();
  public static long SetCreator(Piece obj, long creator) {
    if (!obj) return 0L;
    var previous = obj.GetCreator();
    obj.m_creator = creator;
    obj.m_nview.GetZDO().Set(HashCreator, creator);
    return previous;
  }
  public static void SetVariant(GameObject obj, int variant) {
    SetVariant(obj.GetComponent<ItemDrop>(), variant);
  }
  private static int HashVariant = "variant".GetStableHashCode();
  public static void SetVariant(ItemDrop obj, int variant) {
    if (!obj) return;
    obj.m_itemData.m_variant = variant;
    obj.m_nview.GetZDO().Set(HashVariant, variant);
  }
  public static void SetName(GameObject obj, string name) {
    SetName(obj.GetComponent<Tameable>(), name);
    SetName(obj.GetComponent<ItemDrop>(), name);
  }
  private static int HashTamedName = "TamedName".GetStableHashCode();
  public static void SetName(Tameable obj, string name) {
    if (!obj) return;
    obj.m_nview.GetZDO().Set(HashTamedName, name);
  }
  private static int HashCrafterID = "crafterID".GetStableHashCode();
  private static int HashCrafterName = "crafterName".GetStableHashCode();
  public static void SetName(ItemDrop obj, string name) {
    if (!obj) return;
    obj.m_itemData.m_crafterID = name == "" ? 0 : -1;
    obj.m_nview.GetZDO().Set(HashCrafterID, obj.m_itemData.m_crafterID);
    obj.m_itemData.m_crafterName = name;
    obj.m_nview.GetZDO().Set(HashCrafterName, name);
  }
  private static int HashStack = "stack".GetStableHashCode();
  public static int SetStack(GameObject obj, int remaining) {
    if (remaining <= 0) return 0;
    var item = obj.GetComponent<ItemDrop>();
    if (!item) return 0;
    var stack = Math.Min(remaining, item.m_itemData.m_shared.m_maxStackSize);
    item.m_itemData.m_stack = stack;
    item.m_nview.GetZDO().Set(HashStack, stack);
    return stack;
  }
  public static void SetRotation(GameObject obj, Quaternion rotation) {
    var view = obj.GetComponent<ZNetView>();
    if (view == null) return;
    if (rotation != Quaternion.identity) {
      view.GetZDO().SetRotation(rotation);
      obj.transform.rotation = rotation;
    }
  }
  public static void SetScale(GameObject obj, Vector3 scale) {
    var view = obj.GetComponent<ZNetView>();
    if (view == null) return;
    if (scale != Vector3.one && view.m_syncInitialScale)
      view.SetLocalScale(scale);
  }
  public static void Respawn(GameObject obj) {
    Respawn(obj.GetComponent<Container>());
    Respawn(obj.GetComponent<Pickable>());
    Respawn(obj.GetComponent<CreatureSpawner>());
  }
  public static void Respawn(Container obj) {
    if (!obj) return;
    if (obj.m_defaultItems.m_drops.Count == 0) return;
    obj.m_inventory?.RemoveAll();
    obj.AddDefaultItems();
  }
  public static bool CanRespawn(GameObject obj) =>
    obj.GetComponent<Pickable>() || obj.GetComponent<CreatureSpawner>() || (obj.GetComponent<Container>()?.m_defaultItems.m_drops.Count > 0);

  public static void Respawn(Pickable obj) {
    if (!obj) return;
    obj.SetPicked(false);
  }
  private static int HashAliveTime = "alive_time".GetStableHashCode();
  public static void Respawn(CreatureSpawner obj) {
    if (!obj) return;
    obj.m_nview?.GetZDO().Set("spawn_id", ZDOID.None);
    obj.m_nview?.GetZDO().Set(HashAliveTime, 0L);
  }
  public static void SetModel(GameObject obj, int index) {
    SetModel(obj.GetComponent<Character>(), index);
  }
  public static void SetModel(Character obj, int index) {
    if (!obj) return;
    var equipment = obj.GetComponent<VisEquipment>();
    if (!equipment) return;
    equipment.SetModel(index);
  }
  public static void SetVisual(GameObject obj, Item item) {
    SetVisual(obj.GetComponent<ItemStand>(), item);
  }
  public static void SetVisual(GameObject obj, VisSlot slot, Item? item) {
    SetVisual(obj.GetComponent<Character>(), slot, item);
  }
  private static int HashItem = "item".GetStableHashCode();
  public static void SetVisual(ItemStand obj, Item? item) {
    if (!obj || item == null) return;
    obj.m_nview.GetZDO().Set(HashItem, item.Name);
    obj.m_nview.GetZDO().Set(HashVariant, item.Variant);
    obj.UpdateVisual();
  }
  public static void SetVisual(Character obj, VisSlot slot, Item? item) {
    if (!obj || item == null) return;
    var equipment = obj.GetComponent<VisEquipment>();
    if (!equipment) return;
    equipment.SetItem(slot, item.Name, item.Variant);
  }
  public static void Move(ZNetView obj, Vector3 offset, string origin) {
    var zdo = obj.GetZDO();
    var position = obj.transform.position;
    var rotation = Player.m_localPlayer.transform.rotation;
    if (origin == "world")
      rotation = Quaternion.identity;
    if (origin == "object")
      rotation = obj.transform.rotation;
    position += rotation * Vector3.right * offset.x;
    position += rotation * Vector3.up * offset.y;
    position += rotation * Vector3.forward * offset.z;
    zdo.SetPosition(position);
    obj.transform.position = position;
  }

  public static void Rotate(ZNetView obj, Vector3 relative, string origin, Vector3? center = null) {
    var zdo = obj.GetZDO();
    var originRotation = Player.m_localPlayer.transform.rotation;
    if (origin == "world")
      originRotation = Quaternion.identity;
    if (origin == "object")
      originRotation = obj.transform.rotation;
    var transform = obj.transform;
    var position = center ?? transform.position;
    transform.RotateAround(position, originRotation * Vector3.up, relative.y);
    transform.RotateAround(position, originRotation * Vector3.forward, relative.x);
    transform.RotateAround(position, originRotation * Vector3.right, relative.z);
    zdo.SetRotation(obj.transform.rotation);
  }
  public static void Mirror(ZNetView obj, Vector3 center) {
    var tr = obj.transform;
    tr.position = new(2 * center.x - tr.position.x, tr.position.y, tr.position.z);
    var angles = tr.eulerAngles;
    tr.rotation = Quaternion.Euler(angles.x, -angles.y, angles.z);
    var zdo = obj.GetZDO();
    zdo.SetPosition(tr.position);
    zdo.SetRotation(tr.rotation);
  }
  public static void ResetRotation(ZNetView obj) {
    var zdo = obj.GetZDO();
    zdo.SetRotation(Quaternion.identity);
    obj.transform.rotation = Quaternion.identity;
  }
  public static void Scale(ZNetView obj, Vector3 scale) {
    if (obj.m_syncInitialScale)
      obj.SetLocalScale(scale);
  }

  public static void RemoveZDO(ZDO zdo) {
    if (zdo == null || !zdo.IsValid()) return;
    if (!zdo.IsOwner())
      zdo.SetOwner(ZDOMan.instance.GetMyID());
    if (ZNetScene.instance.m_instances.TryGetValue(zdo, out var view))
      ZNetScene.instance.Destroy(view.gameObject);
    else
      ZDOMan.instance.DestroyZDO(zdo);
  }
}
