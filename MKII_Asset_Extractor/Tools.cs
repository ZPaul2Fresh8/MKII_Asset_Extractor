using System;
using System.Collections.Generic;
using SkiaSharp;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using static MKII_Asset_Extractor.Extract2;

namespace MKII_Asset_Extractor
{
    public class Header
    {
        public int loc { get; set; }
        public short width { get; set; }
        public short height { get; set; }
        public short offsetx { get; set; }
        public short offsety { get; set; }
        public uint gfxloc { get; set; }
        public int draw_att { get; set; }
        public uint palloc { get; set; }
    }

    public class Stage
    {
        List<List<SKColor>> Palettes = new List<List<SKColor>>();

        public static void Process_Module(int loc)
        {

        }

    }


    public static class Tools
    {
        public static bool Is_Bit_Set(int value, int bit)
        {
            return ((value & (1<< bit)) != 0);
        }

        /// <summary>
        /// Returns a word (16-bit value) from provided rom location.
        /// </summary>
        /// <param name="ROM Location"></param>
        /// <returns></returns>
        public static short Get_Word(int rom_loc)
        {
            short value = (short)(Globals.PRG[rom_loc] << 8 | Globals.PRG[rom_loc + 1]);
            return value;
        }

        /// <summary>
        /// Returns a long (32-bit value) from provided rom location.
        /// </summary>
        /// <param name="rom_loc"></param>
        /// <returns></returns>
        public static uint Get_Long(int rom_loc)
        {
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

        public static List<byte> Bytes_To_Bits(List<byte> data)
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

        // reverse bytes to comply
        public static List<byte> reverse_bytes(List<byte> bytes)
        {
            var bytelist = new List<byte>();

            // trim last byte if uneven
            if ((bytes.Count & 1) != 0) bytes.RemoveAt(bytes.Count - 1);

            for (int b = 0; b < bytes.Count / 2; b += 2)
            {
                bytelist.Add(bytes[b + 1]); bytelist.Add(bytes[b]);
            }
            return bytelist;

        }

        public static List<bool> ConvertBytesToBoolArray(List<byte> bytes)
        {
            var bools = new List<bool>();

            foreach (byte b in bytes)
            {
                bools.AddRange(ByteToBoolArray(b));
            }
            return bools;
        }

        public static bool[] ByteToBoolArray(byte b)
        {
            // prepare the return result
            bool[] result = new bool[8];

            // check each bit in the byte. if 1 set to true, if 0 set to false
            for (int i = 0; i < 8; i++)
                result[i] = (b & (1 << i)) != 0;

            // reverse the array
            //Array.Reverse(result);

            return result;
        }

        public static byte BoolArrayToByte(bool[] bools)
        {
            byte result = 0;
            Array.Reverse(bools);

            // This assumes the array never contains more than 8 elements!
            int index = 8 - bools.Length;

            // Loop through the array
            foreach (bool b in bools)
            {
                // if the element is 'true' set the bit at that position
                if (b)
                    result |= (byte)(1 << (7 - index));

                index++;
            }
            return result;
        }

        public static uint RotateLeft(this uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        public static uint RotateRight(this uint value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }

        /// <summary>
        /// Converts a byte array(bit sized) into a byte.
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static int BitArray_To_Byte(List<byte> bits)
        {
            int new_byte = 0;
            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i] == 1)
                {
                    new_byte |= (1 << i);
                }
            }
            return new_byte;
        }

        public static int Convert_To_Palette_Index(List<byte> bits)
        {
            int byte_value = 0;
            for (int i = 0; i < bits.Count; i++)
            {
                byte_value = (byte_value << 1) | bits[bits.Count - 1 - i];
            }
            return byte_value;
        }

        public static Header Build_Header(int location)
        {
            if (location < 0)
            {
                location = (location / 8) & 0xfffff;
            }
            
            Header header = new Header
            {
                loc = location,
                width = Get_Word(location),
                height = Get_Word(location+2),
                offsetx = (short)(Get_Word(location+4)*-1),
                offsety = (short)(Get_Word(location+6)*-1),
                gfxloc =Get_Long(location+8),
                draw_att = Get_Word(location+12),
                palloc = Get_Long(location + 14)
            };

            // validate header palloc is a legitimate pallette
            if(header.palloc < 0xff800000)
            {
                header.palloc = 0;
            }

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
            palette.Add(SKColors.White);

            // CREATE BIT ARRAY
            List<byte> bits = Tools.Bytes_To_Bits(data);

            // CREATE IMAGE
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

        /// <summary>
        /// Used for drawing bitmaps from ROM file.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="create_palette"></param>
        /// <returns></returns>
        public static SKBitmap Draw_Image(Header header)
        {
            
            //var header = Tools.Build_Header(location);
            int location = header.loc;

            if (header.width > 0xff || header.height > 0xff)
            {
                Console.WriteLine($"Header at {location} dimensions exceeded logical expectations.");
                return null;
            }
            if (header.width <= 0 || header.height <= 0)
            {
                Console.WriteLine($"Header at {location} dimensions didn't meet logical expectations.");
                return null;
            }

            int bpp = (int)((header.draw_att & 0xffff) >> 0xc);
            uint gfx_start = (uint)((header.gfxloc - (header.gfxloc % 8)) / 8);
            uint gfx_end = (uint)(gfx_start + ((header.width * header.height * bpp) + (header.gfxloc % 8) / 8));
            List<byte> data = Globals.GFX.Skip((int)gfx_start).Take((int)(gfx_end - gfx_start)).ToList();

            // Add to TOTAL_BYTES_EXTRACTED
            Globals.GFX_BYTES_EXTRACTED += data.Count;

            // CREATE BIT ARRAY
            var bits = Tools.Bytes_To_Bits(data);

            // TRIM STARTING OFFSET
            bits.RemoveRange(0, (int)(header.gfxloc % 8));
            if (bits.Count == 0)
            {
                Console.WriteLine("Problem. GFX bits are empty.");
                return null;
            }

            // CREATE IMAGE
            if (header.palloc != 0)
            {
                return Fill_Pixels((int)header.width, (int)header.height, bits, Converters.Convert_Palette((int)(header.palloc / 8) & 0xfffff), (int)header.draw_att);
            }
            else
            {
                return Fill_Pixels((int)header.width, (int)header.height, bits, Globals.PALETTE, (int)header.draw_att);
            }
        }

        /// <summary>
        /// Used for drawing bitmaps from MKHeaders in memory from TBL files.
        /// </summary>
        /// <param name="header2"></param>
        /// <returns></returns>
        public static SKBitmap Draw_Image2(Extract2.MKHeader header2)
        {

            if (header2.Width > 0x190 || header2.Height > 0xff)
            {
                Console.WriteLine($"Header {header2.Name} dimensions exceeded logical expectations.");
                return null;
            }
            if (header2.Width <= 0 || header2.Height <= 0)
            {
                Console.WriteLine($"Header {header2.Name} dimensions didn't meet logical expectations.");
                return null;
            }

            int bpp = (int)((header2.DMA & 0xffff) >> 0xc);

            if (bpp == 0) { return null; }

            uint gfx_start = (uint)((header2.GFXLocation - (header2.GFXLocation % 8)) / 8);
            uint gfx_end = (uint)(gfx_start + ((header2.Width * header2.Height * bpp) + (header2.GFXLocation % 8) / 8));
            List<byte> data = Globals.GFX.Skip((int)gfx_start).Take((int)(gfx_end - gfx_start)).ToList();

            // Add to TOTAL_BYTES_EXTRACTED
            Globals.GFX_BYTES_EXTRACTED += data.Count;

            // CREATE BIT ARRAY
            var bits = Tools.Bytes_To_Bits(data);

            // TRIM STARTING OFFSET
            bits.RemoveRange(0, (int)(header2.GFXLocation % 8));
            if (bits.Count == 0)
            {
                Console.WriteLine("Problem. GFX bits are empty.");
                return null;
            }

            // CREATE IMAGE
            return Fill_Pixels((int)header2.Width, (int)header2.Height, bits, header2.MK_Pal.Colors, (int)header2.DMA);
        }

        static SKBitmap Fill_Pixels(int width, int height, List<byte> bits, List<SKColor> palette, int draw_att)
        {
            #region DRAW ATTTRIBUTE BITS
            // FEDC BA98 7654 3210
            // |___________________ DMA GO / DMA HALT
            //  |__________________ BIT DEPTH
            //   |_________________ BIT DEPTH
            //    |________________ BIT DEPTH
            //
            //      |______________ DMA COMPRESS TRAIL PIX MULTIPLIER BIT 1
            //       |_____________ DMA COMPRESS TRAIL PIX MULTIPLIER BIT 0
            //        |____________ DMA COMPRESS LEAD  PIX MULTIPLIER BIT 1
            //         |___________ DMA COMPRESS LEAD  PIX MULTIPLIER BIT 0
            //
            //           |_________ COMPRESSION ENABLED
            //            |________ DMA CLIP ON = 1 (USING U,D,L,R METHOD)
            //             |_______ FLAG - Y DRAW START (0=TOP; 1=BOTTOM) - PBV - VFL
            //              |______ FLAG - X DRAW START (0=LEFT; 1=RIGHT) - PBH - HFL
            //
            //                |____ PIXEL CONSTANT/SUBSTITUTION OPS - NOT USED IN MK (blit nonzero pixels as color)
            //                 |___ PIXEL CONSTANT/SUBSTITUTION OPS - NOT USED IN MK (blit zero pixel as color)
            //                  |__ PIXEL CONSTANT/SUBSTITUTION OPS - NOT USED IN MK (blit nonzero pixels)
            //                   |_ PIXEL CONSTANT/SUBSTITUTION OPS - NOT USED IN MK (blit zero pixels)
            #endregion

            SKBitmap bitmap = new(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            int bpp = (draw_att & 0xffff) >> 0xc;
            int pixel = 0;

            for (int y = 0; y < height; y++)
            {
                int cmp_leading = 0;
                int cmp_trailing = 0;
                if (Is_Sprite_Compressed(draw_att))
                {
                    // GET COMPRESSION BYTE
                    int cmp_byte = Tools.BitArray_To_Byte(bits.GetRange(0, 8));

                    // TRIM OFF COMPRESSION BYTE
                    bits.RemoveRange(0, 8);

                    cmp_leading = cmp_byte & 0xf;
                    cmp_trailing = cmp_byte >> 4;

                    // LEADING BLANK PIXEL MULTIPLICATION
                    if(Tools.Is_Bit_Set(draw_att, 8))
                    {
                        if(Tools.Is_Bit_Set(draw_att, 9))
                        {
                            cmp_leading *= 8;
                        }
                        else { cmp_leading *= 2; }
                    }
                    else 
                    {
                        if (Tools.Is_Bit_Set(draw_att, 9))
                        {
                            cmp_leading *= 4;
                        }                            
                    }

                    // TRAILING BLANK PIXEL MULTIPLICATION
                    if(Tools.Is_Bit_Set(draw_att, 10))
                    {
                        if(Tools.Is_Bit_Set(draw_att, 11))
                        {
                            cmp_trailing *= 8;
                        }
                        else { cmp_trailing *= 2;}
                    }
                    else
                    {
                        if (Tools.Is_Bit_Set(draw_att, 11))
                        {
                            cmp_trailing *= 4;
                        }
                    }
                }

                for (int x = 0; x < width; x++)
                {
                    var slice = bits.GetRange(0, bpp);
                    int index = Tools.Convert_To_Palette_Index(slice);

                    if((x < cmp_leading) || (x >= (width - cmp_trailing)))
                    {
                        // PIXEL IS COMPRESSED, SET TRANSPARENT
                        // DEBUG SHOW COMPRESSED PIXELS
                        // bitmap.SetPixel(x, y, SKColors.Green);
                        index = 0;
                        continue;
                    }
                    // ONLY REMOVE PIXEL DATA IF NOT COMPRESSED
                    else { bits.RemoveRange(0, bpp); }

                    if (index < palette.Count) { bitmap.SetPixel(x, y, palette[index]); }

                    pixel += 1;
                }
            }
            return bitmap;
        }

        static bool Is_Sprite_Compressed(int draw_att)
        {
            #region MULTIPLYING OF COMPRESSED 0-PIXELS
            // 11   10   09   08  ->   BIT
            // TM1  TM0  LM1  LM0       MULTIPLIER
            // -----------------------------------
            // X    X    0    0       X 1 TO LEADING PIXELS
            // X    X    0    1       X 2 TO LEADING PIXELS
            // X    X    1    0       X 4 TO LEADING PIXELS
            // X    X    1    1       X 8 TO LEADING PIXELS
            // 0    0    X    X       X 1 TO TRAILING PIXELS
            // 0    1    X    X       X 2 TO TRAILING PIXELS
            // 1    0    X    X       X 4 TO TRAILING PIXELS
            // 1    1    X    X       X 8 TO TRAILING PIXELS
            #endregion
            return Tools.Is_Bit_Set(draw_att, 7);
        }

        public static SKImage ParseSegments(List<SKBitmap> segs, List<Header> headers)
        {
            int left = 0, right = 0, top = 255, bottom = 0, width = 0, height = 0;

            for (int s = 0; s < segs.Count; s++)
            {
                // find left
                if (headers[s].offsetx < left) { left = headers[s].offsetx; }

                // find right
                if (headers[s].width + headers[s].offsetx > right)
                {
                    right = headers[s].width + headers[s].offsetx;
                }

                // find top
                if (headers[s].offsety < top) { top = headers[s].offsety;}

                // find bottom
                if (headers[s].height + headers[s].offsety > bottom)
                {
                    bottom = headers[s].height + headers[s].offsety;
                }
            }

            // set new bitmap dimensions
            width = right - left;
            height = bottom - top;

            var image_info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            var surface = SKSurface.Create(image_info);
           
            // take each bitmap in the list and put those pixels into position on new bitmap
            for (int b = 0; b < segs.Count; b++)
            {
                int y = headers[b].offsety - top;
                int x = headers[b].offsetx - left;
                surface.Canvas.DrawBitmap(segs[b], x, y);
            }
            return surface.Snapshot();
        }
    }

    public static class Converters
    {
        /// <summary>
        /// Given a palette location, it will return a list of colors.
        /// </summary>
        /// <param name="Palette Location"></param>
        /// <returns></returns>
        public static List<SKColor>Convert_Palette(int pal_loc)
        {
            var palette = new List<SKColor>();
            int size = Tools.Get_Word(pal_loc);
            var slice = Globals.PRG.Skip(pal_loc + 2).Take(pal_loc + 2 + (size*2));

            for (int c = 0; c < size*2; c+=2)
            {
                // if color index == 0 and color is 0x0000 then make it transparent
                if (c == 0)
                {
                    // Some sprites have a color that isn't 0 and should be transparent!
                    //if (Tools.Get_Word(pal_loc + 2 + c) == 0)
                    {
                        palette.Add(SKColors.Transparent);
                        continue;
                    }
                }

                int word = Tools.Get_Word(pal_loc + 2 + c);
                palette.Add(Convert_Color(word));
            }
            Globals.PALETTE = palette;
            return palette;
        }

        /// <summary>
        /// Given a list of bytes, it will return a list of colors and remove those bytes
        /// from the list provided.
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public static Tuple<List<SKColor>, List<byte>> Convert_Palette(List<byte> chunk)
        {
            var palette = new List<SKColor>();
            int size = (chunk[0] << 8 | chunk[1]) *2; chunk.RemoveRange(0, 2);
            var slice = chunk.Take(size).ToList(); chunk.RemoveRange(0, slice.Count);

            for (int c = 0; c < size; c += 2)
            {
                // if color index == 0 and color is 0x0000 then make it transparent
                if (c == 0)
                {
                    if ((slice[0] << 8 | slice[1]) == 0)
                    {
                        palette.Add(SKColors.Transparent);
                        slice.RemoveRange(0, 2);
                        continue;
                    }
                }

                int color = slice[0] << 8 | slice[1];
                slice.RemoveRange(0, 2);
                palette.Add(Convert_Color(color));
            }
            return Tuple.Create(palette, chunk);
        }

        public static SKColor Convert_Color(int color_555)
        {
            int red = (color_555 >> 10) & 0x1f;
            int green = (color_555 >> 5) & 0x1f;
            int blue = color_555 & 0x1f;

            red = (red << 3) | (red >> 2);
            green = (green << 3) | (green >> 2);
            blue = (blue << 3) | (blue >> 2);

            return new SKColor((byte)red, (byte)green, (byte)blue, 255);
        }
    }
}
