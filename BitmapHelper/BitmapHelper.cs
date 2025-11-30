using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyTool
{
    public class BitmapHelper
    {
        public static void ParseAndDisplayBitmap(byte[] data, PictureBox pictureBox1)
        {
            try
            {
                int width = 48;
                int height = 36;
                int dataOffset = 0x36; 

                Console.WriteLine($"解析图像: {width}x{height}, 数据偏移: {dataOffset}");

                AnalyzeDataPattern(data, dataOffset);

                ParseSpecialFormatBitmap(data, width, height, dataOffset, pictureBox1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解析位图时出错: {ex.Message}");
            }
        }

        private static void AnalyzeDataPattern(byte[] data, int dataOffset)
        {
            Console.WriteLine("分析数据模式:");
            for (int i = 5; i < 20; i++)
            {
                int pixelIndex = dataOffset + i * 4;
                if (pixelIndex + 3 < data.Length)
                {
                    Console.WriteLine($"像素 {i}: " +
                        $"[{data[pixelIndex]:X2} {data[pixelIndex + 1]:X2} {data[pixelIndex + 2]:X2} {data[pixelIndex + 3]:X2}] -> " +
                        $"模式: 0x{data[pixelIndex + 2]:X2}{data[pixelIndex + 3]:X2} 0x{data[pixelIndex]:X2}{data[pixelIndex + 1]:X2} " +
                        $"值: {data[pixelIndex + 1]}");
                }
            }
        }

        private static void ParseSpecialFormatBitmap(byte[] data, int width, int height, int dataOffset, PictureBox pictureBox1)
        {
            try
            {
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int sourceY = height - 1 - y; 
                        int pixelIndex = dataOffset + (sourceY * width + x) * 4;

                        if (pixelIndex + 3 < data.Length)
                        {
                            Color color = DecodeSpecialColor(
                                data[pixelIndex],
                                data[pixelIndex + 1],
                                data[pixelIndex + 2],
                                data[pixelIndex + 3],
                                x, y);

                            bitmap.SetPixel(x, y, color);
                        }
                    }
                }

                pictureBox1.Image = bitmap;
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                Console.WriteLine("图像解析完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解析特殊格式位图时出错: {ex.Message}");
            }
        }

        private static Color DecodeSpecialColor(byte b0, byte b1, byte b2, byte b3, int x, int y)
        {
            // 分析数据模式: 大多数像素是 D8 9E 19 XX 格式
            // 其中 XX 是变化的，可能是灰度值或颜色分量

            if (b2 == 0xD8 && b3 == 0x9E && b0 == 0x19)
            {
                // 模式: D8 9E 19 XX -> 使用 b1 作为颜色值
                byte intensity = b1;
                return Color.FromArgb(255, intensity, intensity, intensity);
            }
            else if (b0 == 0x00 && b1 == 0x00 && b2 == 0x00 && b3 == 0x00)
            {
                // 全零 - 透明或黑色
                return Color.Transparent;
            }
            else if (IsZeroPixel(b0, b1, b2, b3))
            {
                // 接近零的值 - 背景色
                return Color.Black;
            }
            else
            {
                // 其他情况，尝试作为标准BGRA
                return Color.FromArgb(255, b2, b1, b0);
            }
        }

        private static bool IsZeroPixel(byte b0, byte b1, byte b2, byte b3)
        {
            return b0 == 0x00 && b1 == 0x00 && b2 == 0x00 && b3 == 0x00;
        }

        private static void ParseAs16BitColor(byte[] data, int width, int height, int dataOffset, PictureBox pictureBox1)
        {
            try
            {
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int sourceY = height - 1 - y;
                        int pixelIndex = dataOffset + (sourceY * width + x) * 4;

                        if (pixelIndex + 3 < data.Length)
                        {
                            ushort color16 = (ushort)((data[pixelIndex + 1] << 8) | data[pixelIndex]);
                            Color color = Convert16BitTo32Bit(color16);
                            bitmap.SetPixel(x, y, color);
                        }
                    }
                }

                pictureBox1.Image = bitmap;
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解析16位颜色时出错: {ex.Message}");
            }
        }

        private static Color Convert16BitTo32Bit(ushort color16)
        {
            int r = (color16 >> 11) & 0x1F;
            int g = (color16 >> 5) & 0x3F;
            int b = color16 & 0x1F;

            r = (r * 255) / 31;
            g = (g * 255) / 63;
            b = (b * 255) / 31;

            return Color.FromArgb(255, r, g, b);
        }

        private static void ParseAsPatternBasedGrayscale(byte[] data, int width, int height, int dataOffset, PictureBox pictureBox1)
        {
            try
            {
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int sourceY = height - 1 - y;
                        int pixelIndex = dataOffset + (sourceY * width + x) * 4;

                        if (pixelIndex + 3 < data.Length)
                        {
                            byte intensity;

                            if (data[pixelIndex + 2] == 0xD8 && data[pixelIndex + 3] == 0x9E)
                            {
                                intensity = data[pixelIndex + 1];
                            }
                            else if (data[pixelIndex] == 0x19)
                            {
                                intensity = data[pixelIndex + 1];
                            }
                            else
                            {
                                intensity = (byte)((data[pixelIndex] + data[pixelIndex + 1] + data[pixelIndex + 2]) / 3);
                            }

                            bitmap.SetPixel(x, y, Color.FromArgb(255, intensity, intensity, intensity));
                        }
                    }
                }

                pictureBox1.Image = bitmap;
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解析模式灰度图像时出错: {ex.Message}");
            }
        }

        public static string GetDetailedAnalysis(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            int dataOffset = 0x36;
            sb.AppendLine($"数据长度: {data.Length} 字节");
            sb.AppendLine($"像素数据偏移: 0x{dataOffset:X}");
            sb.AppendLine();
            sb.AppendLine("数据模式分析:");
            sb.AppendLine("得到前50个像素的格式看看:");

            for (int i = 0; i < 50; i++)
            {
                int pixelIndex = dataOffset + i * 4;
                if (pixelIndex + 3 < data.Length)
                {
                    string pattern = GetDataPattern(data[pixelIndex], data[pixelIndex + 1], data[pixelIndex + 2], data[pixelIndex + 3]);
                    sb.AppendLine($"像素 {i,2}: {pattern} -> [{data[pixelIndex]:X2} {data[pixelIndex + 1]:X2} {data[pixelIndex + 2]:X2} {data[pixelIndex + 3]:X2}]");
                }
            }

            return sb.ToString();
        }

        private static string GetDataPattern(byte b0, byte b1, byte b2, byte b3)
        {
            if (b0 == 0x00 && b1 == 0x00 && b2 == 0x00 && b3 == 0x00) return "全零    ";
            if (b2 == 0xD8 && b3 == 0x9E && b0 == 0x19) return "D89E19XX";
            if (b0 == 0x19) return "19XXXXXX";
            return "其他格式";
        }

        public static void ShowMultipleParsingOptions(byte[] data, PictureBox pictureBox1)
        {
            Form optionsForm = new Form();
            optionsForm.Text = "选择解析方法";
            optionsForm.Size = new Size(300, 200);

            Button btn1 = new Button() { Text = "方法1: 特殊格式", Top = 20, Left = 20, Width = 120 };
            Button btn2 = new Button() { Text = "方法2: 16位颜色", Top = 60, Left = 20, Width = 120 };
            Button btn3 = new Button() { Text = "方法3: 模式灰度", Top = 100, Left = 20, Width = 120 };

            btn1.Click += (s, e) => { ParseSpecialFormatBitmap(data, 48, 36, 0x36, pictureBox1); optionsForm.Close(); };
            btn2.Click += (s, e) => { ParseAs16BitColor(data, 48, 36, 0x36, pictureBox1); optionsForm.Close(); };
            btn3.Click += (s, e) => { ParseAsPatternBasedGrayscale(data, 48, 36, 0x36, pictureBox1); optionsForm.Close(); };

            optionsForm.Controls.AddRange(new Control[] { btn1, btn2, btn3 });
            optionsForm.ShowDialog();
        }
    }
}