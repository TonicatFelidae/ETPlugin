using System.Collections.Generic;
using UnityEngine;


// require TMpro to run
namespace ET.SupportKit
{
    public static class ETMath
    {
        /// <summary>
        /// Get result chance with percent input,example if you need chance of 40% input parameter is 40, you will have 40% chance to win
        /// </summary>
        /// <param name="percent">example if you need chance of 40% input parameter is 40</param>
        /// <returns></returns>
        public static bool DiceAndWin(float percent)
        {
            float var = Random.Range(0f, 100f);
            if (var <= percent) return true;
            else return false;
        }

        public static int GetRandom_Differed(this int curValue, int minInclusive, int maxExclusive)
        {
            if (minInclusive == maxExclusive - 1 && minInclusive == maxExclusive) return curValue;
            int ret = UnityEngine.Random.Range(minInclusive, maxExclusive);
            if (ret != curValue) return ret;
            return GetRandom_Differed(curValue, minInclusive, maxExclusive);
        }
        public static float GetRandom_Differed(this float curValue, float minInclusive, float maxInclusive)
        {
            if (minInclusive == maxInclusive - 1 && minInclusive == maxInclusive) return curValue;
            float ret = UnityEngine.Random.Range(minInclusive, maxInclusive);
            if (ret != curValue) return ret;
            return GetRandom_Differed(curValue, minInclusive, maxInclusive);
        }
        public static Vector2 DegToVector2(Vector2 root, float degree, float lenght)
        {
            Vector2 vec = Vector2.up;
            vec.y = lenght * Mathf.Cos(Mathf.Deg2Rad * degree);
            vec.x = lenght * Mathf.Sin(Mathf.Deg2Rad * degree);

            return vec;

        }
        /// <summary>
        /// Simple tween float code that can handle float lept at constain speed
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="speed">Please make this positive (code change direction inside)</param>
        /// <returns></returns>
        public static float LerpToFloat(float from, float to, float speed)
        {
            float dis = to - from;
            if (Mathf.Abs(dis) <= speed) from = to;
            else
            {
                if (dis > 0) from += speed;
                else from -= speed;
            }
            return from;
        }
        public static bool IsBetweenRange(this float thisValue, float minIncluded, float maxIncluded)
        {
            return thisValue >= minIncluded && thisValue <= maxIncluded;
        }
        public static bool IsBetweenRange(this int thisValue, int minIncluded, int maxExcluded)
        {
            return thisValue >= minIncluded && thisValue < maxExcluded;
        }
        public static int ClampInLoop(int value, int minValueIncluded, int maxValueExcluded)
        {
            if (minValueIncluded == maxValueExcluded) return minValueIncluded;
            int range = maxValueExcluded - minValueIncluded;
            int valuex = value % (maxValueExcluded - minValueIncluded);
            if (valuex < 0)
            {
                return range + valuex;
            }
            else
            {
                return minValueIncluded + valuex;
            }
        }
        /// <summary>
        /// Solve of n(n-1)(n-2)...1
        ///  eg 800 + 799 + 798 + 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int NMinusSeries(int n)
        {
            return (n+1)*n/2;
        }
        /// <summary>
        /// Factorial of n 
        /// eg 800 * 799 * 798 * ...
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int FactorialSeries(int n)
        {
            int ret = 1;
            for (int i = 2; i <= n; i++) ret *= i;
            return ret;
        }
        public static Vector3Int SumVector3IntWithinRange(Vector3Int a, Vector3Int b, SquareRange2DInt range)
        {
            Vector3Int ret = new();
            ret.x = a.x + b.x; ret.x = ret.x < range.minX ? range.minX : ret.x; ret.x = ret.x > range.maxX ? range.maxX : ret.x;
            ret.y = a.y + b.y; ret.y = ret.y < range.minY ? range.minY : ret.y; ret.y = ret.y > range.maxY ? range.maxY : ret.y;
            return ret;
        }
        public static Vector2Int RotationRight(this Vector2Int ori)
        {
            Vector2Int ret = ori;
            ret.x = ori.y;
            ret.y = -ori.x;
            return ret;
        }
        public static Vector3Int RotationRight(this Vector3Int ori)
        {
            Vector3Int ret = ori;
            ret.x = ori.y;
            ret.y = -ori.x;
            return ret;
        }
        public static Vector3Int FlipVerticle(this Vector3Int ori)
        {
            Vector3Int ret = ori;
            ret.y = -ret.y;
            return ret;
        }
        public static Vector3Int FlipHorizontal(this Vector3Int ori)
        {
            Vector3Int ret = ori;
            ret.x = -ret.x;
            return ret;
        }
        public static Vector3Int ToVector3Int(this Vector2Int ori)
        {
            return new Vector3Int(ori.x, ori.y, 0);
        }
        public static Vector2Int ToVector2Int(this Vector3Int ori)
        {
            return new Vector2Int(ori.x, ori.y);
        }

    }
}
namespace ET.SupportKit.EMath
{
    public static class ETMathExtension
    {
        /// <summary>
        /// One dimention nomalize
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int Normalize(this float input)
        {
            return input > 0 ? (input == 0 ? 0 : 1) : -1;
        }
        /// <summary>
        /// One dimention nomalize
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int Normalize(this int input)
        {
            return input > 0 ? (input == 0 ? 0 : 1) : -1;
        }
    }
}
