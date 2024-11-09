using UnityEngine;

namespace Data;
// Replicates ZDO from Valheim for an abstract ZDO that isn't in the world.
public class FakeZDO(ZDO zdo)
{
  public readonly PlainDataEntry Data = new(zdo);
  public int Prefab = zdo.m_prefab;
  public Vector3 Position = zdo.m_position;
  public Vector3 Rotation = zdo.m_rotation;
  public ZDO.ObjectType Type = zdo.Type;
  public bool Distant = zdo.Distant;
  public bool Persistent = zdo.Persistent;
  public uint DataRevision = zdo.DataRevision;
  public ushort OwnerRevision = zdo.OwnerRevision;
  public ZDOID Id = zdo.m_uid;


  public ZDO CreateNew()
  {
    var zdo = ZDOMan.instance.CreateNewZDO(Position, Prefab);
    Write(zdo);
    zdo.DataRevision = 0;
    // This is needed to trigger the ZDO sync.
    zdo.IncreaseDataRevision();
    return zdo;
  }
  public ZDO Regenerate()
  {
    Destroy();
    // Since the previous ZDO is always destroyed, it's safe to reuse the same ID.
    var zdo = ZDOMan.instance.CreateNewZDO(Id, Position, Prefab);
    Write(zdo);
    zdo.DataRevision = DataRevision;
    zdo.OwnerRevision = OwnerRevision;
    // This is needed to trigger the ZDO sync.
    zdo.IncreaseDataRevision();
    return zdo;
  }
  public void Write(ZDO zdo)
  {
    zdo.m_prefab = Prefab;
    zdo.m_position = Position;
    zdo.m_rotation = Rotation;
    zdo.Type = Type;
    zdo.Distant = Distant;
    zdo.Persistent = Persistent;
    Data.Write(zdo);
  }
  public void Destroy()
  {
    var zdo = ZDOMan.instance.GetZDO(Id);
    if (zdo == null) return;
    // Revision might have changed since this FakeZDO was created.
    // Updating it ensures changes are synced to other clients.
    if (!zdo.IsOwner())
      zdo.SetOwner(ZDOMan.instance.m_sessionID);
    DataRevision = zdo.DataRevision;
    OwnerRevision = zdo.OwnerRevision;
    ZDOMan.instance.DestroyZDO(zdo);
  }
}
