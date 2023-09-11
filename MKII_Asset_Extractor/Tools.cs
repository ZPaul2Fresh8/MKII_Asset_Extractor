using System;
using System.Collections.Generic;
using SkiaSharp;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

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

            // convert the address if format was given as game address
            if (rom_loc > 0xfffff)
            {
                rom_loc = (rom_loc / 8) & 0xfffff;
            }

            short value = (short)(Globals.PRG[rom_loc] << 8 | Globals.PRG[rom_loc + 1]);

            //if (value > 0x7fff)
            //{
                // turn into negative
            //    value -= 0x10000;
            //}

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

        public static SKBitmap Draw_Image(Header header, bool create_palette)
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

            int bpp = (int)(header.draw_att >> 0xc);
            uint gfx_start = (uint)((header.gfxloc - (header.gfxloc % 8)) / 8);
            uint gfx_end = (uint)(gfx_start + ((header.width * header.height * bpp) + (header.gfxloc % 8) / 8));
            List<byte> data = Globals.GFX.Skip((int)gfx_start).Take((int)(gfx_end - gfx_start)).ToList();

            // Add to TOTAL_BYTES_EXTRACTED
            Globals.GFX_BYTES_EXTRACTED += data.Count;

            // CREATE BIT ARRAY
            var bits = Tools.Bytes_To_Bits(data);

            // TRIM STARTING OFFSET
            bits.RemoveRange(0, (int)(header.gfxloc % 8));

            // CREATE IMAGE
            return Fill_Pixels((int)header.width, (int)header.height, bits, Globals.PALETTE, (int)header.draw_att);

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
            int bpp = draw_att >> 0xc;
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
                        //bitmap.SetPixel(x, y, SKColors.Green);
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

            red = (red << 3) | (red >> 2);
            green = (green << 3) | (green >> 2);
            blue = (blue << 3) | (blue >> 2);

            return new SKColor((byte)red, (byte)green, (byte)blue, 255);
        }
    }
}
