using System;
using System.Collections.Generic;
//using System.Drawing;
//using System.Drawing.Imaging;
using SkiaSharp;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

namespace MKII_Asset_Extractor
{
    public static class Tools
    {
        /// <summary>
        /// Returns a word (16-bit value) from provided rom location.
        /// </summary>
        /// <param name="ROM Location"></param>
        /// <returns></returns>
        public static int Get_Word(int rom_loc)
        {

            // convert the address if format was given as game address
            if (rom_loc > 0xfffff)
            {
                rom_loc = (rom_loc / 8) & 0xfffff;
            }

            var value = Globals.PRG[rom_loc] << 8 | Globals.PRG[rom_loc + 1];

            if (value > 0x7fff)
            {
                // turn into negative
                value -= 0x10000;
            }

            return value;
        }

        /// <summary>
        /// Returns a long (32-bit value) from provided rom location.
        /// </summary>
        /// <param name="rom_loc"></param>
        /// <returns></returns>
        public static uint Get_Long(int rom_loc)
        {
            // convert the address if format was given as game address.
            if (rom_loc > 0xfffff)
            {
                rom_loc = (rom_loc / 8) & 0xfffff;
            }

            var word1 = Globals.PRG[rom_loc] << 8 | Globals.PRG[rom_loc + 1];
            var word2 = Globals.PRG[rom_loc + 2] << 8 | Globals.PRG[rom_loc + 3];
            return (uint)((word2 << 16) | word1);
        }

        /// <summary>
        /// Returns a rom location pointer from a 4-byte game address located at provided rom loc
        /// </summary>
        /// <param name="ROM Location"></param>
        /// <returns></returns>
        public static int Get_Pointer(int rom_loc)
        {
            return (int)((Get_Long(rom_loc) / 8) & 0xfffff);
        }

        public static List<byte> Bits_To_Bytes(List<byte> data)
        {
            var bits = new List<byte>();

            for (int w = 0; w < data.Count;w++)
            {
                var byt = data[w];

                for (int b = 0; b < 8; b++)
                {
                    var bit = (byt >> b) & 1;
                    bits.Add((byte)bit);
                }
                    
            }
            return bits;
        }

        public static List<int> Build_Header(int location)
        {
            var header = new List<int>();

            // 0 - add location for naming/ref pruposes
            header.Add(location);
            // 1 - width
            header.Add(Get_Word(location));
            // 2 - height
            header.Add(Get_Word(location+2));
            // 3 - x offset
            header.Add(Get_Word(location + 4));
            // 4 - y offset
            header.Add(Get_Word(location + 6));
            // 5 - gfx location
            header.Add(Get_Word(location + 8));
            // 6 - draw attribute
            header.Add(Get_Word(location + 12));
            // 7 - palette pointer (if applicable)
            header.Add(Get_Word(location + 14));

            return header;
        }
    }

    public static class Imaging
    {
        public static SKBitmap Draw_Font_Small(int header_loc)
        {
            var width = Tools.Get_Word(header_loc);
            var height = Tools.Get_Word(header_loc + 2);
            var data_ptr = Tools.Get_Pointer(header_loc + 4);
            var byte_width = 1;
            
            // calc char byte size
            if (width > 8)
            {
                byte_width = 2;
                width = 16;
            }
            else
            {
                width = 8;
            }

            // GET DATA CHUNK
            var data = Globals.PRG.Skip(data_ptr).Take(byte_width * height).ToList();

            // REVERSE BYTE ORDER
            var temp_data = new List<byte>();
            for (int i = 0; i < data.Count; i += 2)
            {
                temp_data.Add(data[i + 1]);
                temp_data.Add(data[i]);
            }
            data = temp_data;

            // RED PALETTE
            List <SKColor> palette = new List<SKColor>();
            palette.Add(SKColors.Transparent);
            palette.Add(SKColors.DarkRed);

            // CREATE BIT ARRAY
            List<byte> bits = Tools.Bits_To_Bytes(data);

            // CREATE IMAGE
            //SKBitmap bitmap = new(width, height, SKColorType.Alpha16, SKAlphaType.Unpremul);
            SKBitmap bitmap = new(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            int pixel = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bitmap.SetPixel(x, y, palette[bits[pixel]]);
                    pixel++;
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Returns true if Frame Pointer is multi-segmented
        /// </summary>
        /// <param name="rom_loc"></param>
        /// <returns></returns>
        public static bool Is_Frame_MultiSegmented(int rom_loc)
        {
            // checks to see if whether the 1st frame in the animation frame
            // array points to a frame segment(true) or to a complete frame(false)
            // expected rom_loc value is animation frame array.
            int pointer = Tools.Get_Pointer(rom_loc);
            uint dword = Tools.Get_Long(pointer);

            if (dword < 0xff800000) { return false; }
            if ((dword & 0xf) != 0 ) { return false; }
            return true;
        }

        public static SKBitmap Draw_Image(int location, bool create_palette)
        {
            var header = Tools.Build_Header(location);

            if (header[1] > 0xff || header[2] > 0xff)
            {
                Console.WriteLine($"Header at {location} dimensions exceeded logical expectations.");
            }
            if (header[1] <= 0 || header[2] <= 0)
            {
                Console.WriteLine($"Header at {location} dimensions didn't meet logical expectations.");
            }
        }
    }

    public static class Converters
    {
        public static List<SKColor> Convert_Palette(int pal_loc)
        {
            var palette = new List<SKColor>();
            int size = Tools.Get_Word(pal_loc);
            var slice = Globals.PRG.Skip(pal_loc + 2).Take(pal_loc + 2 + (size*2));

            for (int c = 0; c < size*2; c+=2)
            {
                if(c==0)
                {
                    palette.Add(SKColors.Transparent);
                    continue;
                }
                
                int word = Tools.Get_Word(pal_loc + 2 + c);
                palette.Add(Convert_Color(word));
            }
            return palette;
        }

        static SKColor Convert_Color(int color_555)
        {
            int red = (color_555 >> 10) & 0x1f;
            int green = (color_555 >> 5) & 0x1f;
            int blue = color_555 & 0x1f;

            //red = (red << 3) | (red >> 2)
            //green = (green << 3) | (green >> 2)
            //blue = (blue << 3) | (blue >> 2)

            return new SKColor((byte)red, (byte)green, (byte)blue, 255);
        }
    }
}
