using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ET.SupportKit;

public class UISScrollview : MonoBehaviour
{
    // Start is called before the first frame update
    Transform content;
    public bool is_fixcontentheight;
    public int limitcontent = 0;
    public bool is_fixcontentheight_TMP;
    public bool is_autoscroll_TMP;
    public float scroll_delay;
    public float scroll_speed;
    float _curspacey;
    float _spacey;
    float _viewh = 60f;
    float _viewscrollh;
    void Start()
    {
        _curspacey = _spacey;
        content = transform.Find("Viewport").Find("Content").transform;
    }
    private void OnEnable()
    {
        _curspacey = _spacey =0;
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        if (limitcontent > 0)
        {
            if (content.childCount > limitcontent)
            {
                Destroy(content.GetChild(0).gameObject);
            }
        }
    }
    void Update()
    {
        
        if (is_fixcontentheight_TMP && content.childCount>0)
        {
            _spacey = content.GetChild(0).GetComponent<TextMeshProUGUI>().textBounds.size.y;
            if (_spacey != _curspacey)
            {
                content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, _spacey);
                content.GetComponent<RectTransform>().localPosition = new Vector2(0, 0);
                if (is_autoscroll_TMP)
                {
                    _viewscrollh = _spacey - _viewh;
                    if (_viewscrollh > 0)
                    {
                        StartCoroutine(ScrollContent());
                    }
                }
                _curspacey = _spacey;
            }

        }

        if (is_fixcontentheight)
        {
            float in_spacey = content.GetComponent<GridLayoutGroup>().spacing.y;
            float in_sizey = content.GetComponent<GridLayoutGroup>().cellSize.y;
            float padup = content.GetComponent<GridLayoutGroup>().padding.top;
            float paddown = content.GetComponent<GridLayoutGroup>().padding.bottom;

            Vector2 sizex = content.GetComponent<RectTransform>().sizeDelta;
            int childcount = ET_Transform.active_child_count(content);
            sizex.y = in_sizey * childcount + (childcount - 1) * in_spacey + padup + paddown;
            

            content.GetComponent<RectTransform>().sizeDelta = sizex;
        }
    }
    IEnumerator ScrollContent()
    {
        yield return new WaitForSeconds(scroll_delay);
        while (content.GetComponent<RectTransform>().localPosition.y < _viewscrollh)
        {
            float cury = content.GetComponent<RectTransform>().localPosition.y;
            content.GetComponent<RectTransform>().localPosition = new Vector2(0, cury += scroll_speed);
            yield return new WaitForEndOfFrame();
        }
        yield return StartCoroutine(ScrollContent());
    }
}
