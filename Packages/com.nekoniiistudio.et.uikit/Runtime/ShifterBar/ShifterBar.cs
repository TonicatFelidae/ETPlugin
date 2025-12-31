using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShifterBar : MonoBehaviour
{
    public RectTransform oriImage;
    public RectTransform mask;
    public RectTransform newImage;

    private float _oriWitdh;
    public void Setup()
    {
        _oriWitdh = oriImage.rect.x; 
    }
    [SerializeField]
    public void OnSliderBarMove(float curValue)
    {
        float newWitdh = curValue* _oriWitdh;
        mask.rect.Set(mask.rect.x, mask.rect.y, newWitdh, mask.rect.height); 
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
