using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SeniASCIICreater
{
    class Program
    {
        //$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/\|()1{}[]?-_+~<>i!lI;:,\"^`'. 
        //@W#$OEXC[(/?=^~_.` 
        // `._~^=?/([CXEO$#W@
        public const string ASCIIList = "@W#$OEXC[(/?=^~_.` ";
        public const string colorList = "ABCDEFGHIJKLMNOPQRSTOVWXYZ@#$%&";
        public static Random rand = new Random();
        public const string inPath = "in.png";
        public const string outPath = "out.png";

        public static float scale;
        public static bool isColorful = false;
        public static bool isGaussionBlured = false;
        public static bool isSorted = false;

        static void Main(string[] args)
        {
            if (File.Exists(outPath))
            {
                Console.WriteLine("根目录下已存在out.png.");
                Console.ReadKey();
                return;
            }

            if (!File.Exists(inPath))
            {
                Console.WriteLine("根目录下不存在in.png.");
                Console.ReadKey();
                return;
            }


            string str;
            Console.WriteLine("使用前请在根目录存放in.png(包括文件拓展名),现在，键入A以选择黑白，键入O以选择彩色。");
            try
            {
                str = Console.ReadLine();
            }
            catch
            {
                Console.WriteLine("输入错误.");
                Console.ReadKey();
                return;
            }
            if (str == "A" || str == "a")
                isColorful = false;
            else if (str == "O" || str == "o")
                isColorful = true;
            else
            {
                Console.WriteLine("输入错误.");
                Console.ReadKey();
                return;
            }

            if (!isColorful)
            {
                Console.WriteLine("可选项:直方图均衡化.Y以肯定,N以否定.");
                try
                {
                    str = Console.ReadLine();
                }
                catch
                {
                    Console.WriteLine("输入错误.");
                    Console.ReadKey();
                    return;
                }
                if (str == "N" || str == "n")
                    isSorted = false;
                else if (str == "Y" || str == "y")
                    isSorted = true;
                else
                {
                    Console.WriteLine("输入错误.");
                    Console.ReadKey();
                    return;
                }
            }

            if (isColorful)
            {
                Console.WriteLine("可选项:高斯模糊.Y以肯定,N以否定.尺寸不高于于标准值请勿使用.");
                try
                {
                    str = Console.ReadLine();
                }
                catch
                {
                    Console.WriteLine("输入错误.");
                    Console.ReadKey();
                    return;
                }
                if (str == "N" || str == "n")
                    isGaussionBlured = false;
                else if (str == "Y" || str == "y")
                    isGaussionBlured = true;
                else
                {
                    Console.WriteLine("输入错误.");
                    Console.ReadKey();
                    return;
                }
            }

            Console.WriteLine("输入尺寸(尺寸越大,图片越清晰,1为标准值).");
            try
            {
                scale = float.Parse(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("输入错误.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("稍候.");

            Bitmap bitmap = new Bitmap(inPath);
            if (!isColorful)
                bitmap = RGB2Gray(bitmap);

            Size size = bitmap.Size;
            int width = size.Width;
            int height = size.Height;

            Bitmap imageData = new Bitmap(bitmap, (int)(width / 6 * scale), (int)(height / 9 * scale));
            Bitmap canvas = new Bitmap((int)(width * scale), (int)(height * scale));
            Graphics sourceGra = Graphics.FromImage(canvas);
            Color bg = Color.FromArgb(20, 150, 150, 150);
            SolidBrush brush = new SolidBrush(Color.White);
            if (!isColorful)
            {
                bg = Color.FromArgb(255, 255, 255, 255);
                brush.Color = Color.Black;
                if (isSorted)
                {
                    Bitmap newMap; 
                    Balance(imageData, out newMap);
                    imageData = newMap;
                }
            }

            sourceGra.Clear(bg);
            if (isGaussionBlured)
            {
                Bitmap blur = new Bitmap(bitmap);
                blur = ChangeDPi(blur, 10);
                blur = Resize(blur, canvas.Size);
                blur = BrightnessP(blur, -10);
                sourceGra.DrawImage(blur, 0, 0);
            }

            Font font = new Font("Courier New", 9f, FontStyle.Regular);

            PointF dir = new PointF(-1.35f, -2.5f);

            for (int i = 0; i < imageData.Size.Height; i++)
            {
                for (int j = 0; j < imageData.Size.Width; j++)
                {
                    float brightness = imageData.GetPixel(j, i).GetBrightness();
                    Color color = imageData.GetPixel(j, i);
                    brush.Color = color;
                    sourceGra.DrawString(GetChar(brightness).ToString(), font, brush, dir.X, dir.Y);
                    dir.X += 6f;
                }
                dir = new PointF(-1.35f, -2.5f + (i + 1) * 9);
            }

            canvas.Save("out.png", System.Drawing.Imaging.ImageFormat.Png);

            Console.WriteLine("结束.");

            Console.ReadKey();
        }
        public static char GetChar(float brightness)
        {
            if (!isColorful)
            {
                int index = (int)(brightness * 0.99 * ASCIIList.Count());
                return ASCIIList[index];
            }
            else
            {
                int index = rand.Next(0, colorList.Length);
                return colorList[index];
            }

        }

        /*public static Bitmap HistogramEqualization(Bitmap bitmap)
        {
            if (bitmap == null) return null;

            Bitmap newBitmap = bitmap.Clone() as Bitmap;

            int height = newBitmap.Size.Height;
            int width = newBitmap.Size.Width;
            int size = height * width;

            int[] gray = new int[256];
            double[] grayDense = new double[256];

            //计算各像元值的个数
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    Color pixelColor = newBitmap.GetPixel(i, j);
                    gray[Convert.ToByte(pixelColor.R)]++;

                }
            //计算各像元值占比
            for (int i = 0; i < 256; i++)
            {
                grayDense[i] = gray[i] * 1.0 / size;

            }
            //累计百分比
            for (int i = 1; i < 256; i++)
            {
                grayDense[i] += grayDense[i - 1];

            }

            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    
                    Color pixel = newBitmap.GetPixel(i, j);
                    int oldpixel = Convert.ToByte(pixel.R);//原始灰度
                    int newpixel;
                    if (oldpixel == 0)
                        newpixel = 0;
                    //如果原始灰度值为0则变换后也为0
                    else
                        newpixel = Convert.ToByte(grayDense[Convert.ToByte(pixel.R)] * 255);
                    //如果原始灰度不为0，则执行变换公式为   <新像元灰度 = 原始灰度 * 累计百分比>
                    pixel = Color.FromArgb(newpixel, newpixel, newpixel);
                    newBitmap.SetPixel(i, j, pixel);//读入newbitmap
                }

            return newBitmap;

        }*/

        public static bool Balance(Bitmap srcBmp, out Bitmap dstBmp)
        {
            if (srcBmp == null)
            {
                dstBmp = null;
                return false;
            }
            int[] histogramArrayR = new int[256];//各个灰度级的像素数R
            int[] histogramArrayG = new int[256];//各个灰度级的像素数G
            int[] histogramArrayB = new int[256];//各个灰度级的像素数B
            int[] tempArrayR = new int[256];
            int[] tempArrayG = new int[256];
            int[] tempArrayB = new int[256];
            byte[] pixelMapR = new byte[256];
            byte[] pixelMapG = new byte[256];
            byte[] pixelMapB = new byte[256];
            dstBmp = new Bitmap(srcBmp);
            Rectangle rt = new Rectangle(0, 0, srcBmp.Width, srcBmp.Height);
            BitmapData bmpData = dstBmp.LockBits(rt, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                //统计各个灰度级的像素个数
                for (int i = 0; i < bmpData.Height; i++)
                {
                    byte* ptr = (byte*)bmpData.Scan0 + i * bmpData.Stride;
                    for (int j = 0; j < bmpData.Width; j++)
                    {
                        histogramArrayB[*(ptr + j * 3)]++;
                        histogramArrayG[*(ptr + j * 3 + 1)]++;
                        histogramArrayR[*(ptr + j * 3 + 2)]++;
                    }
                }
                //计算各个灰度级的累计分布函数
                for (int i = 0; i < 256; i++)
                {
                    if (i != 0)
                    {
                        tempArrayB[i] = tempArrayB[i - 1] + histogramArrayB[i];
                        tempArrayG[i] = tempArrayG[i - 1] + histogramArrayG[i];
                        tempArrayR[i] = tempArrayR[i - 1] + histogramArrayR[i];
                    }
                    else
                    {
                        tempArrayB[0] = histogramArrayB[0];
                        tempArrayG[0] = histogramArrayG[0];
                        tempArrayR[0] = histogramArrayR[0];
                    }
                    //计算累计概率函数，并将值放缩至0~255范围内
                    pixelMapB[i] = (byte)(255.0 * tempArrayB[i] / (bmpData.Width * bmpData.Height) + 0.5);//加0.5为了四舍五入取整
                    pixelMapG[i] = (byte)(255.0 * tempArrayG[i] / (bmpData.Width * bmpData.Height) + 0.5);
                    pixelMapR[i] = (byte)(255.0 * tempArrayR[i] / (bmpData.Width * bmpData.Height) + 0.5);
                }
                //映射转换
                for (int i = 0; i < bmpData.Height; i++)
                {
                    byte* ptr = (byte*)bmpData.Scan0 + i * bmpData.Stride;
                    for (int j = 0; j < bmpData.Width; j++)
                    {
                        *(ptr + j * 3) = pixelMapB[*(ptr + j * 3)];
                        *(ptr + j * 3 + 1) = pixelMapG[*(ptr + j * 3 + 1)];
                        *(ptr + j * 3 + 2) = pixelMapR[*(ptr + j * 3 + 2)];
                    }
                }
            }
            dstBmp.UnlockBits(bmpData);
            return true;
        }

        public static Bitmap RGB2Gray(Bitmap basemap)
        {
            Bitmap res = basemap.Clone() as Bitmap;
            int width = res.Size.Width;
            int height = res.Size.Height;
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    Color pixel = basemap.GetPixel(i, j);
                    pixel = GetGray(Convert.ToByte(pixel.R), Convert.ToByte(pixel.G), Convert.ToByte(pixel.B));
                    res.SetPixel(i, j, pixel);
                }
            return res;
        }

        public static Color GetGray(int r, int g, int b)
        {
            int gray = 0;
            gray = (r * 30 + g * 59 + b * 11 + 50) / 100;

            Color retcolor = Color.FromArgb(gray, gray, gray);
            return retcolor;
        }

        public static Bitmap Resize(Bitmap bitmap, Size newSize)
        {
            Bitmap res = new Bitmap(bitmap, newSize);
            return res;
        }

        public static Bitmap ChangeDPi(Bitmap bitmap, int scale)
        {
            Size size = bitmap.Size;
            Bitmap blur = new Bitmap(bitmap, size.Width / scale, size.Height / scale);
            Bitmap bmpDest = new Bitmap(size.Width / scale, size.Height / scale);

            Graphics g = Graphics.FromImage(bmpDest);
            g.DrawImage(blur, 0, 0, new Rectangle(0, 0, size.Width, size.Height), GraphicsUnit.Pixel);
            blur = bmpDest;
            blur = Resize(blur, size);

            return blur;
        }

        private static Bitmap BrightnessP(Bitmap a, int v)
        {
            System.Drawing.Imaging.BitmapData bmpData = a.LockBits(new Rectangle(0, 0, a.Width, a.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int bytes = a.Width * a.Height * 3;
            IntPtr ptr = bmpData.Scan0;
            int stride = bmpData.Stride;
            unsafe
            {
                byte* p = (byte*)ptr;
                int temp;
                for (int j = 0; j < a.Height; j++)
                {
                    for (int i = 0; i < a.Width * 3; i++, p++)
                    {
                        temp = (int)(p[0] + v);
                        temp = (temp > 255) ? 255 : temp < 0 ? 0 : temp;
                        p[0] = (byte)temp;
                    }
                    p += stride - a.Width * 3;
                }
            }
            a.UnlockBits(bmpData);
            return a;
        }
    }
}
