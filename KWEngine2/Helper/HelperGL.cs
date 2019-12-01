using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Helper
{
    public class HelperGL
    {
        
        public static void SetAlphaBlendingEnabled(bool enabled)
        {
            if (!enabled)
            {
                GL.Disable(EnableCap.Blend);
                GL.Enable(EnableCap.DepthTest);
            }
            else
            {
                GL.Enable(EnableCap.Blend);
                GL.Disable(EnableCap.DepthTest);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }
        }

        public static void SwitchToBufferAndClear(int id)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
            GL.ClearColor(0, 0, 0, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        }

        public static bool CheckGLErrors()
        {
            bool hasError = false;
            ErrorCode c;
            while ((c = GL.GetError()) != ErrorCode.NoError)
            {
                hasError = true;
                Debug.WriteLine(c.ToString());
            }
            return hasError;
        }


    }
}
