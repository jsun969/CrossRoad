using UnityEngine;

[AddComponentMenu("Matt's Scripts/Agents/Agent Zone")]
public class AgentZone : MonoBehaviour
{
    public Rect allowedArea;
    
    public Vector3 GetRandom()
    {
        Vector2 samplePoint = allowedArea.min + allowedArea.size * new Vector2(Random.value, Random.value);
        Vector3 worldPoint = transform.TransformPoint(Convert(samplePoint));
        Debug.DrawRay(worldPoint, Vector3.up);
        return worldPoint;
    }

    public bool Contains(in Vector3 point)
    {
        return allowedArea.Contains(Convert(transform.InverseTransformPoint(point)));
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Vector3 center = Convert(allowedArea.center);
        Vector3 size = Convert(allowedArea.size);
        Gizmos.DrawWireCube(center, size);
    }
    
    private float Distance2D(Vector3 a, Vector3 b)
    {
        Vector2 offset = Convert(a) - Convert(b);
        return Mathf.Sqrt(offset.x * offset.x + offset.y * offset.y);
    }

    private Vector2 Convert(Vector3 v) => new Vector2(v.x, v.z);
    private Vector3 Convert(Vector2 v) => new Vector3(v.x, 0, v.y);
}
