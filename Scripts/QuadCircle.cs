/* -*- C# -*-------------------------------------------------------------
 *  QuadCircle.cs
 *  
 *  Create circle composed of several polygon mesh
 *  
 *  DEPENDENCY
 *      - Triangulator.cs
 *  
 *  LIMITATION
 *  
 *  Copyright version 1.0 (26/Dec/2018) Chiang Yuan
 *  
 *      v_1.0   |   build this script
 * ---------------------------------------------------------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;

enum CircleFlag { DRAW, EDIT, DELETE, INACTIVE };

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class QuadCircle : MonoBehaviour
{
    public InputField input;

    public int npoints;                 // number of points to approximate the circle

    private List<Vector3> vertices;
    private Mesh facet;
    private Vector3 normal;
    private Vector3 center, circumference;
    private int touchCount;

    

    private CircleFlag circleFlag;

    public void setNPoints()
    {
        npoints = int.Parse(input.text);
    }
    public void setState(string flag)
    {
        circleFlag = (CircleFlag)System.Enum.Parse(typeof(CircleFlag), flag);
    }

    void Awake()
    {
        //npoints = 100;
        vertices = new List<Vector3>(npoints);
    }

    // Start is called before the first frame update
    void Start()
    {
        circleFlag = CircleFlag.DRAW;
        touchCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (circleFlag == CircleFlag.DRAW) Draw();
        if (circleFlag == CircleFlag.DELETE) Delete();
    }

    /* --------------------------------------------------
    * Private Function
    * -------------------------------------------------- */

    private void OnDrawGizmos()
    {
        if (vertices == null)
        { 
            return;
        }
        Gizmos.color = Color.black;
        foreach (Vector3 item in vertices)
        {
            Gizmos.DrawSphere(item, 1);
        }

    }

    private void Draw()
    {
        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            || (Input.GetMouseButton(0))) && touchCount == 0)
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                center = hit.point;
                circumference = center;
                normal = hit.normal;
                vertices.Add(circumference);
                Vector3 radialRay = circumference - center;
                for (int i = 1; i < npoints; i++)
                {
                    radialRay = Quaternion.AngleAxis(-360f / npoints, normal) * radialRay;
                    vertices.Add(center + radialRay);
                }

                touchCount++;
            }
        }

        else if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            || (Input.GetMouseButton(0))))
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                
                circumference = hit.point;
                vertices[0] = circumference;
                Vector3 radialRay = circumference - center;
                for (int i = 1; i < npoints; i++)
                {
                    radialRay = Quaternion.AngleAxis(-360f / npoints,normal) * radialRay;
                    vertices[i] = center + radialRay;
                    
                }
                
                GenerateMesh();
            }
        }

        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            || (Input.GetMouseButtonUp(0))))
        {
            circleFlag = CircleFlag.INACTIVE;
        }

        
        
    }

    private Vector3 AffineRefToYAxis(Vector3 reference, Vector3 target)
    {
        float cosA = reference.z
            / (Mathf.Sqrt(reference.x * reference.x + reference.z * reference.z));
        if (Mathf.Sqrt(reference.x * reference.x + reference.z * reference.z) == 0) cosA = 0;
        float sinA = Mathf.Sqrt(1 - Mathf.Pow(cosA, 2));

        float cosB = reference.y / reference.magnitude;
        float sinB = Mathf.Sqrt(1 - Mathf.Pow(cosB, 2));

        // Rotation Matrix
        Matrix4x4 RA = new Matrix4x4(new Vector4(cosA, 0, -sinA, 0),
                                      new Vector4(0, 1, 0, 0),
                                      new Vector4(sinA, 0, cosA, 0),
                                      new Vector4(0, 0, 0, 1));
        Matrix4x4 RB = new Matrix4x4(new Vector4(1, 0, 0, 0),
                                      new Vector4(0, cosB, sinB, 0),
                                      new Vector4(0, -sinB, cosB, 0),
                                      new Vector4(0, 0, 0, 1));

        Matrix4x4 R = RB * RA;

        //Debug.Log(target);
        //Debug.Log(R.MultiplyVector(target));

        return R.MultiplyVector(target);
    }

    private void GenerateMesh()
    {

        List<Vector2> vertices_2D = new List<Vector2>();
        foreach (Vector3 item in vertices)
        {
            Vector3 rotVertex = AffineRefToYAxis(normal, item - vertices[0]);
            vertices_2D.Add(new Vector2(rotVertex.x, rotVertex.z));
        }

        // Use triangulator to get the indices for triangulation
        Triangulator triangulator = new Triangulator(vertices_2D);
        int[] indices = triangulator.Triangulate();

        var colors = Enumerable.Range(0, vertices.ToArray().Length)
            .Select(i => UnityEngine.Random.ColorHSV())
            .ToArray();

        facet = new Mesh();
        facet.vertices = vertices.ToArray();
        facet.triangles = indices;
        facet.colors = colors;

        // Check whether the mesh normal is parallel to the face normal.
        // If not, reverse the mesh normal to make it visible.
        // This section makes the drawing direction clockwise insensitive.
        Vector3 meshNormal;
        if (indices.Length / 3 > 0)
        {
            Vector3 v01 = vertices[indices[1]] - vertices[indices[0]];
            Vector3 v12 = vertices[indices[2]] - vertices[indices[1]];

            meshNormal.x = v01.y * v12.z - v01.z * v12.y;
            meshNormal.y = -v01.x * v12.z + v01.z * v12.x;
            meshNormal.z = v01.x * v12.y - v01.y * v12.x;

            if (normal.x * meshNormal.x +
            normal.y * meshNormal.y +
            normal.z * meshNormal.z < 0)
            {
                //indices.Reverse(); // This command cannot reverse int[] array at all!
                Array.Reverse(indices);
                facet.triangles = indices;
            }
        }

        facet.RecalculateNormals();
        facet.RecalculateBounds();
        facet.name = "QuadCircle";

        MeshRenderer renderer = gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
        MeshFilter filter = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
        MeshCollider collider = gameObject.GetComponent(typeof(MeshCollider)) as MeshCollider;
        collider.sharedMesh = facet;
        filter.mesh = facet;


    }

    private void Delete()
    {
        if (circleFlag == CircleFlag.DELETE)
        {
            Destroy(facet);

            Destroy(gameObject);
        }
    }

}


