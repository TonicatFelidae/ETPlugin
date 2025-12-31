using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public interface ITimeManager
    {
        List<ETime> ETimes { get; set; }
        void Add(ETime eTime);
    }

}