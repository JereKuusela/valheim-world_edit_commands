using UnityEngine;
namespace WorldEditCommands;
public class RectangleProjector : CircleProjector
{
  public float m_width = 5f;
  public float m_depth = 5f;
  private Vector3 Cast(Vector3 pos)
  {
    RaycastHit raycastHit;
    if (Physics.Raycast(pos + Vector3.up * 500f, Vector3.down, out raycastHit, 1000f, this.m_mask.value))
      pos.y = raycastHit.point.y;
    return pos;
  }
  private Transform Get(int index) => m_segments[index].transform;
  private void Set(int index, Vector3 pos) => Get(index).localPosition = pos;
  private void Cast(int index)
  {
    var segment = Get(index);
    segment.position = Cast(segment.position);
  }
  private void SetRot(int index, Vector3 rot) => Get(index).localRotation = Quaternion.LookRotation(rot, Vector3.up);
  private void EdgeFix(int index, float percent, float max, float start, float end, Vector3 direction)
  {
    var pos = percent * max;
    var transform = Get(index);
    var scale = m_prefab.transform.localScale;
    if (pos - start < 0.5f)
    {
      scale.z *= pos - start + 0.5f;
      transform.localPosition += direction * (0.5f - scale.z / 2f);
    }
    if (end - pos < 0.5f)
    {
      scale.z *= Mathf.Max(0f, end - pos + 0.5f);
      transform.localPosition -= direction * (0.5f - scale.z / 2f);
    }
    if (scale.z == 0f) transform.gameObject.SetActive(false);
    else transform.gameObject.SetActive(true);
    transform.localScale = scale;
  }
  new private void Update()
  {
    var totalLength = 2 * m_width + 2 * m_depth;
    var forward = (int)Mathf.Max(2, Mathf.Ceil(m_nrOfSegments * m_depth / totalLength));
    var right = (int)Mathf.Max(2, Mathf.Ceil(m_nrOfSegments * m_width / totalLength));
    var back = (int)Mathf.Max(2, Mathf.Ceil(m_nrOfSegments * m_depth / totalLength));
    var left = (int)Mathf.Max(2, Mathf.Ceil(m_nrOfSegments * m_width / totalLength));
    m_nrOfSegments = forward + right + back + left;
    CreateSegments();
    var index = 0;
    for (int i = 0; i < forward; i++, index++)
      SetRot(index, Vector3.forward);
    for (int i = 0; i < right; i++, index++)
      SetRot(index, Vector3.right);
    for (int i = 0; i < back; i++, index++)
      SetRot(index, Vector3.back);
    for (int i = 0; i < left; i++, index++)
      SetRot(index, Vector3.left);
    index = 0;
    var baseTime = Time.time * 0.025f * (m_nrOfSegments - 4);
    var halfLine = 0.5f;
    var basePos = m_width * Vector3.left - (m_depth + halfLine) * Vector3.forward;
    var start = 0.5f;
    var end = start + 2f * m_depth;
    var size = 2f * m_depth * forward / (forward - 1);
    var time = baseTime / (forward);
    for (int i = 0; i < forward; i++, index++)
    {
      var percent = ((float)i / forward + time) % 1f;
      var pos = basePos + percent * size * Vector3.forward;
      Set(index, pos);
      EdgeFix(index, percent, size, start, end, Vector3.forward);
      Cast(index);
    }
    basePos = m_depth * Vector3.forward - (m_width + halfLine) * Vector3.right;
    end = start + 2f * m_width;
    size = 2f * m_width * right / (right - 1);
    time = baseTime / (right);
    for (int i = 0; i < right; i++, index++)
    {
      var percent = ((float)i / right + time) % 1f;
      var pos = basePos + percent * size * Vector3.right;
      Set(index, pos);
      EdgeFix(index, percent, size, start, end, Vector3.right);
      Cast(index);
    }
    basePos = m_width * Vector3.right - (m_depth + halfLine) * Vector3.back;
    end = start + 2f * m_depth;
    size = 2f * m_depth * back / (back - 1);
    time = baseTime / (back);
    for (int i = 0; i < back; i++, index++)
    {
      var percent = ((float)i / back + time) % 1f;
      var pos = basePos + percent * size * Vector3.back;
      Set(index, pos);
      EdgeFix(index, percent, size, start, end, Vector3.back);
      Cast(index);
    }
    basePos = m_depth * Vector3.back - (m_width + halfLine) * Vector3.left;
    end = start + 2f * m_width;
    size = 2f * m_width * left / (left - 1);
    time = baseTime / (left);
    for (int i = 0; i < left; i++, index++)
    {
      var percent = ((float)i / left + time) % 1f;
      var pos = basePos + percent * size * Vector3.left;
      Set(index, pos);
      EdgeFix(index, percent, size, start, end, Vector3.left);
      Cast(index);
    }
  }
}
