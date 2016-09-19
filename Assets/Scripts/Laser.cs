using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class Laser : MonoBehaviour {

    [SerializeField] float setPointMag = 0.5f;  // どのくらい移動したら点を発行するか.
    private float setPointSqrMag;               // 実際の計算で使う.

    [SerializeField] int maxPointCount = 10;
    List<Vector3> points;

    private struct section
    {
        public Vector3 center;
        public Vector3 direction;

        public Vector3 left;
        public Vector3 right;

        public Vector3 side;
        public Vector3 normal;
    }

    private section[]           sections;

    [SerializeField] float      lineWidth;
    [SerializeField] Material   material;

    private Vector3[] vertices;

    [SerializeField] int sortingOrder = 0;

    IPointGetter pointGetter;

    void Start()
    {
        setPointSqrMag = Mathf.Pow(setPointMag, 2);

        pointGetter = transform.GetComponentInChildren<IPointGetter>();

        GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
    }

    // Update is called once per frame
    void Update () {
        setPoints();
        setVector();
        createMesh();
    }

    void setPoints()
    {
        if (!pointGetter.GetPoint().HasValue) return;
        var curPoint = pointGetter.GetPoint().Value;

        // points が空なら初期化.
        if(points == null)
        {
            points = new List<Vector3>();
            points.Add(curPoint);
        }

        var sqrDiff = (curPoint - points[points.Count - 1]).sqrMagnitude;       
        if(sqrDiff > setPointSqrMag)
        {
            if(points.Count > 4)
            {
                // Catmull-Rom Spline.
                var p0 = points[points.Count - 5];
                var p1 = points[points.Count - 4];
                var p2 = points[points.Count - 2];
                var p3 = points[points.Count - 1];

                points[points.Count - 3] = calcCatmullRom(0.5f, p0, p1, p2, p3);
            }

            // 今回のマウス位置を登録.
            points.Add(curPoint);

            while(points.Count >= maxPointCount)
            {
                points.RemoveAt(0);
            }
        }
    }
    
    Vector3 calcCatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = 0.5f * (2f * p1);
        Vector3 b = 0.5f * (p2 - p0);
        Vector3 c = 0.5f * (2f * p0 - 5f * p1 + 4f * p2 - p3);
        Vector3 d = 0.5f * (-p0 + 3f * p1 - 3f * p2 + p3);

        Vector3 pos = a + (b * t) + (c * t * t) + (d * t * t * t);

        return pos;

    }

    void setVector()
    {
        if (points == null || points.Count <= 1) return;

        sections = new section[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            // 中心の設定.
            sections[i].center = points[i];
        }

        for (int i = 0; i < points.Count; i++)
        {
            // 方向ベクトルの計算.

            if (i == 0)
            {
                // 始点.
                sections[i].direction = sections[i + 1].center - sections[i].center;
            }
            else if (i == points.Count - 1)
            {
                // 終点.
                sections[i].direction = sections[i].center - sections[i - 1].center;
            }
            else
            {
                // 途中の点.
                sections[i].direction = sections[i + 1].center - sections[i - 1].center;                
            }

            sections[i].direction.Normalize();

            Vector3 side = Quaternion.AngleAxis(90f, -Vector3.forward) * sections[i].direction;
            side.z = 0f;
            side.Normalize();

            sections[i].left = sections[i].center - side * lineWidth / 2f;
            sections[i].right = sections[i].center + side * lineWidth / 2f;
            sections[i].side = side;
            sections[i].normal = Vector3.Cross(sections[i].direction, sections[i].side).normalized;
        }
    }

    void createMesh()
    {
        if (points == null || points.Count <= 3) return;

        MeshFilter mf = GetComponent<MeshFilter>();
        MeshCollider mc = GetComponent<MeshCollider>();
        Mesh mesh = mf.mesh = new Mesh();
        MeshRenderer mr = GetComponent<MeshRenderer>();

        mesh.name = "LaserMesh";

        int meshCount = points.Count - 1;

        Vector3[] vertices = new Vector3[(meshCount) * 4];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[(meshCount) * 2 * 3];

        for (int i = 0; i < meshCount - 2; i++)
        {
            vertices[i * 4 + 0] = sections[i].left;
            vertices[i * 4 + 1] = sections[i].right;
            vertices[i * 4 + 2] = sections[i + 1].left;
            vertices[i * 4 + 3] = sections[i + 1].right;

            var step = (float)1 / (meshCount -2);

            uvs[i * 4 + 0] = new Vector2(0.0f, i * step);
            uvs[i * 4 + 1] = new Vector2(1.0f, i * step);
            uvs[i * 4 + 2] = new Vector2(0.0f, (i + 1) * step);
            uvs[i * 4 + 3] = new Vector2(1.0f, (i + 1) * step);
        }

        int positionIndex = 0;

        for (int i = 0; i < meshCount - 2; i++)
        {
            triangles[positionIndex++] = (i * 4) + 1;
            triangles[positionIndex++] = (i * 4) + 0;
            triangles[positionIndex++] = (i * 4) + 2;

            triangles[positionIndex++] = (i * 4) + 2;
            triangles[positionIndex++] = (i * 4) + 3;
            triangles[positionIndex++] = (i * 4) + 1;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;

        mesh.triangles = triangles;

        mesh.Optimize();

        mr.material = material;

        mc.sharedMesh = mesh;
        mc.enabled = true;
    }

    void OnDrawGizmos()
    {
        if (sections == null) return;

        Gizmos.color = Color.black;
        for (int i = 0; i < sections.Length; i++)
        {
            Gizmos.DrawSphere(sections[i].center, 0.1f);
        }

        Gizmos.color = Color.blue;
        for (int i = 0; i < sections.Length; i++)
        {
            Gizmos.DrawSphere(sections[i].left, 0.1f);
            Gizmos.DrawSphere(sections[i].right, 0.1f);
        }
    }
}
