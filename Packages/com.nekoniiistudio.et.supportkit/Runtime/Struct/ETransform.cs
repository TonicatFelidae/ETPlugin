using UnityEngine;
using System;

namespace ET
{
    [Serializable]
    public class ETransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public ETransform(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
            scale = Vector3.one;
        }
        public ETransform(Transform transform)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            scale = transform.localScale;
        }
    }

}
