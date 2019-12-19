using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine2.Helper
{
    public static class HelperTexture
    {

        public static int RoundToPowerOf2(int value)
        {
            if (value < 0)
            {
                throw new Exception("Negative values are not allowed.");
            }

            uint v = (uint)value;

            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;

            return (int)v;
        }
        public static int LoadTextureForModelExternal(string filename, bool convertRoughnessToSpecular = false)
        {
            if (!File.Exists(filename))
            {
                throw new Exception("File " + filename + " not found.");
            }

            int texID = GL.GenTexture();
            try
            {
                Bitmap image = new Bitmap(filename);
                if (image == null)
                {
                    throw new Exception("File " + filename + " is not a valid image file.");
                }
                GL.BindTexture(TextureTarget.Texture2D, texID);
                BitmapData data = null;

                if (image.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                     OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                }
                else
                {
                    data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0,
                     OpenTK.Graphics.OpenGL4.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                }
                //glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_MAX_ANISOTROPY_EXT, value);
                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                image.Dispose();
                GL.BindTexture(TextureTarget.Texture2D, 0);

            }
            catch (Exception ex)
            {
                throw new Exception("Could not load image file " + filename + "! Make sure to copy it to the correct output directory. " + "[" + ex.Message + "]");
            }
            return texID;
        }
    }
}
