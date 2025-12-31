using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public enum AnchorTypeW2D // anchor world type
    {
        World,
        AtTransformPosition, // use object bound in xy space
        AtETTransformPivot, // only apply for ET pivot tool
        PointZero,
    }
}
