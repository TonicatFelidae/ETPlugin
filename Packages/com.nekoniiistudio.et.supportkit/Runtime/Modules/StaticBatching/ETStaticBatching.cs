using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ET;

public class ETStaticBatching : MonoBehaviour
{
    public GameObject[] batchingBoxs;
    public bool batchBoxsOnStart;
    // Start is called before the first frame update
    void Start()
    {
        if(batchBoxsOnStart)
        {
            BatchBoxs();
        }
    }
    public void BatchBoxs()
    {
        for(int i = 0; i < batchingBoxs.Length; i++)
        {
            StaticBatchingUtility.Combine(batchingBoxs[i]);
            Debug.Log("Batch Box => " + batchingBoxs[i].name);
        }
    }
}
