// See https://aka.ms/new-console-template for more information
using System.Linq;
using System;

Console.WriteLine("[MKII Asset Extractor]");

Console.WriteLine("Loading Files...");
List<byte> program = new List<byte>();
program.AddRange(File.ReadAllBytes("mk2.program").ToList());
List<byte> graphics = new List<byte>();
graphics.AddRange(File.ReadAllBytes("mk2.graphics").ToList());

// RIP GFX HEADERS
Console.WriteLine("Searching for GFX Headers...");

//int bytecount = 0;
int word = 0;
List<int> header = new List<int>();
int address = 0;
string file = "gfx_headers.txt";
StreamWriter writer = new StreamWriter(file);

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

    if (word > 0xff)
        continue;

    if (word <= 1)
        continue;

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
    string line = $"{address.ToString("X5")}|{header[0].ToString("X4")}|{header[1].ToString("X4")}|{header[2].ToString("X4")}|{header[3].ToString("X4")}" +
        $"|{header[5].ToString("X4")}{header[4].ToString("X4")}|{header[6].ToString("X4")}";

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








return;

