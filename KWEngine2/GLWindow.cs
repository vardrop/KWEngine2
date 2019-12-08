using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using KWEngine2.Engine;
using KWEngine2.GameObjects;
using KWEngine2.Helper;
using KWEngine2.Renderers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace KWEngine2
{
    public abstract class GLWindow : GameWindow
    {
        public World CurrentWorld { get; private set; }

        internal List<RenderObject> _renderObjects = new List<RenderObject>();

        public static GLWindow CurrentWindow { get; internal set; }
        internal Matrix4 _projectionMatrix = Matrix4.Identity;

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
            VSync = vSync ? VSyncMode.Adaptive : VSyncMode.Off;
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
            GL.ClearColor(0.0f, 0.0f, 1.0f, 1f);
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

            if (CurrentWorld != null)
            {
                Matrix4 viewMatrix = Matrix4.LookAt(CurrentWorld.GetCameraPosition(), CurrentWorld.GetCameraTarget(), KWEngine.WorldUp);
                Matrix4 viewProjection = viewMatrix * _projectionMatrix;



                foreach (GameObject g in CurrentWorld.GetGameObjects())
                {
                    KWEngine.Renderers["Standard"].Draw(g, ref viewProjection);
                }
            }
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            KeyboardState ks = Keyboard.GetState();
            MouseState ms = Mouse.GetState();

            foreach(GameObject g in CurrentWorld.GetGameObjects())
            {
                g.Act(ks, ms, DeltaTime.GetDeltaTimeFactor());
                g.ProcessCurrentAnimation();
                
            }

            DeltaTime.UpdateDeltaTime();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(ClientRectangle);
            CalculateProjectionMatrix();
        }

        private void CalculateProjectionMatrix()
        {
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CurrentWorld != null ? CurrentWorld.FOV / 2: 45f), Width / (float)Height, 0.1f, CurrentWorld != null ? CurrentWorld.ZFar : 1000f);
        }

        public void SetWorld(World w)
        {
            if (CurrentWorld == null)
            {
                CurrentWorld = w;
                CurrentWorld.Prepare();
                CalculateProjectionMatrix();
                return;
            }
            else
            {
                lock (CurrentWorld)
                {
                    if (CurrentWorld != null)
                    {
                        CurrentWorld.Dispose();
                    }
                    CurrentWorld = w;
                    CurrentWorld.Prepare();
                    CalculateProjectionMatrix();
                }
            }
        }
    }
}
