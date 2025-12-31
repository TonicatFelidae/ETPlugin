using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public class SC_DontDestroyOnLoad : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake ()
        {
            DontDestroyOnLoad(this);
        }
    }
}
