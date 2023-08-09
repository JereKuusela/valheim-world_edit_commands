using System;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace WorldEditCommands;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("server_devcommands", "1.52")]
public class WorldEditCommands : BaseUnityPlugin
{
  public const string GUID = "world_edit_commands";
  public const string NAME = "World Edit Commands";
  public const string VERSION = "1.41";
  public void Awake()
  {
    new Harmony(GUID).PatchAll();
  }
  public static bool IsSpawnerTweaks = false;
  public static bool IsStructureTweaks = false;
  public static bool IsCLLC = false;
  public void Start()
  {
    IsSpawnerTweaks = Chainloader.PluginInfos.ContainsKey("spawner_tweaks") || Chainloader.PluginInfos.ContainsKey("logic_tweaks");
    IsStructureTweaks = Chainloader.PluginInfos.ContainsKey("structure_tweaks") || Chainloader.PluginInfos.ContainsKey("logic_tweaks");
    IsCLLC = Chainloader.PluginInfos.ContainsKey("rg.bepinex.plugins.creaturelevelcontrol");
  }
}
[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
public class SetCommands
{
  static void Postfix()
  {
    new SpawnLocationCommand();
    new SpawnObjectCommand();
    new ObjectCommand();
    new TerrainCommand();
    new AliasesCommand();
    new DungeonCommand();
    if (WorldEditCommands.IsSpawnerTweaks || WorldEditCommands.IsStructureTweaks)
      new TweakObjectCommand();
    if (WorldEditCommands.IsSpawnerTweaks)
    {
      new TweakAltarCommand();
      new TweakBeehiveCommand();
      new TweakPickableCommand();
      new TweakSpawnerCommand();
      new TweakSpawnPointCommand();
      new TweakItemStandCommand();
      new TweakChestCommand();
      new TweakCreatureCommand();
      new TweakSmelterCommand();
      new TweakFermenterCommand();
    }
    if (WorldEditCommands.IsStructureTweaks)
    {
      new TweakDungeonCommand();
      new TweakRunestoneCommand();
      new TweakPortalCommand();
      new TweakFireplaceCommand();
      new TweakChestCommand();
      new TweakDoorCommand();
    }
  }
}

[HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.Awake))]
public class ASDSA
{

  static void Postfix(MonsterAI __instance)
  {
    if (Utils.GetPrefabName(__instance.gameObject) == "Lox")
    {
      var asd = __instance.gameObject.AddComponent<SpawnPrefabRiderLox>();
      asd.m_spawnPrefab = ZNetScene.instance.GetPrefab("Goblin");
    }
  }
}
public class SpawnPrefabRiderLox : MonoBehaviour
{
  public GameObject m_spawnPrefab;

  private ZNetView m_nview;
  // Cache the rider so it doesn't have to be searched for every update.
  private GameObject m_rider;
  private bool m_riderCached = false;

  public void Start()
  {
    var character = GetComponentInParent<Character>();
    character.m_onDeath = (Action)Delegate.Combine(character.m_onDeath, new Action(DismountRider));
    m_nview = GetComponentInParent<ZNetView>();
    // Check ownership to only spawn the rider once.
    if (!m_nview.IsOwner()) return;
    if (HasSpawnedRider()) return;
    SpawnRider();
    UpdateRider();
  }
  public void LateUpdate()
  {
    // Updates can happen for clients because the rider data is not modified.
    if (!m_riderCached)
      TryCacheRider();
    UpdateRider();
  }

  private void SpawnRider()
  {
    m_rider = Instantiate(m_spawnPrefab, transform.transform.position, transform.transform.rotation);
    var id = m_rider.GetComponent<ZNetView>().GetZDO().m_uid;
    m_nview.GetZDO().SetConnection(ZDOExtraData.ConnectionType.Spawned, id);
    m_riderCached = true;
  }
  private void UpdateRider()
  {
    if (!m_rider) return;
    if (m_rider.layer != 17)
    {
      m_rider.GetComponent<Rigidbody>().isKinematic = true;
      m_rider.layer = 17;
    }
    m_rider.transform.position = transform.position;
    m_rider.transform.rotation = transform.rotation;
  }
  private void DismountRider()
  {
    if (!m_rider) return;
    m_rider.GetComponent<Rigidbody>().isKinematic = false;
    m_rider.layer = 9;
  }
  private bool HasSpawnedRider() => m_nview.GetZDO().GetConnection() != null;
  private void TryCacheRider()
  {
    var id = m_nview.GetZDO().GetConnection();
    if (id == null) return;
    m_riderCached = true;
    m_rider = ZNetScene.instance.FindInstance(id.m_target);
  }
}