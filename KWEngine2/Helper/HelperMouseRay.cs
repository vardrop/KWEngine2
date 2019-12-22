using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KWEngine2.Helper
{
    internal struct HelperMouseRay
    {
        private Vector3 mStart;
        private Vector3 mEnd;

        public Vector3 Start { get { return mStart; } }

        public Vector3 End { get { return mEnd; } }

        public HelperMouseRay(float x, float y, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            mStart = new Vector3(x, y, 0.0f).UnProject(projectionMatrix, viewMatrix, GLWindow.CurrentWindow.Width, GLWindow.CurrentWindow.Height);
            mEnd = new Vector3(x, y, 1.0f).UnProject(projectionMatrix, viewMatrix, GLWindow.CurrentWindow.Width, GLWindow.CurrentWindow.Height);
        }
    }
}
