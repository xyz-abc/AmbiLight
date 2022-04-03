using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;

namespace AmbiWrite
{
    public class AmbiWriter
    {
        private SerialPort p;

        private double brightnessScale = .5;
        private Func<int, Bitmap> bmp;
        private bool running = true;
        public void Start(Func<int, Bitmap> bmpGetter)
        {
            bmp = bmpGetter;

            p = new("COM4", 115200);
            p.ReadTimeout = 2000;
            p.Open();

            Console.WriteLine("port open");

            var cons = Task.Run(() =>
            {
                while (true)
                {

                    Console.WriteLine("reading input");
                    var s = Console.ReadLine();
                    if (double.TryParse(s, out var scale))
                    {
                        Console.WriteLine("updated scale");
                        brightnessScale = scale;
                    }
                    else if(s == "q")
                    {
                        running = false;
                    }
                }
            });

            var outp = Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var s = p.ReadLine();
                        s = s.Replace((char)7, 'A');
                        Console.WriteLine(s);
                    }
                    catch (TimeoutException e)
                    {
                        Console.WriteLine("no readage");
                    }
                }
            });

            UpdateColors();
            cons.Wait();
            outp.Wait();
        }

        private List<(int x, int y)> indices = new()
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
        private byte[] TestBytes()
        {
            return Enumerable.Repeat(new byte[] { 255, 105, 180}, 13).Zip(Enumerable.Repeat(new byte[] { 0, 0, 0 }, 13), (l, r) => l.Concat(r)).SelectMany(_ => _).ToArray();
        }
        private void UpdateColors()
        {
            var sw = Stopwatch.StartNew();
            int i = 0;
            var shift = 30;
            while (running)
            {
                var b = bmp(8);
                //var bytes = indices.SelectMany(b.GetBytesOfPixel).ToArray();
                ////bytes = TestBytes();
                //bytes = bytes.Select(_ => (byte)(_ * brightnessScale)).ToArray();
                //bytes = bytes[shift..].Concat(bytes[..shift]).ToArray();
                //p.Write(bytes, 0, bytes.Length);
                //i++;
                //if (sw.ElapsedMilliseconds >= 1000)
                //{
                //    Console.WriteLine(i);
                //    sw.Restart();
                //    i = 0;
                //}
                //Console.WriteLine($"Wrote {bytes.Length} bytes");
                //Thread.Sleep(50);
            }
        }
    }

    public static class BitmapExt
    {
        public static byte[] GetBytesOfPixel(this Bitmap bmp, (int x, int y) pos)
        {
            var b = new byte[3];
            var pix = bmp.GetPixel(pos.x, pos.y);
            b[0] = pix.R;
            b[1] = pix.G;
            b[2] = pix.B;
            return b;
        }
    }
}