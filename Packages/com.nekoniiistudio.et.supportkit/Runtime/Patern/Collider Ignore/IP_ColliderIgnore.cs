using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IP_ColliderIgnore
{
    List<string> ColliderIgnoreTags { get; }
    public bool IsIgnore(GameObject go)
    {
        foreach (var item in ColliderIgnoreTags) { if (go.tag == item) return true; }
        return false;
    }
}
//public class FastTry : IP_ColliderIgnore
//{
//    public List<string> ColliderIgnoreTags => throw new System.NotImplementedException();
//    public bool IsIgnore(GameObject go)
//    {
//        return false;
//    }
//    public void TryMethod(GameObject go)
//    {
//        bool s = ((IP_ColliderIgnore)new FastTry()).IsIgnore(go);
//    }
//}