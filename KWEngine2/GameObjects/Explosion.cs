using System;
using System.Collections.Generic;
using OpenTK.Input;
using OpenTK;
using KWEngine2.Helper;
using System.Diagnostics;

namespace KWEngine2.GameObjects
{
    /// <summary>
    /// Art der Explosion
    /// </summary>
    public enum ExplosionType { 
        /// <summary>
        /// Würfelpartikel in alle Richtungen
        /// </summary>
        Cube, 
        /// <summary>
        /// Würfelpartikel um die Y-Achse
        /// </summary>
        CubeRingY, 
        /// <summary>
        /// Würfelpartikel um die Z-Achse
        /// </summary>
        CubeRingZ,
        /// <summary>
        /// Kugelpartikel in alle Richtungen
        /// </summary>
        Sphere,
        /// <summary>
        /// Kugelpartikel um die Y-Achse
        /// </summary>
        SphereRingY,
        /// <summary>
        /// Kugelpartikel um die Z-Achse
        /// </summary>
        SphereRingZ
    }

    /// <summary>
    /// Explosionsklasse
    /// </summary>
    public sealed class Explosion : GameObject
    {
        internal static readonly Vector3[] Axes = new Vector3[] {
            Vector3.UnitX,
            Vector3.UnitY,
            Vector3.UnitZ,
            -Vector3.UnitX,
            -Vector3.UnitY,
            -Vector3.UnitZ,

            new Vector3(0.707107f,0.707107f,0),   // right       up
            new Vector3(0.577351f,0.577351f,0.577351f),   // right front up
            new Vector3(0,0.707107f,0.707107f),   // front       up
            new Vector3(-0.577351f,0.577351f,0.577351f),  // left front  up
            new Vector3(-0.707107f,0.707107f,0),  // left        up
            new Vector3(-0.577351f,0.577351f,-0.577351f), // left back   up
            new Vector3(0,0.707107f,-0.707107f),  // back        up
            new Vector3(0.577351f,0.577351f,-0.577351f),  // right back  up

            new Vector3(0.707107f,-0.707107f,0),   // right       down
            new Vector3(0.707107f,-0.707107f,0.707107f),   // right front down
            new Vector3(0,-0.707107f,0.707107f),   // front       down
            new Vector3(-0.577351f,-0.577351f,0.577351f),  // left front  down
            new Vector3(-0.707107f,-0.707107f,0),  // left        down
            new Vector3(-0.577351f,-0.577351f,-0.577351f), // left back   down
            new Vector3(0,-0.707107f,-0.707107f),  // back        down
            new Vector3(0.577351f,-0.577351f,-0.577351f),  // right back  down
        };
        internal static int AxesCount = Axes.Length;
        internal const int MAX_PARTICLES = 512; 

        internal int _textureId = -1;
        internal Vector2 _textureTransform = new Vector2(1, 1);
        internal int _amount = 32;
        internal float _spread = 10;
        internal float _duration = 2;
        internal long _starttime = -1;
        internal float _secondsAlive = 0;
        internal float _particleSize = 0.5f;
        internal float[] _directions;

        /// <summary>
        /// Explosionskonstruktormethode
        /// </summary>
        /// <param name="position">Position der Explosion</param>
        /// <param name="particleCount">Anzahl der Partikel</param>
        /// <param name="particleSize">Größe der Partikel</param>
        /// <param name="radius">Radius der Explosion</param>
        /// <param name="durationInSeconds">Dauer der Explosion in Sekunden</param>
        /// <param name="type">Art der Explosion</param>
        /// <param name="glow">Glühfarbe der Explosion</param>
        /// <param name="texture">Textur der Explosion (optional)</param>
        public Explosion(Vector3 position, int particleCount, float particleSize, float radius, float durationInSeconds, ExplosionType type, Vector4 glow, string texture = null)
        {
            World w = GLWindow.CurrentWindow.CurrentWorld;
            
            if (w == null)
                throw new Exception("World is null. Cannot create Explosion in an empty world.");

            
            if(type == ExplosionType.Cube || type == ExplosionType.CubeRingY || type == ExplosionType.CubeRingZ)
            {
                SetModel("KWCube");
            }
            else
            {
                SetModel("KWSphere");
            }

            Glow = glow;
            Position = position;
            _amount = particleCount >= 4 && particleCount <= MAX_PARTICLES ? particleCount : particleCount < 4 ? 4 : MAX_PARTICLES;
            _spread = radius > 0 ? radius : 10;
            _duration = durationInSeconds > 0 ? durationInSeconds : 2;
            _particleSize = particleSize;

            _directions = new float[_amount * 4];
            for(int i = 0, arrayIndex = 0; i < _amount; i++, arrayIndex += 4)
            {
                if (type == ExplosionType.Cube || type == ExplosionType.Sphere)
                {
                    int randomIndex = HelperRandom.GetRandomNumber(0, AxesCount - 1);
                    _directions[arrayIndex] = Axes[randomIndex].X;
                    _directions[arrayIndex + 1] = Axes[randomIndex].Y;
                    _directions[arrayIndex + 2] = Axes[randomIndex].Z;
                    _directions[arrayIndex + 3] = HelperRandom.GetRandomNumber(0.01f, 1.0f);
                }
                else if(type == ExplosionType.CubeRingY || type == ExplosionType.SphereRingY)
                {
                    _directions[arrayIndex] = 0;
                    _directions[arrayIndex + 1] = 1;
                    _directions[arrayIndex + 2] = 0;
                    _directions[arrayIndex + 3] = HelperRandom.GetRandomNumber(0.01f, 1.0f);
                }
                else
                {
                    _directions[arrayIndex] = 0;
                    _directions[arrayIndex + 1] = 0;
                    _directions[arrayIndex + 2] = 1;
                    _directions[arrayIndex + 3] = HelperRandom.GetRandomNumber(0.01f, 1.0f);
                }
            }

            int texId = -1;
            bool textureFound = false;
            if (texture != null && texture.Length > 0)
            {
                textureFound = KWEngine.CustomTextures[w].TryGetValue(texture, out texId);
            }
            if(textureFound)
                _textureId = texId;
            else
            {
                _textureId = HelperTexture.LoadTextureForModelExternal(texture);
                if (_textureId > 0)
                    KWEngine.CustomTextures[w].Add(texture, _textureId);
            }
        }

        /// <summary>
        /// Act-Methode der Explosion
        /// </summary>
        /// <param name="ks">Keyboardinfos</param>
        /// <param name="ms">MAusinfos</param>
        /// <param name="deltaTimeFactor">Delta-Faktor</param>
        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            if(_starttime >= 0)
            {
                long currentTime = Stopwatch.GetTimestamp() / TimeSpan.TicksPerMillisecond;
                _secondsAlive = (currentTime - _starttime) / 1000f;
                if(_secondsAlive > _duration)
                {
                    CurrentWorld.RemoveGameObject(this);
                    return;
                }
            }
        }
    }
}
