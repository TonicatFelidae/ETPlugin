
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ETRoundStickInput : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public class ET_but_roundmovebutton : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform dragRectTransform;
    //public void End
    public float archorcirclerad;
    public Vector2 locarchor;
    public bool bytransform;
    public Transform archorcircleloc;
    public Vector2 output_vec = new Vector2();
    public float output_vel = 0;
    public float output_click = 0;

    byte clicked = 0;
    float clicktimecount = 0;
    float timearchor = 0;


    public void OnDrag(PointerEventData eve)
    {
        dragRectTransform.anchoredPosition += eve.delta;
        var allowedPos = dragRectTransform.anchoredPosition - locarchor;
        //mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        allowedPos = Vector2.ClampMagnitude(allowedPos, archorcirclerad);
        dragRectTransform.anchoredPosition = locarchor + allowedPos;


        output_vec = dragRectTransform.anchoredPosition - locarchor;
        output_vel = Vector2.Distance(allowedPos, locarchor) / archorcirclerad;
    }
    public void OnPointerDown(PointerEventData eve)
    {

        clicked += 1;
        clicktimecount = 0;
        timearchor = clicktimecount;
    }
    public void OnPointerUp(PointerEventData eve)
    {
        if (clicktimecount < 0.4f)
        {
            timearchor = clicktimecount;
        }
        else
        {
            output_click = clicked;
            clicked = 0;
            Debug.Log("Clicked " + output_click);
        }

        dragRectTransform.anchoredPosition = locarchor;
        output_vel = 0;


        output_vec = Vector2.zero;
    }
    void Update()
    {
        if (clicked > 0)
        {
            clicktimecount += Time.deltaTime;
            if (Mathf.Abs(timearchor - clicktimecount) > 0.5f)
            {
                output_click = clicked;
                clicked = 0;
                Debug.Log("Clicked " + output_click);

            }
        }
        fix_size();
    }
    void fix_size()
    {
        int sizex1 = PlayerPrefs.GetInt("set button size");
        int sizex2 = sizex1 * 4 / 5;
        GetComponent<RectTransform>().sizeDelta = new Vector2(sizex1, sizex1);
        transform.Find("Inner").GetComponent<RectTransform>().sizeDelta = new Vector2(sizex2, sizex2);
        GetComponent<ET_but_roundmovebutton>().archorcirclerad = 30 + (sizex1 - 100) / 4;

    }
}