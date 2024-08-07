using QFramework;
using UnityEngine;

public class ObjectsDetector : MonoBehaviour
{
    public float radius;
    public LayerMask targetLayer;

    Collider2D[] colliders;
    public bool touchable => Physics2D.OverlapCircle(transform.position, radius, targetLayer);
    public InteractiveObject DetectClosestObject()
    {
        colliders = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);
        if (colliders.IsNullOrEmpty()) return null;

        float minDist = 2 * radius + 1;
        InteractiveObject closestObject = null;
        foreach (Collider2D o in colliders)
        {
            if (!(o.transform.GetComponent<InteractiveObject>()?.activable ?? false)) continue;
            float dist = (o.Position2D() - transform.Position2D()).magnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closestObject = o.transform.GetComponent<InteractiveObject>();
            }
        }

        return closestObject;
    }
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}