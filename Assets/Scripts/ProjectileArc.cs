using UnityEngine;

public class ProjectileArc : MonoBehaviour 
{
    [SerializeField]
    public int iterations = 20;
    public ProjectileData projectileData;

    [SerializeField]
    Color errorColor;

    private Color initialColor;
    private LineRenderer lineRenderer;
    private RaycastHit2D collisionHit;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        initialColor = lineRenderer.material.color;
    }

    public void UpdateArc(Vector2 offset, float speed, float distance, float gravity, float angle, Vector3 direction, bool valid)
    {
        Vector2[] arcPoints = ProjectileMath.ProjectileArcPoints(iterations, speed, distance, gravity, angle);
        Vector3[] points3d = new Vector3[arcPoints.Length];

        int hitId = 0;
        for (int i = 0; i < arcPoints.Length; i++)
        {
            if (hitId != 0){
                points3d[i] = points3d[hitId];
                continue;
            }

            points3d[i] = new Vector3(direction.x * arcPoints[i].x + offset.x, arcPoints[i].y + offset.y, 0f);

            // Ground detection. Only check if it hasn't hit ground.
            if (hitId == 0 && i > 0){
                collisionHit = Physics2D.Linecast(points3d[i-1], points3d[i], 1 << 11);

                if (collisionHit){
                    hitId = i;
                }
            }
        }

        lineRenderer.positionCount = arcPoints.Length;
        lineRenderer.SetPositions(points3d);

        lineRenderer.material.color = valid ? initialColor : errorColor;
    }

    private void OnDrawGizmos(){
        if (!collisionHit || projectileData == null) {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(collisionHit.point, projectileData.areaOfEffect);
    }
}