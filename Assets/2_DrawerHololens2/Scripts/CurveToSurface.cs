using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using Microsoft.MixedReality.Toolkit.Input;
public class CurveToSurface : MonoBehaviour, IMixedRealityPointerHandler
{
    //�㼯�洢λ��
    //String filePath = "C:\\Users\\32199\\Desktop\\ TestFile.txt";
    String filePath = "E:\\����\\test\\5.txt";

    //ͳ���ѻ�������
    int surfaceCount = 1;

    //�洢�Ѽ����������������洢��ʽΪ
    //key����1 2 3��  �������������������������С��������
    //value��24.45    �����������
    Dictionary<string, double> areas = new Dictionary<string, double>();

    //�洢�Ѽ���������������Σ��洢��ʽΪ
    //��1 2 3��       �������������������������С��������
    List<string> triangleList = new List<string>();

    //hololens�洢�㼯��λ��
    string pathBase;

    void Awake()
    {
        pathBase = Application.persistentDataPath + "\\";
        
    }
    void Update()
    {
        if (File.Exists(pathBase + surfaceCount + ".txt"))
        {
            
            //Debug.Log(bottomPointsPath);
            DrawSurfaceWithDP();
            surfaceCount++;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void OndrawGizmos()
    {

    }


    //ʹ�ö�̬�滮�Ի�õ�������������
    void DrawSurfaceWithDP(){
        
        filePath = pathBase + surfaceCount + ".txt";
        areas = new Dictionary<string, double>();
        triangleList = new List<string>();

        GameObject surface = new GameObject("Surface" + surfaceCount);
        surface.AddComponent<MeshFilter>();
        surface.AddComponent<MeshRenderer>();

        Mesh surfaceMesh = new Mesh();
        surface.GetComponent<MeshFilter>().mesh = surfaceMesh;
        List<Vector3> pointsList = ReadPointsFromFile();
        //���㹹����������㼯
        GetMeshWithDP(pointsList, 0, pointsList.Count-1);
        //ʹ����һ���õ������㼯�γ�ʵ������
        BuildMesh(surface,pointsList);

        //Ϊ�������Ӳ��ʰ�
        Material mat = (Material)Resources.Load<Material>("Mountain");
        surface.GetComponent<Renderer>().material = mat;
        /*Material mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Mountain.mat", typeof(Material));
        surface.GetComponent<Renderer>().material = mat;*/
    }

    //���ļ���õ㼯
    List<Vector3> ReadPointsFromFile()
    {
        var stream = new FileStream(filePath, FileMode.Open);
        var reader = new StreamReader(stream);
        var pointsList = new List<Vector3>();
        while (!reader.EndOfStream)
        {
            var content = reader.ReadLine();
            var result = content.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            float x = float.Parse(result[0]);
            float y = float.Parse(result[1]);
            float z = float.Parse(result[2]);
            pointsList.Add(new Vector3(x, y, z));
        }
        int length = pointsList.Count;
        reader.Close();
        stream.Close();
        return pointsList;
    }

    //��Ҫ��֤A������С��B������
    void GetMeshWithDP(List<Vector3> pointsList, int pAIndex, int pBIndex)
    {
        if (pBIndex - pAIndex <= 1){
            return;
        }
        //��õ�C��λ��A B֮��
        int pCIndex = GetMinWeightPoint(pointsList,pAIndex,pBIndex);

        GetMeshWithDP(pointsList,pAIndex,pCIndex);
        GetMeshWithDP(pointsList,pCIndex,pBIndex);
    }

    //�ҵ�Ȩ��ֵ��С�ĵ㣬�����õ���ԭ��������ɵ����㼯�洢��������Ϊģ�͵�һ������
    int GetMinWeightPoint(List<Vector3> pointsList, int pAIndex, int pBIndex) {
        return GetMinAreaPoint(pointsList, pAIndex, pBIndex);
    }

    //Ȩ��Ϊ���������
    int GetMinAreaPoint(List<Vector3> pointsList, int pAIndex, int pBIndex) {
        int pCIndex = pAIndex + 1;
        Vector3 pA = pointsList[pAIndex], pB = pointsList[pBIndex], pC = pointsList[pCIndex];
        double minArea = GetTriangleArea(pA,pB,pC);
        for (int i = pCIndex; i < pBIndex; i++) {
            string key = "" + pAIndex + " " + i + " " + pBIndex;
            double nowArea;
            if (areas.ContainsKey(key)){
                nowArea = areas[key];
            }
            else {
                nowArea = GetTriangleArea(pA,pB,pointsList[i]);
                areas.Add(key,nowArea);
            }
            if (nowArea < minArea) {
                pCIndex = i;
                minArea = nowArea;
            }
        }

        triangleList.Add(""+pAIndex+" "+pCIndex+" "+pBIndex);

        return pCIndex; 
    }

    //����ռ����������
    double GetTriangleArea(Vector3 pA, Vector3 pB, Vector3 pC) {
        double x1 = pA.x, x2 = pB.x, x3 = pC.x, y1 = pA.y, y2 = pB.y, y3 = pC.y, z1 = pA.z, z2 = pB.z, z3 = pC.z;
        double sXY, sYZ, sZX,sXYZ;

        sXY = Math.Abs(0.5 * (x1 * y2 + x2 * y3 + x3 * y1 - x2 * y1 - x3 * y2 - x1 * y3));
        sYZ = Math.Abs(0.5 * (y1 * z2 + y2 * z3 + y3 * z1 - y2 * z1 - y3 * z2 - y1 * z3));
        sZX = Math.Abs(0.5 * (z1 * x2 + z2 * x3 + z3 * x1 - z2 * x1 - z3 * x2 - z1 * x3));
        sXYZ = Math.Sqrt(sXY*sXY+sYZ*sYZ+sZX*sZX);

        return sXYZ;
    }

    void BuildMesh(GameObject surface, List<Vector3> verticeList)
    {

        Mesh mesh = surface.GetComponent<MeshFilter>().mesh;

        CreatePointCloud(verticeList, "Surface" + surfaceCount + ": " + "Vertices");

        Vector3[] vertices = verticeList.ToArray();
        mesh.vertices = vertices;
        int numOfTri = triangleList.Count;
        int[] triangles = new int[numOfTri * 3];
        int tri = 0;
        for (int i = 0; i < triangleList.Count; i++) {
            string triangleStr = triangleList[i];
            string[] pointsStr = triangleStr.Split(' ');
            int pAIndex, pBIndex, pCIndex;
            pAIndex = int.Parse(pointsStr[0]);
            pBIndex = int.Parse(pointsStr[1]);
            pCIndex = int.Parse(pointsStr[2]);

            triangles[tri++] = pAIndex;
            triangles[tri++] = pBIndex;
            triangles[tri++] = pCIndex;

        }
        mesh.triangles = triangles;
    }

    //���ڻ滭����
    void CreatePointCloud(List<Vector3> lineList, String name)
    {
        int num = lineList.Count;

        GameObject pointObj = new GameObject();
        pointObj.name = name;
        //pointObj.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
        pointObj.AddComponent<MeshFilter>();
        pointObj.AddComponent<MeshRenderer>();
        Mesh meshNeed = new Mesh();
        //Material mat = new Material(Shader.Find("Custom/VertexColor"));
        pointObj.GetComponent<MeshFilter>().mesh = meshNeed;
        //pointObj.GetComponent<MeshRenderer>().material = mat;

        Vector3[] points = new Vector3[num];
        Color[] colors = new Color[num];
        int[] indecies = new int[num];
        for (int i = 0; i < num; ++i)
        {
            points[i] = lineList[i];
            indecies[i] = i;
            colors[i] = Color.white;
        }

        meshNeed.vertices = points;
        meshNeed.colors = colors;
        meshNeed.SetIndices(indecies, MeshTopology.Points, 0);

    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {

    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {

    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {

    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {

    }
}

