using System.Collections.Generic;
using UnityEngine;


// require TMpro to run
namespace ET.SupportKit.ETileMap
{
    public static class ETTileMapExtension
    {
        public static Vector3Int GetAdjacentPosition(this Vector3Int loc, PositionPresents positionPresents)
        {
            switch (positionPresents)
            {
                case PositionPresents.TopLeft:
                    return new Vector3Int(loc.x - 1, loc.y +1);
                case PositionPresents.TopCenter:
                    return new Vector3Int(loc.x, loc.y + 1);
                case PositionPresents.TopRight:
                    return new Vector3Int(loc.x + 1, loc.y + 1);
                case PositionPresents.MiddleLeft:
                    return new Vector3Int(loc.x - 1, loc.y);
                case PositionPresents.MiddleCenter:
                    return new Vector3Int(loc.x, loc.y);
                case PositionPresents.MiddleRight:
                    return new Vector3Int(loc.x + 1, loc.y);
                case PositionPresents.BottomLeft:
                    return new Vector3Int(loc.x - 1, loc.y - 1);
                case PositionPresents.BottomCenter:
                    return new Vector3Int(loc.x, loc.y - 1);
                case PositionPresents.BottomRight:
                    return new Vector3Int(loc.x + 1, loc.y - 1);
            }
            return loc;
        }
    }
}
