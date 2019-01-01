/* -*- C# -*------------------------------------------------------------- *  PolygonInspector.cs *   *   *  Copyright version 1.0 (2018/12) Chiang Yuan & EricChiuBird *   *      v_1.0   |   build this script *       * ---------------------------------------------------------------------- */

using System;using System.Collections;using System.Collections.Generic;using UnityEngine;using UnityEditor;
using UnityEngine.XR.iOS;
using UnityEngine.UI;

//enum ButtonFlag { ERASER, PENCIL, DESELECT, DRAWRED, DRAWBLUE, DRAWGREEN, POLYGON };

public class PolygonInspector : MonoBehaviour{    public GameObject Polygon;    public GameObject Circle;
    public GenerateAnchorPlane anchorplane;
    public Material Wood;    public Material Concrete;    public Material Carbon;    public Material Brick;    public Material Metal;

    private Material MaterialChange;    private bool drawone = false;    private ButtonFlag buttonFlag = ButtonFlag.DESELECT;
    private MaterialFlag materialflag;
    /* --------------------------------------------------     * Public Function     * -------------------------------------------------- */
    public void ClickConcreteButton()    {        materialflag = MaterialFlag.CONCRETE;    }    public void ClickWoodButton()    {        materialflag = MaterialFlag.WOOD;    }    public void ClickCarbonButton()    {        materialflag = MaterialFlag.CARBON;    }    public void ClickBrickButton()    {        materialflag = MaterialFlag.BRICK;    }    public void ClickMetalButton()    {        materialflag = MaterialFlag.METAL;    }
    public void clickPolygonButton()    {        buttonFlag = ButtonFlag.POLYGON;    }    public void clickCircleButton()    {        buttonFlag = ButtonFlag.CIRCLE;    }    public void clickEraserButton()    {        buttonFlag = ButtonFlag.ERASER;    }    public void clickDoneButton()    {        buttonFlag = ButtonFlag.DESELECT;        foreach (Transform iter in transform)        {            if (iter.gameObject.name == "QuadPolygon") iter.GetComponent<QuadPolygon>().setState("INACTIVE");            if (iter.gameObject.name == "QuadCircle") iter.GetComponent<QuadCircle>().setState("INACTIVE");        }        drawone = false;    }

    // Use this for initialization
    void Start()    {    }

    // Update is called once per frame
    void Update()    {        if (buttonFlag == ButtonFlag.POLYGON && drawone == false) CreatePolygon(MaterialChange);        if (buttonFlag == ButtonFlag.CIRCLE && drawone == false) CreateCircle(MaterialChange);
        if (materialflag == MaterialFlag.WOOD) MaterialChange = Wood;        if (materialflag == MaterialFlag.CARBON) MaterialChange = Carbon;        if (materialflag == MaterialFlag.BRICK) MaterialChange = Brick;        if (materialflag == MaterialFlag.METAL) MaterialChange = Metal;        if (materialflag == MaterialFlag.CONCRETE) MaterialChange = Concrete;        if (buttonFlag == ButtonFlag.ERASER) Eraser();

        if (FindObjectsOfType<QuadCircle>() != null && anchorplane.isUpdate())        {            Debug.Log("Select Lines");            foreach (QuadCircle gameObj in FindObjectsOfType<QuadCircle>())            {                Debug.Log("Transforming Lines");                gameObj.transform.position += anchorplane.getPosition();                gameObj.transform.rotation = anchorplane.getQuaternion();            }        }

        if (FindObjectsOfType<QuadPolygon>() != null && anchorplane.isUpdate())        {            Debug.Log("Select Lines");            foreach (QuadPolygon gameObj in FindObjectsOfType<QuadPolygon>())            {                Debug.Log("Transforming Lines");                gameObj.transform.position += anchorplane.getPosition();                gameObj.transform.rotation = anchorplane.getQuaternion();            }        }    }


    /* --------------------------------------------------    * Private Function    * -------------------------------------------------- */

    private void CreatePolygon(Material material)    {        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)            || Input.GetMouseButtonDown(0))        {            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);            RaycastHit hit;            if (Physics.Raycast(Ray, out hit))            {                GameObject newPolygon = Instantiate(Polygon, hit.point, Quaternion.identity, transform);                newPolygon.GetComponent<Renderer>().material = material;                drawone = true;            }        }    }    private void CreateCircle(Material material)    {        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)            || Input.GetMouseButtonDown(0))        {            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);            RaycastHit hit;            if (Physics.Raycast(Ray, out hit))            {                GameObject newCircle = Instantiate(Circle, hit.point, Quaternion.identity, transform);                newCircle.GetComponent<Renderer>().material = material;                drawone = true;            }        }    }    private void Eraser()    {        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)            || (Input.GetMouseButton(0))))        {            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);            RaycastHit hit;            if (Physics.Raycast(Ray, out hit))            {                if (hit.collider.gameObject.name == "QuadPolygon") hit.collider.GetComponent<QuadPolygon>().setState("DELETE");                if (hit.collider.gameObject.name == "QuadCircle") hit.collider.GetComponent<QuadCircle>().setState("DELETE");            }        }    }}