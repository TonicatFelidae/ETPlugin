using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using System;
using ET.SupportKit.Collection;
using System.IO;
using Random = UnityEngine.Random;
using System.Text;

namespace ET.SupportKit
{
    public static class ET_Draw
    {
        public static void DrawLine(Vector2 start, Vector2 end, GameObject par, Material line_mat, string namex = "line")
        {
            GameObject myLine = new GameObject();
            //myLine.transform.localScale = par.transform.localScale;
            myLine.name = namex;
            myLine.transform.position = par.transform.position;
            myLine.transform.parent = par.transform;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.material = line_mat;

            //lr.SetColors(color, color);
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
        public static void DrawPathline(Vector2 start, Vector2 end, string ID, Transform parentx)
        {
            GameObject myLine = new GameObject();
            //myLine.transform.localScale = par.transform.localScale;
            myLine.name = ID;
            myLine.layer = 10;
            myLine.transform.position = parentx.position;
            myLine.transform.parent = parentx;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.useWorldSpace = false;
            //lr.material = K.list_mat[4];

            //lr.SetColors(color, color);
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            myLine.AddComponent<BoxCollider2D>();
            myLine.GetComponent<BoxCollider2D>().size = new Vector2(Vector2.Distance(start, end), 0.01f);
        }
        public static void DrawColliderLine90(Vector2 start, Vector2 end, string ID, Transform parentx)
        {
            GameObject myLine = new GameObject();
            //myLine.transform.localScale = par.transform.localScale;
            myLine.name = ID;
            myLine.layer = 10;
            myLine.transform.position = parentx.position;
            myLine.transform.parent = parentx;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.useWorldSpace = false;
            //lr.material = K.list_mat[4];

            //lr.SetColors(color, color);
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            myLine.AddComponent<BoxCollider2D>();
            if (start.x == end.x)
            {
                myLine.GetComponent<BoxCollider2D>().size = new Vector2(0.01f, Vector2.Distance(start, end));

            }
            else
            {
                myLine.GetComponent<BoxCollider2D>().size = new Vector2(Vector2.Distance(start, end), 0.01f);

            }
        }
        
        //TODO draw circle still create too many single line objects
        public static void DrawCircle(float x, float y, float xradius, float yradius, int segments, float startangle, float endangle, Material mat, Transform parrent)
        {

            float xo = 0;
            float yo = 0;
            float xn = 0;
            float yn = 0;
            float totalangle = Mathf.Abs(endangle - startangle);
            for (int i = 0; i < (segments + 1); i++)
            {
                xn = Mathf.Sin(Mathf.Deg2Rad * startangle) * xradius;
                yn = Mathf.Cos(Mathf.Deg2Rad * startangle) * yradius;
                if (i > 0)
                    ET_Draw.DrawLine(new Vector2(x + xn, y + yn), new Vector2(x + xo, y + yo), parrent.gameObject, mat);
                xo = xn;
                yo = yn;
                startangle += (totalangle / segments);
            }
        }

    }
    public static class ET_Direction
    {
        public static Vector3Int GetRelativePosition(this Vector3Int ori, ETExtendedDirection eTExtendedDirection, bool far = false)
        {
            if (far)
            {
                switch (eTExtendedDirection)
                {
                    case ETExtendedDirection.Up:
                        eTExtendedDirection = ETExtendedDirection.FarUp;
                        break;
                    case ETExtendedDirection.Down:
                        eTExtendedDirection = ETExtendedDirection.FarDown;
                        break;
                    case ETExtendedDirection.Left:
                        eTExtendedDirection = ETExtendedDirection.FarLeft;
                        break;
                    case ETExtendedDirection.Right:
                        eTExtendedDirection = ETExtendedDirection.FarRight;
                        break;
                }
            }
            switch (eTExtendedDirection)
            {
                case ETExtendedDirection.Up:
                    return ori + new Vector3Int(0, 1, 0);
                case ETExtendedDirection.Down:
                    return ori + new Vector3Int(0, -1, 0);
                case ETExtendedDirection.Left:
                    return ori + new Vector3Int(-1, 0, 0);
                case ETExtendedDirection.Right:
                    return ori + new Vector3Int(1, 0, 0);
                case ETExtendedDirection.FarUp:
                    return ori + new Vector3Int(0, 2, 0);
                case ETExtendedDirection.FarDown:
                    return ori + new Vector3Int(0, -2, 0);
                case ETExtendedDirection.FarLeft:
                    return ori + new Vector3Int(-2, 0, 0);
                case ETExtendedDirection.FarRight:
                    return ori + new Vector3Int(2, 0, 0);
                default:
                    return ori;
            }
        }
    }
    public static class ET_Control
    {
        /// <summary>
        /// Get unity key code by string
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        public static KeyCode Getkeycode(string st)
        {
            string key = PlayerPrefs.GetString(st);
            KeyCode newkey = (KeyCode)System.Enum.Parse(typeof(KeyCode), key);
            return newkey;
        }
    }
    public static class ET_Color
    {
        public static Color GetRandomColor()
        {
            return new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
        }
        public static Color Set_a(this Color cl, float val)
        {
            Color _cl = cl;
            _cl.a = val;
            return _cl;
        }
        public static Color Set_r(this Color cl, float val)
        {
            Color _cl = cl;
            _cl.r = val;
            return _cl;
        }
        public static Color Set_b(this Color cl, float val)
        {
            Color _cl = cl;
            _cl.b = val;
            return _cl;
        }
        public static Color Set_g(this Color cl, float val)
        {
            Color _cl = cl;
            _cl.g = val;
            return _cl;
        }
        public static Color Get(string txHex)
        {
            Color color = Color.white;
            if (txHex[0] != '#')
            {
                txHex = "#" + txHex;
            }
            ColorUtility.TryParseHtmlString(txHex, out color);
            return color;
        }
        /// <summary>
        /// add all RBG value by value 0 ~ 1 
        /// </summary>
        /// <param name="xx"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static Color Add(this Color xx, float num)
        {
            Color cl = new Color();
            cl.r = Mathf.Clamp(xx.r - num, 0, 1);
            cl.b = Mathf.Clamp(xx.b - num, 0, 1);
            cl.g = Mathf.Clamp(xx.g - num, 0, 1);
            cl.a = 1;
            return cl;
        }
    }
    public static class ET_Cam
    {
        public static void LayerCullingShow(this Camera cam, int layerMask)
        {
            cam.cullingMask |= layerMask;
        }
        public static void LayerCullingShow(this Camera cam, string layer)
        {
            LayerCullingShow(cam, 1 << LayerMask.NameToLayer(layer));
        }
        public static void LayerCullingHide(this Camera cam, int layerMask)
        {
            cam.cullingMask &= ~layerMask;
        }
        public static void LayerCullingHide(this Camera cam, string layer)
        {
            LayerCullingHide(cam, 1 << LayerMask.NameToLayer(layer));
        }
        public static void LayerCullingToggle(this Camera cam, int layerMask)
        {
            cam.cullingMask ^= layerMask;
        }
        public static void LayerCullingToggle(this Camera cam, string layer)
        {
            LayerCullingToggle(cam, 1 << LayerMask.NameToLayer(layer));
        }
        public static bool LayerCullingIncludes(this Camera cam, int layerMask)
        {
            return (cam.cullingMask & layerMask) > 0;
        }
        public static bool LayerCullingIncludes(this Camera cam, string layer)
        {
            return LayerCullingIncludes(cam, 1 << LayerMask.NameToLayer(layer));
        }
        public static void LayerCullingToggle(this Camera cam, int layerMask, bool isOn)
        {
            bool included = LayerCullingIncludes(cam, layerMask);
            if (isOn && !included)
            {
                LayerCullingShow(cam, layerMask);
            }
            else if (!isOn && included)
            {
                LayerCullingHide(cam, layerMask);
            }
        }
        public static void LayerCullingToggle(this Camera cam, string layer, bool isOn)
        {
            LayerCullingToggle(cam, 1 << LayerMask.NameToLayer(layer), isOn);
        }
    }
    public static class ET_Enum
    {
        public static List<string> GetListValuesFromEnum<TEnum>() where TEnum : Enum
        {
            if (!typeof(TEnum).IsEnum)
            {
                Debug.LogError("TEnum must be an enum type.");
                return null;
            }
            List<string> enumOptions = new List<string>();
            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            {
                enumOptions.Add(value.ToString());
            }
            return enumOptions;
        }
    }
    public static class ET_SP
    {
        // public static int GetIdFromPos(Vector2 pos)
        // {
        //     int id = Mathf.RoundToInt(pos.y * L.ms + pos.x);
        //     return id;
        // }

        // public static Vector2 GetPosFromId(int id)
        // {
        //     int yy1 = id / L.ms;
        //     float yy = yy1;
        //     float xx = id - yy * L.ms;
        //     Vector2 pos = new Vector2(xx, yy);
        //     return pos;
        // }
        public static Vector2 GposToWpos(Vector2 Gpos)
        {
            Vector2 pos = new Vector2(Gpos.x * 64, Gpos.y * 64);
            return pos;
        }
        public static Vector2 WposToGpos(Vector2 Wpos)
        {

            int xx1 = Mathf.RoundToInt(Wpos.x);
            //int xx1 = Mathf.RoundToInt(Wpos.x + 32);
            int xx = xx1 / 64;
            int yy1 = Mathf.RoundToInt(Wpos.y);
            //int yy1 = Mathf.RoundToInt(Wpos.y + 32);
            int yy = yy1 / 64;
            Vector2 pos = new Vector2(xx, yy);
            return pos;
        }
        public static Vector2 WposToGposToRWpos(Vector2 Wpos)
        {

            int xx1 = Mathf.RoundToInt(Wpos.x);
            //int xx1 = Mathf.RoundToInt(Wpos.x + 32);
            int xx = xx1 / 64;
            int yy1 = Mathf.RoundToInt(Wpos.y);
            //int yy1 = Mathf.RoundToInt(Wpos.y + 32);
            int yy = yy1 / 64;
            Vector2 pos = new Vector2(xx * 64, yy * 64);
            return pos;
        }

        public static Vector2 GetVec2(Vector3 v3)
        {
            Vector2 v2 = new Vector2(v3.x, v3.y);
            return v2;
        }
        public static Vector3 GetVec3(Vector2 v2)
        {
            Vector3 v3 = new Vector3(v2.x, v2.y, 0);
            return v3;
        }
        public static Vector3 GetVec3PostoPos(Vector3 posfrom, Vector3 posto)
        {
            Vector3 v3 = posto - posfrom;
            return v3;
        }
        public static Vector2 WposCen(Vector2 v2)
        {
            Vector2 pos = new Vector2(v2.x + 32, v2.y + 32);
            return pos;
        }
        // public static Vector3Int V3rtov3i(Vector3Ro v3r)
        // {
        //     Vector3Int v3i = new Vector3Int(v3r.x, v3r.y, v3r.z);
        //     return v3i;
        // }
        // public static Vector3 V3rtov3(Vector3Ro v3r)
        // {
        //     Vector3 v3i = new Vector3(v3r.x, v3r.y, v3r.z);
        //     return v3i;
        // }
        // public static Vector3Ro V3itov3r(Vector3Int v3i)
        // {
        //     Vector3Ro v3r = new Vector3Ro();
        //     v3r.x = v3i.x;
        //     v3r.y = v3i.y;
        //     v3r.z = v3i.z;
        //     return v3r;
        // }
        // public static Vector3Ro V3tov3r(Vector3 v3i)
        // {
        //     Vector3Ro v3r = new Vector3Ro();
        //     v3r.x = Mathf.RoundToInt(v3i.x);
        //     v3r.y = Mathf.RoundToInt(v3i.y);
        //     v3r.z = Mathf.RoundToInt(v3i.z);
        //     return v3r;
        // }
        // public static Vector2 V2ftov2(Vector2Fo v2f)
        // {
        //     Vector2 v2 = new Vector2(v2f.x, v2f.y);
        //     return v2;
        // }
        // public static Vector2Fo V2tov2f(Vector2 v2)
        // {
        //     Vector2Fo v2f = new Vector2Fo();
        //     v2f.x = v2.x;
        //     v2f.y = v2.y;
        //     return v2f;
        // }
        //Returns 00-FF, value 0->255
        public static string Dec_to_Hex(int value)
        {
            return value.ToString("X2");
        }
        // Returns a hex string based on a number between 0->1
        public static string Dec01_to_Hex(float value)
        {
            return Dec_to_Hex((int)Mathf.Round(value * 255f));
        }

        // Get Hex Color FF00FF
        public static string GetStringFromColor(Color color)
        {
            string red = Dec01_to_Hex(color.r);
            string green = Dec01_to_Hex(color.g);
            string blue = Dec01_to_Hex(color.b);
            return red + green + blue;
        }

        // Get Hex Color FF00FFAA
        public static string GetStringFromColorWithAlpha(Color color)
        {
            string alpha = Dec01_to_Hex(color.a);
            return GetStringFromColor(color) + alpha;
        }

        // Sets out values to Hex String 'FF'
        public static void GetStringFromColor(Color color, out string red, out string green, out string blue, out string alpha)
        {
            red = Dec01_to_Hex(color.r);
            green = Dec01_to_Hex(color.g);
            blue = Dec01_to_Hex(color.b);
            alpha = Dec01_to_Hex(color.a);
        }

        // Get Hex Color FF00FF
        public static string GetStringFromColor(float r, float g, float b)
        {
            string red = Dec01_to_Hex(r);
            string green = Dec01_to_Hex(g);
            string blue = Dec01_to_Hex(b);
            return red + green + blue;
        }

        // Get Hex Color FF00FFAA
        public static string GetStringFromColor(float r, float g, float b, float a)
        {
            string alpha = Dec01_to_Hex(a);
            return GetStringFromColor(r, g, b) + alpha;
        }


        // Generate random normalized direction
        




        // Get UI Position from World Position
        public static Vector2 GetWorldUIPosition(Vector3 worldPosition, Transform parent, Camera uiCamera, Camera worldCamera)
        {
            Vector3 screenPosition = worldCamera.WorldToScreenPoint(worldPosition);
            Vector3 uiCameraWorldPosition = uiCamera.ScreenToWorldPoint(screenPosition);
            Vector3 localPos = parent.InverseTransformPoint(uiCameraWorldPosition);
            return new Vector2(localPos.x, localPos.y);
        }

        public static Vector3 GetWorldPositionFromUIZeroZ()
        {
            Vector3 vec = GetWorldPositionFromUI(Input.mousePosition, Camera.main);
            vec.z = 0f;
            return vec;
        }

        // Get World Position from UI Position
        public static Vector3 GetWorldPositionFromUI()
        {
            return GetWorldPositionFromUI(Input.mousePosition, Camera.main);
        }

        public static Vector3 GetWorldPositionFromUI(Camera worldCamera)
        {
            return GetWorldPositionFromUI(Input.mousePosition, worldCamera);
        }

        public static Vector3 GetWorldPositionFromUI(Vector3 screenPosition, Camera worldCamera)
        {
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }

        public static Vector3 GetWorldPositionFromUI_Perspective()
        {
            return GetWorldPositionFromUI_Perspective(Input.mousePosition, Camera.main);
        }

        public static Vector3 GetWorldPositionFromUI_Perspective(Camera worldCamera)
        {
            return GetWorldPositionFromUI_Perspective(Input.mousePosition, worldCamera);
        }

        public static Vector3 GetWorldPositionFromUI_Perspective(Vector3 screenPosition, Camera worldCamera)
        {
            Ray ray = worldCamera.ScreenPointToRay(screenPosition);
            Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, 0f));
            float distance;
            xy.Raycast(ray, out distance);
            return ray.GetPoint(distance);
        }

        // Get Mouse Position in World with Z = 0f
        public static Vector3 GetMouseWorldPosition()
        {
            Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
            vec.z = 0f;
            return vec;
        }
        public static Vector3 GetMouseWorldPositionWithZ()
        {
            return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        }
        public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera)
        {
            return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
        }
        public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
        {
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }

        // Create a Sprite in the World, no parent
        public static GameObject CreateWorldSprite(string name, Sprite sprite, Vector3 position, Vector3 localScale, int sortingOrder, Color color)
        {
            return CreateWorldSprite(null, name, sprite, position, localScale, sortingOrder, color);
        }

        // Create a Sprite in the World
        public static GameObject CreateWorldSprite(Transform parent, string name, Sprite sprite, Vector3 localPosition, Vector3 localScale, int sortingOrder, Color color)
        {
            GameObject gameObject = new GameObject(name, typeof(SpriteRenderer));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            transform.localScale = localScale;
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.color = color;
            return gameObject;
        }
        // math
    }
    public static class ET_Vector
    {
        public static Vector3 getVec3fromAngle(float curangle)
        {
            return Vector3.forward * curangle;
        }
        public static float getRandomAngle_deviationangle()
        {
            float ret = 0;
            while (((ret >= 0 && ret <= 10) || (ret >= 80 && ret <= 110) || (ret >= 170 && ret <= 190) || (ret >= 260 && ret <= 280) || (ret >= 350 && ret <= 360)))
                ret = UnityEngine.Random.Range(0, 360);
            return ret;
        }
        public static Vector2 GetRandomVector2(Vector2 minXY, Vector2 maxXY)
        {
            return GetRandomVector2(minXY.x, maxXY.x, minXY.y, maxXY.y);
        }
        public static Vector2 GetRandomVector2(float minX, float maxX, float minY, float maxY)
        {
            float newX = UnityEngine.Random.Range(minX, maxX);
            float newY = UnityEngine.Random.Range(minY, maxY);
            return new Vector2(newX, newY);
        }
        public static Vector3 GetRandomVector3(Vector3 minXYZ, Vector3 maxXYZ)
        {
            return GetRandomVector3(minXYZ.x, maxXYZ.x, minXYZ.y, maxXYZ.y, minXYZ.z, maxXYZ.z);
        }
        public static Vector3 GetRandomVector3(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            float newX = UnityEngine.Random.Range(minX, maxX);
            float newY = UnityEngine.Random.Range(minY, maxY);
            float newZ = UnityEngine.Random.Range(minZ, maxZ);
            return new Vector3(newX, newY, newZ);
        }

        public static Vector3 ToVector3(this List<float> ls)
        {
            Vector3 v3 = new Vector3();
            v3.x = ls[0];
            v3.y = ls[1];
            v3.z = ls[2];
            return v3;
        }
        public static Vector3 ToVector3(this List<int> ls)
        {
            Vector3 v3 = new Vector3();
            v3.x = ls[0];
            v3.y = ls[1];
            v3.z = ls[2];
            return v3;
        }
        public static Vector3 ToVector3(this Vector2 vector2, Vector3 sameZas)
        {
            return new Vector3(vector2.x, vector2.y, sameZas.z);
        }
        public static Vector3 ToVector3(this Vector3 vector3, Vector3 sameZas)
        {
            return new Vector3(vector3.x, vector3.y, sameZas.z);
        }
        public static Vector2 ToVector2(this List<float> ls)
        {
            Vector2 v2 = new Vector2();
            v2.x = ls[0];
            v2.y = ls[1];
            return v2;
        }
        public static List<float> ToList(this Vector3 v3)
        {
            List<float> ls = new List<float>();
            ls.Add(v3.x);
            ls.Add(v3.y);
            ls.Add(v3.z);
            return ls;
        }
        public static List<float> ToList(this Vector2 v2)
        {
            List<float> ls = new List<float>();
            ls.Add(v2.x);
            ls.Add(v2.y);
            return ls;
        }

        public static Vector3Int ToVector3Int(this Vector2Int input, int z = 0)
        {
            return new Vector3Int(input.x, input.y, z);
        }
        #region Combine Size
        /// <summary>
        /// Combine size act: return the smallest size that could cover in both size
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static Vector2 CombineSizeAdd(Vector2 vec1, Vector2 vec2)
        {
            return CombineSizeAdd((Vector3)vec1,(Vector3)vec2);
        }
        /// <summary>
        /// Combine size act: return the smallest size that could cover in both size
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static Vector3 CombineSizeAdd(Vector3 vec1,Vector3 vec2)
        {
            return new Vector3(
                Mathf.Max(vec1.x, vec2.x),
                Mathf.Max(vec1.y, vec2.y),
                Mathf.Max(vec1.z, vec2.z)
                );
        }
        /// <summary>
        /// Combine size act: return the largest size that could cover in both size
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static Vector2 CombineIntersect(Vector2 vec1, Vector2 vec2)
        {
            return CombineIntersect((Vector3)vec1, (Vector3)vec2);
        }
        /// <summary>
        /// Combine size act: return the largest size that could cover in both size
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static Vector3 CombineIntersect(Vector3 vec1, Vector3 vec2)
        {
            return new Vector3(
                Mathf.Min(vec1.x, vec2.x),
                Mathf.Min(vec1.y, vec2.y),
                Mathf.Min(vec1.z, vec2.z)
                );
        }
        #endregion
    }
    public static class ET_Vector2
    {
        public static Vector2 Set_x(this Vector2 vec, float val)
        {
            Vector2 vex = vec;
            vex.x = val;
            return vex;
        }
        public static Vector2 Set_y(this Vector2 vec, float val)
        {
            Vector2 vex = vec;
            vex.y = val;
            return vex;
        }
        public static Vector2 GetRandomDir()
        {
            return new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
        }
        public static Vector3 GetVectorFromAngle(this float angle)
        {
            // angle = 0 -> 360
            float angleRad = angle * (Mathf.PI / 180f);
            return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }

        public static float GetAngleFromVector(this Vector3 dir)
        {
            dir = dir.normalized;
            float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (n < 0) n += 360;
            return n;
        }
        public static int GetAngleFromVector180(Vector3 dir)
        {
            dir = dir.normalized;
            float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            int angle = Mathf.RoundToInt(n);

            return angle;
        }

        public static Vector3 ApplyRotationToVector(Vector3 vec, Vector3 vecRotation)
        {
            return ApplyRotationToVector(vec, GetAngleFromVector(vecRotation));
        }

        public static Vector3 ApplyRotationToVector(Vector3 vec, float angle)
        {
            return Quaternion.Euler(0, 0, angle) * vec;
        }
        public static float DistanceBetweenVector2(this Vector2 from, Vector2 to)
        {
            return (to - from).magnitude;
        }
        public static Vector2 NormalizeLight(this Vector2 inputVector2, float magnitude)
        {
            float num = magnitude;
            if (num > 1E-05f)
            {
                return inputVector2 /= num;
            }
            else
            {
                return Vector2.zero;
            }
        }
    }
    public static class ET_Vector3
    {
        public static Vector3 Set_x(this Vector3 vec, float val)
        {
            Vector3 vex = vec;
            vex.x = val;
            return vex;
        }
        public static Vector3 Set_y(this Vector3 vec, float val)
        {
            Vector3 vex = vec;
            vex.y = val;
            return vex;
        }
        public static Vector3 Set_z(this Vector3 vec, float val)
        {
            Vector3 vex = vec;
            vex.z = val;
            return vex;
        }
        public static float DistanceBetweenVector2(this Vector3 from, Vector3 to)
        {
            return ((Vector2)to - (Vector2)from).magnitude;
        }
        public static float DistanceBetweenVector3(this Vector3 from, Vector3 to)
        {
            return (to - from).magnitude;
        }
    }

        
    /*
    public static class ET_Tile
    {
        public static void Change_Tile(int id, byte lay, string tilename)
        {
            string prename;
            switch (lay)
            {
                case 0:
                    L.map[id].plat.tilename = tilename;
                    L.map[id].plat.bi = K.Bdata[tilename];
                    K.tPlat.SetTile(L.map[id].position, K.BdataM[tilename].tile);
                    break;
                case 1:
                    prename = L.map[id].block.tilename;
                    L.map[id].block.tilename = tilename;
                    L.map[id].block.bi = K.Bdata[tilename];
                    K.tBlock.SetTile(L.map[id].position, K.BdataM[tilename].tile);
                    GO_cremap_support.Autotile_build(id, prename);
                    //GO_cremap_support.Autotile_build_fix_cen(id, prename);

                    break;
                case 2:
                    byte ca = L.map[id].fur.bi.tilestate;
                    prename = L.map[id].fur.tilename;
                    L.map[id].fur.tilename = tilename;
                    L.map[id].fur.bi = K.Bdata[tilename];
                    L.map[id].fur.bi.tilestate = ca;
                    K.tFur.SetTile(L.map[id].position, K.BdataM[tilename].tile);
                    Delete_tile_fur(id, prename);
                    Change_tile_fur(id, tilename);
                    Set_tile_fur_special(id, tilename);
                    break;
            }

            if (L.map[id].plat.tilename == "Space" && L.map[id].block.bi.tilecon == "Plat")
            {
                Change_Tile(id, 1, "Space");
            }
            if (L.map[id].block.tilename == "Space" && L.map[id].fur.bi.tilecon == "Block")
            {
                Change_Tile(id, 2, "Space");
            }
            FixCollision(id);
            Collissionchangefix(id);
            Darknesschangefix(id, lay);
            Airchangefix(id);
            FixDamageBlock(id, lay);
        }

        // change_tile sp //
        public static void Delete_tile_fur(int id, string tilename)
        {
            List<int> b = K.Bdata[tilename].fure; //fure (eup,edown,oxiup,oxidown,storageadd,electriceadd
            if (K.flist.Contains(id)) K.flist.Remove(id);
            if (K.sunon == true) L.ship.res.energyup -= b[0];
            if (L.map[id].fur.bi.tilestate == 1)
            {
                L.ship.res.energydown -= b[1]; L.map[id].fur.bi.tilestate = 0;
            }
            L.ship.res.maxstorage -= b[2];
            L.ship.res.maxenergy -= b[3];
        }
        public static void Change_tile_fur(int id, string tilename)
        {
            bool enavai = L.ship.res.energy + (L.ship.res.energyup - L.ship.res.energydown) >= K.Bdata[tilename].fure[1] && K.Bdata[tilename].fure[1] > 0;
            List<int> b = K.Bdata[tilename].fure; //fure (eup,edown,oxiup,oxidown,storageadd,electriceadd
            if (K.sunon == true) L.ship.res.energyup += b[0];
            if (enavai && tilename != "Oxygen generator" && tilename != "Ele Furnace" && tilename != "LS lamp"
                 && tilename != "RES buffer")
            {
                L.ship.res.energydown += b[1]; L.map[id].fur.bi.tilestate = 1;
            }

            L.ship.res.maxstorage += b[2];
            L.ship.res.maxenergy += b[3];
        }
        public static void Set_tile_fur_special(int id, string tilename)
        {
            L.map[id].fur.furr.st = 0;
            L.map[id].fur.furr.cursec = 0;
            L.map[id].fur.furr.mass = 0;
            L.map[id].fur.furr.curproduct = "";
            L.map[id].fur.furr.itemslist = new List<Elenum>();
            L.map[id].fur.furr.targetlist = new List<int>();
            L.map[id].fur.furr.targetstring = "";

            switch (tilename)
            {
                case "Solar panel":
                    if (K.sunon == true) K.tFur.SetTile(L.map[id].position, SetTileFur.onfur[0]);
                    else K.tFur.SetTile(L.map[id].position, SetTileFur.offur[0]);
                    break;
                case "Lil solar panel":
                    if (K.sunon == true) K.tFur.SetTile(L.map[id].position, SetTileFur.onfur[1]);
                    else K.tFur.SetTile(L.map[id].position, SetTileFur.offur[1]);
                    break;
                case "Bactery":
                    K.numbac += 1;
                    break;
                case "Mini farm":
                    L.map[id].fur.furr.cursec = 1200;
                    L.map[id].fur.furr.farm1.curcare = 0;
                    L.map[id].fur.furr.farm1.maxcare = 12;
                    break;
                case "Auto mini farm":
                    L.map[id].fur.furr.cursec = 1200;
                    L.map[id].fur.furr.farm1.curcare = 0;
                    L.map[id].fur.furr.farm1.maxcare = 12;
                    break;
                case "Iron door":
                    K.tFur.SetTile(L.map[id].position, SetTileFur.setdoor[L.map[id].fur.bi.tilestate]);
                    break;
                case "Death Sigil":
                    L.map[id].fur.furr.cursec = 10;
                    break;
                case "NZA gun":
                    GameObject gun1 = Instantiate(SetSpecificStruct.gun[0], L.map[id].wpos, Quaternion.identity);
                    gun1.transform.SetParent(GameObject.Find("Game_obj_structure").transform, false);
                    gun1.GetComponent<CT_NZAgun>().ID = id;
                    break;
                case "Lazer gun":
                    GameObject gun2 = Instantiate(SetSpecificStruct.gun[1], L.map[id].wpos, Quaternion.identity);
                    gun2.transform.SetParent(GameObject.Find("Game_obj_structure").transform, false);
                    gun2.GetComponent<CT_lazergun>().ID = id;
                    break;
                case "Spawn point":
                    L.map[id].fur.furr.spawnpoint1.dicnum = -1;
                    break;
                    //case "RES buffer":
                    //    L.map[id].fur.bi.tilestate = 0;
                    //    break;

            }
            if (K.flist.Contains(id) == false) K.flist.Add(id);
        }
        public static void Collissionchangefix(int id)
        {
            if (K.Bdata[L.map[id].block.tilename].col == K.Bdata[L.map[id].plat.tilename].col == K.Bdata[L.map[id].fur.tilename].col == false)
                L.way[id].stepon = true;
            if (K.Bdata[L.map[id].block.tilename].col == K.Bdata[L.map[id].plat.tilename].col == K.Bdata[L.map[id].fur.tilename].col == false)
                K.monway[id].stepon = true;
            if (L.map[id].fur.tilename == "Iron door")
                K.monway[id].stepon = false;
        }
        public static void Darknesschangefix(int id, byte lay)
        {
            if ((L.map[id].block.tilename != "Space" || L.map[id].fur.tilename != "Space" || L.map[id].plat.tilename != "Space")
                    && (L.map[id].block.tilename != "Coal stone" && L.map[id].block.tilename != "Gold stone" && L.map[id].block.tilename != "Iron stone" && L.map[id].block.tilename != "Stone" && L.map[id].fur.tilename != "Death Sigil"))
            {
                if (!K.darkness[id].onhsfur.Contains(id)) K.darkness[id].onhsfur.Add(id);
                K.tDark.SetTileFlags(L.map[id].position, TileFlags.None);
                if (K.darkness[id].onsfur.Count == 0 && K.darkness[id].onsref.Count == 0)
                {
                    K.tDark.SetColor(L.map[id].position, Cl.cltrans[4]);
                    K.darkness[id].visible = true;
                }
            }
            else
            {
                if (K.darkness[id].onhsfur.Contains(id)) K.darkness[id].onhsfur.Remove(id);
                K.tDark.SetTileFlags(L.map[id].position, TileFlags.None);
                if (K.darkness[id].onsfur.Count == 0 && K.darkness[id].onsref.Count == 0 &&
                    K.darkness[id].onhsfur.Count == 0 && K.darkness[id].onhsref.Count == 0
                    )
                {
                    K.tDark.SetColor(L.map[id].position, Cl.cltrans[5]);
                    K.darkness[id].visible = false;
                }
            }
            // change lightning obj
            if (lay == 2)
            {
                //remove light
                GO_cremap_support.Removealllightoffur(id, 10);
                //set light
                bool light = false;
                if (L.map[id].fur.bi.sight[0] == 99 && L.map[id].fur.bi.sight[1] - K.sightpen * 2 >= 0)
                {
                    light = true;
                }
                if (L.map[id].fur.bi.sight[0] == 98)
                {
                    light = false;
                    int s3 = 0; if (L.map[id].fur.bi.sight[3] - K.sightpen >= 0) s3 = L.map[id].fur.bi.sight[3];
                    GO_cremap_support.Removealllightoffur(id, s3);
                }
                if (light == true)
                {
                    int s1 = 0; if (L.map[id].fur.bi.sight[1] - K.sightpen * 2 >= 0) s1 = L.map[id].fur.bi.sight[1] - K.sightpen * 2;
                    int s2 = 0; if (L.map[id].fur.bi.sight[2] - K.sightpen >= 0) s2 = L.map[id].fur.bi.sight[2] - K.sightpen;
                    int s3 = 0; if (L.map[id].fur.bi.sight[3] - K.sightpen >= 0) s3 = L.map[id].fur.bi.sight[3] - K.sightpen;
                    GO_cremap_support.Addlighttofur(id, light, s1, s2, s3);
                }
            }

        }
        public static void Airchangefix(int id)
        {
            if (L.map[id].block.tilename != "Space") L.map[id].air.max = K.BdataM[L.map[id].block.tilename].air.max;
            else if (L.map[id].fur.tilename != "Space") L.map[id].air.max = K.BdataM[L.map[id].fur.tilename].air.max;
            else if (L.map[id].plat.tilename != "Space") L.map[id].air.max = K.BdataM[L.map[id].plat.tilename].air.max;
            else L.map[id].air.max = K.BdataM["Space"].air.max;
            L.map[id].air.drain = 0;
            if (L.map[id].block.tilename != "Space") L.map[id].air.drain += K.BdataM[L.map[id].block.tilename].air.drain;
            if (L.map[id].plat.tilename != "Space") L.map[id].air.drain += K.BdataM[L.map[id].plat.tilename].air.drain;
            if (L.map[id].fur.tilename != "Space") L.map[id].air.drain += K.BdataM[L.map[id].fur.tilename].air.drain;
            if (L.map[id].block.tilename == "Space" && L.map[id].fur.tilename == "Space" && L.map[id].plat.tilename == "Space")
            {
                L.map[id].air.drain = K.BdataM["Space"].air.drain;
            }
            if (L.map[id].block.tilename == "Space" && L.map[id].fur.tilename == "Space" && L.map[id].plat.tilename == "Space") L.map[id].air.add = -900;
            else L.map[id].air.add = 0;
        }
        // change_tile sp // 

        public static void Delete_Har_Mark(int id)
        {
            L.map[id].block.plan.planname = "";
            K.hplist.Remove(id);
            K.tSystemhar.SetTile(L.map[id].position, null);
        }
        public static void Delete_Repair_Mark(int id)
        {
            L.map[id].plan.repair = false;
            K.rlist.Remove(id);
            K.tSystemrepair.SetTile(L.map[id].position, null);
        }
        public static void Harres(string prename, int id)
        {
            switch (prename)
            {
                case "Stone": Harresadd("stone", 4); break;
                case "Iron stone": Harresadd("iron ore", 4); break;
                case "Gold stone": Harresadd("gold ore", 4); break;
                case "Coal stone": Harresadd("coal ore", 4); break;
                case "Oxygen stone": Harresadd("oxygen crystal", 4); break;
                case "Crop":
                    Harresadd("carrot", Mathf.RoundToInt(L.map[id].fur.furr.mass));
                    switch (L.map[id].fur.tilename)
                    {
                        case "Mini farm":
                            L.map[id].fur.furr.cursec = 1200;
                            L.map[id].fur.furr.st = 0;
                            L.map[id].fur.furr.mass = 0;
                            K.tFur.SetTile(L.map[id].position, SetTileFur.farm1[0]);
                            break;
                        case "Auto mini farm":
                            L.map[id].fur.furr.cursec = 1200;
                            L.map[id].fur.furr.st = 0;
                            L.map[id].fur.furr.mass = 0;
                            K.tFur.SetTile(L.map[id].position, SetTileFur.farm2[0]);
                            break;
                    }
                    break;
            }
        }
        public static void Harresadd(string ele, int num)
        {
            int index = 1000;
            List<Elenum> it = L.ship.res.items;
            Elenum en = new Elenum() { ele = ele, num = num };
            index = Listhaveeleindex(ele);
            if (index == 1000)
            {
                it.Add(en);
            }
            else
            {
                int am = L.ship.res.items[index].num + num;
                en = L.ship.res.items[index];
                en.num = am;
                L.ship.res.items[index] = en;
            }
        }



        public static bool Addelecost(string tilename, int addosub)
        {
            bool adddone = false;
            List<Elenum> cost = K.Bdata[tilename].cost;
            int desire = 0;
            for (int i = 0; i < cost.Count; i++)
            {
                desire += Checkelenum(cost[i].ele, addosub * cost[i].num);
            }
            if (desire == 0)
            {
                for (int i = 0; i < cost.Count; i++)
                {
                    Addelenum(cost[i].ele, addosub * cost[i].num);
                }
                adddone = true;
            }
            else
            {
                adddone = false;
            }
            return adddone;
        }
        public static bool Addelecostitem(string elename, int addosub)
        {
            bool adddone = false;
            List<Elenum> cost = K.IdataM[elename].cost;
            int desire = 0;
            for (int i = 0; i < cost.Count; i++)
            {
                desire += Checkelenum(cost[i].ele, addosub * cost[i].num);
            }
            if (desire == 0)
            {
                for (int i = 0; i < cost.Count; i++)
                {
                    Addelenum(cost[i].ele, addosub * cost[i].num);
                }
                adddone = true;
            }
            else
            {
                adddone = false;
            }
            return adddone;
        }
        public static bool Replaceelecost1to2(string tilename1, string tilename2)
        {
            bool adddone = false;
            List<Elenum> costr = K.Bdata[tilename1].cost;
            List<Elenum> costa = K.Bdata[tilename1].cost;
            int desire = 0;
            for (int i = 0; i < costr.Count; i++)
            {
                desire += Checkelenum(costr[i].ele, costr[i].num);
            }
            if (desire != 0)
            {
            }
            else
            {
                for (int i = 0; i < costa.Count; i++)
                {
                    desire += Checkelenum(costr[i].ele, -costr[i].num);
                }
                if (desire != 0)
                {
                }
                else
                {
                    for (int i = 0; i < costr.Count; i++)
                    {
                        Addelenum(costr[i].ele, costr[i].num);
                    }
                    for (int i = 0; i < costa.Count; i++)
                    {
                        Addelenum(costa[i].ele, -costa[i].num);
                    }
                    adddone = true;
                }

            }
            return adddone;
        }

        public static int Checkelenum(string ele, int num)
        {
            int Adddone = 1;
            int index = 1000;
            List<Elenum> it = L.ship.res.items;
            Elenum en = new Elenum() { ele = ele, num = num };
            index = Listhaveeleindex(ele);
            if (index == 1000)
            {
                if (num > 0) Adddone = 0;
            }
            else
            {
                int am = L.ship.res.items[index].num + num;
                if (am < 0)
                {

                }
                else
                {
                    Adddone = 0;
                }

            }
            return Adddone;
        }
        public static bool Checkbuildhaveenoughelenum(string tilename)
        {
            List<Elenum> cost = K.Bdata[tilename].cost;
            for (int i = 0; i < cost.Count; i++)
            {
                int cin = Checkelenum(cost[i].ele, -cost[i].num);
                if (cin != 0) return false;

            }

            return true;
        }
        public static bool Checkitemhaveenoughelenum(string itemname)
        {
            List<Elenum> cost = K.IdataM[itemname].cost;
            for (int i = 0; i < cost.Count; i++)
            {
                int cin = Checkelenum(cost[i].ele, -cost[i].num);
                if (cin != 0) return false;

            }

            return true;
        }
        public static void Addelenum(string ele, int num)
        {

            int index = 1000;
            List<Elenum> it = L.ship.res.items;
            Elenum en = new Elenum() { ele = ele, num = num };
            index = Listhaveeleindex(ele);
            if (index == 1000)
            {
                if (num > 0) it.Add(en);
            }
            else
            {
                int am = L.ship.res.items[index].num + num;
                if (am < 0)
                {

                }
                else
                {
                    en = L.ship.res.items[index];
                    en.num = am;
                    L.ship.res.items[index] = en;
                    if (L.ship.res.items[index].num == 0) L.ship.res.items.Remove(en);
                }

            }
        }
        public static int Listhaveeleindex(string ele)
        {
            int index = 1000;
            List<Elenum> it = L.ship.res.items;
            for (int i = 0; i < it.Count; i++)
            {
                if (it[i].ele == ele)
                {
                    index = i;
                }
            }
            return index;
        }

        public static void Delete_Bui_Mark(int id, int lay)
        {

            switch (lay)
            {
                case 0:
                    L.map[id].plat.plan.planname = "";
                    K.tSystemp.SetTile(L.map[id].position, Tile_control.GetTileByNum("Sys", 0));
                    K.tSysp.SetTile(L.map[id].position, Tile_control.GetTileByNum("Sys", 0));
                    K.bplistp.Remove(id);
                    break;
                case 1:
                    L.map[id].block.plan.planname = "";
                    K.tSystemb.SetTile(L.map[id].position, Tile_control.GetTileByNum("Sys", 0));
                    K.tSysb.SetTile(L.map[id].position, Tile_control.GetTileByNum("Sys", 0));
                    K.bplistb.Remove(id);
                    break;
                case 2:
                    L.map[id].fur.plan.planname = "";
                    K.tSystemf.SetTile(L.map[id].position, Tile_control.GetTileByNum("Sys", 0));
                    K.tSysf.SetTile(L.map[id].position, Tile_control.GetTileByNum("Sys", 0));
                    K.bplistf.Remove(id);
                    break;
            }
        }

        public static void FixCollision(int i)
        {
            if ((K.Bdata[L.map[i].block.tilename].col == false) &&
                (K.Bdata[L.map[i].plat.tilename].col == false) &&
                (K.Bdata[L.map[i].fur.tilename].col == false))
            {
                if (L.map[i].fur.tilename == "Iron door")
                {
                    K.monway[i].stepon = false;
                    K.tMoncol.SetTile(L.map[i].position, SetTileDarkness.dark[1]);
                }
                else
                {
                    K.monway[i].stepon = true;
                    K.tMoncol.SetTile(L.map[i].position, null);
                }
                L.way[i].stepon = true;
                K.tRefcol.SetTile(L.map[i].position, null);
            }
            else
            {
                L.way[i].stepon = false;
                K.monway[i].stepon = false;
                K.tMoncol.SetTile(L.map[i].position, SetTileDarkness.dark[1]);
                K.tRefcol.SetTile(L.map[i].position, SetTileDarkness.dark[1]);
            }
        }
        public static void FixDamageBlockStart(int i)
        {
            float ral1 = -1;
            float ral2 = -1;
            float ral3 = -1;
            float hpplat = L.map[i].plat.bi.hp;
            float maxhpplat = K.Bdata[L.map[i].plat.tilename].hp;
            float hpblock = L.map[i].block.bi.hp;
            float maxhpblock = K.Bdata[L.map[i].block.tilename].hp;
            float hpfur = L.map[i].fur.bi.hp;
            float maxhpfur = K.Bdata[L.map[i].fur.tilename].hp;

            if (hpplat != 0 & maxhpplat != 0) ral1 = hpplat / maxhpplat;
            if (hpblock != 0 & maxhpblock != 0) ral2 = hpblock / maxhpblock;
            if (hpfur != 0 & maxhpfur != 0) ral3 = hpfur / maxhpfur;

            byte lay = 3;
            if (ral1 >= ral2 && ral1 >= ral3) lay = 0;
            else if (ral2 >= ral1 && ral2 >= ral3) lay = 1;
            else if (ral3 >= ral2 && ral3 >= ral1) lay = 2;
            switch (lay)
            {
                case 0:
                    if (hpplat == maxhpplat || hpplat == 0) K.tSysdmg.SetTile(L.map[i].position, null);
                    else if (hpplat < maxhpplat && hpplat >= maxhpplat * 75 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[0]);
                    else if (hpplat < maxhpplat * 75 / 100 && hpplat >= maxhpplat * 50 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[1]);
                    else if (hpplat < maxhpplat * 50 / 100 && hpplat >= maxhpplat * 25 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[2]);
                    else if (hpplat < maxhpplat * 25 / 100 && hpplat > 0) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[3]);
                    break;
                case 1:
                    if (hpblock == maxhpblock || hpblock == 0) K.tSysdmg.SetTile(L.map[i].position, null);
                    else if (hpblock < maxhpblock && hpblock >= maxhpblock * 75 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[0]);
                    else if (hpblock < maxhpblock * 75 / 100 && hpblock >= maxhpblock * 50 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[1]);
                    else if (hpblock < maxhpblock * 50 / 100 && hpblock >= maxhpblock * 25 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[2]);
                    else if (hpblock < maxhpblock * 25 / 100 && hpblock > 0) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[3]);
                    break;
                case 2:
                    if (hpfur == maxhpfur || hpfur == 0) K.tSysdmg.SetTile(L.map[i].position, null);
                    else if (hpfur < maxhpfur && hpfur >= maxhpfur * 75 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[0]);
                    else if (hpfur < maxhpfur * 75 / 100 && hpfur >= maxhpfur * 50 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[1]);
                    else if (hpfur < maxhpfur * 50 / 100 && hpfur >= maxhpfur * 25 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[2]);
                    else if (hpfur < maxhpfur * 25 / 100 && hpfur > 0) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[3]);
                    break;
            }
        }
        public static void FixDamageBlock(int i, byte lay)
        {
            switch (lay)
            {
                case 0:
                    int hpplat = L.map[i].plat.bi.hp;
                    int maxhpplat = K.Bdata[L.map[i].plat.tilename].hp;
                    if (hpplat == maxhpplat || hpplat == 0) K.tSysdmg.SetTile(L.map[i].position, null);
                    else if (hpplat < maxhpplat && hpplat >= maxhpplat * 75 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[0]);
                    else if (hpplat < maxhpplat * 75 / 100 && hpplat >= maxhpplat * 50 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[1]);
                    else if (hpplat < maxhpplat * 50 / 100 && hpplat >= maxhpplat * 25 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[2]);
                    else if (hpplat < maxhpplat * 25 / 100 && hpplat > 0) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[3]);
                    break;
                case 1:
                    int hpblock = L.map[i].block.bi.hp;
                    int maxhpblock = K.Bdata[L.map[i].block.tilename].hp;
                    if (hpblock == maxhpblock || hpblock == 0) K.tSysdmg.SetTile(L.map[i].position, null);
                    else if (hpblock < maxhpblock && hpblock >= maxhpblock * 75 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[0]);
                    else if (hpblock < maxhpblock * 75 / 100 && hpblock >= maxhpblock * 50 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[1]);
                    else if (hpblock < maxhpblock * 50 / 100 && hpblock >= maxhpblock * 25 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[2]);
                    else if (hpblock < maxhpblock * 25 / 100 && hpblock > 0) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[3]);
                    break;
                case 2:
                    int hpfur = L.map[i].fur.bi.hp;
                    int maxhpfur = K.Bdata[L.map[i].fur.tilename].hp;
                    if (hpfur == maxhpfur || hpfur == 0) K.tSysdmg.SetTile(L.map[i].position, null);
                    else if (hpfur < maxhpfur && hpfur >= maxhpfur * 75 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[0]);
                    else if (hpfur < maxhpfur * 75 / 100 && hpfur >= maxhpfur * 50 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[1]);
                    else if (hpfur < maxhpfur * 50 / 100 && hpfur >= maxhpfur * 25 / 100) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[2]);
                    else if (hpfur < maxhpfur * 25 / 100 && hpfur > 0) K.tSysdmg.SetTile(L.map[i].position, SetTileSystem.dmg[3]);
                    break;
            }

        }
    public static GameObject GetNearestTagToId(string tagg, int i)
        {
            GameObject[] objs;
            float dis = Mathf.Infinity;
            if (GameObject.FindGameObjectWithTag(tagg) != null)
            {
                int f = 0;
                objs = GameObject.FindGameObjectsWithTag(tagg);
                for (int j = 0; j < objs.Length; j++)
                {
                    float locdis = Vector2.Distance(objs[j].transform.position, L.map[i].wpos);
                    if (locdis < dis)
                    {
                        dis = locdis;
                        f = j;
                    }
                }
                return objs[f];
            }
            return null;
        }
    }
    */
    public static class ET_UI
    {
        public static void UI_showhide(GameObject go)
        {
            if (go.activeSelf == true)
            {
                go.SetActive(false);
            }
            else
            {
                go.SetActive(true);
            }
        }
        public static void UI_inactive_allchild(GameObject go)
        {
            foreach (Transform child in go.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
        public static int UI_visible_child_count(GameObject go)
        {
            int cou = 0;
            foreach (Transform child in go.transform)
            {
                if (child.gameObject.activeSelf == true) cou += 1;
            }
            return cou;
        }
        public static void UI_set_rec_sizex(GameObject go, float sizex)
        {
            Vector2 sizeff = go.GetComponent<RectTransform>().sizeDelta;
            sizeff.x = sizex;
            go.GetComponent<RectTransform>().sizeDelta = sizeff;
        }
        public static void UI_set_rec_sizey(GameObject go, float sizey)
        {
            Vector2 sizeff = go.GetComponent<RectTransform>().sizeDelta;
            sizeff.y = sizey;
            go.GetComponent<RectTransform>().sizeDelta = sizeff;
        }
        public static void UI_set_rec_Aposy(GameObject go, float sizey)
        {
            Vector2 sizeff = go.GetComponent<RectTransform>().anchoredPosition;
            sizeff.y = sizey;
            go.GetComponent<RectTransform>().anchoredPosition = sizeff;
        }
        public static void DrawUILine(Vector3 start, Vector3 end, Color color, Material matxx, GameObject par)
        {
            GameObject myLine = new GameObject();
            //myLine.transform.localScale = par.transform.localScale;
            myLine.transform.parent = par.transform;
            myLine.transform.position = par.transform.position;

            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.material = matxx;
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = 0.01f;
            lr.endWidth = 0.01f;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
        public static void UI_set_but_color(GameObject go, Color cl)
        {
            go.GetComponent<Image>().color = cl;
        }


    }
    public static class ET_Transform
    {
        public static void DestroyAllChild(this Transform tr, int from = 0)
        {
            if (tr && tr.childCount > 0)
            {
                if (from == 0)
                {
                    for (int i = 0; i < tr.childCount; i++)
                    {
                        GameObject.Destroy(tr.GetChild(i).gameObject);
                    }
                    tr.DetachChildren();
                }
                else
                {
                    if (tr.childCount > from)
                    {
                        List<Transform> detechchilds = new List<Transform>();
                        for (int i = 0; i < from; i++)
                        {
                            detechchilds.Add(tr.GetChild(0));
                            tr.GetChild(0).parent = null;
                        }
                        for (int i = 0; i < tr.childCount; i++)
                        {
                            GameObject.Destroy(tr.GetChild(i).gameObject);
                        }
                        tr.DetachChildren();
                        for (int i = 0; i < detechchilds.Count; i++)
                        {
                            detechchilds[i].parent = tr;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Use while loop to destroy, WANING do not use in runtime
        /// </summary>
        /// <param name="tr"></param>
        public static void DestroyAllChildWhileLoop(this Transform tr)
        {
            while (tr.childCount>0)
            {
                GameObject.Destroy(tr.GetChild(0));
            }
        }
        public static void DestroyAllChildImmediate(this Transform tr, int from = 0)
        {
            if (tr && tr.childCount > 0)
            {
                if (from == 0)
                {
                    for (int i = 0; i < tr.childCount; i++)
                    {
                        GameObject.DestroyImmediate(tr.GetChild(i).gameObject);
                    }
                }
            }
        }
        public static Transform get_child_name(Transform par, string namex)
        {
            foreach (Transform tr in par)
            {
                if (tr.name == namex) return tr;
            }
            return null;
        }
        public static void hide_childs(Transform par)
        {
            foreach (Transform tr in par)
            {
                tr.gameObject.SetActive(false);
            }
        }
        public static int active_child_count(Transform par)
        {
            int num = 0;
            foreach (Transform tr in par)
            {
                if (tr.gameObject.activeSelf == true)
                    num += 1;
            }
            return num;
        }

        public static Transform get_child_name_contain(Transform par, string namex)
        {
            foreach (Transform tr in par)
            {
                if (tr.name.Contains(namex)) return tr;
            }
            return null;
        }
        public static void change_name(Transform tr, string namex)
        {
            if (tr != null)
            {
                tr.name = namex;
            }
        }
        public static void SetX(this Transform transform, float x)
        {
            var pos = transform.position;
            pos.x = x;
            transform.position = pos;
        }

        public static void SetY(this Transform transform, float y)
        {
            var pos = transform.position;
            pos.y = y;
            transform.position = pos;
        }

        public static void SetZ(this Transform transform, float z)
        {
            var pos = transform.position;
            pos.z = z;
            transform.position = pos;
        }

        public static void SetEuler(this Transform transform, Vector3 eulerAngle)
        {
            Quaternion q = transform.rotation;
            q.eulerAngles = eulerAngle;
            transform.rotation = q;
        }
        public static void SetEulerX(this Transform transform, float x)
        {
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.x = x;
            transform.eulerAngles = eulerAngles;
        }
        public static void SetEulerY(this Transform transform, float y)
        {
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.y = y;
            transform.eulerAngles = eulerAngles;
        }
        public static void SetEulerZ(this Transform transform, float z)
        {
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.z = z;
            transform.eulerAngles = eulerAngles;
        }
        public static GameObject GetNearestGameObject_WithTag(this GameObject go, string tagx)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tagx);
            float dis = Mathf.Infinity;
            if (objs.Length > 0)
            {
                GameObject ret = objs[0];
                objs = GameObject.FindGameObjectsWithTag(tagx);
                for (int i = 0; i < objs.Length; i++)
                {
                    float locDis = Vector2.Distance(objs[i].transform.position, go.transform.position);
                    if (locDis < dis)
                    {
                        dis = locDis;
                        ret = objs[i];
                    }
                }
                return ret;
            }
            else
            {
                return null;
            }
        }
        public static GameObject GetNearestGameObject_InList(this GameObject go, List<GameObject> gameObjectsList)
        {
            if (gameObjectsList.Count > 0)
            {
                GameObject ret = gameObjectsList[0];
                float dis = Mathf.Infinity;
                for (int i = 0; i < gameObjectsList.Count; i++)
                {
                    float locDis = Vector2.Distance(gameObjectsList[i].transform.position, go.transform.position);
                    if (locDis < dis)
                    {
                        dis = locDis;
                        ret = gameObjectsList[i];
                    }
                }
                return ret;
            }
            else return null;

        }
        /// <summary>
        /// Get nearest T, return default1 if not found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="thisPos"></param>
        /// <param name="objList"></param>
        /// <returns></returns>
        public static T GetNearestObject_InList<T>(this Vector3 thisPos, List<T> objList)
        {

            if (objList.Count > 0)
            {
                return objList[GetNearestIndex_InList(thisPos, objList)];
            }
            return default(T);  
        }
        /// <summary>
        /// Get nearest index, return -1 if not found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="thisPos"></param>
        /// <param name="objList"></param>
        /// <returns></returns>
        public static int GetNearestIndex_InList<T>(this Vector3 thisPos, List<T> objList)
        {
            if (objList.Count>0)
            {
                List<Vector3> posList = new List<Vector3>();
                foreach (var item in objList)
                {
                    switch (item)
                    {
                        case Transform t:
                            posList.Add(t.position);
                            break;
                        case Vector3 v:
                            posList.Add(v);
                            break;
                        case Vector2 vtwo:
                            posList.Add(vtwo);
                            break;
                        case GameObject g:
                            posList.Add(g.transform.position);
                            break;
                        case EPositionItem epos:
                            posList.Add(epos.Position);
                            break;
                        default:
                            break;
                    }
                }
                int ret = 0;
                float dis = Mathf.Infinity;
                for (int i = 0; i < posList.Count; i++)
                {
                    float locDis = Vector3.Distance(posList[i], thisPos);
                    if (locDis < dis)
                    {
                        dis = locDis;
                        ret = i;
                    }
                }
                return ret;
            }
            else return -1;

        }
        public static Transform FindOrCreate(this Transform root, string childName)
        {
            Transform ret = root.Find(childName);
            if (ret == null)
            {
                GameObject go = new GameObject(childName);
                go.transform.parent = root;
                go.transform.localPosition = Vector3.zero;
                return go.transform;
            }
            else
            {
                return ret;
            }
        }
        public static Transform FindOrCreate(string childName)
        {
            GameObject goFind = GameObject.Find(childName);
            if (goFind == null)
            {
                GameObject go = new GameObject(childName);
                go.transform.localPosition = Vector3.zero;
                return go.transform;
            }
            else
            {
                return goFind.transform;
            }
        }

        private static Vector2 GetBoundPostionOn2DObjectWithoutCheck(this Transform ob, PositionPresents positionPresents, Vector2 offset = default)
        {
            Bounds bound = ob.GetComponent<Renderer>().bounds;
            Vector2 vector2 = new Vector2();
            switch (positionPresents)
            {
                case PositionPresents.TopLeft:
                    vector2 = new Vector2(bound.min.x, bound.max.y);
                    break;
                case PositionPresents.TopCenter:
                    vector2 = new Vector2(bound.center.x, bound.max.y);
                    break;
                case PositionPresents.TopRight:
                    vector2 = new Vector2(bound.max.x, bound.max.y);
                    break;
                case PositionPresents.MiddleLeft:
                    vector2 = new Vector2(bound.min.x, bound.center.y);
                    break;
                case PositionPresents.MiddleCenter:
                    vector2 = new Vector2(bound.center.x, bound.center.y);
                    break;
                case PositionPresents.MiddleRight:
                    vector2 = new Vector2(bound.max.x, bound.center.y);
                    break;
                case PositionPresents.BottomLeft:
                    vector2 = new Vector2(bound.min.x, bound.min.y);
                    break;
                case PositionPresents.BottomCenter:
                    vector2 = new Vector2(bound.center.x, bound.min.y);
                    break;
                case PositionPresents.BottomRight:
                    vector2 = new Vector2(bound.max.x, bound.min.y);
                    break;
                case PositionPresents.None:
                    vector2 = new Vector2();
                    break;
                default:
                    vector2 = new Vector2((bound.min.x + bound.max.x) / 2, (bound.min.y + bound.max.y) / 2);
                    break;
            }
            return vector2 + offset;
        }
        public static Vector2 GetBoundPostionOn2DObject(Transform ob, PositionPresents positionPresents, Vector2 offset = default)
        {
            if (!ob.GetComponent<Renderer>())
            {
                Debug.LogError("There are no Renderer component");
                return Vector2.zero;
            }
            return GetBoundPostionOn2DObjectWithoutCheck(ob, positionPresents, offset);
        }
        public static Vector2[] GetAllBoundPositionOn2DObject(this Transform ob)
        {
            if (!ob.GetComponent<Renderer>())
            {
                Debug.LogError("There are no Renderer component");
                return new Vector2[0];
            }
            return new Vector2[9]
                {
                GetBoundPostionOn2DObjectWithoutCheck(ob, PositionPresents.TopLeft),
                GetBoundPostionOn2DObjectWithoutCheck(ob, PositionPresents.TopCenter),
                GetBoundPostionOn2DObjectWithoutCheck(ob, PositionPresents.TopRight),
                GetBoundPostionOn2DObjectWithoutCheck(ob, PositionPresents.MiddleLeft),
                GetBoundPostionOn2DObjectWithoutCheck(ob, PositionPresents.MiddleCenter),
                GetBoundPostionOn2DObjectWithoutCheck(ob, PositionPresents.MiddleRight),
                GetBoundPostionOn2DObjectWithoutCheck(ob, PositionPresents.BottomLeft),
                GetBoundPostionOn2DObjectWithoutCheck(ob, PositionPresents.BottomCenter),
                GetBoundPostionOn2DObjectWithoutCheck(ob, PositionPresents.BottomRight),
                };

        }
        private static Vector2 GetPivotPostionOn2DObjectWithoutCheck(this Transform ob, PositionPresents positionPresents, Vector2 offset = default)
        {
            Bounds bound = ob.GetComponent<Renderer>().bounds;
            Vector2 vector2 = new Vector2();
            switch (positionPresents)
            {
                case PositionPresents.TopLeft:
                    vector2 = new Vector2(-bound.extents.x, bound.extents.y);
                    break;
                case PositionPresents.TopCenter:
                    vector2 = new Vector2(0, bound.extents.y);
                    break;
                case PositionPresents.TopRight:
                    vector2 = new Vector2(bound.extents.x, bound.extents.y);
                    break;
                case PositionPresents.MiddleLeft:
                    vector2 = new Vector2(-bound.extents.x, 0);
                    break;
                case PositionPresents.MiddleCenter:
                    vector2 = new Vector2(0, 0);
                    break;
                case PositionPresents.MiddleRight:
                    vector2 = new Vector2(bound.extents.x, 0);
                    break;
                case PositionPresents.BottomLeft:
                    vector2 = new Vector2(-bound.extents.x, -bound.extents.y);
                    break;
                case PositionPresents.BottomCenter:
                    vector2 = new Vector2(0, -bound.extents.y);
                    break;
                case PositionPresents.BottomRight:
                    vector2 = new Vector2(bound.extents.x, -bound.extents.y);
                    break;
                case PositionPresents.None:
                    vector2 = new Vector2(0,0);
                    break;
                default:
                    break;
            }
            return vector2 + offset;
        }
        public static Vector2 GetPivotPostionOn2DObject(Transform ob, PositionPresents positionPresents, Vector2 offset = default)
        {
            if (!ob.GetComponent<Renderer>())
            {
                Debug.LogError("There are no Renderer component");
                return Vector2.zero;
            }
            return GetPivotPostionOn2DObjectWithoutCheck(ob, positionPresents, offset);
        }
        public static Vector2[] GetAllPivotPositionOn2DObject(this Transform ob)
        {
            if (!ob.GetComponent<Renderer>())
            {
                Debug.LogError("There are no Renderer component");
                return new Vector2[0];
            }
            return new Vector2[9]
                {
                GetPivotPostionOn2DObjectWithoutCheck(ob, PositionPresents.TopLeft),
                GetPivotPostionOn2DObjectWithoutCheck(ob, PositionPresents.TopCenter),
                GetPivotPostionOn2DObjectWithoutCheck(ob, PositionPresents.TopRight),
                GetPivotPostionOn2DObjectWithoutCheck(ob, PositionPresents.MiddleLeft),
                GetPivotPostionOn2DObjectWithoutCheck(ob, PositionPresents.MiddleCenter),
                GetPivotPostionOn2DObjectWithoutCheck(ob, PositionPresents.MiddleRight),
                GetPivotPostionOn2DObjectWithoutCheck(ob, PositionPresents.BottomLeft),
                GetPivotPostionOn2DObjectWithoutCheck(ob, PositionPresents.BottomCenter),
                GetPivotPostionOn2DObjectWithoutCheck(ob, PositionPresents.BottomRight),
                };

        }
    }
    public static class ET_RecTransform
    {
        public static void SetAnchor(this RectTransform source, AnchorPresets allign)
        {
            switch (allign)
            {
                case (AnchorPresets.TopLeft):
                    {
                        source.anchorMin = new Vector2(0, 1);
                        source.anchorMax = new Vector2(0, 1);
                        break;
                    }
                case (AnchorPresets.TopCenter):
                    {
                        source.anchorMin = new Vector2(0.5f, 1);
                        source.anchorMax = new Vector2(0.5f, 1);
                        break;
                    }
                case (AnchorPresets.TopRight):
                    {
                        source.anchorMin = new Vector2(1, 1);
                        source.anchorMax = new Vector2(1, 1);
                        break;
                    }

                case (AnchorPresets.MiddleLeft):
                    {
                        source.anchorMin = new Vector2(0, 0.5f);
                        source.anchorMax = new Vector2(0, 0.5f);
                        break;
                    }
                case (AnchorPresets.MiddleCenter):
                    {
                        source.anchorMin = new Vector2(0.5f, 0.5f);
                        source.anchorMax = new Vector2(0.5f, 0.5f);
                        break;
                    }
                case (AnchorPresets.MiddleRight):
                    {
                        source.anchorMin = new Vector2(1, 0.5f);
                        source.anchorMax = new Vector2(1, 0.5f);
                        break;
                    }

                case (AnchorPresets.BottomLeft):
                    {
                        source.anchorMin = new Vector2(0, 0);
                        source.anchorMax = new Vector2(0, 0);
                        break;
                    }
                case (AnchorPresets.BottonCenter):
                    {
                        source.anchorMin = new Vector2(0.5f, 0);
                        source.anchorMax = new Vector2(0.5f, 0);
                        break;
                    }
                case (AnchorPresets.BottomRight):
                    {
                        source.anchorMin = new Vector2(1, 0);
                        source.anchorMax = new Vector2(1, 0);
                        break;
                    }

                case (AnchorPresets.HorStretchTop):
                    {
                        source.anchorMin = new Vector2(0, 1);
                        source.anchorMax = new Vector2(1, 1);
                        break;
                    }
                case (AnchorPresets.HorStretchMiddle):
                    {
                        source.anchorMin = new Vector2(0, 0.5f);
                        source.anchorMax = new Vector2(1, 0.5f);
                        break;
                    }
                case (AnchorPresets.HorStretchBottom):
                    {
                        source.anchorMin = new Vector2(0, 0);
                        source.anchorMax = new Vector2(1, 0);
                        break;
                    }

                case (AnchorPresets.VertStretchLeft):
                    {
                        source.anchorMin = new Vector2(0, 0);
                        source.anchorMax = new Vector2(0, 1);
                        break;
                    }
                case (AnchorPresets.VertStretchCenter):
                    {
                        source.anchorMin = new Vector2(0.5f, 0);
                        source.anchorMax = new Vector2(0.5f, 1);
                        break;
                    }
                case (AnchorPresets.VertStretchRight):
                    {
                        source.anchorMin = new Vector2(1, 0);
                        source.anchorMax = new Vector2(1, 1);
                        break;
                    }

                case (AnchorPresets.StretchAll):
                    {
                        source.anchorMin = new Vector2(0, 0);
                        source.anchorMax = new Vector2(1, 1);
                        break;
                    }
            }
        }

        public static void SetPivot(this RectTransform source, PivotPresets preset)
        {

            switch (preset)
            {
                case (PivotPresets.TopLeft):
                    {
                        source.pivot = new Vector2(0, 1);
                        break;
                    }
                case (PivotPresets.TopCenter):
                    {
                        source.pivot = new Vector2(0.5f, 1);
                        break;
                    }
                case (PivotPresets.TopRight):
                    {
                        source.pivot = new Vector2(1, 1);
                        break;
                    }

                case (PivotPresets.MiddleLeft):
                    {
                        source.pivot = new Vector2(0, 0.5f);
                        break;
                    }
                case (PivotPresets.MiddleCenter):
                    {
                        source.pivot = new Vector2(0.5f, 0.5f);
                        break;
                    }
                case (PivotPresets.MiddleRight):
                    {
                        source.pivot = new Vector2(1, 0.5f);
                        break;
                    }

                case (PivotPresets.BottomLeft):
                    {
                        source.pivot = new Vector2(0, 0);
                        break;
                    }
                case (PivotPresets.BottomCenter):
                    {
                        source.pivot = new Vector2(0.5f, 0);
                        break;
                    }
                case (PivotPresets.BottomRight):
                    {
                        source.pivot = new Vector2(1, 0);
                        break;
                    }
            }
        }

        /// <summary>
        /// Get canvas that currentl hold the rectransform, work with multi level children in canvas
        /// </summary>
        /// <param name="rt"></param>
        /// <returns></returns>
        public static Canvas GetCanvas(this RectTransform rt)
        {
            Canvas ss = rt.gameObject.GetComponentInParent<Canvas>();
            return rt.gameObject.GetComponentInParent<Canvas>();
        }
        /// <summary>
        /// GetRectTransformWidth in canvas space
        /// </summary>
        /// <param name="rt"></param>
        /// <returns></returns>
        public static float GetRectTransformWidth(this RectTransform rt)
        {
            var w = (rt.anchorMax.x - rt.anchorMin.x) * Screen.width + rt.sizeDelta.x * rt.GetCanvas().scaleFactor;
            return w;
        }

        /// <summary>
        /// GetRectTransformHeight in canvas space
        /// </summary>
        /// <param name="rt"></param>
        /// <returns></returns>
        public static float GetRectTransformHeight(this RectTransform rt)
        {
            var h = (rt.anchorMax.y - rt.anchorMin.y) * Screen.height + rt.sizeDelta.y * rt.GetCanvas().scaleFactor;
            return h;
        }
        /// <summary>
        /// Clamp position of a rectrans from across to other RectTransform bound. Pu this after moveverment so it clamp the position correctly 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Vector3 ClampPos(Vector3 pos, RectTransform rect)
        {
            float w = rect.GetRectTransformWidth();
            float h = rect.GetRectTransformHeight();
            Vector2 min = new Vector2(-w / 2, -h / 2);
            Vector2 max = new Vector2(w / 2, h / 2);
            Vector2 minCorner = (Vector2)rect.transform.position + min;
            Vector2 maxCorner = (Vector2)rect.transform.position + max;
            pos.x = Mathf.Clamp(pos.x, minCorner.x, maxCorner.x);
            pos.y = Mathf.Clamp(pos.y, minCorner.y, maxCorner.y);
            return pos;

        }
    }
    public static class ET_D
    {
        public static void DrawLine(Vector2 start, Vector2 end, string namex = "Debug line")
        {

            GameObject myLine = new GameObject();
            //myLine.transform.localScale = par.transform.localScale;
            myLine.name = namex;
            //myLine.transform.position = D.w_debug_box.transform.position;
            //myLine.transform.parent = D.w_debug_box.transform;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.useWorldSpace = false;
            //lr.material = D.mats[0];
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
        public static void DrawLine(D_line line, string namex = "Debug line")
        {

            GameObject myLine = new GameObject();
            //myLine.transform.localScale = par.transform.localScale;
            myLine.name = namex;
            //myLine.transform.position = D.w_debug_box.transform.position;
            //myLine.transform.parent = D.w_debug_box.transform;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.useWorldSpace = false;
            //lr.material = D.mats[0];
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.SetPosition(0, line.start);
            lr.SetPosition(1, line.end);
        }
        public static void DrawMultiLine(List<D_line> lines)
        {
            foreach (D_line line in lines)
            {
                DrawLine(line);
            }
        }
        public static void DrawDot(Vector2 loc, byte type = 0)
        {
            GameObject go = new GameObject();
            go.transform.position = loc;
            //go.transform.parent = D.w_debug_box;
            SpriteRenderer sprr = go.AddComponent<SpriteRenderer>();
            //sprr.sprite = D.spr[0];
            switch (type)
            {
                case 0:
                    sprr.color = Color.white;
                    break;
                case 1:
                    sprr.color = Color.red;
                    break;
                case 2:
                    sprr.color = Color.blue;
                    break;
                case 3:
                    sprr.color = Color.green;
                    break;
            }



        }
        public static void DrawMultiDot(List<Vector2> locs, byte type = 0)
        {
            foreach (Vector2 loc in locs)
            {
                DrawDot(loc, type);
            }
        }
        public static void debug_string_list(List<string> lsting)
        {
            string ret = "String list: ";
            foreach (string loxs in lsting)
            {
                ret += loxs + "/";
            }
            Debug.Log(ret);
        }

        public static void t1()
        {
            Debug.Log("ET_D: Pass 1");
        }
        public static void t2()
        {
            Debug.Log("ET_D: Pass 2");
        }
        public static void t3()
        {
            Debug.Log("ET_D: Pass 3");
        }
        public static void t4()
        {
            Debug.Log("ET_D: Pass 4");
        }
        public static void t5()
        {
            Debug.Log("ET_D: Pass 5");
        }
        public static void t6()
        {
            Debug.Log("ET_D: Pass 6");
        }
        public static void t7()
        {
            Debug.Log("ET_D: Pass 7");
        }
        public static void t8()
        {
            Debug.Log("ET_D: Pass 8");
        }
        public static void t9()
        {
            Debug.Log("ET_D: Pass 9");
        }
    }
    public static class ET_Component
    {
        /// <summary>
        /// Try add component to game opbject if componetn already exsit will do nothing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <param name="type"></param>
        public static T TryAddComponent<T>(this GameObject gameObject) where T : Component
        {

            if (gameObject.GetComponent<T>())
            {
                return (T)gameObject.GetComponent<T>();
            }
            else
            {
                return gameObject.AddComponent<T>();
            }
        }
        /// <summary>
        /// Force add component to game object if component already exsit will replace with new one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <param name="type"></param>
        public static T ForceAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.GetComponent<T>())
            {
                GameObject.Destroy(gameObject.GetComponent<T>());
            }
            return gameObject.AddComponent<T>();
        }
    }
    /*
    public static class ET_SF_Draw
    {
        public static void DrawPathColliderLine(Vector2 start, Vector2 end, byte type = 1, string namex = "line_path_collider")
        {
            GameObject myLine = new GameObject();
            if (start.y == end.y) myLine.layer = 14;
            else myLine.layer = 12;
            myLine.name = namex;
            myLine.transform.position = W.w_path_box.transform.Find("Pathcolliders").transform.position;
            //parrent
            Transform parrent_t = W.w_path_box.transform.Find("Pathcolliders");
            switch (type)
            {
                case 0:
                    parrent_t = W.w_path_box.transform.Find("Pathcolliders_base");
                    break;
                case 1:
                    parrent_t = W.w_path_box.transform.Find("Pathcolliders");
                    break;

            }
            myLine.transform.parent = parrent_t;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.material = K.list_mat[3];
            lr.SetWidth(0.01f, 0.01f);
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            //add collider
            BoxCollider2D col = new GameObject("Collider").AddComponent<BoxCollider2D>();
            if (start.y == end.y) col.gameObject.layer = 14;
            else col.gameObject.layer = 12;
            col.transform.parent = myLine.transform;

            float lineLength = Vector3.Distance(start, end);
            col.size = new Vector3(lineLength, 0.01f);
            Vector3 midPoint = (start + end) / 2;
            col.transform.position = midPoint;
            float angle = (Mathf.Abs(start.y - end.y) / Mathf.Abs(start.x - end.x));
            if ((start.y < end.y && start.x > end.x) || (end.y < start.y && end.x > start.x))
            {
                angle *= -1;
            }
            angle = Mathf.Rad2Deg * Mathf.Atan(angle);
            col.transform.Rotate(0, 0, angle);
        }
        public static void DrawPathColliderDiaLine(Vector2 start, Vector2 end)
        {
            float colpolhe = 0.25f;
            float radipole = 0.25f;
            float diapole = 0.5f;
            float niih = 0.6f + diapole;
            float dis_layerrange = 1f;

            if (start.y == end.y)
            {
                // 6 point
                // 
                // platform efect bellow layer

                Vector2 pe1 = new Vector2(start.x, start.y + radipole);
                Vector2 pe2 = new Vector2(end.x, end.y + radipole);
                Vector2 pe3 = new Vector2(start.x, pe1.y + colpolhe);
                Vector2 pe4 = new Vector2(end.x, pe2.y + colpolhe);
                Vector2 pe5 = new Vector2(start.x, pe1.y - dis_layerrange);
                Vector2 pe6 = new Vector2(end.x, pe2.y - dis_layerrange);
                Vector2 pe7 = new Vector2(start.x, pe5.y + colpolhe);
                Vector2 pe8 = new Vector2(end.x, pe6.y + colpolhe);
                DrawPathColliderLine(pe1, pe2);
                DrawPathColliderLine(pe1, pe3);
                DrawPathColliderLine(pe2, pe4);
                DrawPathColliderLine(pe5, pe7);
                DrawPathColliderLine(pe6, pe8);
            }
            //
            else
            {


                Vector2 startup = new Vector2();
                Vector2 enddown = new Vector2();
                float niiiheight = 0.9f;
                float longfix, shortfix;
                if (start.y > end.y) { startup = start; enddown = end; }
                if (start.y < end.y) { startup = end; enddown = start; }
                Vector2 vecendstart = startup - enddown;
                float angleline = (startup.x > enddown.x) ? Vector2.Angle(vecendstart, Vector2.right) : Vector2.Angle(vecendstart, Vector2.left);
                float tan_O = Mathf.Tan(Mathf.Deg2Rad * angleline);
                float cos_O = Mathf.Cos(Mathf.Deg2Rad * angleline);
                float tan_hO = Mathf.Tan(Mathf.Deg2Rad * angleline / 2);
                float lx, rx, uy, dy;
                //
                int dis_totallayer = Mathf.RoundToInt(Mathf.Abs(start.y - end.y));
                List<Vector2> downveclist = new List<Vector2>();
                longfix = (radipole / tan_hO) + (niiiheight / tan_O);
                shortfix = ((radipole / cos_O) - radipole) / tan_O;
                //
                for (int i = 0; i < dis_totallayer + 1; i++)
                {
                    Vector2 newdownvec = new Vector2();
                    newdownvec.y = startup.y - (i + 1);
                    newdownvec.x = startup.x + (startup.x - enddown.x) / (startup.y - enddown.y) * (i + 1) * (-1);
                    downveclist.Add(newdownvec);
                }
                //ET_D.draw_dot_multi(downveclist, 3);
                //
                foreach (Vector2 downvec in downveclist)
                {
                    //ifstart left
                    if (startup.x <= enddown.x)
                    {
                        dy = downvec.y;
                        lx = downvec.x - longfix;
                        rx = downvec.x + shortfix;
                        lx = (lx < startup.x - radipole) ? startup.x - radipole : lx;
                    }
                    //ifstart right
                    else
                    {
                        dy = downvec.y;
                        lx = downvec.x - shortfix;
                        rx = downvec.x + longfix;
                        rx = (rx > startup.x + radipole) ? startup.x + radipole : rx;
                    }
                    DrawPathColliderLine(new Vector2(lx, dy + 0.5f), new Vector2(lx, dy + 0.25f));
                    DrawPathColliderLine(new Vector2(rx, dy + 0.5f), new Vector2(rx, dy + 0.25f));
                }

            }
        }
    }
    */
    public static class ET_PlayerPref
    {
        /// <summary>
        /// Try set int key, if success return true
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TrySetInt(string key, int value)
        {
            bool d = PlayerPrefs.HasKey(key);
            if (!d)
            {
                PlayerPrefs.SetInt(key, value);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Try set string key, if success return true
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TrySetString(string key, string value)
        {
            bool d = PlayerPrefs.HasKey(key);
            if (!d)
            {
                PlayerPrefs.SetString(key, value);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Try set float key, if success return true
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TrySetFloat(string key, float value)
        {
            bool d = PlayerPrefs.HasKey(key);
            if (!d)
            {
                PlayerPrefs.SetFloat(key, value);
                return true;
            }
            return false;
        }
        public static int Getintkey(string key)
        {
            bool d = PlayerPrefs.HasKey(key);
            if (d) return PlayerPrefs.GetInt(key);
            if (!d) PlayerPrefs.SetInt(key, 0);
            return 0;
        }
        public static string CreatePlayerPrefKey(KeyCode keyCode)
        {
            string key = keyCode.ToString();
            Debug.Log(key);
            return key;
        }
    }
    public static class ET_GO
    {
        public static GameObject[] combine_GO_tags(List<string> tagxs)
        {
            GameObject[] retlist = GameObject.FindGameObjectsWithTag(tagxs[0]);
            if (tagxs.Count>0)
            {
                for (int i = 1; i < tagxs.Count;i++)
                {
                    retlist = retlist.Concat(GameObject.FindGameObjectsWithTag(tagxs[i])).ToArray();
                }
            }
            return retlist;
        }
        public static List<GameObject> getGO_byname(string namex, Transform tr)
        {
            List<GameObject> ret = new List<GameObject>();
            for (int i = 0; i < tr.childCount; i++)
            {
                if (tr.GetChild(i).name == namex)
                {
                    ret.Add(tr.GetChild(i).gameObject);
                }
            }
            return ret;
        }
        public static GameObject getGOfromTransform_inrange_random(GameObject go, Transform tr, float range)
        {
            GameObject idret = null;
            float dis = range;
            List<GameObject> oblist = new List<GameObject>();
            for (int i = 0; i < tr.childCount; i++)
            {
                float disx = Vector2.Distance(go.transform.position, tr.GetChild(i).transform.position);
                if (disx <= dis)
                {
                    oblist.Add(tr.GetChild(i).gameObject);
                }
            }
            if (oblist.Count > 0) idret = ET_List.GetRandom(oblist);
            return idret;
        }
        public static GameObject getGOfromTransform_inrange_random(GameObject go, string tagx, float range)
        {
            GameObject idret = null;
            float dis = range;
            List<GameObject> oblist = new List<GameObject>();
            GameObject[] gotaged = GameObject.FindGameObjectsWithTag(tagx);
            for (int i = 0; i < gotaged.Length; i++)
            {
                float disx = Vector2.Distance(go.transform.position, gotaged[i].transform.position);
                if (disx <= dis)
                {
                    oblist.Add(gotaged[i]);
                }
            }
            if (oblist.Count > 0) idret = ET_List.GetRandom(oblist);
            return idret;
        }
        public static GameObject getGOfromTransform_inrange_random(GameObject go, List<string> tagxs, float range)
        {
            GameObject idret = null;
            float dis = range;
            List<GameObject> oblist = new List<GameObject>();
            GameObject[] gotaged = combine_GO_tags(tagxs);
            for (int i = 0; i < gotaged.Length; i++)
            {
                float disx = Vector2.Distance(go.transform.position, gotaged[i].transform.position);
                if (disx <= dis)
                {
                    oblist.Add(gotaged[i]);
                }
            }
            if (oblist.Count > 0) idret = ET_List.GetRandom(oblist);
            return idret;
        }
        public static List<GameObject> getGOlistfromTransform_inrange(GameObject go, List<string> tagxs, float range)
        {
            float dis = range;
            List<GameObject> oblist = new List<GameObject>();
            GameObject[] gotaged = combine_GO_tags(tagxs);
            for (int i = 0; i < gotaged.Length; i++)
            {
                float disx = Vector2.Distance(go.transform.position, gotaged[i].transform.position);
                if (disx <= dis)
                {
                    oblist.Add(gotaged[i]);
                }
            }
            return ET_List.arraygo_to_listgo(gotaged);
        }
        public static GameObject getGOfromTransform_tag(string tagx)
        {
            GameObject[] gotaged = GameObject.FindGameObjectsWithTag(tagx);
            if (gotaged.Length > 0) return gotaged[UnityEngine.Random.Range(0, gotaged.Length)];
            return null;
        }
        public static GameObject getGOfromTransform_tags(List<string> tagxs)
        {
            GameObject[] gotaged = combine_GO_tags(tagxs);
            if (gotaged.Length > 0) return gotaged[UnityEngine.Random.Range(0, gotaged.Length)];
            return null;
        }
        public static GameObject getGOfromTransform_list(List<Transform> go_list)
        {
            if (go_list!=null && go_list.Count > 0) return go_list[UnityEngine.Random.Range(0, go_list.Count)].gameObject;
            return null;
        }

        
        public static void HideAllChild(this Transform tr, int from = 0)
        {
            if (tr.childCount > 0)
            {
                if (from == 0)
                {
                    for (int i = 0; i < tr.childCount; i++)
                    {
                     tr.GetChild(i).gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (tr.childCount > from)
                    {
                        for (int i = 1; i < tr.childCount; i++)
                        {
                            tr.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
        public static Vector2[] GetAllPos2D(this GameObject[] Os)
        {
            Vector2[] ret = new Vector2[Os.Length];
            for (int i = 0; i < Os.Length; i++)
            {
                ret[i] = Os[i].transform.position;
            }
            return ret;
        }
        public static Vector3[] GetAllPos3D(this GameObject[] Os)
        {
            Vector3[] ret = new Vector3[Os.Length];
            for (int i = 0; i < Os.Length; i++)
            {
                ret[i] = Os[i].transform.position;
            }
            return ret;
        }


        /// <summary>
        /// Performance: S
        /// Find all object that have component in active screen.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> FindObjectsByComponent<T>(bool includeInactive = false)
        {
            List<T> objects = new List<T>();
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var rootGameObject in rootGameObjects)
            {
                T[] childrenInterfaces = rootGameObject.GetComponentsInChildren<T>(includeInactive);
                foreach (var childInterface in childrenInterfaces)
                {
                    objects.Add(childInterface);
                }
            }
            return objects;
        }
    }
    public static class ET_String
    {
        /// <summary>
        /// Slice string from char firstIncluded too char lastIncluded. If firstIncluded and lastIncluded not included the function will nor excute.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static string Slice(this string self, char firstIncluded, char lastIncluded)
        {
            if (!self.Contains(firstIncluded) || !self.Contains(lastIncluded)) return self;
            return firstIncluded + self.Split(firstIncluded).Last().Split(lastIncluded).First() + lastIncluded;
        }
        public static string Add_UnderscoreCase(this string self)
        {
            if (string.IsNullOrEmpty(self))
            {
                return self;
            }

            string result = char.ToLowerInvariant(self[0]).ToString();
            for (int i = 1; i < self.Length; i++)
            {
                if (char.IsUpper(self[i]))
                {
                    result += "_" + char.ToLowerInvariant(self[i]);
                }
                else
                {
                    result += self[i];
                }
            }

            return result;
        }
        public static string FormatWith(this string self, params object[] args)
        {
            try
            {
                return String.Format(self, args);
            }
            catch (Exception e)
            {
                return e.GetType().Name + " in [" + self + "]:" + e.Message;
            }
        }
        public static string Join<T>(this IEnumerable<T> self, string separator = ", ", bool omitNullsAndEmptyStrings = true)
        {
            var nameItems = self.ToStrings(omitNullsAndEmptyStrings);
            return String.Join(separator, nameItems.ToArray());
        }
        public static string Replace_LowerAndUnderscoreSpace(this string self)
        {
            return self.ToLowerInvariant().Replace(" ", "_");
        }
        public static string Set_UpperFirst(this string self)
        {
            if (self == null || self.Length == 0)
            {
                return self;
            }
            return Char.ToUpper(self[0]) + self.Substring(1);
        }
        public static char GetRandom_Char(string lisx)
        {
            int ran = UnityEngine.Random.Range(0, lisx.Length);
            return lisx[ran];
        }
        public static char GetRandom_Char()
        {
            string st = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            char c = st[UnityEngine.Random.Range(0, st.Length)];
            return c;
        }
        public static IEnumerable<string> ToStrings<T>(this IEnumerable<T> self, bool omitNullsAndEmptyStrings = true)
        {
            var nameItems = self.Select(i => i == null ? null : i.ToString());
            if (omitNullsAndEmptyStrings)
            {
                nameItems = nameItems.WhereNot(String.IsNullOrEmpty);
            }
            return nameItems;
        }
        public static string ToLexicalList<T>(this IEnumerable<T> self, bool useOxfordComma = true)
        {
            string msg;
            var nameItems = self.Select(i => i == null ? "null" : i.ToString()).ToArray();
            switch (nameItems.Length)
            {
                case 0:
                    msg = null;
                    break;

                case 1:
                    msg = nameItems[0];
                    break;

                case 2:
                    msg = nameItems[0] + " and " + nameItems[1];
                    break;

                default:
                    msg = nameItems[0];
                    for (int i = 1; i < nameItems.Length - 1; ++i)
                        msg = msg + ", " + nameItems[i];
                    if (useOxfordComma)
                        msg = msg + ", and " + nameItems[nameItems.Length - 1];
                    else
                        msg = msg + " and " + nameItems[nameItems.Length - 1];
                    break;
            }
            return msg;
        }
        public static T GetNextValueInList<T>(this T value, List<T> listVal)
        {
            int cur_id = listVal.IndexOf(value);
            if (cur_id == -1)
            {
                Debug.LogError($"{value} don't exist in list");
                return default;
            }
            int id = cur_id == listVal.Count - 1 ? 0 : cur_id + 1;
            return listVal[id];
        }
        public static T GetPreviousValueInList<T>(this T value, List<T> listVal)
        {
            int cur_id = listVal.IndexOf(value);
            if (cur_id == -1)
            {
                Debug.LogError($"{value} don't exist in list");
                return default;
            }
            int id = cur_id == 0 ? listVal.Count - 1 : cur_id - 1;
            return listVal[id];
        }
        public static string GarbageFreeString(StringBuilder sb)
        {
            string str = (string)sb.GetType().GetField(
                "_str",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance).GetValue(sb);

            //Optional: clear out the string
            //for (int i = 0; i < sb.Capacity; i++) {
            //  sb.Append(" ");
            //}
            return str;
        }
    }
    public static class ET_List
    {
        public static List<GameObject> arraygo_to_listgo(GameObject[] go_array)
        {
            List<GameObject> go_ret = new List<GameObject>();
            foreach (GameObject gox in go_array)
            {
                go_ret.Add(gox);
            }
            return go_ret;
        }
        public static List<Transform> arraygo_to_listtransform(GameObject[] go_array)
        {
            List<Transform> go_ret = new List<Transform>();
            foreach (GameObject gox in go_array)
            {
                go_ret.Add(gox.transform);
            }
            return go_ret;
        }
        public static List<Transform> listgo_to_listtransform(List<GameObject> go_array)
        {
            List<Transform> go_ret = new List<Transform>();
            foreach (GameObject gox in go_array)
            {
                go_ret.Add(gox.transform);
            }
            return go_ret;
        }
        public static T GetRandom<T>(List<T> lisx)
        {
            int ran = UnityEngine.Random.Range(0, lisx.Count);
            return lisx[ran];
        }
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
    
    

    public static class EFile
    {
        /// <summary>
        /// Count file type in direction    
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileTypes"></param>
        /// <returns></returns>
        public static int FilesCount(DirectoryInfo directory, List<string> fileTypes)
        {
            int ret = 0;
            foreach (string fileType in fileTypes)
            {
                FileInfo[] imageFiles = directory.GetFiles("*." + fileType);
                ret += imageFiles.Length;
            }
            return ret;
        }
        /// <summary>
        /// Rename file to newFileName = name.filetype ex: object1.obj
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="newFileName">newFileName = name.filetype ex: object1.obj</param>
        public static void Rename(FileInfo sourceFile, string newFileName)
        {
            // Update name
            File.Move(sourceFile.FullName, newFileName);
            // Update the corresponding .meta file
            string oldMetaFilePath = sourceFile.FullName + ".meta";
            if (File.Exists(oldMetaFilePath))
            {
                string newMetaFilePath = newFileName + ".meta";
                File.Move(oldMetaFilePath, newMetaFilePath);
            }
        }
    }

}