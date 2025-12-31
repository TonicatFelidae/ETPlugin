using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    /*
     * Direction code
     */
    public enum PositionPresents // with respect to start position // 9 position 
    {
        TopLeft = 1,
        TopCenter = 2,
        TopRight = 3,

        MiddleLeft = 4,
        MiddleCenter = 5, // special
        MiddleRight = 6,

        BottomLeft = 7,
        BottomCenter = 8,
        BottomRight = 9,

        None = 0,
    }
    public enum PositionPresents_Square
    {
        Top,
        Bot,
        Left,
        Right,
        
    }
    public enum DistancePresent // with respect to start position // 9 position 
    {
        Cube,
        Absolute
    }
    public enum ClampType
    {
        Continuous, // develop late
        SwitchPosition, //with position off set
        Symmetry,
    }
    public enum UIType
    {
        Image
    }
    public enum AnchorPresets
    {
        TopLeft = 1,
        TopCenter = 2,
        TopRight = 3,

        MiddleLeft = 4,
        MiddleCenter = 5,
        MiddleRight = 6,

        BottomLeft = 7,
        BottonCenter = 8,
        BottomRight = 9,

        BottomStretch,

        VertStretchLeft,
        VertStretchRight,
        VertStretchCenter,

        HorStretchTop,
        HorStretchMiddle,
        HorStretchBottom,

        StretchAll
    }

    public enum PivotPresets
    {
        TopLeft = 1,
        TopCenter = 2,
        TopRight = 3,

        MiddleLeft = 4,
        MiddleCenter = 5,
        MiddleRight = 6,

        BottomLeft = 7,
        BottomCenter = 8,
        BottomRight = 9,
    }
    public enum ETDirection
    {
        Up,
        Down,
        Left,
        Right,
        None,
    }
    public enum ETExtendedDirection
    {
        Up,
        Down,
        Left,
        Right,
        FarUp,
        FarDown,
        FarLeft,
        FarRight,
        None,
    }
}
