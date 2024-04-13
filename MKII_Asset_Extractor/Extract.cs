using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using static System.Net.Mime.MediaTypeNames;

namespace MKII_Asset_Extractor
{
    static public class Extract
    {
        const int PRIMARY_PAL = 0x20F22;    // PRIMARY PALETTE ARRAY
        const int FATAL_PAL = 0x21920;          // FATALITY PALETTE ARRAY
        const int STONE_PAL = 0x7CE34;          // SK STONE PALETTE
        const int RAIDEN_GETUP_PAL = 0x73BCE;   // RAIDEN GET UP PALETTE

        static public void Fonts()
        {

            const int FONT8_CHARS_LOC   = 0x4AEFA;  // CMOS FONT STYLE
            const int FONT_RNDM_SELECT  = 0x43F7E;  // RANDOM SELECT FONT STYLE
            const int FONT_FLAWL_VICTR  = 0x440F2;  // FLAWLESS VICTORY FONT STYLE
            const int FONT_MK2          = 0x44266;  // MK2 FONT STYLE

            #region 'CMOS' FONT
            // Small 8 point font. These are normally created with a blitter
            //# operation within original hardware

            string FONT_SMALL_DIR = "assets/gfx/fonts/cmos/";
            if (!Directory.Exists(FONT_SMALL_DIR)) { Directory.CreateDirectory(FONT_SMALL_DIR); }

            int chars_processed = 0;
            
            while (chars_processed < 64)
            {
                // get character header
                int header_loc = (int)Tools.Get_Pointer(FONT8_CHARS_LOC + (chars_processed * 4));
                var skbitmap = Imaging.Draw_Font_Small(header_loc);
                
                chars_processed += 1;
                if (skbitmap != null)
                {
                    var image = SKImage.FromBitmap(skbitmap);
                    var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    File.WriteAllBytes(FONT_SMALL_DIR + $"{chars_processed}.png", data.ToArray());
                    image.Dispose();
                    data.Dispose();
                }
			        
                // MAME Breakpoint For String Display at the startup:
		        // FFA59580
            }
            #endregion

            #region 'RANDOM SELECT' FONT
            string FONT_RS = "assets/gfx/fonts/random_select/";
            if (!Directory.Exists(FONT_RS)) { Directory.CreateDirectory(FONT_RS); }

            // setup palette
            List<SKColor> palette = new List<SKColor>();
            palette.Add(SKColors.Transparent);
            palette.Add(SKColors.White);
            Globals.PALETTE = palette;

            chars_processed = 0;

            while (chars_processed < 93)
            {
                int header_loc = (int)Tools.Get_Pointer(FONT_RNDM_SELECT + (chars_processed * 4));
                var skbitmap = Imaging.Draw_Image(Tools.Build_Header(header_loc));

                chars_processed += 1;
                if (skbitmap != null)
                {
                    var image = SKImage.FromBitmap(skbitmap);
                    var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    File.WriteAllBytes(FONT_RS + $"{chars_processed}.png", data.ToArray());
                    image.Dispose();
                    data.Dispose();
                }
            }
            #endregion

            #region 'FLAWLESS VICTORY' FONT
            string FONT_FV = "assets/gfx/fonts/flawless_victory/";
            if (!Directory.Exists(FONT_FV)) { Directory.CreateDirectory(FONT_FV); }

            chars_processed = 0;

            while (chars_processed < 93)
            {
                int header_loc = (int)Tools.Get_Pointer(FONT_FLAWL_VICTR + (chars_processed * 4));
                var skbitmap = Imaging.Draw_Image(Tools.Build_Header(header_loc));

                chars_processed += 1;
                if (skbitmap != null)
                {
                    var image = SKImage.FromBitmap(skbitmap);
                    var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    File.WriteAllBytes(FONT_FV + $"{chars_processed}.png", data.ToArray());
                    image.Dispose();
                    data.Dispose();
                }
            }
            #endregion

            #region 'MK2' FONT
            string FONT_MK2DIR = "assets/gfx/fonts/mk2/";
            if (!Directory.Exists(FONT_MK2DIR)) { Directory.CreateDirectory(FONT_MK2DIR); }

            chars_processed = 0;

            while (chars_processed < 93)
            {
                int header_loc = (int)Tools.Get_Pointer(FONT_MK2 + (chars_processed * 4));
                var skbitmap = Imaging.Draw_Image(Tools.Build_Header(header_loc));

                chars_processed += 1;
                if (skbitmap != null)
                {
                    var image = SKImage.FromBitmap(skbitmap);
                    var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    File.WriteAllBytes(FONT_MK2DIR + $"{chars_processed}.png", data.ToArray());
                    image.Dispose();
                    data.Dispose();
                }
            }
            #endregion
        }

        static public void Arenas()
        {
            // 0x40732 Arena PTR Array
            //const int DEADPOOL = 0x406FA;
            const int SPRITES = 0x4f4b8;
        }

        static public void Bulk_Headers()
        {
            if (!Directory.Exists(Globals.PATH_IMAGES))
                Directory.CreateDirectory(Globals.PATH_IMAGES);

            List<string> lines = File.ReadAllLines(Globals.FILE_HEADERS).ToList();

            foreach (var line in lines)
            {
                // 2EBD2|0013|001B|0073|005B|05492AC6|6080|FFAE44A0
                List<string> members = line.Split("|").ToList();
                Header header = Tools.Build_Header(Convert.ToInt32(members[0], 16));
                Globals.PALETTE = Converters.Convert_Palette((int)(header.palloc / 8) & 0xfffff);
                SKBitmap bitmap = Imaging.Draw_Image(header);

                // DRAW FRAME
                if (bitmap != null)
                {
                    var image = SKImage.FromBitmap(bitmap);
                    var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    File.WriteAllBytes($"{Globals.PATH_IMAGES}/{header.loc}.png", data.ToArray());
                    //File.WriteAllBytes($"{Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)}/{Globals.PATH_IMAGES}/{header.loc}.png", data.ToArray());
                    image.Dispose();
                    data.Dispose();
                }

            }
        }

        static public void Animations()
        {
            // SPECIFICS
            int FIGHTER_ANIMS_LOC   = 0x20c2a;  // FIGHTERS ANIMATION ARRAYS
            int FIGHTER_SPEC_ANIMS_LOC = 0x20c6e; // FIGHTER SPECIAL ANIMATION ARRAYS
            
            int START = FIGHTER_ANIMS_LOC;

            // GENERIC DIR
            string FIGHTER_ANI_DIR = "assets/gfx/fighters/";
            
            // for animations that have more than 1 part
            string[] postfixnames = { "", "part1", "part2", "part3", "part4", "part5", "part6", "part7" };

            RUN_IT:
            // MAKE FIGHTER DIR & GET ANIMATION PTR
            //for (int ochar = 0; ochar < Enum.GetNames(typeof(Enums.Fighters)).Length; ochar++)
            for (int ochar = 0; ochar < 1; ochar++)
            {
                string fighter = Enum.GetName(typeof(Enums.Fighters), ochar);

                // CREATE FIGHTER DIRECTORY
                if (!Directory.Exists(FIGHTER_ANI_DIR + fighter)){Directory.CreateDirectory(FIGHTER_ANI_DIR + fighter);}

                // GET PTR TO FIGHTER ANIMATIONS
                int animations = (int)Tools.Get_Pointer(START + (ochar * 4));

                // SET ANIMATION COUNT
                int ani_count;
                string ani_name;

                // SET ANIMATION COUNT
                switch (ochar)
                {
                    case (int)Enums.Fighters.KINTARO:
                        if(START == FIGHTER_ANIMS_LOC)
                        {
                            ani_count = Enum.GetNames(typeof(Enums.Ani_IDs_Kintaro)).Length;
                        }
                        else
                        {
                            ani_count = 6;
                        }
                        break;

                    case (int)Enums.Fighters.SHAO_KAHN:
                        if (START == FIGHTER_ANIMS_LOC)
                        {
                            ani_count = Enum.GetNames(typeof(Enums.Ani_IDs_Kahn)).Length;
                        }
                        else
                        {
                            ani_count = 6;
                        }
                        break;

                    default:
                        if (START == FIGHTER_ANIMS_LOC)
                        {
                            ani_count = Enum.GetNames(typeof(Enums.Ani_IDs_Fighters)).Length;
                        }
                        else
                        {
                            ani_count = 6;
                        }
                        break;
                }

                // MAKE ANIMATION DIRS
                for (int Anim_ID = 0; Anim_ID < ani_count; Anim_ID++)
                {

                    string AnimPath;
                    string AnimName;
                    switch (ochar)
                    {
                        case (int)Enums.Fighters.KINTARO:
                            if(START == FIGHTER_ANIMS_LOC)
                            {
                                AnimName = Enum.GetName(typeof(Enums.Fighters), ochar) + "/" + Enum.GetName(typeof(Enums.Ani_IDs_Kintaro), Anim_ID);
                            }
                            else
                            {
                                AnimName = Enum.GetName(typeof(Enums.Fighters), ochar) + "/" + Enum.GetName(typeof(Enums.Ani_IDs_Fighters2), Anim_ID);
                            }
                            AnimPath = FIGHTER_ANI_DIR + AnimName;
                            break;

                        case (int)Enums.Fighters.SHAO_KAHN:
                            if (START == FIGHTER_ANIMS_LOC)
                            {
                                AnimName = Enum.GetName(typeof(Enums.Fighters), ochar) + "/" + Enum.GetName(typeof(Enums.Ani_IDs_Kahn), Anim_ID);
                            }
                            else
                            {
                                AnimName = Enum.GetName(typeof(Enums.Fighters), ochar) + "/" + Enum.GetName(typeof(Enums.Ani_IDs_Fighters2), Anim_ID);
                            }
                            AnimPath = FIGHTER_ANI_DIR + AnimName;
                            break;

                        default:
                            if (START == FIGHTER_ANIMS_LOC)
                            {
                                AnimName = Enum.GetName(typeof(Enums.Fighters), ochar) + "/" + Enum.GetName(typeof(Enums.Ani_IDs_Fighters), Anim_ID);
                            }
                            else
                            {
                                AnimName = Enum.GetName(typeof(Enums.Fighters), ochar) + "/" + Enum.GetName(typeof(Enums.Ani_IDs_Fighters2), Anim_ID);
                            }
                            AnimPath = FIGHTER_ANI_DIR + AnimName;
                            break;
                    }

                    if (!Directory.Exists(AnimPath)){Directory.CreateDirectory(AnimPath);}

                    Console.WriteLine($"...{AnimName}");

                    // GET ANIMATION POINTER
                    int Ani_Ptr = Tools.Get_Pointer(animations + (Anim_ID * 4));

                    // IF 0 GOTO NEXT ANIMATION
                    if (Ani_Ptr == 0) { continue; }

                    // GET FRAME PTRS HERE
                    int Frame = Ani_Ptr;
                    int Frame_Num = 0;
                    var Frames = new List<uint>();
                    bool Anim_Cont;

                    // ADDED FOR MULTIPLE PART ANIMATIONS
                    int Part = 1;
                    int File_Pos = 0;

                // JUMP BACK HERE IF ANIMATION HAS 2-PARTS WHICH ARE SEPARATED BY A TERMINATOR, IE WINPOSE.
                animation_continuation:;
                    Anim_Cont = false;
                    
                    // if 0, done with animation, goto check for continuations.
                    while (Tools.Get_Long(Frame) != 0)
                    {
                        // 0x7780 ANIMATION ROUTINE ARRAY LOC
                        uint Frame_Ptr = Tools.Get_Long(Frame);
                        
                        // add to frame list if valid frame
                        if (Frame_Ptr > 0xff800000)
                        {
                            Frames.Add(Frame_Ptr);
                        }
                        
                        switch (Frame_Ptr)
                        {
                            case 0:
                                break;

                            case 1:
                                if (Anim_ID == 39)
                                {
                                    Frame += 12;
                                    continue;
                                    //break;
                                }

                                // NEXT LONG = WHERE TO JUMP FOR ANIMATION LOOP
                                int Ani_Command = 0;

                                // add a check because there might be an animation that starts with a jump flag (0000 0001)
                                if (Frame_Ptr < 0xff800000)
                                {
                                    Ani_Command = (int)Frame_Ptr;
                                }
                                else
                                {
                                    Ani_Command = ((int)((Tools.Get_Long(Frame + 4) / 8) & 0xfffff));
                                }
                                
                                int Frame_Jump = 0;

                                // Check if punch animations, if so follow the jump to get all the parts.
                                if (Anim_ID == 19 || Anim_ID == 20)
                                {
                                    // since we're jumping to follow the animation, we need to increment our position in file to return
                                    // to later. +8 To pass over Flag and jump pointer and terminator.
                                    //File_Pos = Frame += 8;

                                    File_Pos = Frame + 12;
                                    Frame = Tools.Get_Pointer(Frame + 4);
                                    break;
                                }

                                if (Ani_Ptr == Ani_Command)
                                {
                                    File.Open(AnimPath + "/1.0.end", FileMode.OpenOrCreate, FileAccess.Write);
                                }
                                else
                                {
                                    Frame_Jump = Frame_Num - ((Frame - Ani_Command) / 4);
                                    var file = File.OpenWrite(AnimPath + "/1." + Frame_Jump.ToString() + ".end");
                                }
                                
                                goto end_of_animation;

                            case 2:
                                // FLIP X
                                Frame += 4;
                                continue;

                            case 3:
                                // ADJUST POSITION
                                Frame += 6;
                                continue;

                            case 4:
                                // ADJUST X AND Y POSITIONS
                                Frame += 8;
                                continue;

                            case 5:
                                // NEXT DATA (LONG)SPRITE PTR
                                Frame += 4;
                                continue;

                            case 6:
                                // NEXT DATA = (LONG)FUNCTION PLAY AUDIO VARIANT
                                Frame += 8;
                                continue;

                            case 7:
                                // NEXT DATA = (LONG)FUNCTION PLAY AUDIO VARIANT
                                Frame += 8;
                                Anim_Cont = true;
                                continue;

                            case 8:
                                // NEXT DATA = (WORD)CHAR ID COMPARE FOR SHARED SPRITES IN NINJAS
                                // GET NEXT WORD (CHAR ID)
                                while (Tools.Get_Long(Frame) == 8)
                                {
                                    Frame += 4;
                                    if (Tools.Get_Word(Frame) == ochar)
                                    {
                                        Frame = Tools.Get_Pointer(Frame + 2);
                                        break;
                                    }
                                    Frame += 6;
                                }
                                break;

                            case 9:
                                break;
                        }

                        // SET SPECIFIC PALETTE FOR SPRITE CREATIONS.
                        Choose_Palette(Frame_Num, Anim_ID, ochar, START);

                        // CHECK IF MULTISEGMENTED FRAME BY LOOKING AT *PTR
                        if (Imaging.Is_Frame_MultiSegmented(Frame))
                        {
                            // GET SEGMENT PTRS HERE
                            int Segment = Tools.Get_Pointer(Frame);
                            int Seg_Num = 0;

                            // VARS FOR PARSING BITMAPS AFTER PROCESSING IMAGES
                            List<SKBitmap> Segs = new List<SKBitmap>();
                            List<Header> Headers = new List<Header>();

                            while (Tools.Get_Long(Segment) != 0)
                            {
                                // DRAW SEGMENT
                                Header header = Tools.Build_Header(Tools.Get_Pointer(Segment));

                                // FOR PARSING
                                Headers.Add(header);

                                bool create_palette = (header.palloc != 0);
                                //bool create_palette = (Seg_Num + Frame_Num + Anim_ID > 0);

                                // MAKE PALETTE IF PROJECTILE
                                if ((Anim_ID == 39) && (Frame_Num == 0) && (Seg_Num == 0))
                                //if ((Anim_ID == 39) && (Seg_Num == 0))
                                {
                                    Globals.PALETTE = Converters.Convert_Palette((int)((header.palloc / 8) & 0xfffff));
                                }

                                // DRAW SEGMENT
                                SKBitmap bitmap = Imaging.Draw_Image(header);
                                
                                Segs.Add(bitmap);

                                // DISABLED -- PREVIOUSLY USED FOR SEGMENT
                                /*  if (bitmap != null)
                                    {
                                        var image = SKImage.FromBitmap(bitmap);
                                        var data = image.Encode(SKEncodedImageFormat.Png, 100);
                                        File.WriteAllBytes($"{AnimPath}/{Frame_Num}/{Seg_Num}_{header.width}_{header.height}" +
                                            $"_{header.offsetx}_{header.offsety}.png", data.ToArray());
                                        image.Dispose();
                                        data.Dispose();
                                    }*/

                                Segment += 4;
                                Seg_Num += 1;
                            }

                            // PARSE SEGMENTS HERE
                            SKImage parsed_image = Imaging.ParseSegments(Segs, Headers);
                            SKData parsed_data = parsed_image.Encode(SKEncodedImageFormat.Png, 100);

                            // find left-most offset to use as x offset value
                            List<Header> SortedOffsetX = Headers.OrderBy(o => o.offsetx).ToList();
                            List<Header> SortedOffsetY = Headers.OrderBy(o => o.offsety).ToList();


                            File.WriteAllBytes($"{AnimPath}/{Frame_Num}_Frame_{SortedOffsetX[0].offsetx}_{SortedOffsetY[0].offsety}_{parsed_image.Width}_{parsed_image.Height}_{postfixnames[Part]}_{(Tools.Get_Pointer(Frame) * 8) + 0xff800000:X8}.png", parsed_data.ToArray());
                            parsed_image.Dispose();
                            parsed_data.Dispose();

                        }
                        // DRAW FRAME
                        else
                        {
                            var header = Tools.Build_Header(Tools.Get_Pointer(Frame));
                            bool create_palette = (header.palloc != 0);
                            //bool create_palette = (Frame_Num + Anim_ID > 0);
                            SKBitmap bitmap = Imaging.Draw_Image(header);
                            if (bitmap != null)
                            {
                                var image = SKImage.FromBitmap(bitmap);
                                var data = image.Encode(SKEncodedImageFormat.Png, 100);
                                File.WriteAllBytes($"{AnimPath}/{Frame_Num}_Frame_{header.offsetx}_{header.offsety}_{image.Width}_{image.Height}_{postfixnames[Part]}_{(Tools.Get_Pointer(Frame) * 8) + 0xff800000:X8}.png", data.ToArray());
                                image.Dispose();
                                data.Dispose();
                            }
                        }
                    
                        Frame += 4;
                        Frame_Num += 1;
                    }

                    // MANUALLY DEFINE WHICH ANIMS HAS MULTIPLE PARTS SEPARARTED BY TERMINATOR.
                    switch (Part)
                    {
                        case 1:
                            // look for animation ids that have a second part
                            switch (Anim_ID)
                            {
                                // jump up land
                                case 0x6:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // unblock hi
                                case 0xb:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // knocked down
                                case 0xf:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // crouch unblock
                                case 0xc:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // high kick
                                case 0xd:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // high kick
                                case 0xe:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // hp
                                case 0x13:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // lp
                                case 0x14:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // sweep
                                case 0x15:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // swept
                                case 0x16:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // low punch crouched
                                case 0x18:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // crouch hk
                                case 0x19:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // low kick crouched
                                case 0x1a:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // uppercut
                                case 0x1c:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // flying kick
                                case 0x1e:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // jump up kick
                                case 0x1d:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // jump up punch
                                case 0x1f:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // roundhouse
                                case 0x20:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // knee to midsection
                                case 0x21:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // close hp
                                case 0x22:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // grab opponent to throw
                                case 0x24:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // projectile object
                                case 39:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // winpose
                                //case 0x29:
                                //    Frame += 4;
                                //    Part++;
                                //    goto animation_continuation;

                                // thrown by kang
                                case 0x2b:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // thrown by cage
                                case 0x2c:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // thrown by lao
                                case 0x2a:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // thrown by baraka
                                case 0x2d:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // thrown by kitana
                                case 0x2e:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // thrown by shang
                                case 0x30:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // thrown by raiden
                                case 0x31:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // thrown by ninja
                                case 0x32:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // nut punch react
                                case 0x36:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // soul drain
                                case 0x38:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // torso
                                case 0x39:
                                    Frame += 4;
                                    Part++;
                                    Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 8 + FATAL_PAL));
                                    goto animation_continuation;

                                // decap
                                case 0x3c:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;
                            }
                            break;

                        case 2:
                            switch (Anim_ID)
                            {
                                // high punch
                                case 0xd:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // knocked down
                                case 0xf:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // high punch
                                case 0x13:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                case 0x14:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // grab opponent to throw
                                case 0x24:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // projectile
                                /*case 0x27:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;
                                */

                                // thrown by lao
                                case 0x2a:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // thrown by kang
                                case 0x2b:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // thrown by cage -- crashes app
                                //case 0x2c:
                                //    Frame += 4;
                                //    Part++;
                                //    goto animation_continuation;

                                // thrown by shang
                                case 0x30:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // thrown by raiden
                                case 0x31:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // thrown by ninja
                                case 0x32:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // torso
                                case 0x39:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // decap
                                case 0x3c:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;
                            }
                            break;

                        case 3:
                            switch(Anim_ID)
                            {
                                // high punch
                                case 0x13:
                                    Frame = File_Pos;
                                    //Frame += 8;
                                    Part++;
                                    goto animation_continuation;

                                // low punch
                                case 0x14:
                                    Frame = File_Pos;
                                    Part++;
                                    goto animation_continuation;

                                // projectile
                                //case 0x27:
                                  //  Frame += 4;
                                    //Part++;
                                    //goto animation_continuation;

                                // thrown by lao
                                case 0x2a:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // torso
                                case 0x39:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;
                            }
                            break;

                        case 4:
                            switch (Anim_ID)
                            {
                                // high punch
                                case 0x13:
                                    //File_Pos = 0;
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // low punch
                                case 0x14:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // thrown by lao
                                case 0x2a:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                // torso
                                case 0x39:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;
                            }
                            break;

                        case 5:
                            switch (Anim_ID)
                            {
                                case 0x13:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

                                case 0x14:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;
                            }
                            break;

                        case 6:
                            switch (Anim_ID)
                            {
                                case 0x13:
                                    Frame = File_Pos;
                                    Part++;
                                    goto animation_continuation;

                                case 0x14:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;
                            }
                            break;  
                    }


                    // this bool tells us to grab part 2 of an animation which is separated by a terminator (part 2 animations)
                    if (Anim_Cont == true)
                    {
                        Frame += 4;
                        goto animation_continuation;
                    }

                end_of_animation:;
                }

            }
            
            // CHECK IF SPECIAL ANIMATIONS HAVE BEEN DONE.
            if(START != FIGHTER_SPEC_ANIMS_LOC)
            {
                START = FIGHTER_SPEC_ANIMS_LOC;
                goto RUN_IT;
            }; 
        }

        static public void Sprite_List()
        {

            uint[] Portraits = 
            {
                // 0x12980 - Portraits (Need Secrets/Bosses)
                0xffb23640,0xffb235b0,0xffb236d0,0xffb23760,0xffb23520,0xffb23400,0xffb232e0,0xffb23370,
                0xffb23250,0xffb23880,0xffb237f0,0xffb23490,0xFFFA5120,0xFFFA51B0,0xFFFA5240
            };

            // 0xfe04 = baby sprites
            uint[] Babies =
            {
                0xffb1ccc0,0xffb1c960,0xffb1c9f0,0xffb1cb10,0xffb1cc30,0xffb1cba0,0xffb1c840,0xffb1c8d0,
                0xff87f1a0,0xff87f230,0xff87f2c0,0xffb1ca80,0xFFC84000,0xFFC84680,0xFFC84800
            };

            uint[] Misc =
            {
                0xFFFE0D00 // victor

            };

            #region SCORAREA.TBL
            uint[] UI =
            {
                // 0xffaf3130 palette
                0xFFA78DA0, // lifebar
                0xFFA78E30, // insert coin
                0xFFA78EC0, // push start
                0xFFA78F30, // shad wins
                0xFFA78FA0, // shad num1
                0xFFA79010, // shad num2
                0xFFA79080, // shad num3
                0xFFA790F0, // shad num4
                0xFFA79160, // shad num5
                0xFFA791D0, // shad num6
                0xFFA79240, // shad num7
                0xFFA792B0, // shad num8
                0xFFA79320, // shad num9
                0xFFA79390, // shad num0
                0xffa79400, // bar kang
                0xffa79470, // bar scorpion
                0xffa794e0, // bar subzero
                0xffa79550, // bar reptile
                0xffa795c0, // bar raiden
                0xffa79630, // bar shang
                0xffa796a0, // bar cage
                0xFFA79710, // redfont0
                0xFFA79780, // redfont1
                0xFFA797F0, // redfont2
                0xFFA79860, // redfont3
                0xFFA798D0, // redfont4
                0xFFA79940, // redfont5
                0xFFA799B0, // redfont6
                0xFFA79A20, // redfont7
                0xFFA79A90, // redfont8
                0xFFA79B00, // redfont9
                0xffa79b70, // bar kitana
                0xffa79be0, // bar baraka
                0xffa79c50, // bar kung lao
                0xffa79cc0, // bar smoke
                0xffa79d30, // bar shao kahn
                0xFFA79DA0, // mkmedal2
                0xffa79e10, // bar jax
                0xffa79e80, // bar mileena
                0xffa79ef0, // bar kintaro
                0xFFA79F60, // bar noob
                0xFFA79FD0, // bar saibot
                0xFFFE0C00, // bar jade
                0xFFFE0E00, // bar hornbuckle
                0xFFFE0E80, // bar chameleon
            };

            uint[] Badge =
            {
                0xFFA7A040, // badge2
            };
            
            uint[] Battle =
            {
                0xFFA7A0D0, // battle
                0xFFA7A160, // battle1
                0xFFA7A1D0, // battle2
                0xFFA7A240, // battle3
                0xFFA7A2B0, // battle4
                0xFFA7A320, // battle5
                0xFFA7A390, // battle6
                0xFFA7A400, // battle7
                0xFFA7A470, // battle8
                0xFFA7A4E0, // battle9
                0xFFA7A550, // battle0
            };

            #endregion

            #region MKBLOOD.TBL
            // table @ 0x34b90
            uint[] Fatality =
            {
                // 0xffaf2930 palette
                0xFFBE86C0,0xFFBE8750,0xFFBE87E0,0xFFBE8850,0xFFBE88C0,0xFFBE8930,0xFFBE89A0,0xFFBE8A10,
                0xFFBE8A80,0xFFBE8AF0,0xFFBE8B60,0xFFBE8BD0,0xFFBE8C40,0xFFBE8CB0,0xFFBE8D20,0xFFBE8D90,
                0xFFBE8E00,0xFFBE8E70,0xFFBE8EE0,0xFFBE8F50,0xFFBE8FC0
            };

            uint[] Stab =
            {
                // 0xffaf2a40 palette
                0xFFBE9030,0xFFBE90C0,0xFFBE9150,0xFFBE91C0,0xFFBE9230,0xFFBE92A0,0xFFBE9310,0xFFBE9380,
                0xFFBE93F0,0xFFBE9460,0xFFBE94D0,0xFFBE9540,0xFFBE95B0,0xFFBE9620,0xFFBE9690,0xFFBE9700,
                0xFFBE9770,0xFFBE97E0,0xFFBE9850,0xFFBE98C0,0xFFBE9930,0xFFBE99A0,0xFFBE9A10,0xFFBE9A80,
                0xFFBE9AF0,0xFFBE9B60,0xFFBE9BD0,0xFFBE9C40,0xFFBE9CB0,0xFFBE9D20,0xFFBE9D90,0xFFBE9E00,
                0xFFBE9E70,0xFFBE9EE0,0xFFBE9F50,0xFFBE9FC0,0xFFBEA030,0xFFBEA0A0,0xFFBEA110,0xFFBEA180,
                0xFFBEA1F0,0xFFBEA260,0xFFBEA2D0,0xFFBEA340,0xFFBEA3B0,0xFFBEA420,0xFFBEA490,0xFFBEA500,
                0xFFBEA570,0xFFBEA5E0,0xFFBEA650,0xFFBEA6C0,0xFFBEA730,0xFFBEA7A0,0xFFBEA810,0xFFBEA880,
                0xFFBEA8F0,0xFFBEA960,0xFFBEA9D0,0xFFBEAA40,0xFFBEAAB0,0xFFBEAB20,0xFFBEAB90,0xFFBEAC00,
                0xFFBEAC70,0xFFBEACE0,0xFFBEAD50,0xFFBEADC0,0xFFBEAE30,0xFFBEAEA0,0xFFBEAF10,0xFFBEAF80,
                0xFFBEAFF0,
                
                // spill
                0xFFBEB060,0xFFBEB0F0,0xFFBEB160,0xFFBEB1D0,0xFFBEB240,0xFFBEB2B0,0xFFBEB320,0xFFBEB390,
                0xFFBEB400,0xFFBEB470,0xFFBEB4E0,0xFFBEB550,0xFFBEB5C0,0xFFBEB630,0xFFBEB6A0,0xFFBEB710,
                0xFFBEB780,0xFFBEB7F0,0xFFBEB860,0xFFBEB8D0,0xFFBEB940,0xFFBEB9B0,0xFFBEBA20,0xFFBEBA90,
                0xFFBEBB00,0xFFBEBB70,0xFFBEBBE0,0xFFBEBC50,0xFFBEBCC0,0xFFBEBD30,0xFFBEBDA0,0xFFBEBE10,
                0xFFBEBE80,0xFFBEBEF0,0xFFBEBF60,0xFFBEBFD0,0xFFBEC040,0xFFBEC0B0,0xFFBEC120,0xFFBEC190,
                0xFFBEC200
            };

            uint[] Big =
            {
                // 0xffaf2b40 palette
                0xFFBEC270,0xFFBEC300,0xFFBEC370,0xFFBEC3E0,0xFFBEC450,0xFFBEC4C0,0xFFBEC530,0xFFBEC5A0,
                0xFFBEC610,0xFFBEC680,0xFFBEC6F0,0xFFBEC760,0xFFBEC7D0,0xFFBEC840,0xFFBEC8B0,0xFFBEC920,
                0xFFBEC990,0xFFBECA00,0xFFBECA70,0xFFBECAE0,0xFFBECB50,0xFFBECBC0,0xFFBECC30,0xFFBECCA0,
                0xFFBECD10,0xFFBECD80,0xFFBECDF0,0xFFBECE60,0xFFBECED0,0xFFBECF40,0xFFBECFB0,0xFFBED020,
                0xFFBED090,0xFFBED100,0xFFBED170,0xFFBED1E0,0xFFBED250,0xFFBED2C0,0xFFBED330,0xFFBED3A0,
                0xFFBED410,0xFFBED480,0xFFBED4F0,0xFFBED560
            };

            uint[] Mid_Pr =
            {
                0xFFBED5D0
            };

            // drip_ani_table
            uint[] a_rotate_12 =
            {
                0xFFBEb630, 0xffbeb6a0, 0xffbeb710
            };

            uint[] a_rotate_standard =
            {
                0xffbeb780, 0xffbeb7f0
            };

            uint[] a_rotate_9 =
            {
                0xffbeb860, 0xffbeb8d0, 0xffbeb940
            };

            uint[] a_rotate_7 =
            {
                0xffbeb9b0, 0xffbeba20, 0xffbeba90, 0xffbebb00
            };

            #endregion

            // draw sprites from header location (must include palette in header)
            Create_Sprite_From_Location_Array(Portraits, nameof(Portraits), false);
            Create_Sprite_From_Location_Array(Babies, nameof(Babies), false);
            Create_Sprite_From_Location_Array(Misc, nameof(Misc), false);
            Create_Sprite_From_Location_Array(Fatality, "MKBLOOD/" + nameof(Fatality), true);
            Create_Sprite_From_Location_Array(Stab, "MKBLOOD/" + nameof(Stab), true);
            Create_Sprite_From_Location_Array(Big, "MKBLOOD/" + nameof(Big), true);
            Create_Sprite_From_Location_Array(Mid_Pr, "MKBLOOD/" + nameof(Mid_Pr), true);
            
            Create_Sprite_From_Location_Array(a_rotate_12, "MKBLOOD/" + nameof(a_rotate_12), true);
            Create_Sprite_From_Location_Array(a_rotate_standard, "MKBLOOD/" + nameof(a_rotate_standard), true);
            Create_Sprite_From_Location_Array(a_rotate_9, "MKBLOOD/" + nameof(a_rotate_9), true);
            Create_Sprite_From_Location_Array(a_rotate_7, "MKBLOOD/" + nameof(a_rotate_7), true);

            Create_Sprite_From_Location_Array(UI, "SCORAREA/" + nameof(UI), true);
            Create_Sprite_From_Location_Array(Badge, "SCORAREA/" + nameof(Badge), true);
            Create_Sprite_From_Location_Array(Battle, "SCORAREA/" + nameof(Battle), true);
            
            //Unzip_Images(); <-- WIP


        }

        static void Create_Sprite_From_Location_Array(uint[] uints, string folder_name, bool share_palette)
        {
            Console.WriteLine($"\nExtracting {folder_name}...");

            // set directory
            folder_name = $"{Globals.PATH_IMAGES}{folder_name}/";
            
            // get parent header and create parent palette
            int rom_loc = (int)((uints[0] / 8) & 0xfffff);
            Header header = Tools.Build_Header(rom_loc);
            Globals.PALETTE = Converters.Convert_Palette((int)(header.palloc / 8) & 0xfffff);

            foreach (var loc in uints)
            {
                rom_loc = (int)((loc / 8) & 0xfffff);
               
                header = Tools.Build_Header(rom_loc);
                if(share_palette == false)
                {
                    Globals.PALETTE = Converters.Convert_Palette((int)(header.palloc / 8) & 0xfffff);
                }
                SKBitmap bitmap = Imaging.Draw_Image(header);

                // DRAW FRAME
                if (bitmap != null)
                {
                    var image = SKImage.FromBitmap(bitmap);
                    var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    
                    if(!Directory.Exists(folder_name))
                        Directory.CreateDirectory(folder_name);

                    File.WriteAllBytes($"{folder_name}{header.loc}.png", data.ToArray());
                    image.Dispose();
                    data.Dispose();
                }
            }
        }

        static void Create_Sprites_From_Parent(uint parent_loc, int children, string folder_name)
        {
            Console.WriteLine($"\nExtracting {folder_name}...");

            // set directory
            folder_name = $"{Globals.PATH_IMAGES}{folder_name}/";

            // get parent header and draw it
            int rom_loc = (int)((parent_loc / 8) & 0xfffff);
            Header header = Tools.Build_Header(rom_loc);
            Globals.PALETTE = Converters.Convert_Palette((int)(header.palloc / 8) & 0xfffff);
            SKBitmap bitmap = Imaging.Draw_Image(header);
            if (bitmap != null)
            {
                var image = SKImage.FromBitmap(bitmap);
                var data = image.Encode(SKEncodedImageFormat.Png, 100);

                if (!Directory.Exists(folder_name))
                    Directory.CreateDirectory(folder_name);

                File.WriteAllBytes($"{folder_name}{header.loc}.png", data.ToArray());
                image.Dispose();
                data.Dispose();
            }

            for (int i = 0; i < children; i++)
            {
                // find location of next child
                header = Tools.Build_Header((int)(i *14 + parent_loc + 18));
                bitmap = Imaging.Draw_Image(header);
                if (bitmap != null)
                {
                    var image = SKImage.FromBitmap(bitmap);
                    var data = image.Encode(SKEncodedImageFormat.Png, 100);

                    if (!Directory.Exists(folder_name))
                        Directory.CreateDirectory(folder_name);

                    File.WriteAllBytes($"{folder_name}{header.loc}.png", data.ToArray());
                    image.Dispose();
                    data.Dispose();
                }
            }
        }

        static void Unzip_Images()
        {
            uint[] Zips =
            {
                0xFFD00000, // revx
            };

            foreach (uint z in Zips)
            {

                Unzip_Image(z);

                //Header header = new Header();
                //header.loc = (int)(z / 8) & 0xfffff;
                //header.width = Tools.Get_Word(header.loc);
                //header.height = Tools.Get_Word(header.loc + 2);
                //header.offsetx = 0;
                //header.offsety = 0;
                //header.draw_att = 0x8000;
                //header.palloc = (uint)header.loc + 6;

                //SKBitmap bitmap = Imaging.Draw_Image(header, true);
                //if (bitmap != null)
                //{
                //    var image = SKImage.FromBitmap(bitmap);
                //    var data = image.Encode(SKEncodedImageFormat.Png, 100);

                //    if (!Directory.Exists($"{Globals.PATH_IMAGES}Zips/"))
                //        Directory.CreateDirectory($"{Globals.PATH_IMAGES}Zips/");

                //    File.WriteAllBytes($"{$"{Globals.PATH_IMAGES}Zips/"}{header.loc}.png", data.ToArray());
                //    image.Dispose();
                //    data.Dispose();
                //}
            }

            
        }

        static SKBitmap Unzip_Image(uint z)
        {
            SKBitmap bitmap = null;
            int loc=0;
            List<byte> chunk = new List<byte>();
            
            // convert game address and grab chunk of data from
            // address(z) provided
            if (z > 0xff800000 && z < 0xffffffff)
            {
                loc = (int)(z / 8) & 0xfffff;
                chunk.AddRange(Globals.PRG.Skip(loc).Take(Tools.Get_Word(loc) * Tools.Get_Word(loc + 2)).ToList());
            }
            else
            {
                loc = (int)(z / 8) & 0xffffff;
                chunk.AddRange(Globals.GFX.Skip(loc).Take(Tools.Get_Word(loc) * Tools.Get_Word(loc + 2)).ToList());
            }
            
            int width = chunk[0] << 8 | chunk[1];
            int height = chunk[2] << 8 | chunk[3];
            int frames = chunk[4] << 8 | chunk[5];
            chunk.RemoveRange(0,6);

            // create palette list for frames
            List<List<SKColor>> frame_pals = new List<List<SKColor>>();

            for (int f = 0; f < frames; f++)
            {
                var t = Converters.Convert_Palette(chunk);
                frame_pals.Add(new List<SKColor>((IEnumerable<SKColor>)t.Item1));
                chunk = t.Item2;
            }

            // remove tree data
            chunk = Tools.reverse_bytes(chunk);
            List<byte> compression = new List<byte>(chunk.Take(25).ToList());
            List<int> tree1= new List<int>();
            List<List<int>> tree2 = new List<List<int>>();
            chunk.RemoveRange(0, 25);

            Uncompress_Tree();

            // reverse bytes to comply
            List<bool> bools = Tools.ConvertBytesToBoolArray(chunk);

            List<byte>temp = new List<byte>();

            for (int f = 0; f < frames; f++)
            {
                int x = 0;
                int y = 0;

                while (y < height)
                {


                check_if_encoded:
                    //var t = get_next_bit(chunk);
                    var decode = bools[0]; bools.RemoveAt(0);


                    // check if decoded needed
                    if (decode == false)
                        goto decode_still;

                    // read 8-bits and copy
                    //bitmap.SetPixel(x, y, frame_pals[f].ElementAt(chunk[0]));
                    temp.Add(Tools.BoolArrayToByte(bools.Take(8).ToArray()));
                    bools.RemoveRange(0, 8);
                    x++;

                    // blow out line if end of row
                    if (x == width - 1)
                    {

                        y++;
                    }
                    goto check_if_encoded;

                //if bit = 0, decode from trees
                decode_still:
                    temp.Add(Tools.BoolArrayToByte(bools.Take(6).ToArray()));
                    bools.RemoveRange(0, 6);

                }
            }

            #region unused
            //for (int f = 0; f < frames; f++)
            //{
            //    List<int> indexes = new List<int>();
            //    int chksum = 0;
            //    int compressed_bytes = get_next_byte() + 1; // compressed bytes to describe tree - 1
            //    int A3_And = 0xf;
            //    int A6_num_of_code_in_tree = 0;

            //    // utr0
            //    for (int i = 0; i < compressed_bytes; i++)
            //    {
            //        int nextbyte = get_next_byte();//;( codes - 1 << 4) | bit lngth - 1

            //        chksum += nextbyte;

            //        int A2_num_of_code_this_length = (nextbyte >> 4) + 1; // number of codes of this bit length

            //        A6_num_of_code_in_tree += A2_num_of_code_this_length;
            //        nextbyte = nextbyte & A3_And;
            //        nextbyte++;

            //        int dupes = nextbyte << 16;

            //        // utr1
            //        for (int c = 0; c < A2_num_of_code_this_length; c++)
            //        {
            //            indexes.Add(nextbyte);
            //        }

            //    }

            //    int A2 = 0;
            //    compressed_bytes = get_next_byte() + 1; // compressed bytes to describe tree - 1

            //    // chklp
            //    for (int d = 0; d < compressed_bytes; d++)
            //    {
            //        A2 += compressed_bytes;
            //    }

            //    if(A2 != chksum)
            //    {
            //        Console.WriteLine("Checksums don't match...");
            //    }

            //    //*Sort Tree by increasing Bit Length.
            //    //* The translation index is placed in the upper byte
            //    //* of the long word.



            //    // a quick byte re-arrangement
            //    int get_next_byte()
            //    {
            //        short sh = Tools.Get_Word(cur_pos);
            //        int temp = 0;

            //        if (Tools.Is_Bit_Set(sh, 0))
            //        {
            //            temp = Globals.PRG[cur_pos + 1];
            //        }
            //        else
            //        {
            //            temp = Globals.PRG[cur_pos - 1];
            //        }

            //        cur_pos++;
            //        return temp;
            //    }
            //}


            //********************************
            //*Uncompress a single frame
            //* A0 = Address mask for circular buffer
            //*A8 = *to compressed data
            //* A9 = *to buffer for uncompressed bytes
            //*A11 = How many to place before returning
            //* B0 = *Length tree
            //* B1 = *Distance tree
            //*
            //*Trashes:
            //*a1 = Distance
            //* a2 = ptr to leftover data if there is any
            //* a4 = Length
            //*
            //*ReadTree uses A2-A5,A7,A14,B6
            //* Need to Preserve: 	B9 - B10
            // bp ff904a30
            // if bit = 1, read 8 bits and copy
            #endregion

            return bitmap;

            void Uncompress_Tree()
            {
                // copy table for later use
                List<byte> temp = compression.ToList();

                int a4 = 0;
                int a0 = compression[0]; compression.RemoveAt(0);
                a0 += 1;
                

                int a3 = 0xf;
                int a6 = 0;

                // not needed, just makes thing easier
                int a1 = 0;
                int a2 = 0;
                int a11 = 0;
                int a14 = 0;

                for (int a0i = 0; a0i < a0; a0i++)
                {
                    a1 = compression[0]; compression.RemoveAt(0);

                    a4 += a1;
                    a2 = a1;
                    a2 >>= 4;
                    a2++;
                    a6 += a2;
                    a1 &= a3;
                    a1++;

                    a11 = a1;
                    a11 = a11 << 0x10;
                    a1 = a11 + a1;

                    for (int a2i = 0; a2i < a2; a2i++)
                    {
                        tree1.Add(a1);
                    }
                }

                // part 2
                compression = temp;

                a2 = 0;
                a0 = compression[0]; compression.RemoveAt(0);
                a0++;

                for (int a0i = 0; a0i < a0; a0i++)
                {
                    a1 = compression[0]; compression.RemoveAt(0);
                    a2 += a1;
                }

                // if checksums don't match, throw error
                if (a2 != a4)
                    goto error3;

                // a0 = tree2 (0x10b6100)
                // a7 = tree1 (0x10b2100)
                int a9 = a6;
                List<int> A11_tree1 = tree1.ToList();

                for (int a9i = 0; a9i < a9; a9i++)
                {
                    // move a5,a7 (reset to start location on tree1)
                    int tree_index = 0;
                    a14 = 0x6543;
                    int b6 = a6;
                    a1 = 0x7654;
                    a11 = 0;
                    
                    for (int b6i = 0; b6i < b6; b6i++)
                    {
                        a2 = (tree1[tree_index] & 0xffff);  //; look at next bit length

                        if (a2 >= a14)  //; is it less than the last minimum(a14)
                            goto add20;

                        a14 = a2;   //; if yes, save new minimum
                        a11 = tree_index;
                        //A11_tree1 = tree1.ToList(); //save pointer to minimum

                    add20:
                        // adk20 replication
                        tree_index++;
                    }

                    int value = ((tree1[a11] >> 0x10) << 0x10) + a1;
                    
                    tree1.Insert(a11, value);
                    tree1.RemoveAt(a11+1);
                    tree2.Add(A11_tree1);
                }

                a11 = 0;    // code
                a1 = 0;     // code inc
                a2 = 0;     // last bit length
                a14 = a6 - 1;   // loop counter
                //List<int> temp_tree = new List<int>();
                
                // test...
                tree1.RemoveAt(tree1.Count - 1);

                for (int a14i = 0; a14i < a14; a14i++)
                {
                    // get translated pointer (goign from last to first)
                    //temp_tree = tree2[tree2.Count-1];
                    //tree2.RemoveAt(tree2.Count-1);
                    int value = tree1[tree1.Count - 1 - a14i];
                    a11 += a1;
                    a3 = (value >> 0x10) & 0xff ;
                    if (a3 == a2)
                        goto ff904130;

                    a2 = a3;
                    a3 = 0x10;
                    a3 -= a2;
                    a1 = 1;
                    a1 <<= a3;

                ff904130:
                    int a5 = a11;
                    a9 = 0x10;
                    
                    for (int a9i = 0; a9i < a9; a9i++)
                    {
                        a5 <<= 1;
                        a3 = (((a5 >> 0x10) & 0xffff) << 0x10) + (a3 & 0xffff);
                        a3 >>= 1;
                        a5 &= 0xffff;
                    }
                    tree1[tree1.Count - 1 -a14i] = (value >> 0x10) << 0x10 | (a3 & 0xffff);
                }

                

                error3:
                Console.WriteLine("Checksum mismatch unzipping");
            }

            void read_tree()
            {
                int a2 = 1;
                uint temp1 = Tools.BoolArrayToByte(bools.Take(1).ToArray());
                temp1 = Tools.RotateLeft(temp1, 0x1f);
                int b6 = 0;
                // get bit from table
                // get another bit from table
                int a14 = 0x20;
                a14 -= a2;

            }

        }


        // returns the address in ROM that follows a 0-terminator
        static int Find_Next_Part(int frame)
        {
            while (Tools.Get_Long(frame) != 0)
            {
                frame += 4;
            }
            return frame += 4;
        }


        /// <summary>
        /// Forces a specific palette based on Char/Anim IDs
        /// </summary>
        /// <param name="frame_num"></param>
        /// <param name="ani_id"></param>
        /// <param name="ochar"></param>
        /// /// <param name="The starting location of the animation array."></param>
        static void Choose_Palette(int frame_num, int ani_id, int ochar, int START)
        {

            // ONLY CREATE PALETTE IF ON FRAME 0
            if (frame_num != 0) { return; }

            // PALETTE ASSIGNMENTS PER BASIC ANIMATION TABLE
            if (START != 0x20c6e)
            {
                switch (ochar)
                {
                    case (int)Enums.Fighters.SHAO_KAHN:
                        switch (ani_id)
                        {
                            case (int)Enums.Ani_IDs_Kahn.A_STONE_CRACK:
                                {
                                    Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(STONE_PAL));
                                    break;
                                }
                            case (int)Enums.Ani_IDs_Kahn.STONE_EXPLODE:
                                {
                                    Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(STONE_PAL));
                                    break;
                                }
                            default:
                                {
                                    Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 4 + PRIMARY_PAL));
                                    break;
                                }
                        }
                        break;

                    default:
                        switch (ani_id)
                        {
                            //case 0:
                            //    {
                            //        Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 4 + PRIMARY_PAL));
                            //        break;
                            //    }
                            case (int)Enums.Ani_IDs_Fighters.ANI_19_HIGH_PUNCH:
                                {
                                    if (ochar == (int)Enums.Fighters.RAIDEN)
                                    {
                                        Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 4 + PRIMARY_PAL));
                                    }
                                    break;
                                }
                            case (int)Enums.Ani_IDs_Fighters.ANI_23_GET_UP_FROM_SWEPT:
                                {
                                    if (ochar == (int)Enums.Fighters.RAIDEN)
                                    {
                                        Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(RAIDEN_GETUP_PAL));
                                    }
                                    
                                    break;
                                }
                            case (int)Enums.Ani_IDs_Fighters.ANI_24_LOW_PUNCH_CROUCHED:
                                {
                                    if (ochar == (int)Enums.Fighters.RAIDEN)
                                    {
                                        Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 4 + PRIMARY_PAL));
                                    }
                                    break;
                                }
                            case (int)Enums.Ani_IDs_Fighters.ANI_40_STUNNED:
                                {
                                    Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 4 + PRIMARY_PAL));
                                    break;
                                }
                            case (int)Enums.Ani_IDs_Fighters.ANI_58_SLOW_PROJ_BANG:
                                {
                                    Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 8 + PRIMARY_PAL));
                                    break;
                                }
                            case 59:
                                {
                                    Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 8 + FATAL_PAL));
                                    break;
                                }
                            case 62:
                                {
                                    Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 4 + PRIMARY_PAL));
                                    break;
                                }
                            case (int)Enums.Ani_IDs_Fighters.ANI_65_SCORPION_SLICED_ME:
                                {
                                    Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 8 + FATAL_PAL));
                                    break;
                                }
                            case (int)Enums.Ani_IDs_Fighters.SPEC_ANI_00:
                                {
                                    // Superman Move
                                    if (ochar == (int)Enums.Fighters.RAIDEN || ochar == (int)Enums.Fighters.SUBZERO)
                                    {
                                        Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 4 + PRIMARY_PAL));
                                    }
                                    break;
                                }
                            case (int)Enums.Ani_IDs_Fighters.SPEC_ANI_01:
                                {
                                    // slide
                                    if (ochar == (int)Enums.Fighters.SUBZERO)
                                    {
                                        Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 4 + PRIMARY_PAL));
                                    }
                                    break;
                                }
                        }
                        break;
                }
            }
            // PALETTE ASSIGNMENTS PER SPECIAL ANIMATION TABLE
            else
            {
                switch (ochar)
                {
                    case (int)Enums.Fighters.SHAO_KAHN:
                        Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 4 + PRIMARY_PAL));
                        break;

                    case (int)Enums.Fighters.KINTARO:
                        Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 4 + PRIMARY_PAL));
                        break;

                    default:

                        switch (ani_id)
                        {
                            // projectile
                            //case 0:
                            //    break;

                            // spin move (Lao)
                            case 1:
                                Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 4 + PRIMARY_PAL));
                                break;

                            // impaled fatality
                            //case 5:
                            //    Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 8 + FATAL_PAL));
                            //    break;
                        }
                        break;
                }
            }
        }

    }

    static public class Extract2
    {
        public class MKPalette
        {
            public string Name;
            public List<SKColor> Colors;
        }

        public class MKHeader
        {
            public string Name;
            public string Origin;
            public short Width;
            public short Height;
            public short XOffset;
            public short YOffset;
            public uint GFXLocation;
            public short DMA;
            public MKPalette? MK_Pal;
        }

        // #1 (.ASM)
        public static List<MKPalette> ReadPalettesIntoMemory()
        {
            List<MKPalette> MKPalettes = new List<MKPalette>();

            //get all the files in our palette directory and process them.
            Console.WriteLine("Getting palettes...");

            foreach (var item in Directory.GetFiles("src/pals/"))
            {
                Console.WriteLine($"...{item}");
                string[] lines = File.ReadAllLines(item);
                
                for (int l = 0; l < lines.Length; l++)
                {
                    // see if we are at a label
                    if (!lines[l].EndsWith(":") || lines[l].StartsWith(';'))
                        continue;

                    // make new palette
                    MKPalette pal = new MKPalette();
                    pal.Name = lines[l].TrimEnd(':');
                    l++;

                    // get palette size.  not used anyhow
                    try
                    {
                        int size = int.Parse(lines[l][^2..]);
                        l++;
                    }
                    catch (Exception)
                    {

                        continue;
                    }


                    // iterate through palette color lines
                    List<string> names = new List<string>();
                    while (l < lines.Length && !string.IsNullOrEmpty(lines[l]) )
                    {
                        // get colors as strings
                        string[] str_colors = lines[l].Trim('\t', ' ').Substring(5).Split(',');
                        names.AddRange(str_colors.ToList());
                        l++;
                    }

                    // palette over, convert hex strings to color list
                    pal.Colors = Tools2.ConvertColorList(names);
                    MKPalettes.Add(pal);
                }
            }
            return MKPalettes;
        }

        // #2 (.TBL)
        public static List<MKHeader> ReadHeadersIntoMemory(List<MKPalette> pals)
        {
            List<MKHeader> headers = new List<MKHeader>();
            string dir = "src/sprites/";

            //get all the files in our palette directory and process them.
            Console.WriteLine("Getting sprites...");

            foreach (var item in Directory.GetFiles(dir))
            {
                Console.WriteLine($"...{item}");
                string[] lines = File.ReadAllLines(item);
                
                // reset last palette before any file processing
                // still need a bullet-proof method of palette assignment
                MKPalette last_palette = null;

                for (int l = 0; l < lines.Length; l++)
                {
                    // see if we are at a label
                    if (!lines[l].EndsWith(":")) continue;
                    
                    // make new header
                    MKHeader header = new MKHeader();
                    header.Name = lines[l].Trim(':');
                    header.Origin = item.Substring(dir.Length);

                check_for_dimensions:
                    l++;

                    // check for dimensions
                    Regex rx_dim = new Regex(@"\d+,\d+,-?\d+,-?\d+");
                    var match = rx_dim.Match(lines[l]);
                    if (!match.Success)
                    {
                        Console.WriteLine($"Dimensions mismatch on line {l-1}. Value={lines[l]}");
                        goto check_for_dimensions;
                    } 
                    string[] dimensions = match.Value.Split(',');
                    header.Width = short.Parse(dimensions[0]);
                    header.Height = short.Parse(dimensions[1]);
                    header.XOffset = short.Parse(dimensions[2]);
                    header.YOffset = short.Parse(dimensions[3]);

                check_for_address:
                    l++;
                    if(l == lines.Length) { continue; }

                    // check if valid gfx address
                    //Regex rx_addy = new Regex(@"(0[xX]){1}[A-Fa-f0-9]{8}H$ | [A-Fa-f0-9]{8}H$");
                    Regex rx_addy = new Regex(@"[A-Fa-f0-9]+H$");
                    match = rx_addy.Match(lines[l]);
                    if (!match.Success)
                    {
                        Console.WriteLine($"GFX Address mismatch {lines[l]}");
                        goto check_for_address;
                    } 
                    header.GFXLocation = uint.Parse(match.Value.Trim('H','h'), System.Globalization.NumberStyles.HexNumber);

                check_for_dma:
                    l++;
                    if (l == lines.Length) { continue; }

                    // check if valid dma
                    Regex rx_dma = new Regex(@"[A-Fa-f0-9]+H$");
                    match = rx_dma.Match(lines[l]);
                    if (!match.Success)
                    {
                        Console.WriteLine($"DMA mismatch on line {l-1}. Value={lines[l]}");
                        goto check_for_dma;
                    }
                    header.DMA = short.Parse((string)match.Value.Trim('H','h'), System.Globalization.NumberStyles.HexNumber);

                    // check if new label or palette
                    l++;
                    if (l == lines.Length) { continue; }

                    if (lines[l].EndsWith(':') || lines[l] == "\t.TEXT")
                    {
                        l--;
                        header.MK_Pal = last_palette;
                        goto add_header;
                    }

                    // no label detected, check for palette
                    Regex rx_pal = new Regex(@"\w+_P$", RegexOptions.IgnoreCase);
                    match = rx_pal.Match(lines[l]);
                    if(match.Success)
                    {
                        goto assign_pal;
                    }
                    else
                    {
                        // see if we have a uniquely named palette with a less strict regex
                        // and see if it's in our palette list
                        Regex rx_pal2 = new Regex(@"\w+$", RegexOptions.IgnoreCase);
                        match = rx_pal2.Match(lines[l]);
                        if (match.Success)
                        {
                            if(pals.Find(x => x.Name == match.Value) != null)
                            {
                                Console.WriteLine($"Palette unique: {match.Value}");
                                goto assign_pal;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Palette mismatch on line {l - 1}. Value={lines[l]}");
                            header.MK_Pal = null;
                        }   
                    }

                    assign_pal:
                    if (pals.Find(x => x.Name == match.Value) != null)
                    {
                        header.MK_Pal = pals.Find(x => x.Name == match.Value);
                        last_palette = header.MK_Pal;
                    }
                    else
                    {
                        Console.WriteLine($"Couldn't find palette \"{match.Value}\" ");
                        header.MK_Pal = null;
                        last_palette = null;
                    }
                        


                add_header:
                    headers.Add(header);
                }
            }
            return headers;
        }

        // #3 Create Sprites From Headers in Memory.
        public static class Tools2
        {
            // Make color list from string hex value
            public static List<SKColor> ConvertColorList(List<string> str_colors)
            {
                List<SKColor> sKColors = new List<SKColor>();

                int i = 0;
                foreach (string s in str_colors)
                {
                    if (i == 0)
                    {
                        sKColors.Add(SKColors.Transparent);
                        i++;
                        continue;
                    }
                    string t = s.TrimEnd('H', 'h');
                    sKColors.Add(Converters.Convert_Color(Int16.Parse(t, System.Globalization.NumberStyles.HexNumber)));
                }

                return sKColors;
            }
        }

    }
}
