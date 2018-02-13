using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Compression
{
    public partial class Form1 : Form
    {
        OpenFileDialog ofd;
        Bitmap imagePrimary;
        Bitmap yCbCrImage;
        Bitmap subsampleImage;
        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            String path;
            ofd = new OpenFileDialog();
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                path = ofd.FileName;
                pictureBox1.ImageLocation = path;
                imagePrimary = new Bitmap(path);
            }
        }

        private Bitmap rgbToyCbCr(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            byte[,] Y = new byte[width,height];
            byte[,] cb = new byte[width, height];
            byte[,] cr = new byte[width, height];

            unsafe
            {
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                //int widthInBytes = width * 3;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < width; x++)
                    {
                        int xPor3 = x * 3;
                        float blue = currentLine[xPor3++];
                        float green = currentLine[xPor3++];
                        float red = currentLine[xPor3];


                        Y[x, y] = (byte)((0.299 * red) + (0.587 * green) + (0.114 * blue));
                        cb[x, y] = (byte)(128 - (0.168736 * red) + (0.331264 * green) + (0.5 * blue));
                        cr[x, y] = (byte)(128 + (0.5 * red) + (0.418688 * green) + (0.081312 * blue));
                    }
                }
                bitmap.UnlockBits(bitmapData);
            }
            

            return makeBitmap(ref Y, ref cb, ref cr, width, height);
        }

        private Bitmap makeBitmap(ref byte[,] Y, ref byte[,] cb, ref byte[,] cr, int width, int height)
        {
            byte alpha = 250;
            Bitmap bit = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    Color color = Color.FromArgb(alpha, Y[i, j], cb[i, j], cr[i, j]);
                    //Color color = Color.FromArgb(alpha, 0, 0, cr[i, j]);
                    bit.SetPixel(i, j, color);
                }
            }
            yCbCrImage = bit;
            return bit;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            imagePrimary = rgbToyCbCr(imagePrimary);
            pictureBox2.Image = imagePrimary;
        }

        private byte[][] subSample(Bitmap bmp)
        {

            int subHeight = bmp.Height/2;
            int subWidth = bmp.Width / 2;

            Bitmap bit = new Bitmap(subWidth, subHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int i = 0; i < subWidth; i++)
            {
                for(int j = 0; j < subHeight; j++)
                {
                    Color color = bmp.GetPixel(i*2, j*2);
                    bit.SetPixel(i, j, color);
                }
            }
            pictureBox3.Image = bit;
            subsampleImage = bit;
            return null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            subSample(yCbCrImage);
            Console.WriteLine("Original Height: " + imagePrimary.Height + "  Width: " + imagePrimary.Width);
            Console.WriteLine("yCbCr Height: " + yCbCrImage.Height + "  Width: " + yCbCrImage.Width);
            Console.WriteLine("subsample Height: " + subsampleImage.Height + "  Width: " + subsampleImage.Width);

        }
    }
}
