// See https://aka.ms/new-console-template for more information
using System.Linq;
using System;
using System.Runtime.CompilerServices;
using MKII_Asset_Extractor;
using SkiaSharp;
using System.Reflection.Metadata;

Console.WriteLine("[MKII ASSET EXTRACTOR]");
Thread.Sleep(500);
ListOptions();

while (true)
{
    ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); // Read a key input without displaying it

    switch (keyInfo.KeyChar)
    {
        case (char)ConsoleKey.NumPad1:
            ExtractImageHeaders();
            Console.WriteLine("Done. Header listings output @ gfx_headers.txt");
            ListOptions();
            break;

        case (char)ConsoleKey.NumPad2:
            Console.WriteLine("\nCreating Images From Headers...");
            CreateImages();
            ListOptions();
            break;

        case (char)ConsoleKey.NumPad3:
            Console.WriteLine("\nExtracting Sounds...");
            ExtractSounds();
            ListOptions();
            break;

        case (char)ConsoleKey.NumPad4:
            Console.WriteLine("\nInterleaving Program Roms...");
            Interleave_PRG();
            ListOptions();
            break;

        case (char)ConsoleKey.NumPad5:
            Console.WriteLine("\nInterleaving Graphic Roms...");
            Interleave_GFX();
            ListOptions();
            break;

        case (char)ConsoleKey.NumPad6:
            Console.WriteLine("\nExtracting Fonts...");
            Extract_Fonts();
            ListOptions();
            break;

        case (char)ConsoleKey.NumPad7:
            Console.WriteLine("\nExtracting Animations...");
            Extract_Animations();
            ListOptions();
            break;

        case (char)ConsoleKey.NumPad8:
            Console.WriteLine("\nExtracting Assets...");
            Extract_Fonts();
            Extract_Animations();
            ListOptions();
            break;

        case (char)ConsoleKey.NumPad9:
            Console.WriteLine("\nExtracting Sprite Lists...");
            Extract_Sprite_Lists();
            ListOptions();
            break;

        case (char)ConsoleKey.P:
            Console.WriteLine("\nExtracting Assets From Source...");
            List<Extract2.MKHeader> SRC_Headers = Extract2.ReadHeadersIntoMemory(Extract2.ReadPalettesIntoMemory());
            Extract_Sprites_Src(SRC_Headers);
            ListOptions();
            break;

        case (char)ConsoleKey.M:
            
            // FIRST TEST = BP FF8c6040 - Lao Slice
            // animation = ff8ce070

            if (!PRG_Check()) { return; }
            if (!GFX_Check()) { return; }

            Console.WriteLine("\nInput Arguments: \t(string)FOLDER NAME|(string)NAME|(int)SPRITE HDR ADDRESS|(int)PALETTE ADDRESS|(bool)MULTI-FRAME?");


            int frame_num = 0;
            string[] vars = Console.ReadLine().Split('|');
            
            ME_Table mE_Table = new ME_Table();
            mE_Table.folder = vars[0];
            mE_Table.subfolder = vars[1];
            
            string frameloc = vars[2];
            try // convert to Int
            {
                mE_Table.frameloc = Convert.ToInt32(frameloc);
            }
            catch // failed, see if hex?
            {
                try
                {
                    int i = Convert.ToInt32(frameloc, 16);
                    mE_Table.frameloc = (i / 8) & 0xfffff;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            try
            {
                // if input arguments = 3 then header has palette in it
                if (vars.Length == 3)
                {
                    // FRIENDSHIPS|Other|408098
                    mE_Table.pal_loc = Tools.Get_Pointer(mE_Table.frameloc + 14);
                }
                else
                {
                    // if pal loc is low, use value as palette array index
                    if (Convert.ToInt32(vars[3]) < 18)
                    {
                        mE_Table.pal_loc = Constants.m_palettes[Convert.ToInt32(vars[3])];
                    }
                    else
                    {
                        mE_Table.pal_loc = Convert.ToInt32(vars[3]);
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
          
            if (vars.Length > 4)
            {
                mE_Table.multi_frame = true;
            }

            Globals.PALETTE = Converters.Convert_Palette(mE_Table.pal_loc);

            // check if multi-frame
            if (mE_Table.multi_frame)
            {
                int start_loc = mE_Table.frameloc;
                
                while (Tools.Get_Long(start_loc) > 0xff800000)
                {
                    mE_Table.frameloc = start_loc;
                    save_frame(mE_Table, frame_num);
                    frame_num++;
                    start_loc += 4;
                }

                if (Tools.Get_Long(start_loc) == 1)
                {
                    // get final frame
                    start_loc += 12;
                    mE_Table.frameloc = start_loc;
                    save_frame(mE_Table, frame_num);
                    start_loc += 12;
                }
            }
            else
            {
                save_frame(mE_Table, frame_num);
                frame_num++;
            }
            break;

        case (char)ConsoleKey.Escape:
            Console.WriteLine("Exiting...");
            return; // Exit the program

        default:
            Console.WriteLine("Invalid key pressed.");
            break;
    }
}

static void save_frame (ME_Table mE_Table, int frame_num)
{
    // GET SEGMENT PTRS HERE
    int Segment = 0;
    int Seg_Num = 0;

    // VARS FOR PARSING BITMAPS AFTER PROCESSING IMAGES
    List<SKBitmap> Segs = new List<SKBitmap>();
    List<Header> Headers = new List<Header>();

    if (mE_Table.multi_frame)
    {
        Segment = Tools.Get_Pointer(mE_Table.frameloc);

        // GRAB SEGMENTS AND DRAW THEM
        while (Tools.Get_Long(Segment) != 0)
        {
            // DRAW SEGMENT
            Header header = Tools.Build_Header(Tools.Get_Pointer(Segment));

            // FOR PARSING
            Headers.Add(header);

            bool create_palette = (header.palloc != 0);
            //bool create_palette = (Seg_Num + Frame_Num + Anim_ID > 0);

            // DRAW SEGMENT
            SKBitmap bitmap = Imaging.Draw_Image(header);

            Segs.Add(bitmap);
            Segment += 4;
            Seg_Num += 1;
        }
    }
    else
    {
        // DRAW SEGMENT
        Header header = Tools.Build_Header(mE_Table.frameloc);
        header.palloc = (uint)mE_Table.pal_loc;

        // FOR PARSING
        Headers.Add(header);

        // DRAW SEGMENT
        SKBitmap bitmap = Imaging.Draw_Image(header);
        Segs.Add(bitmap);
    }

    // PARSE SEGMENTS HERE
    SKImage parsed_image = Imaging.ParseSegments(Segs, Headers);
    SKData parsed_data = parsed_image.Encode(SKEncodedImageFormat.Png, 100);

    // find left-most offset to use as x offset value
    List<Header> SortedOffsetX = Headers.OrderBy(o => o.offsetx).ToList();
    List<Header> SortedOffsetY = Headers.OrderBy(o => o.offsety).ToList();

    if (!Directory.Exists($"assets/gfx/manual_extracts/{mE_Table.folder}/{mE_Table.subfolder}"))
    {
        Directory.CreateDirectory($"assets/gfx/manual_extracts/{mE_Table.folder}/{mE_Table.subfolder}");
    }
    File.WriteAllBytes($"assets/gfx/manual_extracts/{mE_Table.folder}/{mE_Table.subfolder}/{frame_num}_Frame_{SortedOffsetX[0].offsetx}_{SortedOffsetY[0].offsety}_{parsed_image.Width}_{parsed_image.Height}_x_{(mE_Table.frameloc * 8) + 0xff800000:X8}.png", parsed_data.ToArray());
    parsed_image.Dispose();
    parsed_data.Dispose();
    Console.WriteLine($"{mE_Table.subfolder} - {mE_Table.frameloc} extracted.");

}

static void ListOptions()
{
    Console.WriteLine("\nCHOOSE AN OPTION:");
    Thread.Sleep(20);
    Console.WriteLine("1(A): Extract Image Headers.\t\t -Extract and save all bulk headers.");
    Thread.Sleep(20);
    Console.WriteLine("2(B): Create Images From Headers.\t -Create Images from extracted headers.");
    Thread.Sleep(20);
    Console.WriteLine("3(C): Extract Color Palettes.\t\t -Extract all Palettes from game.");
    Thread.Sleep(20);
    Console.WriteLine("4(D): Interleave Program ROMs.\t\t -Self-explanatory");
    Thread.Sleep(20);
    Console.WriteLine("5(E): Interleave Graphic ROMs.\t\t -Self-explanatory");
    Thread.Sleep(20);
    Console.WriteLine("6(F): Extract Fonts.\t\t\t -Extract fonts.");
    Thread.Sleep(20);
    Console.WriteLine("7(G): Extract Animations.\t\t -Extract fighter array animations.");
    Thread.Sleep(20);
    Console.WriteLine("8(H): Extract All Assets.");
    Thread.Sleep(20);
    Console.WriteLine("9(I): Extract Sprite Lists.\t\t -Extract defined sprites.Must have PAL in header.");
    Thread.Sleep(20);
    Console.WriteLine("P: Extract Sprites (SRC).\t\t -Extract sprites from provided TBL files (src)");
    Thread.Sleep(20);
    Console.WriteLine("M: Extract Sprite @Location.\t\t -Extract sprite from address.");
    Thread.Sleep(20);
    Console.WriteLine("ESC: Exit");
}

void Interleave_PRG()
{
    var ug12 = new List<byte>(File.ReadAllBytes("roms/ug12.l31"));
    var uj12 = new List<byte>(File.ReadAllBytes("roms/uj12.l31"));
    Globals.PRG.Clear();
   
    for(int i=0; i < ug12.Count; i++)
    {
        Globals.PRG.Add(ug12[i]);
        Globals.PRG.Add(uj12[i]);
    }

    File.WriteAllBytes(Globals.FILE_PROGRAM, Globals.PRG.ToArray());

    MSG_Success();
}

static void Interleave_GFX()
{
    var ug14 = new List<byte>(File.ReadAllBytes("roms/ug14-vid"));
    var uj14 = new List<byte>(File.ReadAllBytes("roms/uj14-vid"));
    var ug19 = new List<byte>(File.ReadAllBytes("roms/ug19-vid"));
    var uj19 = new List<byte>(File.ReadAllBytes("roms/uj19-vid"));

    var ug16 = new List<byte>(File.ReadAllBytes("roms/ug16-vid"));
    var uj16 = new List<byte>(File.ReadAllBytes("roms/uj16-vid"));
    var ug20 = new List<byte>(File.ReadAllBytes("roms/ug20-vid"));
    var uj20 = new List<byte>(File.ReadAllBytes("roms/uj20-vid"));

    var ug17 = new List<byte>(File.ReadAllBytes("roms/ug17-vid"));
    var uj17 = new List<byte>(File.ReadAllBytes("roms/uj17-vid"));
    var ug22 = new List<byte>(File.ReadAllBytes("roms/ug22-vid"));
    var uj22 = new List<byte>(File.ReadAllBytes("roms/uj22-vid"));

    Globals.GFX.Clear();

    // bank 1
    for (int i = 0; i < ug14.Count; i++)
    {
        Globals.GFX.Add(ug14[i]);
        Globals.GFX.Add(uj14[i]);
        Globals.GFX.Add(ug19[i]);
        Globals.GFX.Add(uj19[i]);
    }

    // bank 2
    for (int i = 0; i < ug14.Count; i++)
    {
        Globals.GFX.Add(ug16[i]);
        Globals.GFX.Add(uj16[i]);
        Globals.GFX.Add(ug20[i]);
        Globals.GFX.Add(uj20[i]);
    }

    // bank 3
    for (int i = 0; i < ug14.Count; i++)
    {
        Globals.GFX.Add(ug17[i]);
        Globals.GFX.Add(uj17[i]);
        Globals.GFX.Add(ug22[i]);
        Globals.GFX.Add(uj22[i]);
    }

    File.WriteAllBytes(Globals.FILE_GRAPHICS, Globals.GFX.ToArray());

    MSG_Success();
}

static void ExtractImageHeaders()
{
    if (!File.Exists(Globals.FILE_HEADERS))
    {
        FileStream fileStream = File.Create(Globals.FILE_HEADERS);
        fileStream.Close();
    }
        
    Console.WriteLine("\nSearching for GFX Headers...\n");
    Thread.Sleep(500);
    Console.WriteLine("ROM  |W   |H   |XOFF|YOFF|GFX LOC |DMA |PALETTE\n" +
      "------------------------------------------------");

    if (!PRG_Check()) { return; }

    short word = 0;
    List<int> header = new();
    int address = 0;
    StreamWriter writer = new(Globals.FILE_HEADERS);
    writer.AutoFlush = true;


    for (int bytecount = 0; bytecount < Globals.PRG.Count - 128;)
    {
        // format header
        header.Clear();
        address = bytecount;

        //check width criteria
        make_word();
        if (word > 0xff || word <= 1)  { continue; }

        //check height criteria
        make_word();
        if (word > 0xff || word <= 1) { bytecount -= 2; continue; }

        // grab x offset
        make_word();
        if (word > 399 || word < -399) { bytecount -= 4; continue; }

        // grab y offset
        make_word();
        if (word > 253 || word < -253) { bytecount -= 6; continue; }

        // grab sprite address minor - data doesn't really matter here either
        make_word();

        // grab sprite address major - highest value should be < 0x0800
        make_word();
        if (word > 0x07ff) { bytecount -= 8; continue; }
        if (word == 0) { bytecount -= 8; continue; }

        // check draw attribute criteria
        make_word();
        word = (short)(word & 0xffff);
        if ((word >> 0xc) > 8) { bytecount -= 10; continue; }
        if ((word >> 0xc) == 0) { bytecount -= 10; continue; }
        if ((word & 0xf) != 0) { bytecount -= 10; continue; }

        // check for valid palette address (7)
        make_word();
        if ((word & 0xf) != 0 ) { bytecount -= 12; continue; }

        // grab palette address major - highest value should be < 0xFF80 (8)
        make_word();
        if ((word & 0xffff) < 0xff80) { bytecount -= 14; continue; }

        // build address and check if the value is a valid palette size
        var pal_loc = ((header[8] << 16 | header[7]) / 8) & 0xfffff;
        var size = Globals.PRG[pal_loc] | Globals.PRG[pal_loc + 1];
        if (size > 0xFF) { bytecount -= 14; continue; }

        // build string 0x7dc88
        string line = $"{address:X5}|{(short)header[0]:X4}|{(short)header[1]:X4}|{(short)header[2]:X4}|{(short)header[3]:X4}" +
            $"|{(short)header[5]:X4}{(short)header[4]:X4}|{(short)header[6]:X4}|{(short)header[8]:X4}{(short)header[7]:X4}";

        writer.WriteLine(line);
        Console.WriteLine(line);

        void make_word()
        {
            word = (short)(Globals.PRG[bytecount] << 8 | Globals.PRG[bytecount + 1]);
            header.Add(word);
            bytecount += 2;
        }
    }
}

static void CreateImages()
{
    if (!PRG_Check()) { return; }
    if (!GFX_Check()) { return; }

    Extract.Bulk_Headers();
    Console.WriteLine("\n IMAGES FROM HEADER EXTRACTION DONE!");
    Thread.Sleep(200);
}

static void ExtractSounds()
{
    List<byte> sounds = new();
    sounds.AddRange(File.ReadAllBytes(Globals.FILE_SOUNDS).ToList());

    if (!Directory.Exists(Globals.PATH_SOUNDS))
        Directory.CreateDirectory(Globals.PATH_SOUNDS);
}

static void Extract_Fonts()
{
    if (!PRG_Check()) { return; }
    if (!GFX_Check()) { return; }

    Extract.Fonts();
    Console.WriteLine("\n FONTS DONE!");
    Thread.Sleep(200);
}

static void Extract_Animations()
{
    if (!GFX_Check()) { return; }
    Extract.Animations();
    Console.WriteLine("\n ANIMATIONS DONE!");
    Thread.Sleep(200);
}

static void Extract_Sprite_Lists()
{
    if (!GFX_Check()) { return; }
    Extract.Sprite_List();
    Console.WriteLine("\n SPRITE LISTS DONE!");
    Thread.Sleep(200);
}

static void Extract_Sprites_Src(List<Extract2.MKHeader> src)
{
    if (!GFX_Check())
    {
        Interleave_GFX();
    }

    foreach (Extract2.MKHeader hdr in src)
    {
        if(hdr.MK_Pal == null) continue;

        SKBitmap bm = Imaging.Draw_Image2(hdr);
        if(bm == null) continue;

        SKData parsed_data = bm.Encode(SKEncodedImageFormat.Png, 100);
        Directory.CreateDirectory($"src/{hdr.Origin}/");
        File.WriteAllBytes($"src/{hdr.Origin}/x_{hdr.Name}_{hdr.XOffset}_{hdr.YOffset}_{hdr.Width}_{hdr.Height}.png", parsed_data.ToArray());
    }
}

static bool PRG_Check()
{
    // check if progam files have been interleaved first
    if (Globals.PRG == null | Globals.PRG.Count == 0)
    {
        if (File.Exists(Globals.FILE_PROGRAM))
        {
            Globals.PRG = File.ReadAllBytes(Globals.FILE_PROGRAM).ToList();
            return true;
        }
        else
        {
            Console.WriteLine("\nProgram File NOT found! Try Interleaving Program files first!");
            return false;
        }
    }
    else
        return true;
}

static bool GFX_Check()
{
    // check if progam files have been interleaved first
    if (Globals.GFX == null | Globals.GFX.Count == 0)
    {
        if (File.Exists(Globals.FILE_GRAPHICS))
        {
            Globals.PRG = File.ReadAllBytes(Globals.FILE_PROGRAM).ToList();
            Globals.GFX = File.ReadAllBytes(Globals.FILE_GRAPHICS).ToList();
            return true;
        }
        else
        {
            Console.WriteLine("\nGraphics File NOT found! Try Interleaving Graphic files first!");
            return false;
        }
    }
    else
        return true;
}

static void MSG_Success()
{
    Console.WriteLine("\n INTERLEAVING DONE!");
    Thread.Sleep(200);
}