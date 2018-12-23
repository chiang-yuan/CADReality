/* -*- C# -*-------------------------------------------------------------
 *  QuadPolygon.cs
 *  
 *  Create and Mesh convex polygon using Trangulator. Visulize the vertices
 *  , lines and mesh with Gizmos, MeshFilter and MeshRenderer.
 *  
 *  DEPENDENCY
 *      - Triangulator.cs
 *  
 *  LIMITATION
 *      - This script only supports convex and simply connected polygons
 *  
 *  Copyright version 2.0 (14/Dec/2018) Chiang Yuan
 *  
 *      v_2.0   |   add clockwise insensitivity
 *      v_1.0   |   create this script
 * ---------------------------------------------------------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

enum PolygonFlag { DRAW, EDIT, DELETE, INACTIVE };

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class QuadPolygon : MonoBehaviour {   

    public List<Vector3> vertices;  // user defined vertices
    public float pSize;             // point size
    private Mesh facet;
    private Vector3 normal;         // normal vector of facet

    private PolygonFlag polygonFlag = PolygonFlag.DRAW;
   

	// Use this for initialization
	void Start () {
        pSize = 1.0f;
    }
	
	// Update is called once per frame
	void Update () {
        if (polygonFlag == PolygonFlag.DRAW) Draw();
    }


    /* --------------------------------------------------
    * Private Function
    * -------------------------------------------------- */

    private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            Debug.Log("vertices[] are not defined.");
            return;
        }
        Gizmos.color = Color.black;
        foreach(Vector3 item in vertices)
        {
            Gizmos.DrawSphere(item, pSize);
        }

    }

    private void Draw()
    {
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) || Input.GetMouseButtonUp(0))
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                Vector3 point = hit.point;

                if (vertices.Count < 3)
                {
                    vertices.Add(point);
                    normal = hit.normal;
                }
                else if (!InsidePolygon(point))
                {
                    vertices.Add(point);
                    
                }
            }
            GenerateMesh();
        }

        
        /*
        float thresold = 0.001f;
        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            || (Input.GetMouseButton(0))) && touchCount == 0)
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                Vector3 point = hit.point;
                vertices.Add(point);
                normal = hit.collider.GetComponent<Plane>().normal;
                touchCount++;
            }
        }

        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                Vector3 point = hit.point;
                vertices.Add(point);
                normal = hit.collider.GetComponent<Plane>().normal;
            }
        }
        */
        /*
        else if (((Input.touchCount > 0) 
            && (Input.GetTouch(0).phase == TouchPhase.Moved)
            && (Input.GetTouch(0).deltaPosition.magnitude < thresold)))
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                Vector3 point = hit.point;
                vertices.Add(point);
                normal = hit.collider.GetComponent<Plane>().normal;
            }
            
        }
        */

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

        Matrix4x4 R = RB * RA ;

        //Debug.Log(target);
        //Debug.Log(R.MultiplyVector(target));

        return R.MultiplyVector(target);
    }

    private void GenerateMesh()
    {
        
        List<Vector2> vertices_2D = new List<Vector2>();
        foreach(Vector3 item in vertices)
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
        if (indices.Length/3 > 0)
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
        facet.name = "QuadPolygon";

        MeshRenderer renderer = gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
        MeshFilter filter = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = facet;
    

    }

    private bool InsidePolygon(Vector3 point_)
    {
        Vector3 rotpoint = AffineRefToYAxis(normal, point_ - vertices[0]);
        Vector2 rotpoint_2D = new Vector2(rotpoint.x, rotpoint.z);

        List<Vector2> vertices_2D = new List<Vector2>();
        foreach (Vector3 item in vertices)
        {
            Vector3 rotVertex = AffineRefToYAxis(normal, item - vertices[0]);
            vertices_2D.Add(new Vector2(rotVertex.x, rotVertex.z));
        }

        // Use triangulator to get the indices for triangulation
        Triangulator triangulator = new Triangulator(vertices_2D);
        int[] indices = triangulator.Triangulate();

        bool inside = false;
        for(int i = 0; i < indices.Length/3; i++)
        {
            Debug.Log(vertices_2D[indices[3 * i]]);
            Debug.Log(vertices_2D[indices[3 * i + 1]]);
            Debug.Log(vertices_2D[indices[3 * i + 2]]);
            Debug.Log(rotpoint_2D);
            if (triangulator.InsideTriangle(vertices_2D[indices[3 * i + 2]],
                                            vertices_2D[indices[3 * i + 1]],
                                            vertices_2D[indices[3 * i]],
                                            rotpoint_2D))
            {
                inside = true;
                break;
            }
        }

        return inside;
    }
}
