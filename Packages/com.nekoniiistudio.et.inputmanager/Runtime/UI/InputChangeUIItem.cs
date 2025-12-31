using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ET.Module.ETInput;
using UnityEngine.UI;
using TMPro;

public class InputChangeUIItem : MonoBehaviour
{
    public TextMeshProUGUI tx_label;
    public Button but_changeInput;
    public InputChangeUI source;
    public InputCode inputCode;
    private void Start()
    {
        but_changeInput.onClick.AddListener(()=>
            {
                source.StartChangeInput(inputCode);
            }
            );
    }

}
