using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KWEngine2.Helper;
using KWEngine2.Model;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine2.GameObjects
{
    public enum ParticleType { 
        BurstFire1, 
        BurstFire2, 
        BurstFire3, 
        BurstElecricity1, 
        BurstBubblesColored, 
        BurstBubblesMonochrome,
        BurstFirework1,
        BurstFirework2,
        BurstHearts,
        BurstOneUps,
        BurstShield,
        BurstTeleport1,
        BurstTeleport2,
        BurstTeleport3,
        LoopSmoke1,
        LoopSmoke2,
        LoopSmoke3
    }

    public sealed class ParticleObject
    {
        internal GeoModel _model = KWEngine.Models["KWRect"];
        internal Vector3 _position = new Vector3(0, 0, 0);
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
        private Vector3 _scale = new Vector3(1, 1, 1);
        internal Matrix4 _rotation = Matrix4.Identity;
        internal Matrix4 _modelMatrix = Matrix4.Identity;
        internal Vector4 _tint = new Vector4(0, 0, 0, 0);
        internal ParticleType _type = ParticleType.BurstFire1;
        internal long _starttime = -1;
        internal long _lastUpdate = -1;
        internal long _durationInMS = 5000;
        internal int _frame = 0;
        internal long _aliveInMS = 0;
        internal ParticleInfo _info;

        public void SetDuration(float durationInSeconds)
        {
            _durationInMS = durationInSeconds > 0 ? (int)(durationInSeconds * 1000) : 5000;
        }

        public void SetPosition(Vector3 pos)
        {
            Position = pos;
        }

        public ParticleObject(Vector3 position, Vector3 scale, ParticleType type, float red = 1, float green = 1, float blue = 1, float intensity = 1)
        {
            _scale.X = HelperGL.Clamp(scale.X, 0.001f, float.MaxValue);
            _scale.Y = HelperGL.Clamp(scale.Y, 0.001f, float.MaxValue);
            _scale.Z = HelperGL.Clamp(scale.Z, 0.001f, float.MaxValue);

            _tint.X = HelperGL.Clamp(red, 0, 1);
            _tint.Y = HelperGL.Clamp(green, 0, 1);
            _tint.Z = HelperGL.Clamp(blue, 0, 1);
            _tint.W = HelperGL.Clamp(intensity, 0, 1);

            Position = position;

            _type = type;

            _info = KWEngine.ParticleDictionary[_type];
        }

        internal void Act()
        {
            long now = Stopwatch.GetTimestamp() / TimeSpan.TicksPerMillisecond;
            if (KWEngine.CurrentWorld.IsFirstPersonMode)
            {
                Vector3 fpPos = KWEngine.CurrentWorld.GetFirstPersonObject().Position;
                fpPos.Y += KWEngine.CurrentWorld.GetFirstPersonObject().FPSEyeOffset;
                _rotation = HelperRotation.GetRotationForPoint(fpPos, Position);
            }
            else
            {
                //_rotation = HelperRotation.GetRotationForPoint(Position, KWEngine.CurrentWorld.GetCameraPosition());
                _rotation = HelperRotation.GetRotationForPoint(KWEngine.CurrentWorld.GetCameraPosition(), Position);
            }

            _modelMatrix = Matrix4.CreateScale(_scale) * _rotation * Matrix4.CreateTranslation(Position);
            long diff = _lastUpdate < 0 ? 0 : now - _lastUpdate;
            _aliveInMS += diff;
            _frame = (int)(_aliveInMS / 32);
            //Console.WriteLine("frame: " + _frame);
            if (_frame > _info.Samples)
            {
                if(_type == ParticleType.LoopSmoke1 || _type == ParticleType.LoopSmoke2 || _type == ParticleType.LoopSmoke3)
                {
                    _frame = 0;
                    if(now - _starttime > _durationInMS)
                    {
                        KWEngine.CurrentWorld.RemoveParticleObject(this);
                    }
                }
                else
                    KWEngine.CurrentWorld.RemoveParticleObject(this);
            }

            _lastUpdate = now;
        }


    }
}
