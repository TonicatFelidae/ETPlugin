using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EUColorField : MonoBehaviour
{
    public Button colorPickerButton;
    public Image colorPresent;
    public CPColorGrid CP_ColorGrid;
    public UnityEvent onColorSellect;
    public Color color;

    private void Awake()
    {
        colorPickerButton.onClick.AddListener(() => CP_ColorGrid.gameObject.SetActive(true));
        CP_ColorGrid.onColorSelect.AddListener((color) => ColorSellect(color));
    }
    private void ColorSellect(Color choosedColor)
    {
        colorPresent.color = choosedColor;
        //onColorSellect.Invoke();
    }
}
