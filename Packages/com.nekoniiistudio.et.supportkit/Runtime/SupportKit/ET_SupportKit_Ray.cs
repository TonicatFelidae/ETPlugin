using ET.SupportKit.ETPhysic;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET.SupportKit.ETRay
{
    public static class ET_Ray
    {
        //public static void ShootRay2DToPath(Vector2 loc, float length, float angle, out string path, out Vector2 rawloc)
        //{
        //    var direction = Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.up;
        //    path = null;
        //    rawloc = new Vector2();
        //
        //    Ray2D ray = new Ray2D(loc, direction);
        //    //ET_D.draw_line(ray.origin, ray.origin + ray.direction * length);
        //    RaycastHit2D hit1 = Physics2D.Raycast(ray.origin, ray.direction, length, 1024);
        //    if (hit1.collider != null)
        //    {
        //        path = hit1.collider.name;
        //        rawloc = hit1.point;
        //        Debug.Log("hit " + path);
        //    }
        //
        //}

        public static Collider2D GetCollider2D_BetweenTwoPoint(Vector2 from, Vector2 to, int layerMask)
        {
            Vector2 dir = to - from;
            return Physics2D.Raycast(from, dir, dir.magnitude, layerMask).collider;
        }
        public static Collider GetCollider_BetweenTwoPoint(Vector3 from, Vector3 to, int layerMask)
        {
            Vector3 dir = to - from;
            RaycastHit hit;
            Physics.Raycast(from, dir,out hit, dir.magnitude, layerMask);
            return hit.collider;
        }
        /// <summary>
        /// Shoot ray from collider (click point that point toward colldier object) use to get mouse point on object
        /// With Mathf.Infinity distance
        /// </summary>
        /// <returns></returns>
        public static Collider GetCollider_RayFirstHitFromMouse(int layerMask)
        {
            return GetCollider_RayFirstHitFromMouse(Mathf.Infinity, layerMask);
        }
        /// <summary>
        /// Shoot ray from collider (click point that point toward colldier object) use to get mouse point on object
        /// </summary>
        /// <returns></returns>
        public static Collider GetCollider_RayFirstHitFromMouse(float maxDistance, int layerMask)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, layerMask))
            {
                return hitInfo.collider;
            }

            return null;
        }
        public static RaycastHit2D GetFirstHit(Vector2 pointA, Vector2 pointB, LayerMask layerMask)
        {
            Vector2 direction = pointB - pointA;
            float distance = direction.magnitude;
            direction.NormalizeLight(distance);
            return Physics2D.Raycast(pointA, direction, Mathf.Infinity, layerMask);
        }
    }
}
