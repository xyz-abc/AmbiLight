using AmbiWrite;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;
using System.Drawing.Imaging;

namespace ScreenGrabber
{

    public class P
    {

        private static List<(int x, int y)> indices = new()
        {
            (0, 0),
            (1, 0),
            (2, 0),
            (3, 0),
            (4, 0),
            (5, 0),
            (6, 0),
            (7, 0),
            (8, 0),
            (9, 0),
            (9, 1),
            (9, 2),
            (9, 3),
            (9, 4),
            (8, 4),
            (7, 4),
            (6, 4),
            (5, 4),
            (4, 4),
            (3, 4),
            (2, 4),
            (1, 4),
            (0, 4),
            (0, 3),
            (0, 2),
            (0, 1),
        };
        static Color FromByteArr(byte[] arr) => Color.FromArgb((255 << 24) | (arr[0] << 16) | (arr[1] << 8) | arr[2]);

        public static void Main()
        {
            var bits = File.ReadAllBytes("b.bmp")!;

            var chunks = bits.Chunk(3);

            Bitmap bmp = new(10, 5);
            var things = chunks.Zip(indices, (l, r) => (l, r)).ToList();

            things.ForEach(_ => bmp.SetPixel(_.r.x, _.r.y, FromByteArr(_.l)));
            bmp.Save("bb.bmp");


            var sg = new ScreenGrab();
            var aw = new AmbiWriter();
            aw.Start(sg.GetPixels);
        }
    }

    public class ScreenGrab
    {
        const int maxLevel = 12;

        private int w = 2560;
        private int h = 1440;

        private Bitmap bmp;
        private Graphics g;

        private TextureHandle th;
        private NativeWindow nw;
        private GLFWBindingsContext provider;

        public Dictionary<int, (int w, int h)> LevelToSize = new();

        public ScreenGrab(int width = 2560, int height = 1440)
        {
            w = width;
            h = height;

            var nws = new NativeWindowSettings()
            {
                StartFocused = false,
                StartVisible = false,
                NumberOfSamples = 0,
                APIVersion = new Version(4, 6),
                Flags = ContextFlags.Offscreen,
                Profile = ContextProfile.Core,
                WindowBorder = WindowBorder.Hidden,
                WindowState = WindowState.Minimized
            };

            nw = new NativeWindow(nws);
            provider = new GLFWBindingsContext();

            GLLoader.LoadBindings(provider);

            th = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2d, th);

            bmp = new Bitmap(w, h);
            g = Graphics.FromImage(bmp);

            LevelToSize.Clear();
            LevelToSize.Add(0, (w, h));
            for (int i = 1; i < maxLevel + 1; i++)
            {
                LevelToSize[i] = (LevelToSize[i - 1].w / 2, LevelToSize[i - 1].h / 2);
            }
        }

        public Bitmap GetPixels(int lvl)
        {
            GetScreen();

            GL.GenerateTextureMipmap(th);
            GL.TextureParameteri(th, TextureParameterName.TextureBaseLevel, 0);
            GL.TextureParameteri(th, TextureParameterName.TextureMaxLevel, maxLevel);

            var d = bmp.LockBits(new Rectangle(0, 0, LevelToSize[lvl].w, LevelToSize[lvl].h), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.GetTexImage(TextureTarget.Texture2d, lvl, OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, d.Scan0);
            bmp.UnlockBits(d);
            return bmp;
        }

        private void GetScreen()
        {
            g.CopyFromScreen(0, 0, 0, 0, new Size(w, h));

            var data = bmp.LockBits(Rectangle.FromLTRB(0, 0, w, h), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            GL.TexImage2D(TextureTarget.Texture2d, 0, (int)InternalFormat.Rgb, w, h, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);
        }
    }
}