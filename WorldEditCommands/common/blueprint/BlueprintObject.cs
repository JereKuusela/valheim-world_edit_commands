// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all.
using UnityEngine;

namespace Data;

public class BlueprintObject
{
  public string Prefab = "";
  public Vector3 Pos;
  public Quaternion Rot;
  public DataEntry? Data;
  public float Chance = 1f;
  // Objects of the same group are alternatives: one of them is picked, using their chances as probabilities.
  public string Group = "";
  public Vector3? Scale;
  public bool SnapToGround = false;
  // Raw info and data columns, kept so editors can save the object as it was read.
  public string Info = "";
  public string RawData = "";
  public BlueprintObject(string name, Vector3 pos, Quaternion rot, Vector3 scale, DataEntry? data, float chance, bool snapToGround = false)
  {
    Prefab = name;
    Pos = pos;
    Rot = rot.normalized;
    Data = data;
    Chance = chance;
    SnapToGround = snapToGround;
    // Some blueprints have the scale only in the data, so don't override it with the default.
    if (scale != Vector3.one)
      Scale = scale;
  }
}

public class SnapPoint
{
  public Vector3 Pos;
  public Quaternion Rot;
  public SnapPoint(Vector3 pos, Quaternion rot)
  {
    Pos = pos;
    Rot = rot;
  }
}

