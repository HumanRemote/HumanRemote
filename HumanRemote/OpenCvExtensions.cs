using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using OpenCvSharp;

namespace HumanRemote
{
    static class OpenCvExtensions
    {
        public static byte GetByte(this IplImage image, int x = 0, int y = 0, int z = 0)
        {
            int offset = GetIndex(image, x, y, z);
            return Marshal.ReadByte(image.ImageData, offset);
        }

        public static void SetByte(this IplImage image, int x, int y, byte value)
        {
            SetByte(image, x, y, 0, value);
        }
        public static void SetByte(this IplImage image, int x, int y, int z, byte value)
        {
            int offset = GetIndex(image, x, y, z);
            Marshal.WriteByte(image.ImageData, offset, value);
        }

        private static int GetIndex(IplImage image, int x = 0, int y = 0, int z = 0)
        {
            return (y*image.WidthStep) + (x*image.NChannels) + z;
        }
    }
}
