// See https://aka.ms/new-console-template for more information
using System.Linq;
using System;
using System.Runtime.CompilerServices;
using MKII_Asset_Extractor;

Console.WriteLine("[MKII ASSET EXTRACTOR]");
Thread.Sleep(500);
ListOptions();

while (true)
{
    ConsoleKeyInfo keyInfo = Console.ReadKey(true); // Read a key input without displaying it

    switch (keyInfo.Key)
    {
        case ConsoleKey.NumPad1:
            ExtractImageHeaders();
            Console.WriteLine("Done. Header listings output @ gfx_headers.txt");
            ListOptions();
            break;

        case ConsoleKey.NumPad2:
            Console.WriteLine("Creating Images...");
            CreateImages();
            ListOptions();
            break;

        case ConsoleKey.NumPad3:
            Console.WriteLine("Extracting Sounds...");
            ExtractSounds();
            ListOptions();
            break;

        case ConsoleKey.NumPad4:
            Console.WriteLine("Interleaving Program Roms...");
            Interleave_PRG();
            ListOptions();
            break;

        case ConsoleKey.NumPad5:
            Console.WriteLine("Interleaving Graphic Roms...");
            Interleave_GFX();
            ListOptions();
            break;

        case ConsoleKey.NumPad6:
            Console.WriteLine("Extracing Fonts...");
            Extract_Fonts();
            ListOptions();
            break;

        case ConsoleKey.NumPad7:
            Console.WriteLine("Extracing Animations...");
            Extract_Animations();
            ListOptions();
            break;

        case ConsoleKey.Escape:
            Console.WriteLine("Exiting...");
            return; // Exit the program

        default:
            Console.WriteLine("Invalid key pressed.");
            break;
    }
}

static void ListOptions()
{
    Console.WriteLine("\nCHOOSE AN OPTION:");
    Thread.Sleep(200);
    Console.WriteLine("1: Extract Image Headers.");
    Thread.Sleep(100);
    Console.WriteLine("2: Create Images From Headers.");
    Thread.Sleep(100);
    Console.WriteLine("3: Extract Color Palettes.");
    Thread.Sleep(100);
    Console.WriteLine("4: Interleave Program ROMs.");
    Thread.Sleep(100);
    Console.WriteLine("5: Interleave Graphic ROMs.");
    Thread.Sleep(100);
    Console.WriteLine("6: Extract Fonts.");
    Thread.Sleep(100);
    Console.WriteLine("7: Extract Animations.");
    Thread.Sleep(100);
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

void Interleave_GFX()
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
    Thread.Sleep(1000);
    Console.WriteLine("ROM  |W   |H   |XOFF|YOFF|GFX LOC |DMA\n" +
      "---------------------------------------");

    List<byte> program = new();
    program.AddRange(File.ReadAllBytes(Globals.FILE_PROGRAM).ToList());

    int word = 0;
    List<int> header = new();
    int address = 0;
    StreamWriter writer = new(Globals.FILE_HEADERS);

    for (int bytecount = 0; bytecount < program.Count - 128;)
    {
        // format header
        header.Clear();
        address = bytecount;

        //check width criteria
        make_word();
        if (word > 0xff) { continue; }
        if (word <= 1) { continue; }

        //check height criteria
        make_word();
        if (word > 0xff) { continue; }
        if (word <= 1) { continue; }

        // grab x offset
        make_word();
        if (word > 399 || word < -399) { continue; }

        // grab y offset
        make_word();
        if (word > 253 || word < -253) { continue; }

        // grab sprite address minor - data doesn't really matter here either
        make_word();

        // grab sprite address major - highest value should be < 0x0800
        make_word();
        if (word > 0x07ff) { continue; }
        if (word == 0) { continue; }

        // check draw attribute criteria
        make_word();
        if ((word >> 0xc) > 6) { continue; }
        else if ((word >> 0xc) == 0) { continue; }
        else if ((word & 0xf) != 0) { continue; }
        
        // build string
        string line = $"{address:X5}|{header[0]:X4}|{header[1]:X4}|{header[2]:X4}|{header[3]:X4}" +
            $"|{header[5]:X4}{header[4]:X4}|{header[6]:X4}";

        // print(line)	
        writer.WriteLine(line);
        Console.WriteLine(line);

        void make_word()
        {
            word = program[bytecount] << 8 | program[bytecount + 1];
            header.Add(word);
            bytecount += 2;
        }
    }
}

static void CreateImages()
{
    List<byte> graphics = new();
    graphics.AddRange(File.ReadAllBytes(Globals.FILE_GRAPHICS).ToList());
    
    if(!Directory.Exists(Globals.PATH_IMAGES))
        Directory.CreateDirectory(Globals.PATH_IMAGES);
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

    Extract.Fonts();
    Console.WriteLine("\n FONTS DONE!");
    Thread.Sleep(200);
}

static void Extract_Animations()
{
    if (!PRG_Check()) { return; }
    Extract.Animations();
    Console.WriteLine("\n ANIMATIONS DONE!");
    Thread.Sleep(200);
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

static void MSG_Success()
{
    Console.WriteLine("\n INTERLEAVING DONE!");
    Thread.Sleep(200);
}