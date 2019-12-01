using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using KWEngine2.Engine;
using KWEngine2.Helper;
using KWEngine2.Renderers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine2
{
    public abstract class GLWindow : GameWindow
    {
        public World CurrentWorld { get; private set; }

        public static GLWindow CurrentWindow { get; internal set; }

        /// <summary>
        /// Konstruktormethode
        /// </summary>
        public GLWindow()
           : this(1280, 720, GameWindowFlags.FixedWindow, 0, true)
        {

        }

        /// <summary>
        /// Konstruktormethode
        /// </summary>
        /// <param name="width">Breite des Fensters</param>
        /// <param name="height">Höhe des Fensters</param>
        /// <param name="flag">FixedWindow oder FullScreen</param>
        /// <param name="antialiasing">FSAA-Wert (Anti-Aliasing)</param>
        /// <param name="vSync">VSync aktivieren</param>
        public GLWindow(int width, int height, GameWindowFlags flag, int antialiasing = 0, bool vSync = true)
            : base(width, height, GraphicsMode.Default, "KWEngine2 - C# 3D Gaming", flag, DisplayDevice.Default, 4, 5, GraphicsContextFlags.ForwardCompatible, null, false)
        {
            
            if (flag != GameWindowFlags.Fullscreen)
            {
                X = Screen.PrimaryScreen.Bounds.Width / 2 - Width / 2;
                Y = Screen.PrimaryScreen.Bounds.Height / 2 - Height / 2;
            }
            else
            {
                X = 0;
                Y = 0;
            }

            CurrentWindow = this;

            BasicInit();
        }

        private void BasicInit()
        {
            string productVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductMajorPart + 
                "." + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductMinorPart;
            Console.Write("\n\n\n================================================\n" + "Running KWEngine " + productVersion + " ");
            Console.WriteLine("on OpenGL 4 Core Profile.\n" + "================================================\n");

            KWEngine.InitializeShaders();
            KWEngine.InitializeModels();
        }

        

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            // Only needed for tesselation... maybe later?
            //GL.PatchParameter(PatchParameterInt.PatchVertices, 4);

            HelperGL.CheckGLErrors();
        }

        protected override void Dispose(bool manual)
        {
            base.Dispose(manual);

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // TODO

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle);
        }

        public void SetWorld(World w)
        {
            lock (CurrentWorld)
            {
                if(CurrentWorld != null)
                {
                    CurrentWorld.Dispose();
                }
                CurrentWorld = null;
                CurrentWorld = w;
            }
        }
    }
}
