using System;
using UnityEngine;

public class SC_MoveFollow: MonoBehaviour
{
    public ET_objeffect_follow followEffect;
    void Update()
    {
        //follow
        if (followEffect.on && followEffect.go)
        {
            if (Vector2.Distance(followEffect.go.transform.position, transform.position) <= followEffect.triggerMoveDis)
            {
                Vector2 movedir = followEffect.go.transform.position - transform.position;
                movedir = Vector2.ClampMagnitude(movedir, 1);
                transform.Translate(movedir * Time.deltaTime * followEffect.speed);
            }
        }
    }
    [Serializable]
    public struct ET_objeffect_follow
    {
        public bool on;
        public Transform go;
        public float triggerMoveDis;
        public float speed;
    }
}
