using UnityEngine;

namespace Data;
// Replicates ZDO from Valheim for an abstract ZDO that isn't in the world.
public class FakeZDO(ZDO zdo)
{
  public readonly PlainDataEntry Data = new(zdo);
  public readonly ZDO Source = zdo.Clone();
  public int Prefab => Source.m_prefab;
  public Vector3 Position => Source.m_position;

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
    zdo.m_prefab = Source.m_prefab;
    zdo.m_position = Source.m_position;
    zdo.m_rotation = Source.m_rotation;
    zdo.Type = Source.Type;
    zdo.Distant = Source.Distant;
    zdo.Persistent = Source.Persistent;
    Data.Write(zdo);
  }
  public void Destroy()
  {
    if (!Source.IsOwner())
      Source.SetOwner(ZDOMan.instance.m_sessionID);
    ZDOMan.instance.DestroyZDO(Source);
  }
}
