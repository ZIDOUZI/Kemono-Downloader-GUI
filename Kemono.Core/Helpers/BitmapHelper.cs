using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

namespace Kemono.Core.Helpers;

public static class BitmapHelper
{
    public static int CompareDHash(string file, string other) =>
        CompareDHash(new Bitmap(File.OpenRead(file)), new Bitmap(File.OpenRead(other)));

    public static byte[] GetByteArray(this Bitmap bitmap, ImageFormat format = default)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream, format ?? ImageFormat.Png);
        return stream.GetBuffer();
    }

    public static Stream GetStream(this Bitmap bitmap, ImageFormat format = default)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream, format ?? ImageFormat.Png);
        stream.Position = 0;
        return stream;
    }

    public static Bitmap FromStream(this Stream stream)
    {
        using (stream)
        {
            return new Bitmap(stream);
        }
    }

    public static Bitmap FromByteArray(this byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return new Bitmap(stream);
    }

    public static int CompareDHash(Bitmap img1, Bitmap img2, int width = 8, int height = 8)
    {
        var hash1 = DHash(img1, width, height);
        // img1.Dispose();
        var hash2 = DHash(img2, width, height);
        // img2.Dispose();
        return Hamming(hash1, hash2);
    }

    public static ulong DHash(Bitmap img)
    {
        var k = img.ResizeImage(new Size(9, 8)).GreyScaling().Matrix();
        var dif = DifferenceMatrix(k);
        var bits = SetOfBitsDHash(dif);
        return Hash(bits);
    }

    public static BitArray DHash(Bitmap img, int width = 8, int height = 8)
    {
        var k = img.ResizeImage(new Size(width + 1, height)).GreyScaling().Matrix();
        var dif = DifferenceMatrix(k, width, height);
        return SetOfBitsDHash(dif, width, height);
    }

    private static Bitmap ResizeImage(this Image imgToResize, Size size) => new(imgToResize, size);

    private static Bitmap GreyScaling(this Bitmap c)
    {
        var d = new Bitmap(c.Width, c.Height);
        for (var i = 0; i < c.Width; i++)
        for (var j = 0; j < c.Height; j++)
        {
            var oc = c.GetPixel(i, j);
            var grayScale = (int)(oc.R * 0.3 + oc.G * 0.59 + oc.B * 0.11);
            var nc = Color.FromArgb(oc.A, grayScale, grayScale, grayScale);
            d.SetPixel(i, j, nc);
        }

        return d;
    }

    private static double[,] Matrix(this Bitmap b)
    {
        var clrs = new Color[b.Height, b.Width];
        var d = new double[b.Height, b.Width];
        for (var i = 0; i < b.Height; i++)
        for (var j = 0; j < b.Width; j++)
        {
            clrs[i, j] = b.GetPixel(j, i);
            d[i, j] = clrs[i, j].B;
        }

        return d;
    }

    private static double[,] DifferenceMatrix(double[,] d, int width = 8, int height = 8)
    {
        var dif = new double[width, height];
        for (var i = 0; i < width; i++)
        for (var j = 0; j < height; j++)
        {
            dif[i, j] = d[i, j + 1] - d[i, j];
        }

        return dif;
    }

    private static BitArray SetOfBitsDHash(double[,] y, int width = 8, int height = 8)
    {
        var arr = new BitArray(width * height);
        var k = 0;
        for (var i = 0; i < width; i++)
        for (var j = 0; j < height; j++)
        {
            arr[k++] = y[i, j] > 0;
        }

        return arr;
    }

    private static ulong Hash(BitArray arr)
    {
        var i = 0ul;
        foreach (bool b in arr)
        {
            i <<= 1;
            if (b)
            {
                i++;
            }
        }

        return i;
    }

    public static int Hamming(ulong x, ulong y)
    {
        var dist = 0;
        var val = x ^ y;

        // Count the number of bits set
        while (val != 0)
        {
            // A bit is set, so increment the count and clear the bit
            dist++;
            val &= val - 1;
        }

        // Return the number of differing bits
        return dist;
    }

    private static int Hamming(BitArray x, BitArray y)
    {
        var r = 0;
        switch (x.Length)
        {
            case < 64:
            {
                var x0 = new byte[8];
                x.CopyTo(x0, 0);
                var x1 = BitConverter.ToInt64(x0);
                y.CopyTo(x0, 0);
                var y1 = BitConverter.ToInt64(x0);
                var xor = x1 ^ y1;
                while (xor != 0)
                {
                    xor &= xor - 1;
                    r++;
                }

                break;
            }
            case < 128:
                for (var i = 0; i < x.Length; i++)
                {
                    if (x[i] ^ y[i])
                    {
                        r++;
                    }
                }

                break;
        }

        return r;
    }
}