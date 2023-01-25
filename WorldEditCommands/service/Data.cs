using UnityEngine;
namespace Service;

public class DataHelper
{
  public static void Init(GameObject obj, ZDO? zdo, Vector3 pos, Quaternion rot, Vector3 scale)
  {
    if (!obj.TryGetComponent<ZNetView>(out var view)) return;
    ZNetView.m_initZDO = ZDOMan.instance.CreateNewZDO(pos);
    if (zdo != null)
      Copy(zdo, ZNetView.m_initZDO);
    ZNetView.m_initZDO.m_rotation = rot;
    ZNetView.m_initZDO.m_type = view.m_type;
    ZNetView.m_initZDO.m_distant = view.m_distant;
    ZNetView.m_initZDO.m_persistent = view.m_persistent;
    ZNetView.m_initZDO.m_prefab = Utils.GetPrefabName(obj).GetStableHashCode();
    if (view.m_syncInitialScale)
      ZNetView.m_initZDO.Set("scale", scale);
    ZNetView.m_initZDO.m_dataRevision = 1;
  }
  public static void CleanUp()
  {
    ZNetView.m_initZDO = null;
  }
  public static ZDO Load(string data)
  {
    ZDO zdo = new();
    if (data != "")
    {
      ZPackage pkg = new(data);
      Deserialize(zdo, pkg);
    }
    return zdo;
  }
  private static void Deserialize(ZDO zdo, ZPackage pkg)
  {
    int num = pkg.ReadInt();
    if ((num & 1) != 0)
    {
      zdo.InitFloats();
      int num2 = (int)pkg.ReadByte();
      for (int i = 0; i < num2; i++)
      {
        int key = pkg.ReadInt();
        zdo.m_floats[key] = pkg.ReadSingle();
      }
    }
    else
    {
      zdo.ReleaseFloats();
    }
    if ((num & 2) != 0)
    {
      zdo.InitVec3();
      int num3 = (int)pkg.ReadByte();
      for (int j = 0; j < num3; j++)
      {
        int key2 = pkg.ReadInt();
        zdo.m_vec3[key2] = pkg.ReadVector3();
      }
    }
    else
    {
      zdo.ReleaseVec3();
    }
    if ((num & 4) != 0)
    {
      zdo.InitQuats();
      int num4 = (int)pkg.ReadByte();
      for (int k = 0; k < num4; k++)
      {
        int key3 = pkg.ReadInt();
        zdo.m_quats[key3] = pkg.ReadQuaternion();
      }
    }
    else
    {
      zdo.ReleaseQuats();
    }
    if ((num & 8) != 0)
    {
      zdo.InitInts();
      int num5 = (int)pkg.ReadByte();
      for (int l = 0; l < num5; l++)
      {
        int key4 = pkg.ReadInt();
        zdo.m_ints[key4] = pkg.ReadInt();
      }
    }
    else
    {
      zdo.ReleaseInts();
    }
    if ((num & 64) != 0)
    {
      zdo.InitLongs();
      int num6 = (int)pkg.ReadByte();
      for (int m = 0; m < num6; m++)
      {
        int key5 = pkg.ReadInt();
        zdo.m_longs[key5] = pkg.ReadLong();
      }
    }
    else
    {
      zdo.ReleaseLongs();
    }
    if ((num & 16) != 0)
    {
      zdo.InitStrings();
      int num7 = (int)pkg.ReadByte();
      for (int n = 0; n < num7; n++)
      {
        int key6 = pkg.ReadInt();
        zdo.m_strings[key6] = pkg.ReadString();
      }
    }
    else
    {
      zdo.ReleaseStrings();
    }
    if ((num & 128) != 0)
    {
      zdo.InitByteArrays();
      int num8 = (int)pkg.ReadByte();
      for (int num9 = 0; num9 < num8; num9++)
      {
        int key7 = pkg.ReadInt();
        zdo.m_byteArrays[key7] = pkg.ReadByteArray();
      }
      return;
    }
    zdo.ReleaseByteArrays();
  }
  public static void Copy(ZDO from, ZDO to)
  {
    to.m_floats = from.m_floats;
    to.m_vec3 = from.m_vec3;
    to.m_quats = from.m_quats;
    to.m_ints = from.m_ints;
    to.m_longs = from.m_longs;
    to.m_strings = from.m_strings;
    to.m_byteArrays = from.m_byteArrays;
    to.IncreseDataRevision();
  }
}