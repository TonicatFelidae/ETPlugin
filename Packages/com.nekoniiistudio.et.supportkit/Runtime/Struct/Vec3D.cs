using System;

namespace ET.Engine
{
    [Serializable]
    public struct Vec3D
    {
        public float x;
        public float y;
        public float z;

        public Vec3D(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        #region Operator

        public override bool Equals(object obj)
        {
            return Equals((Vec3D)obj);
        }

        public bool Equals(Vec3D p)
        {
            if (ReferenceEquals(p, null))
            {
                return false;
            }

            if (ReferenceEquals(this, p))
            {
                return true;
            }

            if (GetType() != p.GetType())
            {
                return false;
            }

            return Magnitude(this, p) <= float.Epsilon;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode() + z.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", x, y, z);
        }

        public static bool operator ==(Vec3D l, Vec3D r)
        {
            if (ReferenceEquals(l, null))
            {
                if (ReferenceEquals(r, null))
                {
                    return true;
                }

                return false;
            }
            return l.Equals(r);
        }

        public static bool operator !=(Vec3D l, Vec3D r)
        {
            return !(l == r);
        }
        public static Vec3D operator +(Vec3D a, Vec3D b)
        {
            return new Vec3D(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static Vec3D operator -(Vec3D a, Vec3D b)
        {
            return new Vec3D(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public static Vec3D operator *(Vec3D a, float d)
        {
            return new Vec3D(a.x * d, a.y * d, a.z * d);
        }
        public static Vec3D operator *(Vec3D a, Vec3D b)
        {
            return new Vec3D(a.x * b.x, a.y * b.y, a.z * b.z);
        }
        #endregion

        public static float Magnitude(Vec3D a, Vec3D b)
        {
            return (float)Math.Sqrt(((a.x - b.x) * (a.x - b.x)) + ((a.y - b.y) * (a.y - b.y)) + ((a.z - b.z) * (a.z - b.z)));
        }

        public static Vec3D Zero = new Vec3D(0, 0, 0);
        public static Vec3D One = new Vec3D(1, 1, 1);
        public static Vec3D Left = new Vec3D(-1, 0, 0);
        public static Vec3D Right = new Vec3D(1, 0, 0);
        public static Vec3D Up = new Vec3D(0, 1, 0);
        public static Vec3D Down = new Vec3D(0, -1, 0);
        public static Vec3D Back = new Vec3D(0, 0, -1);
        public static Vec3D Forward = new Vec3D(0, 0, 1);
    }
}
