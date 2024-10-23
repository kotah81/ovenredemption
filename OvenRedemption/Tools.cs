using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static System.Runtime.InteropServices.JavaScript.JSType;
#pragma warning disable CS8603 
namespace OvenRedemption
{
    public static class Tools
    {
        public static Brush HexBrush(string B)
        {
            return new BrushConverter().ConvertFrom(B) as Brush;
        }

        public static void ClearFolder(string Path)
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
            else
            {
                var Dir = new DirectoryInfo(Path) { Attributes = FileAttributes.Normal };

                foreach (var Info in Dir.GetFileSystemInfos("*", SearchOption.AllDirectories))
                    Info.Attributes = FileAttributes.Normal;


                Dir.Delete(true);
                Directory.CreateDirectory(Path);
            }
        }

      

        public static float Approach(float a, float b, float amount)
        {
            if (a == b) return b;
            if (a < b)
            {
                a += amount;
                if (a > b)
                    return b;
            }
            else
            {
                a -= amount;
                if (a < b)
                    return b;
            }
            return a;
        }
    }
}
