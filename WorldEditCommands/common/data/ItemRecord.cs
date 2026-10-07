// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System.Collections.Generic;
using UnityEngine;

namespace Data;

/// <summary>Plain data snapshot of an item, decoupled from ItemDrop.ItemData/prefab lifetime.</summary>
public class ItemRecord
{
  public int PrefabHash;
  public string PrefabName = "";
  public int Stack;
  public float Durability;
  public Vector2i GridPos;
  public bool Equipped;
  public int Quality = 1;
  public int Variant;
  public long CrafterID;
  public string CrafterName = "";
  public Dictionary<string, string> CustomData = [];
  public int WorldLevel;
  public bool PickedUp;
  public bool Cheated;
}
