using Assimp;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Helper
{
    public static class HelperMatrix
    {
        public static Matrix4 BiasedMatrixForShadowMapping = new Matrix4(
            0.5f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.5f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.5f, 0.0f,
            0.5f, 0.5f, 0.5f, 1.0f);

        public static void ConvertAssimpToOpenTKMatrix(Matrix4x4 source, out Matrix4 convertedMatrix)
        {
            convertedMatrix = new Matrix4(source.A1, source.A2, source.A3, source.A4,
                                                            source.B1, source.B2, source.B3, source.B4,
                                                            source.C1, source.C2, source.C3, source.C4,
                                                            source.D1, source.D2, source.D3, source.D4);
            convertedMatrix.Transpose();
        }

        public static Matrix4 ConvertAssimpToOpenTKMatrix(Matrix4x4 source)
        {
            Matrix4 convertedMatrix = new Matrix4(source.A1, source.A2, source.A3, source.A4,
                                                            source.B1, source.B2, source.B3, source.B4,
                                                            source.C1, source.C2, source.C3, source.C4,
                                                            source.D1, source.D2, source.D3, source.D4);
            convertedMatrix.Transpose();
            return convertedMatrix;
        }

        public static void ConvertAssimpToOpenTKMatrix(Matrix3x3 source, out Matrix3 convertedMatrix)
        {
            convertedMatrix = new Matrix3(source.A1, source.A2, source.A3,
                                                            source.B1, source.B2, source.B3,
                                                            source.C1, source.C2, source.C3);
            convertedMatrix.Transpose();
        }
    }
}
