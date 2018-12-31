using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ExtrudeFlag { ACTIVE, EDIT, INACTIVE, DELETE }

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshExtrusion : MonoBehaviour
{
    public float initExtrusion = 0.1f;
    public Vector3 extDirection;
    public float swipeScale = 1f;    // sensitivtiy for swipe to extrude 

    private ExtrudeFlag extrudeState;

    /* --------------------------------------------------
     *  Extrusion Data
     *  -------------------------------------------------- */

    /***** Handlers of polygon to be exturded *****/
    private Mesh polygon;

    private Vector3 faceNormal;

    /***** Handlers of exturded polyhedron *****/
    private Mesh extrusion;
    private List<Vector3> vertices;
    private List<int> triangles;

    private MeshCollider extCollider;       // collider for extrusion
    private MeshFilter extFilter;           // filter for extrusion

    /***** Handlers of track facet *****/
    private List<Vector3> vertData;
    private List<int> vertIndex;


    /* --------------------------------------------------
    *  Public Methods
    *  -------------------------------------------------- */

    public void setState(string flag)
    {
        extrudeState = (ExtrudeFlag)System.Enum.Parse(typeof(ExtrudeFlag), flag);
    }

    // Start is called before the first frame update
    void Start()
    {
        
        name = "MeshExtrusion";
        extCollider = gameObject.AddComponent<MeshCollider>();
        extFilter = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
        extrudeState = ExtrudeFlag.ACTIVE;
    }

    // Update is called once per frame
    void Update()
    {
        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            || (Input.GetMouseButton(0))) && extrudeState == ExtrudeFlag.ACTIVE)
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit)
                && (hit.collider.gameObject.name == "QuadPolygon" || hit.collider.gameObject.name == "QuadCircle"))
            {
                // Clone original polygon rather than pass by reference
                polygon = duplicate(hit.collider.GetComponent<MeshFilter>().mesh);

                extDirection = hit.normal;
                faceNormal = hit.normal;

                // convert plane to extrudable solid
                extrusion = convertSolid(polygon, extDirection);
                updateMesh();

                // setting up for manual extrusion 
                extCollider = gameObject.GetComponent<MeshCollider>();
                extCollider.sharedMesh = polygon;
                initializeList();
                
                initializeData();

                updateMesh();
                
                extrudeState = ExtrudeFlag.EDIT;
            }

        }

        else if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
           || (Input.GetMouseButton(0))) && extrudeState == ExtrudeFlag.EDIT)
        {
            
            //Debug.Log(Input.GetTouch(0).deltaPosition * swipeScale);
            Vector3 swipe = new Vector3(Input.GetTouch(0).deltaPosition.x * swipeScale, Input.GetTouch(0).deltaPosition.y * swipeScale);
            //Debug.Log(swipe);

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

        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            || (Input.GetMouseButtonUp(0))) && extrudeState == ExtrudeFlag.EDIT)
        { 
            extrudeState = ExtrudeFlag.INACTIVE;
        }

        if (extrudeState == ExtrudeFlag.DELETE)
        {
            Destroy(polygon);
            Destroy(extrusion);
            Destroy(gameObject);
        }

    }


    /* --------------------------------------------------
     *  Private Methods
     *  -------------------------------------------------- */
    private void updateMesh()
    {
        
        polygon.Clear();
        polygon.vertices = extrusion.vertices;      // replace vertices of original polygon
        polygon.triangles = extrusion.triangles;    // replace indices of original polygon
        polygon.RecalculateNormals();
        polygon.RecalculateTangents();
        polygon.RecalculateBounds();

        extFilter.mesh = polygon;
        extCollider = gameObject.GetComponent<MeshCollider>();
        extCollider.sharedMesh = polygon;
        
        /*
        extrusion.RecalculateNormals();
        extrusion.RecalculateTangents();
        extrusion.RecalculateBounds();

        extFilter.mesh = extrusion;
        extCollider = gameObject.GetComponent<MeshCollider>();
        extCollider.sharedMesh = extrusion;
        */
    }

    private Mesh duplicate(Mesh mesh_)
    {
        // Deep copy mesh data
        Mesh clone = new Mesh();
        clone.vertices = mesh_.vertices;
        clone.triangles = mesh_.triangles;
        clone.name = mesh_.name;
        clone.bounds = mesh_.bounds;
        clone.normals = mesh_.normals;
        clone.tangents = mesh_.tangents;
        clone.indexFormat = mesh_.indexFormat;
        return clone;
    }

    private Mesh convertSolid(Mesh mesh_, Vector3 dir_)
    {
        Mesh Solid = new Mesh();
        Solid.vertices = initVertices(mesh_, dir_);
        Solid.triangles = initTriangles(mesh_);
        return Solid;
    }

    private Vector3[] initVertices(Mesh mesh_, Vector3 dir_)
    { //Generate vertices for mesh

        Vector3[] a = new Vector3[mesh_.vertexCount * 2 + mesh_.vertexCount * 4];
        
        for (int i = 0; i < mesh_.vertexCount; i++)
        {
            a[i] = mesh_.vertices[i] + initExtrusion * dir_;        //top face vertices in relation to normal
            a[i + mesh_.vertexCount] = mesh_.vertices[i];           //bottom face vertices
        }

        // side face vertices
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
        int topTriCount = mesh_.vertexCount - 2;        // number of triangles on top face
        int tolTriCount = 4 * mesh_.vertexCount - 4;    // (mesh.vertexCount-2)*2+mesh.vertexCount*2; number of total triangles

        int[] a = new int[tolTriCount * 3];

        for (int i = 0; i < topTriCount * 3; i++)
        {
            a[i] = mesh_.triangles[i];  // completes top face
            a[i + topTriCount * 3] = mesh_.triangles[topTriCount * 3 - 1 - i] + mesh_.vertexCount;  // completes bottom face "inverted normal"
        }

        int k = topTriCount * 6;
        int l = mesh_.vertexCount * 2;

        /*
        if (isClockwise(mesh_, a)) 
        {
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
        }
        else
        {
            for (int j = 0; j < mesh_.vertexCount; j++)
            {
                a[k] = l + 2;           //d
                a[k + 1] = l + 1;       //b
                a[k + 2] = l + 3;       //c
                a[k + 3] = l + 3;       //c
                a[k + 4] = l + 1;       //b
                a[k + 5] = l;           //a
                k = k + 6;
                l = l + 4;
            }
        }
        */

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

        // Check whether the drawing direction is forward or reversed to extrude
        // direction "extDirection". If reversed, reverse the indices order.

        k = topTriCount * 6;
        l = mesh_.vertexCount * 2;
        if (!isClockwise(mesh_, a, l))
        {
            for (int j = 0; j < mesh_.vertexCount; j++)
            {
                a[k] = l + 2;           //d
                a[k + 1] = l + 1;       //b
                a[k + 2] = l + 3;       //c
                a[k + 3] = l + 3;       //c
                a[k + 4] = l + 1;       //b
                a[k + 5] = l;           //a
                k = k + 6;
                l = l + 4;
            }
        }

        return a;
    }

    private bool isClockwise(Mesh mesh_, int[] indices_, int str_)
    {
        if (mesh_.vertices == null) Debug.LogError("Mesh vertices have not initialized");

        Vector3 drawingNormal;

        Vector3 v01 = mesh_.vertices[1] - mesh_.vertices[0];
        Vector3 v12 = mesh_.vertices[2] - mesh_.vertices[1];

        drawingNormal.x = v01.y * v12.z - v01.z * v12.y;
        drawingNormal.y = -v01.x * v12.z + v01.z * v12.x;
        drawingNormal.z = v01.x * v12.y - v01.y * v12.x;

        if (extDirection == null) Debug.LogError("\"extDirection\" has not initialized");

        if (extDirection.x * drawingNormal.x +
                    extDirection.y * drawingNormal.y +
                    extDirection.z * drawingNormal.z > 0)
        {
            Debug.Log("Forward dircection");
            return true;
        }
        else if (extDirection.x * drawingNormal.x +
                    extDirection.y * drawingNormal.y +
                    extDirection.z * drawingNormal.z < 0)
        {
            Debug.Log("Reversed direction");
            return false;
        }
        else
        {
            Debug.Log("Perpendicular to \"extDirection\"");
            return true;
        }

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

        for (int i = 0; i < vertIndex.Count; i++) vertices[vertIndex[i]] += extDirection * initExtrusion;
        for (int i = 0; i < vertData.Count; i++) vertData[i] += extDirection * initExtrusion;
    }

    private void getVertices()
    {
        //use face normal to get vertices with same normal. P.S. WILL NOT WORK ON SMOOTH SURFACE!!

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) 
        {
            faceNormal = hit.normal;


            vertData.Clear();
            vertIndex.Clear();

            for (int i = 0; i < polygon.vertexCount; i++)
            {
                if (polygon.normals[i] == faceNormal)
                    vertData.Add(polygon.vertices[i]);
            }
        }
    }

    void getVerticesIndices()
    {
        for (int i = 0; i < polygon.vertexCount; i++)
        {
            for (int j = 0; j < vertData.Count; j++)
            {
                if (vertices[i] == vertData[j]) vertIndex.Add(i);
            }
        }
    }
}
