using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ET.SupportKit.ETRegion
{/// <summary>
 /// Zone Region what ever, powerful position indicator source code
 /// Reverse logic should implement but i dont have tiem yet TO DO
 /// </summary>
    public static class ERegion
    {
        /// <summary>
        /// Bound dependence
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetRandomPositionIn2DBound(SpriteRenderer spriteRenderer)
        {
            Vector2 minB = spriteRenderer.bounds.min;
            Vector2 maxB = spriteRenderer.bounds.max;
            return ET_Vector.GetRandomVector2(minB, maxB);
        }
        /// <summary>
        /// Bound dependence
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetRandomPositionIn3DBound(Renderer renderer)
        {
            Vector3 minB = renderer.bounds.min;
            Vector3 maxB = renderer.bounds.max;
            return ET_Vector.GetRandomVector3(minB, maxB);
        }
        /// <summary>
        /// Get random position around 2D position
        /// </summary>
        /// <param name="ob_pos"></param>
        /// <param name="minRangeIncluded"></param>
        /// <param name="maxrange"></param>
        /// <returns></returns>
        public static Vector2 GetRandomPosAroundPos(this Vector2 ob_pos, float minRangeIncluded, float maxRangeIncluded)
        {
            Vector2 retpos = new Vector2();
            if (maxRangeIncluded > minRangeIncluded)
            {
                retpos.x = UnityEngine.Random.Range(ob_pos.x - maxRangeIncluded, ob_pos.x + maxRangeIncluded);
                retpos.y = UnityEngine.Random.Range(ob_pos.y - maxRangeIncluded, ob_pos.y + maxRangeIncluded);
                while (!IsPointAroundPos2D(retpos, ob_pos, minRangeIncluded, maxRangeIncluded))
                {
                    retpos.x = UnityEngine.Random.Range(ob_pos.x - maxRangeIncluded, ob_pos.x + maxRangeIncluded);
                    retpos.y = UnityEngine.Random.Range(ob_pos.y - maxRangeIncluded, ob_pos.y + maxRangeIncluded);
                }
            }
            return retpos;
        }
        /// <summary>
        /// Get Random Pos In Rectangle. All min max value included.
        /// </summary>
        /// <param name="minx"></param>
        /// <param name="maxx"></param>
        /// <param name="miny"></param>
        /// <param name="maxy"></param>
        /// <returns></returns>
        public static Vector2 GetRandomPosInRectangle(float minx, float maxx, float miny, float maxy)
        {
            Vector2 retpos = new Vector2();
            retpos.x = UnityEngine.Random.Range(minx, maxx);
            retpos.y = UnityEngine.Random.Range(miny, maxy);
            return retpos;
        }
        public static Vector2 GetRandomPosInRectangle_AroundPos(Vector2 ob_pos, float range, float minx, float maxx, float miny, float maxy)
        {
            Vector2 retpos = GetRandomPosInRectangle(minx, maxx, miny, maxy);
            while (Vector2.Distance(retpos, ob_pos) > range)
            {
                retpos = GetRandomPosInRectangle(minx, maxx, miny, maxy);
            }
            return retpos;
        }
        public static Vector2 GetRandomPosInRectangle_NotAroundPos(Vector2 ob_pos, float range, float minx, float maxx, float miny, float maxy)
        {
            Vector2 retpos = GetRandomPosInRectangle(minx, maxx, miny, maxy);
            while (Vector2.Distance(retpos, ob_pos) <= range)
            {
                retpos = GetRandomPosInRectangle(minx, maxx, miny, maxy);
            }
            return retpos;
        }
        /// <summary>
        /// OTag objects have tags
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="range"></param>
        /// <param name="minx"></param>
        /// <param name="maxx"></param>
        /// <param name="miny"></param>
        /// <param name="maxy"></param>
        /// <returns></returns>
        public static Vector2 GetRandomPosInRectangle_AroundPos_OTags(float minRangeIncluded, float maxRangeIncluded, float minx, float maxx, float miny, float maxy, List<string> tags)
        {
            Vector2 retpos = GetRandomPosInRectangle(minx, maxx, miny, maxy);
            GameObject[] gos = GameObject.FindGameObjectsWithTag(tags[0]);
            if (tags.Count > 1)
            {
                for (int i = 1; i < tags.Count; i++)
                {
                    gos = gos.Concat(GameObject.FindGameObjectsWithTag(tags[i])).ToArray();
                }
            }
            Vector2[] poss = gos.GetAllPos2D();
            while (!IsPointAroundAllPos2D(poss, retpos, minRangeIncluded, maxRangeIncluded))
            {
                retpos = GetRandomPosInRectangle(minx, maxx, miny, maxy);
            }
            return retpos;
        }
        /// <summary>
        /// OTag objects have tags
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="range"></param>
        /// <param name="minx"></param>
        /// <param name="maxx"></param>
        /// <param name="miny"></param>
        /// <param name="maxy"></param>
        /// <returns></returns>
        public static Vector2 GetRandomPosInRectangle_NotAroundPos_OTags(float minRangeIncluded, float maxRangeIncluded, float minx, float maxx, float miny, float maxy, List<string> tags)
        {
            Vector2 retpos = GetRandomPosInRectangle(minx, maxx, miny, maxy);
            GameObject[] gos = GameObject.FindGameObjectsWithTag(tags[0]);
            if (tags.Count > 1)
            {
                for (int i = 1; i < tags.Count; i++)
                {
                    gos = gos.Concat(GameObject.FindGameObjectsWithTag(tags[i])).ToArray();
                }
            }
            Vector2[] poss = gos.GetAllPos2D();
            while (!IsPointNotAroundAllPos2D(poss,retpos, minRangeIncluded,maxRangeIncluded))
            {
                retpos = GetRandomPosInRectangle(minx, maxx, miny, maxy);
            }
            return retpos;
        }
        /// <summary>
        /// Around all pos 2D, it is not oposite with IsPointNotAroundAllPos2D
        /// </summary>
        /// <param name="oris"></param>
        /// <param name="pos"></param>
        /// <param name="minRangeIncluded"></param>
        /// <param name="maxRangeIncluded"></param>
        /// <returns></returns>
        public static bool IsPointAroundAllPos2D(Vector2[] oris, Vector2 pos, float minRangeIncluded, float maxRangeIncluded)
        {
            foreach (Vector2 ori in oris)
            {
                if (!IsPointAroundPos2D(ori,pos,minRangeIncluded,maxRangeIncluded))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Not around all pos 2D, it is not oposite with IsPointAroundAllPos2D
        /// </summary>
        /// <param name="oris"></param>
        /// <param name="pos"></param>
        /// <param name="minRangeIncluded"></param>
        /// <param name="maxRangeIncluded"></param>
        /// <returns></returns>
        public static bool IsPointNotAroundAllPos2D(Vector2[] oris, Vector2 pos, float minRangeIncluded, float maxRangeIncluded)
        {
            foreach (Vector2 ori in oris)
            {
                if (IsPointAroundPos2D(ori, pos, minRangeIncluded, maxRangeIncluded))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsPointAroundPos2D(Vector2 ori, Vector2 pos, float minRangeIncluded, float maxRangeIncluded)
        {
            return ETMath.IsBetweenRange(Vector2.Distance(pos, ori), minRangeIncluded, maxRangeIncluded);
        }

    }

}