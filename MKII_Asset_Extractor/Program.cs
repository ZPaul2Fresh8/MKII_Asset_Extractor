// See https://aka.ms/new-console-template for more information
using System.Linq;
using System;
using System.Runtime.CompilerServices;

const string FILE_PROGRAM = "mk2.program";
const string FILE_GRAPHICS = "mk2.graphics";
const string FILE_SOUNDS = "mk2.sounds";
const string FILE_HEADERS = "gfx_headers.txt";
const string PATH_IMAGES = "Images/";
const string PATH_SOUNDS = "Sounds/";

Console.WriteLine("[MKII ASSET EXTRACTOR]");
Thread.Sleep(1000);
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
    Thread.Sleep(400);
    Console.WriteLine("1: Extract Image Headers.");
    Thread.Sleep(200);
    Console.WriteLine("2: Create Images From Headers.");
    Thread.Sleep(200);
    Console.WriteLine("3: Extract Color Palettes.");
    Thread.Sleep(200);
    Console.WriteLine("ESC: Exit");
}

static void InterleaveFiles()
{

}

static void ExtractImageHeaders()
{
    if (!File.Exists(FILE_HEADERS))
    {
        FileStream fileStream = File.Create(FILE_HEADERS);
        fileStream.Close();
    }
        
    Console.WriteLine("\nSearching for GFX Headers...\n");
    Thread.Sleep(1000);
    Console.WriteLine("ROM  |W   |H   |XOFF|YOFF|GFX LOC |DMA\n" +
      "---------------------------------------");

    List<byte> program = new();
    program.AddRange(File.ReadAllBytes(FILE_PROGRAM).ToList());

    int word = 0;
    List<int> header = new();
    int address = 0;
    StreamWriter writer = new(FILE_HEADERS);

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
    graphics.AddRange(File.ReadAllBytes(FILE_GRAPHICS).ToList());
    
    if(!Directory.Exists(PATH_IMAGES))
        Directory.CreateDirectory(PATH_IMAGES);
}

static void ExtractSounds()
{
    List<byte> sounds = new();
    sounds.AddRange(File.ReadAllBytes(FILE_SOUNDS).ToList());

    if (!Directory.Exists(PATH_SOUNDS))
        Directory.CreateDirectory(PATH_SOUNDS);
}