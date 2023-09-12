using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGrass : MonoBehaviour
{
    public Material grassMat;

    int index = 1;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(GameObject.Find("Surface"+index.ToString())!=null){
            Generate(FilterVertice(GameObject.Find("Surface"+index.ToString()).GetComponent<MeshFilter>().sharedMesh.vertices));
            index++;
        }
    }

    //根据mesh上的顶点生成草的位置
    Vector3[] FilterVertice(Vector3[] vertices){
        List<Vector3> roots = new List<Vector3>();
        int[] triangles = GameObject.Find("Surface"+index.ToString()).GetComponent<MeshFilter>().sharedMesh.triangles;

        //计算mesh上三角形的平均面积
        float avg_area = 0.0f;
        for(int i=0;i<triangles.Length;i+=3){
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i+1]];
            Vector3 v3 = vertices[triangles[i+2]];
            float area = Vector3.Cross(v2-v1, v3-v1).magnitude * 0.5f;
            avg_area += area;
        }
        avg_area /= triangles.Length / 3;

        //设置mesh上点之间的平均距离
        float avg_dist = 0.003f;

        Debug.Log("avg_area: " + avg_area);
        Debug.Log("avg_dist: " + avg_dist);

        //根据平均距离和平均面积计算每个三角形上的草的数量
        for(int i=0;i<triangles.Length;i+=3){
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i+1]];
            Vector3 v3 = vertices[triangles[i+2]];

            CheckDistance(roots,v1,avg_dist);
            CheckDistance(roots,v2,avg_dist);
            CheckDistance(roots,v3,avg_dist);
            
            float area = Vector3.Cross(v2-v1, v3-v1).magnitude * 0.5f;
            CheckSize(roots, v1, v2, v3, area, avg_area, avg_dist);
        }

        return roots.ToArray();
    }

    //检查点是否保持距离
    void CheckDistance(List<Vector3> roots, Vector3 v, float avg_dist){
        for(int i=0;i<roots.Count;i++){
            if(Vector3.Distance(roots[i], v) < avg_dist){
                return;
            }
        }
        roots.Add(v);
    }

    //检查三角形的大小
    void CheckSize(List<Vector3> roots, Vector3 v1, Vector3 v2, Vector3 v3,float area,float avg_area, float avg_dist){
        float size = area / avg_area;
        if(size > 1.0f){
            Vector3 center = (v1 + v2 + v3) / 3;
            CheckDistance(roots, center, avg_dist);

            CheckSize(roots, v1, v2, center, Vector3.Cross(v2-v1, center-v1).magnitude * 0.5f, avg_area, avg_dist);
            CheckSize(roots, v2, v3, center, Vector3.Cross(v3-v2, center-v2).magnitude * 0.5f, avg_area, avg_dist);
            CheckSize(roots, v3, v1, center, Vector3.Cross(v1-v3, center-v3).magnitude * 0.5f, avg_area, avg_dist);
        }
    }

    void Generate(Vector3[] vertices){
        GameObject grassLayer;
        MeshFilter mf;
        MeshRenderer renderer;
        Mesh m;
        List<int> indices = new List<int>();
        for (int i = 0; i < 65000; i++)
        {
            indices.Add(i);
        }

        m = new Mesh();
        m.vertices = vertices;
        m.SetIndices(indices.GetRange(0, vertices.Length).ToArray(), MeshTopology.Points, 0);
        grassLayer = new GameObject("grassLayer");
        mf = grassLayer.AddComponent<MeshFilter>();
        renderer = grassLayer.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = grassMat;
        mf.mesh = m;

        grassLayer.transform.position = gameObject.transform.position;
        grassLayer.transform.rotation = gameObject.transform.rotation;
    }
}
