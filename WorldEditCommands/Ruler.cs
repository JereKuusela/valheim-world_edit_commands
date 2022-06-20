using System;
using UnityEngine;
namespace WorldEditCommands;

public class RulerParameters {
  public float? Radius;
  public float? Width;
  public float? Depth;
  public Vector3 Position;
  public float Angle;
  public bool FixedPosition;
  public bool FixedAngle;
}
public class Ruler {
  private static GameObject? Projector = null;
  private static CircleProjector? BaseProjector = null;

  private static CircleProjector GetBaseProjector() {
    var workbench = ZNetScene.instance.GetPrefab("piece_workbench");
    if (!workbench) throw new InvalidOperationException("Error: Unable to find the workbench object.");
    BaseProjector = workbench.GetComponentInChildren<CircleProjector>();
    return BaseProjector;
  }
  private static bool MoveWithPlayer = false;
  private static bool RotateWithPlayer = false;
  public static void Update() {
    if (Projector == null || !Player.m_localPlayer) return;
    if (MoveWithPlayer)
      Projector.transform.position = Player.m_localPlayer.transform.position;
    if (RotateWithPlayer)
      Projector.transform.rotation = Player.m_localPlayer.transform.rotation;

  }
  private static GameObject InitializeGameObject(RulerParameters pars) {
    Projector = new();
    Projector.layer = LayerMask.NameToLayer("character_trigger");
    MoveWithPlayer = !pars.FixedPosition;
    RotateWithPlayer = !pars.FixedAngle;
    if (pars.FixedPosition)
      Projector.transform.position = pars.Position;
    if (pars.FixedAngle)
      Projector.transform.rotation = Quaternion.Euler(0f, 180f * pars.Angle / Mathf.PI, 0f);
    return Projector;
  }
  public static void InitializeProjector(RulerParameters pars, GameObject obj) {
    if (BaseProjector == null)
      BaseProjector = GetBaseProjector();
    if (pars.Radius.HasValue) {
      var circle = obj.AddComponent<CircleProjector>();
      circle.m_prefab = BaseProjector.m_prefab;
      circle.m_mask = BaseProjector.m_mask;
      circle.m_radius = pars.Radius.Value;
      circle.m_nrOfSegments = Math.Max(3, (int)(circle.m_radius * 4));
    }
    if (pars.Depth.HasValue && pars.Width.HasValue) {
      var rect = obj.AddComponent<RectangleProjector>();
      rect.m_prefab = BaseProjector.m_prefab;
      rect.m_mask = BaseProjector.m_mask;
      rect.m_depth = pars.Depth.Value;
      rect.m_width = pars.Width.Value;
      rect.m_nrOfSegments = Math.Max(3, (int)((rect.m_depth + rect.m_width) * 2));
    }
  }
  public static void Create(RulerParameters pars) {
    Remove();
    if (pars.Radius == null && pars.Width == null && pars.Depth == null) return;
    var obj = InitializeGameObject(pars);
    InitializeProjector(pars, InitializeGameObject(pars));
  }

  private static void Remove() {
    if (Projector != null)
      UnityEngine.Object.Destroy(Projector);
    Projector = null;
  }
}
