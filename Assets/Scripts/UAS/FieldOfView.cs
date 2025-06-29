using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    private Mesh mesh;
    private MeshRenderer renderMesh;
    public LayerMask layerMask;
    private float fov;  
    private Vector3 origin;
    private float startingAngle = 0f;
    private float viewDistance = 6f;
    public Material wander, chase;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        renderMesh = GetComponent<MeshRenderer>();
        GetComponent<MeshFilter>().mesh = mesh;
        origin = Vector3.zero;
        fov = 40f;
    }

    // Update is called once per frame
    void Update()
    {
        int rayCount = 30;
        float angle = startingAngle + 20f;
        float angleIncrease = fov / rayCount;

        Vector3[] vertice = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertice.Length];
        int[] triangle = new int[rayCount * 3];

        vertice[0] = origin;
        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            RaycastHit2D ray2D = Physics2D.Raycast(transform.position, GetvectorFromAngle(angle), viewDistance, layerMask);
            if (ray2D.collider == null)
            {  
                vertex = origin + GetvectorFromAngle(angle) * viewDistance;
            }
            else
            {
                vertex = transform.InverseTransformPoint(ray2D.point);
            }

            vertice[vertexIndex] = vertex;

            if (i > 0)
            {
                triangle[triangleIndex] = 0;
                triangle[triangleIndex + 1] = vertexIndex - 1;
                triangle[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }
            vertexIndex++;
            angle -= angleIncrease;
        }
        mesh.vertices = vertice;
        mesh.uv = uv;
        mesh.triangles = triangle;
    }

    public Vector3 GetvectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public void SetFov(float start, float distance)
    {
        this.startingAngle = start;
        this.viewDistance = distance;
    }

    public void setMaterial(string Status)
    {
        if(Status == "Wander")
        {
            renderMesh.material = wander;
        }
        else
        {
            renderMesh.material = chase;
        }
    
    }
}
