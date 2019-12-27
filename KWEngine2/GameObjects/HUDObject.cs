using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using KWEngine2.Helper;
using KWEngine2;
using System.IO;
using OpenTK.Input;

namespace KWEngine2.GameObjects
{
    public enum HUDObjectType { Image, Text }

    public sealed class HUDObject
    {
        internal Vector2 _absolute = new Vector2(0, 0);
        internal Vector4 _tint = new Vector4(1, 1, 1, 1);
        internal HUDObjectType _type = HUDObjectType.Image;
        internal int[] _textureIds = new int[] { KWEngine.TextureDefault };
        public Vector3 Position { get; internal set; } = Vector3.Zero;
        public World CurrentWorld { get; internal set; } = null;
        internal Vector3 _scale = new Vector3(32f, 32f, 1f);
        
        internal Vector3[] _positions = new Vector3[1];
        internal Matrix4[] _modelMatrices = new Matrix4[1];
        internal Matrix4 _scaleMatrix = Matrix4.CreateScale(32f,32f, 1f);
        internal string _text = null;
        internal int _count = 1;

        internal float _spread = 28f;
        public float CharacterSpreadFactor
        {
            get
            {
                return _spread;
            }
            set
            {
                _spread = HelperGL.Clamp(value, 1f, 1024f);
                UpdatePositions();
            }
        }

        public void SetColor(float red, float green, float blue, float intensity)
        {
            _tint.X = HelperGL.Clamp(red, 0, 1);
            _tint.Y = HelperGL.Clamp(green, 0, 1);
            _tint.Z = HelperGL.Clamp(blue, 0, 1);
            _tint.W = HelperGL.Clamp(intensity, 0, 1);
        }

        private void UpdateTextures()
        {
            _textureIds = new int[_count];
            for (int i = 0; i < _count; i++)
            {
                int letterIndex = HelperFont.LETTERS.IndexOf(_text[i]);
                if(letterIndex > 0)
                {
                    _textureIds[i] = HelperFont.TEXTURES[letterIndex];
                }
                else
                {
                    _textureIds[i] = KWEngine.TextureAlpha;
                }
            }
        }

        public void SetScale(float width, float height)
        {
            _scale.X = HelperGL.Clamp(width, 0.001f, float.MaxValue);
            _scale.Y = HelperGL.Clamp(height, 0.001f, float.MaxValue);
            _scale.Z = 1;
            _scaleMatrix = Matrix4.CreateScale(_scale);
            UpdatePositions();
        }

        public void SetText(string text)
        {
            if(_type == HUDObjectType.Text && text != null)
            {
                _text = text.Trim();
                _count = _text.Length;
                UpdatePositions();
                UpdateTextures();
            }
            else
            {
                throw new Exception("SetText() may only be called if the HUDObject is of type 'Text'.");
            }
        }

        public void SetTexture(string filename)
        {
            if (File.Exists(filename) && _type == HUDObjectType.Image)
            {
                if (KWEngine.CustomTextures[KWEngine.CurrentWorld].ContainsKey(filename))
                {
                    _textureIds[0] = KWEngine.CustomTextures[KWEngine.CurrentWorld][filename];
                }
                else
                {
                    _textureIds[0] = HelperTexture.LoadTextureForBackgroundExternal(filename);
                }
                
                _count = 1;
            }
            else
            {
                throw new Exception("Error: Is your HUD Type set to 'Image'? Or maybe the file " + filename + " does not exist. Is your path correct?");
            }
        }

        public HUDObject(HUDObjectType type, float x, float y)
        {
            _type = type;
            Position = new Vector3(x - KWEngine.CurrentWindow.Width / 2, KWEngine.CurrentWindow.Height - y - KWEngine.CurrentWindow.Height / 2, 0);
            //Position = new Vector3(x, y, 0);
            _absolute.X = x;
            _absolute.Y = y;
            
            UpdatePositions();
            
        }

        public void SetPosition(float x, float y)
        {
            Position = new Vector3(x - KWEngine.CurrentWindow.Width / 2, KWEngine.CurrentWindow.Height - y - KWEngine.CurrentWindow.Height / 2, 0);
            //Position = new Vector3(x, y, 0);
            _absolute.X = x;
            _absolute.Y = y;
            UpdatePositions();
        }

        private void SetPosition(int index, Vector3 pos)
        {
            pos.X = pos.X + (CharacterSpreadFactor * index);
            _positions[index] = pos;
            _modelMatrices[index] = _scaleMatrix * Matrix4.CreateTranslation(pos);
        }

        internal void UpdatePositions()
        {
            _positions = new Vector3[_count];
            _modelMatrices = new Matrix4[_count];
            for (int i = 0; i < _count; i++)
            {
                SetPosition(i, Position);
            }
        }

        public bool IsMouseCursorOnMe(MouseState ms)
        {
            
            GLWindow w = KWEngine.CurrentWindow;

            if (w._windowRect.Contains(ms.X, ms.Y)){
                Vector2 coords = HelperGL.GetNormalizedMouseCoords(ms.X, ms.Y, w);
               

                float left = _absolute.X - _scale.X * 0.5f;
                float right = _absolute.X + ((_count - 1) * _spread) + _scale.X * 0.5f;

                float top = _absolute.Y - _scale.Y * 0.5f;
                float bottom = _absolute.Y + _scale.Y * 0.5f;


                if(coords.X >= left && coords.X <= right && coords.Y >= top && coords.Y <= bottom)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
                return false;
        }
    }
}
