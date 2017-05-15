using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using static System.Math;

namespace PatternPainter
{
    class Program
    {
        private static string name; 
        private static int x_res;
        private static int y_res;
        private static double x_scale;
        private static double y_scale;
        private static double rotation;
        private static double x_shear;
        private static double y_shear;
        private static int pattern_nmbr;
        private static bool color_1_b;
        private static bool color_2_b;
        private static bool blur_b;
        private static int iterations;

        static void Main(string[] args)
        {
            if (args.Length == 13)
            {
                name = args[0];
                Color color_1 = Color.Empty;
                Color color_2 = Color.Empty;
                Bitmap image_1 = new Bitmap(1, 1);
                Bitmap image_2 = new Bitmap(1, 1);

                try
                {
                    x_res = Convert.ToInt32(args[1]);
                    y_res = Convert.ToInt32(args[2]);
                    x_scale = 1.0/Convert.ToDouble(args[3]);
                    y_scale = 1.0/Convert.ToDouble(args[4]);
                    rotation = Convert.ToDouble(args[5])*PI/180;
                    x_shear = Convert.ToDouble(args[6]);
                    y_shear = Convert.ToDouble(args[7]);

                    int color_1_int;
                    if (int.TryParse(args[8], NumberStyles.HexNumber, null, out color_1_int))
                    {
                        color_1 = Color.FromArgb(color_1_int);
                        color_1_b = true;
                    }
                    else
                    {
                        image_1 = new Bitmap(args[8]);
                        if (image_1.Width < x_res && image_1.Height < y_res)
                        {
                            Console.Error.WriteLine("Image 1 too small");
                            Console.ReadKey(true);
                            return;
                        }
                        color_1_b = false;
                    }

                    int color_2_int;
                    if (int.TryParse(args[9], NumberStyles.HexNumber, null, out color_2_int))
                    {
                        color_2 = Color.FromArgb(color_2_int);
                        color_2_b = true;
                    }
                    else
                    {
                        image_2 = new Bitmap(args[9]);
                        if (image_2.Width < x_res && image_2.Height < y_res)
                        {
                            Console.Error.WriteLine("Image 2 too small");
                            Console.ReadKey(true);
                            return;
                        }
                        color_2_b = false;
                    }

                    pattern_nmbr = Convert.ToInt32(args[10]);
                    blur_b = Convert.ToBoolean(args[11]);
                    iterations = Convert.ToInt32(args[12]);
                }
                catch (Exception)
                {
                    Console.Error.WriteLine("Invalid argument");
                    Console.ReadKey(true);
                    return;
                }

                Bitmap outputImage = new Bitmap(x_res, y_res, PixelFormat.Format24bppRgb);

                for (var i = 0; i < y_res; i++)
                {
                    for (var j = 0; j < x_res; j++)
                    {
                        int alpha = (int)(get_alpha(j, i) * 255.0);

                        outputImage.SetPixel(j, i, Color.FromArgb(alpha, alpha, alpha));
                    }
                }

                if (blur_b)
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        outputImage = blur_image(ref outputImage);
                    }
                }

                for (var i = 0; i < y_res; i++)
                {
                    for (var j = 0; j < x_res; j++)
                    {
                        double alpha = outputImage.GetPixel(j, i).R / 255.0;

                        Color color_l_1 = color_1_b ? color_1 : image_1.GetPixel(j, i);
                        Color color_l_2 = color_2_b ? color_2 : image_2.GetPixel(j, i);

                        int red = (int)(alpha * color_l_1.R + (1 - alpha) * color_l_2.R);
                        int green = (int)(alpha * color_l_1.G + (1 - alpha) * color_l_2.G);
                        int blue = (int)(alpha * color_l_1.B + (1 - alpha) * color_l_2.B);

                        outputImage.SetPixel(j, i, Color.FromArgb(red, green, blue));
                    }
                }

                outputImage.Save(name, ImageFormat.Bmp);
            }
            else
            {
                Console.Error.WriteLine("Not enough arguments");
                Console.ReadKey(true);
            }
        }

        static double get_alpha(int x, int y)
        {
            double alpha = 0;

            switch (pattern_nmbr)
            {
                case 1:
                {
                    alpha = pattern_1(x, y);
                    break;
                }
                case 2:
                {
                    alpha = pattern_2(x, y);
                    break;
                }
                case 3:
                {
                    alpha = pattern_3(x, y);
                    break;
                }
                case 4:
                {
                    alpha = pattern_4(x, y);
                    break;
                }
                case 5:
                {
                    alpha = pattern_5(x, y);
                    break;
                }
                case 6:
                {
                    alpha = pattern_6(x, y);
                    break;
                }
                case 7:
                {
                    alpha = pattern_7(x, y);
                    break;
                }
                case 8:
                {
                    alpha = pattern_8(x, y);
                    break;
                }
            }

            return alpha;
        }

        static void transform(ref double x, ref double y)
        {
            double x_t = x*(x_scale * Cos(rotation) - y_scale * x_shear * Sin(rotation)) + 
                            y * (x_scale * Sin(rotation) + y_scale  *x_shear * Cos(rotation));
            double y_t = x * (x_scale * y_shear * Cos(rotation) - 
                                                    (y_scale * x_shear * y_shear + y_scale) * Sin(rotation)) + 
                        y*(x_scale * y_shear * Sin(rotation) + 
                                                        (y_scale * x_shear * y_shear + y_scale)*Cos(rotation));
            x = x_t;
            y = y_t;
        }

        static Bitmap blur_image(ref Bitmap old_image)
        {
            Bitmap result = new Bitmap(old_image);

            blur_horizontal(ref old_image, ref result);

            old_image = result;

            blur_vertical(ref old_image, ref result);

            return result;
        }

        static void blur_horizontal(ref Bitmap old_image, ref Bitmap new_image)
        {
            for (int y = 0; y < old_image.Height; y++)
            {
                int value = old_image.GetPixel(0, y).R
                            + old_image.GetPixel(1, y).R
                            + old_image.GetPixel(2, y).R;

                value /= 3;

                new_image.SetPixel(0, y, Color.FromArgb(value, value, value));

                value = old_image.GetPixel(0, y).R
                        + old_image.GetPixel(1, y).R
                        + old_image.GetPixel(2, y).R
                        + old_image.GetPixel(3, y).R;

                value /= 4;

                new_image.SetPixel(1, y, Color.FromArgb(value, value, value));

                for (int x = 2; x < old_image.Width - 2; x++)
                {
                    value = old_image.GetPixel(x - 2, y).R
                            + old_image.GetPixel(x - 1, y).R
                            + old_image.GetPixel(x, y).R
                            + old_image.GetPixel(x + 1, y).R
                            + old_image.GetPixel(x + 2, y).R;

                    value /= 5;

                    new_image.SetPixel(x, y, Color.FromArgb(value, value, value));
                }

                value = old_image.GetPixel(old_image.Width - 4, y).R
                        + old_image.GetPixel(old_image.Width - 3, y).R
                        + old_image.GetPixel(old_image.Width - 2, y).R
                        + old_image.GetPixel(old_image.Width - 1, y).R;

                value /= 4;

                new_image.SetPixel(old_image.Width - 2, y, Color.FromArgb(value, value, value));

                value = old_image.GetPixel(old_image.Width - 3, y).R
                        + old_image.GetPixel(old_image.Width - 2, y).R
                        + old_image.GetPixel(old_image.Width - 1, y).R;

                value /= 3;

                new_image.SetPixel(old_image.Width - 1, y, Color.FromArgb(value, value, value));
            }
        }

        static void blur_vertical(ref Bitmap old_image, ref Bitmap new_image)
        {
            for (int x = 0; x < old_image.Height; x++)
            {
                int value = old_image.GetPixel(x, 0).R
                            + old_image.GetPixel(x, 1).R
                            + old_image.GetPixel(x, 2).R;

                value /= 3;

                new_image.SetPixel(x, 0, Color.FromArgb(value, value, value));

                value = old_image.GetPixel(x, 0).R
                        + old_image.GetPixel(x, 1).R
                        + old_image.GetPixel(x, 2).R
                        + old_image.GetPixel(x, 3).R;

                value /= 4;

                new_image.SetPixel(x, 1, Color.FromArgb(value, value, value));

                for (int y = 2; y < old_image.Width - 2; y++)
                {
                    value = old_image.GetPixel(x, y - 2).R
                            + old_image.GetPixel(x, y - 1).R
                            + old_image.GetPixel(x, y).R
                            + old_image.GetPixel(x, y + 1).R
                            + old_image.GetPixel(x, y + 2).R;

                    value /= 5;

                    new_image.SetPixel(x, y, Color.FromArgb(value, value, value));
                }

                value = old_image.GetPixel(x, old_image.Height - 4).R
                        + old_image.GetPixel(x, old_image.Height - 3).R
                        + old_image.GetPixel(x, old_image.Height - 2).R
                        + old_image.GetPixel(x, old_image.Height - 1).R;

                value /= 4;

                new_image.SetPixel(x, old_image.Height - 2, Color.FromArgb(value, value, value));

                value = old_image.GetPixel(x, old_image.Height - 3).R
                        + old_image.GetPixel(x, old_image.Height - 2).R
                        + old_image.GetPixel(x, old_image.Height - 1).R;

                value /= 3;

                new_image.SetPixel(x, old_image.Height - 1, Color.FromArgb(value, value, value));
            }
        }

        static double triangle_wave(double t)
        {
            double a = 10.0;

            return 2 / a * (t - a * Floor(t / a + 0.5)) * Pow(-1, Floor(t / a + 0.5));
        }

        static double pattern_1(int x, int y)
        {
            int x_c = x_res / 2;
            int y_c = y_res / 2;

            double x_t = x - x_c;
            double y_t = y - y_c;

            transform(ref x_t, ref y_t);

            double d = Sqrt(y_t * y_t + x_t * x_t);

            double alpha = 1.5 * triangle_wave(d);

            alpha = Sign(alpha);

            return Max(Min(alpha, 1), 0);
        }

        static double pattern_2(int x, int y)
        {
            int x_c = x_res / 2;
            int y_c = y_res / 2;

            double x_t = x - x_c;
            double y_t = y - y_c;

            transform(ref x_t, ref y_t);

            double alpha_x = 2 * triangle_wave(x_t);
            double alpha_y = 2 * triangle_wave(y_t);

            alpha_x = Sign(alpha_x);
            alpha_y = Sign(alpha_y);

            double alpha = alpha_x*alpha_y;

            return Max(Min(alpha, 1), 0);
        }

        static double pattern_3(int x, int y)
        {
            int x_c = x_res / 2;
            int y_c = y_res / 2;

            double x_t = x - x_c;
            double y_t = y - y_c;

            transform(ref x_t, ref y_t);

            double alpha_x = Abs(4 * triangle_wave(x_t)) - 1;
            double alpha_y = Abs(4 * triangle_wave(y_t)) - 1;

            alpha_x = Sign(alpha_x);
            alpha_y = Sign(alpha_y);

            double alpha = (Max(alpha_x, 0)*Max(alpha_y, 0));

           return Max(Min(alpha, 1), 0);
        }

        static double pattern_4(int x, int y)
        {
            int x_c = x_res / 2;
            int y_c = y_res / 2;

            double x_t = x - x_c;
            double y_t = y - y_c;

            transform(ref x_t, ref y_t);

            double d = Sqrt(y_t * y_t + x_t * x_t);

            double a = 500;

            double alpha = 1.5 * triangle_wave(a/(d+1));

            alpha = Sign(alpha);

            return Max(Min(alpha, 1), 0);
        }

        static double pattern_5(int x, int y)
        {
            int x_c = x_res / 2;
            int y_c = y_res / 2;

            double x_t = x - x_c;
            double y_t = y - y_c;

            transform(ref x_t, ref y_t);

            double alpha = 2 * Abs(Sin(0.1 * x_t) * Cos(0.1 * y_t)) - 1;

            alpha = Sign(alpha);

            return Max(Min(alpha, 1), 0);
        }

        static double pattern_6(int x, int y)
        {
            int a = 4;
            int column_width = x_res / a;
            int row_height = y_res / a;

            int column_nmbr = x / column_width;
            int row_nmbr = y / row_height;

            int x_c = x_res / (2 * a) + column_width * column_nmbr;
            int y_c = y_res / (2 * a) + row_height * row_nmbr;

            double x_t = x - x_c;
            double y_t = y - y_c;

            transform(ref x_t, ref y_t);

            double d = Sqrt(y_t * y_t + x_t * x_t);

            double alpha = 1.5 * triangle_wave(d);

            alpha = Sign(alpha);

            return Max(Min(alpha, 1), 0);
        }

        static double pattern_7(int x, int y)
        {
            int x_c = x_res / 2;
            int y_c = y_res / 2;

            double x_t = x - x_c;
            double y_t = y - y_c;

            transform(ref x_t, ref y_t);

            double d = Atan2(y_t, x_t);

            int a = 4;

            double alpha = 2 * triangle_wave(a * 2 * PI * d);

            alpha = Sign(alpha);

            return Max(Min(alpha, 1), 0);
        }

        static double pattern_8(int x, int y)
        {
            int x_c = x_res / 2;
            int y_c = y_res / 2;

            double x_t = x - x_c;
            double y_t = y - y_c;

            transform(ref x_t, ref y_t);

            double d_1 = Atan2(y_t, x_t);
            double d_2 = Sqrt(y_t * y_t + x_t * x_t);

            int a = 4;

            double alpha = 2 * triangle_wave(a * 2 * PI * d_1) * triangle_wave(d_2);

            alpha = Sign(alpha);

            return Max(Min(alpha, 1), 0);
        }
    }
}
