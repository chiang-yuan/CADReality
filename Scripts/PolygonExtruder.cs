using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

enum ExtrudeFlag { ACTIVE, EDITONE , INACTIVE }

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PolygonExtruder : MonoBehaviour
{
    public float initExtrusion = 0.1f;

    private ExtrudeFlag extrudeState;

    /* --------------------------------------------------
     *  Extrusion Data
     *  -------------------------------------------------- */

    private Mesh polygon;           // holder of polygon to be extruded
    private Mesh extrusion;         // holder of extruded polyhedron
    MeshCollider extCollider;       // collider for extrusion
    MeshFilter extFilter;           // filter for extrusion

    List<Vector3> vertices, vertData;
    List<int> triangles, vertIndex;
    Vector3 faceNormal;

    Vector3[] initialVertices;
    bool faceDetected = false;

    bool sameFace = false;


    public void clickExtrudeButton()
    {
        extrudeState = ExtrudeFlag.ACTIVE;
    }

    public void setState(string flag)
    {
        extrudeState = (ExtrudeFlag)System.Enum.Parse(typeof(ExtrudeFlag), flag);
    }

    // Start is called before the first frame update
    void Start()
    {
        extCollider = this.gameObject.AddComponent<MeshCollider>();
        extFilter = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
        extrudeState = ExtrudeFlag.INACTIVE;
    }

    // Update is called once per frame
    void Update()
    {
        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            || (Input.GetMouseButton(0))))
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit)
                && (hit.collider.gameObject.name == "QuadPolygon" || hit.collider.gameObject.name == "QuadCircle")
                && extrudeState == ExtrudeFlag.ACTIVE)
            {
                polygon = duplicate(hit.collider.GetComponent<MeshFilter>().mesh);
                
                // convert plane to extrudable solid
                extrusion = convertSolid(polygon, initExtrusion);
                updateMesh();

                // setting up for manual extrusion 
                extCollider = gameObject.GetComponent<MeshCollider>();
                extCollider.sharedMesh = polygon;
                initializeList();
                initialVertices = vertices.ToArray();
                initializeData();

                updateMesh();
                faceDetected = true;
                extrudeState = ExtrudeFlag.EDITONE;
            }
            else if ((Physics.Raycast(Ray, out hit) && extrudeState == ExtrudeFlag.EDITONE)){

                float scale = 1f;
                
                Debug.Log(Input.GetTouch(0).deltaPosition * scale);
                Vector3 swipe = new Vector3(Input.GetTouch(0).deltaPosition.x * scale, Input.GetTouch(0).deltaPosition.y * scale);
                Debug.Log(swipe);

                if (swipe.magnitude > 0)
                {
                    for (int i = 0; i < vertIndex.Count; i++)
                    {
                        vertices[vertIndex[i]] += (swipe.x * faceNormal.x + swipe.y * faceNormal.y) * faceNormal;
                    }

                    for (int i = 0; i < vertData.Count; i++)
                    {
                        vertData[i] += (swipe.x * faceNormal.x + swipe.y * faceNormal.y) * faceNormal;
                    }
                }

                extrusion.vertices = vertices.ToArray();
                updateMesh();
            }
        }

        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            || (Input.GetMouseButtonUp(0))) && extrudeState == ExtrudeFlag.EDITONE)
        {
            extrudeState = ExtrudeFlag.INACTIVE;
        }
    }


    /* --------------------------------------------------
    * Private Function
    * -------------------------------------------------- */

    private Mesh duplicate(Mesh mesh_)
    {
        Mesh clone = new Mesh();
        clone.vertices = mesh_.vertices;
        clone.triangles = mesh_.triangles;
        clone.name = mesh_.name;
        clone.bounds = mesh_.bounds;
        clone.normals = mesh_.normals;
        clone.indexFormat = mesh_.indexFormat;
        
        return clone;
    }

    private Mesh convertSolid(Mesh mesh_, float initExtrusion_)
    {
        Mesh newMesh = new Mesh();
        newMesh.vertices = initVertices(mesh_, initExtrusion_);
        newMesh.triangles = initTriangles(mesh_);
        //Debug.Log("Generated Solid Mesh");
        return newMesh;
    }

    private Vector3[] initVertices(Mesh mesh_, float initExtrusion_)
    { //Generate vertices for mesh

        Vector3[] a = new Vector3[mesh_.vertexCount * 2 + mesh_.vertexCount * 4];
        Vector3 dir = getNormal(mesh_, initExtrusion_);

        for (int i = 0; i < mesh_.vertexCount; i++)
        {
            //a[i]=mesh.vertices[i]+new Vector3(0,initExtrusion,0);  //top face vertices
            a[i] = mesh_.vertices[i] + dir;              //top face vertices in relation to normal
            a[i + mesh_.vertexCount] = mesh_.vertices[i]; //bottom face vertices
        }

        int j = 2 * mesh_.vertexCount;
        for (int i = 0; i < mesh_.vertexCount; i++)
        {  //0374 1045 2156 3267 for quad
            a[j] = a[i];
            if (i == 0)
            {
                a[j + 1] = a[mesh_.vertexCount - 1];
                a[j + 2] = a[2 * mesh_.vertexCount - 1];
            }
            else
            {
                a[j + 1] = a[i - 1];
                a[j + 2] = a[i - 1 + mesh_.vertexCount];
            }
            a[j + 3] = a[i + mesh_.vertexCount];
            j = j + 4;
        }
        return a;
    }

    private int[] initTriangles(Mesh mesh_)
    {
        int topTriCount = mesh_.vertexCount - 2;    //number of triangles on top face
        int triCount = 4 * mesh_.vertexCount - 4;   //(mesh.vertexCount-2)*2+mesh.vertexCount*2; number of total triangles

        int[] a = new int[triCount * 3];
        for (int i = 0; i < topTriCount * 3; i++)
        {
            a[i] = mesh_.triangles[i];  //completes top face
            a[i + topTriCount * 3] = mesh_.triangles[topTriCount * 3 - 1 - i] + mesh_.vertexCount;  //completes bottom face "inverted normal"

        }
        int k = topTriCount * 6;
        int l = mesh_.vertexCount * 2;
        for (int j = 0; j < mesh_.vertexCount; j++)
        {
            a[k] = l;               //a
            a[k + 1] = l + 1;       //b
            a[k + 2] = l + 3;       //c
            a[k + 3] = l + 3;       //c
            a[k + 4] = l + 1;       //b
            a[k + 5] = l + 2;       //d
            k = k + 6;
            l = l + 4;
        }
        return a;
    }

    Vector3 getNormal(Mesh mesh_, float a)
    {
        Vector3 dir = mesh_.normals[0];
        //check for unevenness
        for (int i = 1; i < mesh_.normals.Length; i++)
        {
            dir += mesh_.normals[i];
        }
        dir /= mesh_.normals.Length;
        return dir * a;
    }

    private void initializeList()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        vertData = new List<Vector3>();
        vertIndex = new List<int>();
        vertices.AddRange(polygon.vertices);
        triangles.AddRange(polygon.triangles);
    }

    private void initializeData()
    {
        getVertices();
        getVerticesIndices();

        for (int i = 0; i < vertIndex.Count; i++)
        {
            vertices[vertIndex[i]] += faceNormal * 0.1f;
        }

        for (int i = 0; i < vertData.Count; i++)
        {
            vertData[i] += faceNormal * 0.1f;
        }
    }

    private void getVertices()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //use face normal to get vertices with same normal. P.S. WILL NOT WORK ON SMOOTH SURFACE!!
            MeshCollider meshCollider = hit.collider as MeshCollider;
            faceNormal = hit.normal;
            print("Normal=" + faceNormal);
            int j = 0;
            vertData.Clear();
            vertIndex.Clear();
            for (int i = 0; i < polygon.vertexCount; i++)
            {
                if (polygon.normals[i] == faceNormal)
                {
                    //vertPos[j]=mesh.vertices[i];
                    vertData.Add(polygon.vertices[i]);
                    j++;
                    print("found one! vertex is " + polygon.vertices[i]);
                }
            }
        }
    }

    void getVerticesIndices()
    {
        //print("tempVertIndex.Length=" + vertIndex.Count);
        //print("vertPos.Length=" + vertData.Count);

        //relate vertPos to actual vertice data and return their referencing numbers
        for (int i = 0; i < polygon.vertexCount; i++)
        {
            for (int j = 0; j < vertData.Count; j++)
            {
                if (vertices[i] == vertData[j]) vertIndex.Add(i);
            }
        }
    }

    bool isSameFace()
    {
        bool sameFace = false;
        double facePara = initialVertices[vertIndex[0]].x * faceNormal.x + initialVertices[vertIndex[0]].y * faceNormal.y + initialVertices[vertIndex[0]].z * faceNormal.z;
        int intfacePara = (int)(facePara * 10);
        double tempPara = 0f;
        int inttempPara = 0;
        for (int i = 0; i < vertIndex.Count; i++)
        {
            tempPara = vertices[vertIndex[i]].x * faceNormal.x + vertices[vertIndex[i]].y * faceNormal.y + vertices[vertIndex[i]].z * faceNormal.z;
            inttempPara = (int)(tempPara * 10);
            if (inttempPara == intfacePara)
            {
                sameFace = true;
                break;
            }
        }
        //print("lala");
        print(intfacePara);
        print(inttempPara);
        return sameFace;
    }

    private void updateMesh()
    {
        polygon.Clear();
        polygon.vertices = extrusion.vertices;      // replace vertices of original polygon
        polygon.triangles = extrusion.triangles;    // replace indices of original polygon
        polygon.RecalculateNormals();
        polygon.RecalculateTangents();
        polygon.RecalculateBounds();
        
        extFilter.mesh = polygon;
        extCollider.sharedMesh = polygon;
    }

}
