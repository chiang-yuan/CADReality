using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PolygonExtruder : MonoBehaviour
{
    public GameObject meshExtrusion;

    private bool editone = false;
    private ButtonFlag buttonFlag = ButtonFlag.DESELECT;

    public void clickExtrudeButton()
    {
        buttonFlag = ButtonFlag.EXTRUDE;
        foreach (Transform iter in transform)
        {
            iter.GetComponent<MeshExtrusion>().setState("ACTIVE");
        }
    }

    public void clickDoneButton()
    {
        buttonFlag = ButtonFlag.DESELECT;
        foreach (Transform iter in transform)
        {
            iter.GetComponent<MeshExtrusion>().setState("INACTIVE");
        }
    }

    public void clickEraserButton()
    {
        buttonFlag = ButtonFlag.ERASER;
        
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (buttonFlag == ButtonFlag.EXTRUDE && editone == false) Extrude();
        if (buttonFlag == ButtonFlag.DESELECT) editone = false;
        if (buttonFlag == ButtonFlag.ERASER) Eraser();
    }


    /* --------------------------------------------------
     *  Private Function
     * -------------------------------------------------- */

    private void Extrude()
    {
        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            || (Input.GetMouseButton(0))) && buttonFlag == ButtonFlag.EXTRUDE)
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit)
                && (hit.collider.gameObject.name == "QuadPolygon" || hit.collider.gameObject.name == "QuadCircle"))
            {
                GameObject newExtrusion = Instantiate(meshExtrusion, transform);
                editone = true;
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
                hit.collider.GetComponent<MeshExtrusion>().setState("DELETE");
            }
        }
    }
}