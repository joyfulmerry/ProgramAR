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
    //单笔画出曲线模式下，点集存储位置
    //String filePath = "C:\\Users\\32199\\Desktop\\ TestFile.txt";
    String filePath;

    //多笔画出曲线模式下，点集存储位置
    //底边点集
    //String bottomPointsPath = "C:\\Users\\32199\\Desktop\\1.txt";
    String bottomPointsPath = "C:\\Users\\32199\\Desktop\\1.txt";

    //侧边点集
    //String sidePointsPath = "C:\\Users\\32199\\Desktop\\2.txt";
    String sidePointsPath = "C:\\Users\\32199\\Desktop\\2.txt";

    String pathBase;
    //值为0时多笔画模式，值为1时单笔画模式
    int singleCurveMode = 0;

    //统计已画表面数
    int surfaceCount = 1;


    List<Vector3> ReadOneCurvePointFromFile()
    {
        var stream = new FileStream(filePath, FileMode.Open);
        var reader = new StreamReader(stream);
        var pointsList = new List<Vector3>();
        while (!reader.EndOfStream)
        {
            var content = reader.ReadLine();
            var result = content.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            float x = float.Parse(result[0]) * 100;
            float y = float.Parse(result[1]) * 100;
            float z = float.Parse(result[2]) * 100;
            pointsList.Add(new Vector3(x, y, z));
        }
        int length = pointsList.Count;
        reader.Close();
        stream.Close();
        return pointsList;
    }
    void ReadMultiCurvePointFromFile(List<Vector3> line1List, List<Vector3> sidePointsList)
    {
        var stream = new FileStream(bottomPointsPath, FileMode.Open);
        var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            var content = reader.ReadLine();
            var result = content.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            float x = float.Parse(result[0]);
            float y = float.Parse(result[1]);
            float z = float.Parse(result[2]);
            Debug.Log(content);
            if (line1List.Count > 0)
            {
                float lastx = line1List[line1List.Count - 1].x, lasty = line1List[line1List.Count - 1].y, lastz = line1List[line1List.Count - 1].z;
                if (lastx == x && lasty == y && lastz == z)
                { }
                else
                {
                    line1List.Add(new Vector3(x, y, z));
                }
            }
            else
            {
                line1List.Add(new Vector3(x, y, z));
            }
        }
        reader.Close();
        stream.Close();

        stream = new FileStream(sidePointsPath, FileMode.Open);
        reader = new StreamReader(stream);
        int isFirstVertice = 1;
        while (!reader.EndOfStream)
        {
            var content = reader.ReadLine();
            var result = content.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            float x = float.Parse(result[0]);
            float y = float.Parse(result[1]);
            float z = float.Parse(result[2]);

            if (sidePointsList.Count > 0)
            {
                float lastx = sidePointsList[sidePointsList.Count - 1].x, lasty = sidePointsList[sidePointsList.Count - 1].y, lastz = sidePointsList[sidePointsList.Count - 1].z;
                if (lastx == x && lasty == y && lastz == z)
                { }
                else
                {
                    sidePointsList.Add(new Vector3(x, y, z));
                }
            }
            else
            {
                sidePointsList.Add(new Vector3(x, y, z));
            }
        }

        float d1, d2;
        d1 = Vector3.Distance(sidePointsList[0], line1List[line1List.Count-1]);
        d2 = Vector3.Distance(sidePointsList[sidePointsList.Count-1], line1List[line1List.Count - 1]);
        if (d1 > d2) {
            sidePointsList.Reverse();
        }
        reader.Close();
        stream.Close();
    }

    void Awake() {
        pathBase = Application.persistentDataPath+"\\";
    }
    void Update()
    {
        if (File.Exists(pathBase + surfaceCount * 2 + ".txt"))
        {
            bottomPointsPath = pathBase + (surfaceCount * 2 - 1) + ".txt";
            sidePointsPath = pathBase + surfaceCount * 2 + ".txt";
            //Debug.Log(bottomPointsPath);
            DrawSurface();
            surfaceCount++;
        }
        
    }
    void DrawSurface()
    {
        GameObject surface = new GameObject("Surface"+surfaceCount);
        surface.AddComponent<MeshFilter>();
        surface.AddComponent<MeshRenderer>();

        Mesh surfaceMesh = new Mesh();
        surface.GetComponent<MeshFilter>().mesh = surfaceMesh;
        List<Vector3> line1List = new List<Vector3>();
        List<Vector3> line2List = new List<Vector3>();
        List<Vector3> line3List = new List<Vector3>();

        if (singleCurveMode==1)
        {
            List<Vector3> pointsList = ReadOneCurvePointFromFile();
            AllocatePoint(pointsList, line1List, line2List, line3List);
        }
        else {
            List<Vector3> sidePointsList = new List<Vector3>();
            ReadMultiCurvePointFromFile(line1List,sidePointsList);
            AllocateSidePoints(sidePointsList,line2List,line3List);
        }
        Generate(surface, line1List, line2List, line3List);
        CreatePointCloud(line1List, "Surface"+surfaceCount+": "+"Line1");
        CreatePointCloud(line2List, "Surface" + surfaceCount + ": " + "Line2");
        CreatePointCloud(line3List, "Surface"+surfaceCount+": "+"Line3");
        Material mat = (Material)Resources.Load<Material>("Mountain");
        surface.GetComponent<Renderer>().material = mat;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    void OndrawGizmos()
    {

    }
    void AllocatePoint(List<Vector3> pointsList, List<Vector3> line1List, List<Vector3> line2List, List<Vector3> line3List)
    {
        Vector3[] points = RotateList(pointsList);
        //构建初始底边，包括最低点在内有三个顶点
        line1List.Add(points[points.Length / 2]);
        int lptr = points.Length / 2, rptr = points.Length / 2;
        int count = 2;
        int oriCount = count;
        float lastYgap = 0;
        while (count != 0)
        {
            if (lptr == rptr && count != oriCount) break;
            int llptr, rrptr;
            if (lptr - 1 < 0) llptr = points.Length - 1;
            else llptr = lptr - 1;
            if (rptr + 1 >= points.Length) rrptr = 0;
            else rrptr = rptr + 1;
            float yGapL = Math.Abs(points[llptr].y - points[lptr].y);
            float yGapR = Math.Abs(points[rrptr].y - points[rptr].y);
            if (yGapL <= yGapR)
            {
                line1List.Insert(0, points[llptr]);
                lptr = llptr;
                if (count == 1)
                    lastYgap = yGapL;
            }
            else
            {
                line1List.Add(points[rrptr]);
                rptr = rrptr;
                if (count == 1)
                    lastYgap = yGapR;
            }
            count--;
        }
        Debug.Log("底边初始顶点：");
        for (int i = 0; i < line1List.Count; ++i)
        {
            Debug.Log("" + line1List[i].x + " " + line1List[i].y + " " + line1List[i].z);
        }
        //以初始底边为基础，构建底边
        //底边上的点集数不能大于总点集的一半
        count = points.Length / 2 - 3;
        oriCount = count;
        while (count > 0)
        {
            //Debug.Log("开始加其他点：count = " + count + " lastGap = " + lastZgap);
            if (lptr == rptr && count != oriCount) break;
            int llptr, rrptr;
            if (lptr - 1 < 0) llptr = points.Length - 1;
            else llptr = lptr - 1;
            if (rptr + 1 >= points.Length) rrptr = 0;
            else rrptr = rptr + 1;
            float yGapL = Math.Abs(points[llptr].y - points[lptr].y);
            float yGapR = Math.Abs(points[rrptr].y - points[rptr].y);
            //Debug.Log("zGapL = " + zGapL + " zGapR = " + zGapR);
            //斜率判断是否触及底边端点
            float kL = Math.Abs(points[llptr].y - points[lptr].y) / (points[llptr].x - points[lptr].x);
            float kR = Math.Abs(points[rrptr].y - points[rptr].y) / (points[rrptr].x - points[rptr].x);
            if (kL<kR)
            {
                if (kL>0.6) break;
                //Debug.Log("添加顶点："+points[llptr].x+" "+ points[llptr].y+" "+ points[llptr].z);
                line1List.Insert(0, points[llptr]);
                lptr = llptr;
                lastYgap = yGapL;
            }
            else
            {
                if (kR>0.6) break;
                line1List.Add(points[rrptr]);
                //Debug.Log("添加顶点：" + points[rrptr].x + " " + points[rrptr].y + " " + points[rrptr].z);
                rptr = rrptr;
                lastYgap = yGapR;
            }
            count--;
        }

        int length = line1List.Count;
        Debug.Log("line1底边上的顶点数：" + line1List.Count);
        Debug.Log("底边上的顶点：");
        for (int i = 0; i < length; ++i)
        {
            Debug.Log("" + line1List[i].x + " " + line1List[i].y + " " + line1List[i].z);
        }
        //构建两条侧边
        List<Vector3> sidePointsList = new List<Vector3>();

        while (rptr != lptr)
        {
            sidePointsList.Add(points[rptr]);
            rptr++;
            if (rptr == points.Length)
                rptr = 0;
        }
        sidePointsList.Add(points[lptr]);
        AllocateSidePoints(sidePointsList, line2List, line3List);

    }
    void AllocateSidePoints(List<Vector3> sidePointsList, List<Vector3> line2List, List<Vector3> line3List) {
        float highY = sidePointsList[0].y;
        int highYIndex = 0;
        //找到侧边最高点
        for (int i = 0; i < sidePointsList.Count; ++i)
        {
            if (sidePointsList[i].y > highY)
            {
                highY = sidePointsList[i].y;
                highYIndex = i;
            }
        }
        for (int i = sidePointsList.Count - 1; i >= highYIndex; i--)
        {
            line2List.Add(sidePointsList[i]);
        }
        for (int i = 0; i <= highYIndex; i++)
        {
            line3List.Add(sidePointsList[i]);
        }
        int pointGap = line2List.Count - line3List.Count;
        if (pointGap > 0)
        {
            AddPointsToLine(line3List, pointGap);

        }
        else if (pointGap < 0)
        {
            AddPointsToLine(line2List, -pointGap);
        }
        Debug.Log("line2侧边上的顶点数：" + line2List.Count);
        Debug.Log("line2侧边上的顶点：");
        for (int i = 0; i < line2List.Count; ++i)
        {
            Debug.Log("" + line2List[i].x + " " + line2List[i].y + " " + line2List[i].z);
        }
        Debug.Log("line3侧边上的顶点数：" + line3List.Count);
        Debug.Log("line3侧边上的顶点：");
        for (int i = 0; i < line3List.Count; ++i)
        {
            Debug.Log("" + line3List[i].x + " " + line3List[i].y + " " + line3List[i].z);
        }
    }
    //将两条侧边的点数对齐
    void AddPointsToLine(List<Vector3> lessPointsLine, int countToAdd)
    {
        int basePtr = 0;
        while (countToAdd != 0)
        {
            if (basePtr == lessPointsLine.Count - 1) basePtr = 0;
            float newX = (lessPointsLine[basePtr].x + lessPointsLine[basePtr + 1].x) / 2;
            float newY = (lessPointsLine[basePtr].y + lessPointsLine[basePtr + 1].y) / 2;
            float newZ = (lessPointsLine[basePtr].z + lessPointsLine[basePtr + 1].z) / 2;
            Vector3 newPoint = new Vector3(newX, newY, newZ);
            lessPointsLine.Insert(basePtr + 1, newPoint);
            basePtr += 2;
            countToAdd--;
            if (basePtr >= lessPointsLine.Count)
                basePtr = 0;
        }
    }

    Vector3[] RotateList(List<Vector3> pointsList)
    {
        Vector3 lowest = pointsList[0];
        foreach (var point in pointsList)
        {
            if (point.y < lowest.y)
                lowest = point;
        }

        Vector3[] points = pointsList.ToArray();
        int lowestIndex = 0;
        for (int i = 0; i < pointsList.Count; i++)
        {
            if (lowest == pointsList[i])
            {
                lowestIndex = i;
                break;
            }
        }
        int midIndex = pointsList.Count / 2;
        int gap = midIndex - lowestIndex;
        if (gap < 0)
        {
            gap = points.Length + gap;
        }
        Vector3[] temPoints = new Vector3[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            if (i + gap < points.Length)
            {
                temPoints[i + gap] = points[i];
            }
            else
            {
                temPoints[i + gap - points.Length] = points[i];
            }
        }
        points = temPoints;
        return points;
    }

    void Generate(GameObject surface, List<Vector3> line1List, List<Vector3> line2List, List<Vector3> line3List)
    {
        int lineSize1 = line1List.Count, lineSize2 = line2List.Count, lineSize3 = line3List.Count;
        Vector3[] line1 = line1List.ToArray();
        Vector3[] line2 = line2List.ToArray();
        Vector3[] line3 = line3List.ToArray();


        Vector3[] vertices = new Vector3[lineSize1 * (lineSize2 - 1) + 1];
        //构建面上的点
        GetVertices(vertices, line1, line2, line3);

        //构建三角形
        GetTriangles(surface, vertices, line1, line2, line3);

    }
    void GetVertices(Vector3[] vertices, Vector3[] line1, Vector3[] line2, Vector3[] line3)
    {
        int lineSize1 = line1.Length;
        int lineSize2 = line2.Length;
        Debug.Log("lineSize1: " + lineSize1 + " lineSize2: " + lineSize2 + " lineSize3: " + line3.Length);
        for (int i = 0; i < lineSize1; i++)
        {
            vertices[i] = line1[i];
        }
        Vector3[] lastVertice = new Vector3[lineSize1];

        for (int i = 1; i < lineSize2 - 1; i++)
        {
            Debug.Log("————————————————————");
            vertices[i * lineSize1] = line2[i];
            vertices[i * lineSize1 + lineSize1 - 1] = line3[i];
            for (int j = 1; j < lineSize1 - 1; j++)
            {
                if (i == 1)
                {
                    lastVertice[j] = line1[j];
                }
                //vertices[i * lineSize1 + j] = getSimilarVertice(lastVertice[j],line2[i],line3[i]);
                vertices[i * lineSize1 + j] = getSimilarVertice(lastVertice[j], line2[i], line3[i], line2[i-1],line3[i-1]);
                lastVertice[j] = vertices[i * lineSize1 + j];
                //Debug.Log("(" + x1 + ", " + y1 + ", " + z1 + "), " + "(" + x2 + ", " + y2 + ", " + z2 + "), " + "(" + x3 + ", " + y3 + ", " + z3 + "), " + "(" + xMid + ", " + yMid + ", " + zMid + ")");
            }
        }
        vertices[lineSize1 * (lineSize2 - 1)] = line2[lineSize2 - 1];

    }

    Vector3 getSimilarVertice(Vector3 lastVertice, Vector3 B, Vector3 C, Vector3 lastB, Vector3 lastC)
    {
        float x, y, z;
        float x1 = lastVertice.x, y1 = lastVertice.y, z1 = lastVertice.z;
        float x2 = B.x, y2 = B.y, z2 = B.z;
        float x3 = C.x, y3 = C.y, z3 = C.z;

        float X2 = lastB.x, Y2 = lastB.y, Z2 = lastB.z;
        float X3 = lastC.x, Y3 = lastC.y, Z3 = lastC.z;

        if (X3 != X2)
            x = (x3 * (x1 - X2) - x2 * (x1 - X3)) / (X3 - X2);
        else {
            x = (x2 + x3) / 2;
        }
        if (Z3 != Z2)
            z = (z3 * (z1 - Z2) - z2 * (z1 - Z3)) / (Z3 - Z2);
        else {
            z = (z2 + z3) / 2;
        }

        float d2 = Vector3.Distance(new Vector3(x, 0, z), new Vector3(x2, 0, z2));
        float d3 = Vector3.Distance(new Vector3(x, 0, z), new Vector3(x3, 0, z3));
        if (y3 > y2)
        {
            y = d2 * (y3 - y2) / (d2 + d3) + y2;
        }
        else
        {
            y = d3 * (y2 - y3) / (d3 + d2) + y3;
        }
        return new Vector3(x, y, z);

    }
    void GetTriangles(GameObject surface, Vector3[] vertices, Vector3[] line1, Vector3[] line2, Vector3[] line3)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertice = vertices[i];
            Debug.Log("vertice: (" + vertice.x + ", " + vertice.y + ", " + vertice.z + ")");
            if (i % line1.Length == 0)
                Debug.Log("————————————————————");
        }
        int lineSize1 = line1.Length;
        int lineSize2 = line2.Length;
        int xSize = lineSize1, zSize = lineSize2;
        Mesh mesh = surface.GetComponent<MeshFilter>().mesh;

        List<Vector3> verticeList = new List<Vector3>(vertices);
        CreatePointCloud(verticeList, "Surface" + surfaceCount + ": " + "MyMesh");

        mesh.vertices = vertices;
        int numOfTri = (xSize - 1) * (zSize - 2) * 2 + xSize - 1;
        int[] triangles = new int[numOfTri * 3];
        int ti = 0, vi = 0;
        for (int z = 0; z < zSize - 2; z++, vi++)
        {
            for (int x = 0; x < xSize - 1; x++, vi++, ti += 6)
            {
                //Debug.Log(ti+" "+vi);
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize;
                triangles[ti + 5] = vi + xSize + 1;
            }
        }
        Debug.Log("next");
        for (int i = 0; i < xSize - 1; i++)
        {
            //Debug.Log(ti + " " + vi);
            triangles[ti] = vi;
            triangles[ti + 1] = lineSize1 * (lineSize2 - 1);
            triangles[ti + 2] = vi + 1;
            vi++;
            ti += 3;
        }
        mesh.triangles = triangles;
    }
    // Update is called once per frame

    //用于画出点云
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
