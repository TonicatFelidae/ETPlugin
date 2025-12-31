using ET.Module.VersionControlSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoVerisionControl : MonoBehaviour
{
    public ETVersionControl versionControl = new();
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Before");
        versionControl.Setup(GotInfo, FailedGotInfo, PassBarrier, NotPassBarrier, true);
        versionControl.RunVersionInfoProtocol();
        Debug.Log("Done check");
        //Invoke("CloseScene",3f);
    }
    public void CloseScene()
    {
        SceneManager.LoadScene("New Scene");
    }
    public void GotInfo(VersionInfo versionInfo)
    {
        Debug.Log("GotInfo");
    }
    public void FailedGotInfo()
    {
        Debug.Log("FailedGotInfo");
    }
    public void PassBarrier()
    {
        Debug.Log("PassBarrier");
    }
    public void NotPassBarrier()
    {
        Debug.Log("NotPassBarrier");
    }

}
