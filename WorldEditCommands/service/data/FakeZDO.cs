using UnityEngine;

namespace Data;
// Replicates ZDO from Valheim for an abstract ZDO that isn't in the world.
public class FakeZDO(ZDO zdo)
{
  public readonly PlainDataEntry Data = new(zdo);
  public int Prefab = zdo.m_prefab;
  public Vector3 Position = zdo.m_position;
  public Vector3 Rotation = zdo.m_rotation;
  public ZDOID Id = zdo.m_uid;


  public ZDO Create()
  {
    var zdo = ZDOMan.instance.CreateNewZDO(Position, Prefab);
    Write(zdo);
    zdo.DataRevision = 0;
    // This is needed to trigger the ZDO sync.
    zdo.IncreaseDataRevision();
    return zdo;
  }
  public void Write(ZDO zdo)
  {
    zdo.m_prefab = Prefab;
    zdo.m_position = Position;
    zdo.m_rotation = Rotation;
    Data.Write(zdo);
  }
  public void Destroy()
  {
    var zdo = ZDOMan.instance.GetZDO(Id);
    if (zdo == null) return;
    if (!zdo.IsOwner())
      zdo.SetOwner(ZDOMan.instance.m_sessionID);
    ZDOMan.instance.DestroyZDO(zdo);
  }
}
