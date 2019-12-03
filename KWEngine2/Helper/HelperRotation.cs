using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Helper
{
    public static class HelperRotation
    
    {
        public static float CalculateRadiansFromDegrees(float degrees)
        {
            return (float)Math.PI * degrees / 180f;
        }

        public static float CalculateDegreesFromRadians(float radiant)
        {
            return (180f * radiant) / (float)Math.PI;
        }

        public static Vector3 RotateVector(Vector3 vector, float degrees)
        {
            return Vector3.TransformNormal(vector, Matrix4.CreateRotationY(CalculateRadiansFromDegrees(degrees)));
        }

        /// <summary>
        /// Berechnet den Vektor, der entsteht, wenn der übergebene Vektor um die angegebenen Grad rotiert wird
        /// </summary>
        /// <param name="vector">zu rotierender Vektor</param>
        /// <param name="degrees">Rotation (in Grad)</param>
        /// <param name="unitVector">Einheitsvektor, um den rotiert wird</param>
        /// <returns>Rotierter Vektor</returns>
        public static Vector3 RotateVector(Vector3 vector, float degrees, Vector3 unitVector)
        {
            if (unitVector == Vector3.UnitX)
            {
                return Vector3.TransformNormal(vector, Matrix4.CreateRotationX(CalculateRadiansFromDegrees(degrees)));
            }
            else if (unitVector == Vector3.UnitZ)
            {
                return Vector3.TransformNormal(vector, Matrix4.CreateRotationZ(CalculateRadiansFromDegrees(degrees)));
            }
            else
            {
                return Vector3.TransformNormal(vector, Matrix4.CreateRotationY(CalculateRadiansFromDegrees(degrees)));
            }
        }

        /// <summary>
        /// Konvertiert eine in Quaternion angegebene Rotation in eine XYZ-Rotation (in Grad)
        /// </summary>
        /// <param name="q">zu konvertierendes Quaternion</param>
        /// <returns>XYZ-Rotation als Vector3 (in Grad)</returns>
        public static Vector3 ConvertQuaternionToEulerAngles(Quaternion q)
        {
            Vector3 result = new Vector3(0, 0, 0);
            // roll (x-axis rotation)
            double sinr = +2.0 * (q.W * q.X + q.Y * q.Z);
            double cosr = +1.0 - 2.0 * (q.X * q.X + q.Y * q.Y);
            result.X = (float)Math.Atan2(sinr, cosr);

            // pitch (y-axis rotation)
            double sinp = +2.0 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                result.Y = sinp < 0 ? ((float)Math.PI / 2.0f) * -1.0f : (float)Math.PI / 2.0f;
            }
            else
                result.Y = (float)Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny = +2.0 * (q.W * q.Z + q.X * q.Y);
            double cosy = +1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z);
            result.Z = (float)Math.Atan2(siny, cosy);

            result.X = CalculateDegreesFromRadians(result.X);
            result.Y = CalculateDegreesFromRadians(result.Y);
            result.Z = CalculateDegreesFromRadians(result.Z);

            return result;
        }
    }

}
