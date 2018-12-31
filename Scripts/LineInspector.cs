/* -*- C# -*-------------------------------------------------------------
 *  CylindrLine.cs
 *  
 *  
 *  Copyright version 3.1 (2018/12) Chiang Yuan & EricChiuBird
 *  
 *      v_3.1   |   add eraser button
 *      v_3.0   |   add Begin() and End() function
 *      v_2.0   |   add Input.TouchCount & Input.GetTouch condition in 
 *                  Update()
 * ---------------------------------------------------------------------- */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//using UnityEngine.XR.iOS;
using UnityEngine.UI;

//enum ButtonFlag { ERASER, PENCIL, DESELECT, DRAWRED, DRAWBLUE, DRAWGREEN };

public class LineInspector : MonoBehaviour
{
    public GameObject Line;
    [Range(0.01f, 0.1f)]
    float z = 0.1f;
    [Range(0.01f, 0.1f)]
    float x = 0.1f;
    public Slider slider;

    private ButtonFlag buttonFlag;

    /* --------------------------------------------------
     * Public Function
     * -------------------------------------------------- */

    public void clickPencilButton()
    {
        buttonFlag = ButtonFlag.PENCIL;
        //CancelInvoke();

        //InvokeRepeating("Pencil", 0f, 0.01f);
    }
    public void clickDrawRedButton()
    {
        buttonFlag = ButtonFlag.DRAWRED;
        //CancelInvoke();

        //InvokeRepeating("Pencil", 0f, 0.01f);
    }
    public void clickDrawBlueButton()
    {
        buttonFlag = ButtonFlag.DRAWBLUE;
        //CancelInvoke();

        //InvokeRepeating("Pencil", 0f, 0.01f);
    }
    public void clickDrawGreenButton()
    {
        buttonFlag = ButtonFlag.DRAWGREEN;
        //CancelInvoke();

        //InvokeRepeating("Pencil", 0f, 0.01f);
    }
    public void clickEraserButton()
    {
        buttonFlag = ButtonFlag.ERASER;
        //CancelInvoke();
        /*
        foreach (Transform iter in transform)
        {
            iter.GetComponent<CylinderLine>().setState("DELETE");
        }
        */
        //InvokeRepeating("Eraser", 0f, 0.01f);
    }
    public void clickDoneButton()
    {
        buttonFlag = ButtonFlag.DESELECT;
        //CancelInvoke();
        foreach (Transform iter in transform)
        {
            iter.GetComponent<CylinderLine>().setState("INACTIVE");
        }
    }
    public void Begin()
    {
        // Call TimedUpdate immediately, repeat every 0.01 seconds
        InvokeRepeating("Draw", 0f, 0.01f);
    }

    public void End()
    {
        // Call TimedUpdate immediately, repeat every 0.1 seconds
        CancelInvoke();
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (buttonFlag == ButtonFlag.PENCIL) Pencil(Color.black, slider);
        if (buttonFlag == ButtonFlag.DRAWRED) Pencil(Color.red, slider);
        if (buttonFlag == ButtonFlag.DRAWBLUE) Pencil(Color.blue, slider);
        if (buttonFlag == ButtonFlag.DRAWGREEN) Pencil(Color.green, slider);
        if (buttonFlag == ButtonFlag.ERASER) Eraser();
    }

    /* --------------------------------------------------
    * Private Function
    * -------------------------------------------------- */

    private void Pencil(Color color, Slider mainslider)
    {
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            || (Input.GetMouseButtonDown(0)))
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                GameObject newLine = Instantiate(Line, hit.point, Quaternion.identity, transform);
                newLine.GetComponent<Renderer>().material.color = color;
                newLine.transform.localScale = new Vector3(mainslider.value, 0.01f, mainslider.value);
            }
        }
    }

    private void Eraser()
    {
        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            || (Input.GetMouseButton(0))))
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                hit.collider.GetComponent<CylinderLine>().setState("DELETE");
            }
        }
    }
}
