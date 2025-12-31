using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InputChangeUIConfirmDuplicate : MonoBehaviour
{
    public TextMeshProUGUI tx_warning;
    public void SetConflictInput(string keyLabel, string inputLabel)
    {

        string tx = $"The key <color=#7811ed><u><b>{keyLabel}</b></u></color> already used by <color=#e6103b><u><b>{inputLabel}</b></u></color>. " +
            $"Change it anyway?";
        tx_warning.text = tx;
    }
}
