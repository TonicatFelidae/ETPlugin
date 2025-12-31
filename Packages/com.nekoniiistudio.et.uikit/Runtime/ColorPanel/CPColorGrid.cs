using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CPColorGrid : MonoBehaviour
{
    public Color[] colors;
    public UnityEvent<Color> onColorSelect;
    //
    public Button but_close;
    //
    public Transform box;
    public GameObject pp_CpColorGridItem;
    public void Start()
    {
        Init();
    }
    void Init()
    {
        foreach (Color item in colors)
        {
            GameObject go = Instantiate(pp_CpColorGridItem, box);
            go.GetComponent<Image>().color = item;
            go.GetComponent<Button>().onClick.AddListener(() => {
                onColorSelect.Invoke(item);
                });

        }
    }
    

}
