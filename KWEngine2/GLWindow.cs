using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
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

        public static GLWindow CurrentWindow { get; internal set; }
        internal Matrix4 _viewMatrix = Matrix4.Identity;
        internal Matrix4 _projectionMatrix = Matrix4.Identity;
        internal Matrix4 _projectionMatrixShadow = Matrix4.Identity;

        /// <summary>
        /// Konstruktormethode
        /// </summary>
        public GLWindow()
           : this(1280, 720, GameWindowFlags.FixedWindow, 0, true)
        {

        }

        private int _fsaa = 0;

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
            Width = width;
            Height = height;

            if(antialiasing >= 0 && antialiasing <= 8)
            {
                if (antialiasing == 1 || antialiasing == 3 || antialiasing == 5 || antialiasing == 6 || antialiasing == 7)
                    antialiasing = 0;
            }
            else
            {
                antialiasing = 0;
            }
            _fsaa = antialiasing;

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
            Console.WriteLine("on OpenGL 4.5 Core Profile.\n" + "================================================\n");

            KWEngine.InitializeShaders();
            KWEngine.InitializeModels();
        }

        

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1f);
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
                _viewMatrix = Matrix4.LookAt(CurrentWorld.GetCameraPosition(), CurrentWorld.GetCameraTarget(), KWEngine.WorldUp);
                Matrix4 viewProjection = _viewMatrix * _projectionMatrix;

                Matrix4 viewMatrixShadow = Matrix4.LookAt(CurrentWorld.GetSunPosition(), CurrentWorld.GetSunTarget(), KWEngine.WorldUp);
                Matrix4 viewProjectionShadow = viewMatrixShadow * _projectionMatrixShadow;
                lock (CurrentWorld)
                {
                    CurrentWorld.SortByZ();

                    SwitchToBufferAndClear(FramebufferShadowMap);
                    GL.Viewport(0, 0, KWEngine.ShadowMapSize, KWEngine.ShadowMapSize);
                    GL.UseProgram(KWEngine.Renderers["Shadow"].GetProgramId());
                    foreach (GameObject g in CurrentWorld.GetGameObjects())
                    {
                        KWEngine.Renderers["Shadow"].Draw(g, ref viewProjectionShadow);
                    }
                    GL.UseProgram(0);

                    SwitchToBufferAndClear(0);
                    GL.Viewport(0,0,Width,Height);
                    foreach (GameObject g in CurrentWorld.GetGameObjects())
                    {
                        KWEngine.Renderers["Standard"].Draw(g, ref viewProjection, ref viewProjectionShadow);
                    }
                }
            }
            SwapBuffers();
        }

        private static void SwitchToBufferAndClear(int id)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            KeyboardState ks = Keyboard.GetState();
            MouseState ms = Mouse.GetState();

            if (ks.IsKeyDown(Key.AltLeft) && ks.IsKeyDown(Key.F4))
            {
                Close();
                return;
            }

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

            InitializeFramebuffers();

            CalculateProjectionMatrix();
        }

        private void CalculateProjectionMatrix()
        {
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CurrentWorld != null ? CurrentWorld.FOV / 2: 45f), Width / (float)Height, 0.1f, CurrentWorld != null ? CurrentWorld.ZFar : 1000f);
            _projectionMatrixShadow = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CurrentWorld != null ? CurrentWorld.FOV / 2 : 45f), Width / (float)Height, 1f, CurrentWorld != null ? CurrentWorld.ZFar : 1000f);
        }

        public void SetWorld(World w)
        {
            if (CurrentWorld == null)
            {
                CurrentWorld = w;
                KWEngine.CubeTextures.Add(w, new Dictionary<string, int>());
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
                        Dictionary<string, int> worldTextures = KWEngine.CubeTextures[CurrentWorld];
                        foreach (int texId in worldTextures.Values)
                        {
                            GL.DeleteTexture(texId);
                        }
                        KWEngine.CubeTextures.Remove(CurrentWorld);
                    }
                    CurrentWorld = w;
                    KWEngine.CubeTextures.Add(w, new Dictionary<string, int>());
                    CurrentWorld.Prepare();
                    CalculateProjectionMatrix();
                }
            }
        }

        #region Framebuffers

        internal int FramebufferShadowMap = -1;
        internal int FramebufferBloom1 = -1;
        internal int FramebufferBloom2 = -1;
        internal int FramebufferMainMultisample = -1;
        //internal int FramebufferMainDownsampled = -1;
        internal int FramebufferMainFinal = -1;

        internal int TextureShadowMap = -1;
        internal int TextureMain = -1;
        internal int TextureMainDepth = -1;
        internal int TextureBloom = -1;
        internal int TextureBloom1 = -1;
        internal int TextureBloom2 = -1;
        internal int TextureMainFinal = -1;
        internal int TextureBloomFinal = -1;


        private void InitializeFramebuffers()
        {
            bool ok = false;
            while (!ok)
            {
                try
                {
                    if (TextureMain >= 0)
                    {
                        GL.DeleteTextures(8, new int[] { TextureMainDepth, TextureMain, TextureShadowMap, TextureBloom1, TextureBloom2, TextureMainFinal, TextureBloomFinal, TextureBloom });
                        GL.DeleteFramebuffers(5, new int[] { FramebufferShadowMap, FramebufferBloom1, FramebufferBloom2, FramebufferMainMultisample, FramebufferMainFinal, }); // FramebufferMainDownsampled,  });

                        Thread.Sleep(250);
                    }

                    InitFramebufferOriginal();
                    InitFramebufferOriginalDownsampled();
                    InitFramebufferShadowMap();
                    InitFramebufferBloom();
                }
                catch (Exception)
                {
                    ok = false;
                }
                ok = true;
            }
            
        }

        private void InitFramebufferShadowMap()
        {
            int framebufferId = -1;
            int depthTexId = -1;

            framebufferId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);

            depthTexId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, depthTexId);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32,
                KWEngine.ShadowMapSize, KWEngine.ShadowMapSize, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, new int[] { (int)TextureCompareMode.CompareRefToTexture });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, new int[] { (int)DepthFunction.Lequal });
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, new float[] { 1, 1, 1, 1 });
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureParameterName.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureParameterName.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (float)TextureParameterName.ClampToBorder);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthTexId, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            FramebufferErrorCode code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("GL_FRAMEBUFFER_COMPLETE failed. Cannot use FrameBuffer object.");
            }
            else
            {
                FramebufferShadowMap = framebufferId;
                TextureShadowMap = depthTexId;
            }
        }

        private void InitFramebufferOriginalDownsampled()
        {
            int framebufferId = -1;
            int renderedTexture = -1;
            int renderedTextureAttachment = -1;

            //Init des frame buffer:
            framebufferId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);

            // Init der Textur auf die gerendet wird:
            renderedTexture = GL.GenTexture();
            renderedTextureAttachment = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, renderedTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);


            GL.BindTexture(TextureTarget.Texture2D, renderedTextureAttachment);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);

            //Konfig. des frame buffer:
            GL.DrawBuffers(2, new DrawBuffersEnum[2] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, renderedTexture, 0);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, renderedTextureAttachment, 0);

            FramebufferErrorCode code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("GL_FRAMEBUFFER_COMPLETE failed. Cannot use FrameBuffer object.");
            }
            else
            {
                FramebufferMainFinal = framebufferId;
                TextureMainFinal = renderedTexture;
                TextureBloomFinal = renderedTextureAttachment;
            }
        }

        private void InitFramebufferOriginal()
        {
            int framebufferId = -1;
            int renderedTexture = -1;
            int renderedTextureAttachment = -1;
            int renderbufferFSAA = -1;
            int renderbufferFSAA2 = -1;
            int depthTexId = -1;

            // FULL RESOLUTION

            //Init des frame buffer:
            framebufferId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);

            // Init der Textur auf die gerendet wird:
            renderedTexture = GL.GenTexture();
            renderedTextureAttachment = GL.GenTexture();

            depthTexId = GL.GenTexture();


            //Konfig. des frame buffer:
            GL.DrawBuffers(2, new DrawBuffersEnum[2] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });

            GL.BindTexture(TextureTarget.Texture2D, renderedTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);

            //render buffer fsaa:
            renderbufferFSAA = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbufferFSAA);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, _fsaa, RenderbufferStorage.Rgba8, Width, Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, renderbufferFSAA);

            GL.BindTexture(TextureTarget.Texture2D, renderedTextureAttachment);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);

            renderbufferFSAA2 = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbufferFSAA2);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, _fsaa, RenderbufferStorage.Rgba8, Width, Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, RenderbufferTarget.Renderbuffer, renderbufferFSAA2);

            // depth buffer fsaa:
            int depthRenderBuffer = GL.GenRenderbuffer();
            GL.BindTexture(TextureTarget.Texture2D, depthTexId);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24,
                Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, new int[] { (int)TextureCompareMode.CompareRefToTexture });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, new int[] { (int)DepthFunction.Lequal });
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureParameterName.ClampToEdge);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthTexId, 0);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRenderBuffer);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, _fsaa, RenderbufferStorage.DepthComponent24, Width, Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthRenderBuffer);

            FramebufferErrorCode code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("GL_FRAMEBUFFER_COMPLETE failed. Cannot use FrameBuffer object.");
            }
            else
            {
                FramebufferMainMultisample = framebufferId;
                TextureMain = renderedTexture;
                TextureBloom = renderedTextureAttachment;
                TextureMainDepth = depthTexId;
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void InitFramebufferBloom()
        {
            int framebufferTempId = -1;
            int renderedTextureTemp = -1;

            // =========== TEMP ===========

            //Init des frame buffer:
            framebufferTempId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferTempId);

            // Init der Textur auf die gerendet wird:
            renderedTextureTemp = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, renderedTextureTemp);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureParameterName.ClampToEdge);

            //Konfig. des frame buffer:
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, renderedTextureTemp, 0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            FramebufferErrorCode code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("GL_FRAMEBUFFER_COMPLETE failed. Cannot use FrameBuffer object.");
            }
            else
            {
                FramebufferBloom1 = framebufferTempId;
                TextureBloom1 = renderedTextureTemp;
            }

            // =========== TEMP 2 ===========

            //Init des frame buffer:
            int framebufferId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);

            // Init der Textur auf die gerendet wird:
            int renderedTextureTemp2 = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, renderedTextureTemp2);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureParameterName.ClampToEdge);
            //Konfig. des frame buffer:
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, renderedTextureTemp2, 0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("GL_FRAMEBUFFER_COMPLETE failed. Cannot use FrameBuffer object.");
            }
            else
            {
                FramebufferBloom2 = framebufferId;
                TextureBloom2 = renderedTextureTemp2;
            }
        }
        #endregion
    }
}
