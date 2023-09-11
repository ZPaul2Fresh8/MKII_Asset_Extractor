using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MKII_Asset_Extractor
{
    static public class Extract
    {
        const int PRIMARY_PAL = 0x20F22;    // PRIMARY PALETTE ARRAY
        const int FATAL_PAL = 0x21920;  // FATALITY PALETTE ARRAY
        const int STONE_PAL = 0x7CE34;  // SK STONE PALETTE

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
                var skbitmap = Imaging.Draw_Image(Tools.Build_Header(header_loc), false);

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
                var skbitmap = Imaging.Draw_Image(Tools.Build_Header(header_loc), false);

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
                var skbitmap = Imaging.Draw_Image(Tools.Build_Header(header_loc), false);

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

            List<string> headers = File.ReadAllLines(Globals.FILE_HEADERS).ToList();

            foreach (var header in headers)
            {
                List<string> members = header.Split("|").ToList();
            }
        }

        static public void Animations()
        {
            int FIGHTER_ANIMS_LOC   = 0x20c2a;  // FIGHTERS ANIMATION ARRAYS
            string FIGHTER_ANI_DIR = "assets/gfx/fighters/";
            
            // for animations that have more than 1 part
            string[] postfixnames = { "", "part1", "part2", "part3", "part4", "part5", "part6", "part7" };

            // MAKE FIGHTER DIR & GET ANIMATION PTR
            //for (int ochar = 0; ochar < Enum.GetNames(typeof(Enums.Fighters)).Length; ochar++)
            for (int ochar = 0; ochar < 1; ochar++)
            {
                string fighter = Enum.GetName(typeof(Enums.Fighters), ochar);

                // CREATE FIGHTER DIRECTORY
                if (!Directory.Exists(FIGHTER_ANI_DIR + fighter)){Directory.CreateDirectory(FIGHTER_ANI_DIR + fighter);}

                // GET PTR TO FIGHTER ANIMATIONS
                int animations = (int)Tools.Get_Pointer(FIGHTER_ANIMS_LOC + (ochar * 4));

                // SET ANIMATION COUNT
                int ani_count;
                string ani_name;

                switch (ochar)
                {
                    case (int)Enums.Fighters.KINTARO:
                        ani_count = Enum.GetNames(typeof(Enums.Ani_IDs_Kintaro)).Length;
                        break;

                    case (int)Enums.Fighters.SHAO_KAHN:
                        ani_count = Enum.GetNames(typeof(Enums.Ani_IDs_Kahn)).Length;
                        break;

                    default:
                        ani_count = Enum.GetNames(typeof(Enums.Ani_IDs_Fighters)).Length;
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
                            AnimName = Enum.GetName(typeof(Enums.Fighters), ochar) + "/" + Enum.GetName(typeof(Enums.Ani_IDs_Kintaro), Anim_ID);
                            AnimPath = FIGHTER_ANI_DIR + AnimName;
                            break;

                        case (int)Enums.Fighters.SHAO_KAHN:
                            AnimName = Enum.GetName(typeof(Enums.Fighters), ochar) + "/" + Enum.GetName(typeof(Enums.Ani_IDs_Kahn), Anim_ID);
                            AnimPath = FIGHTER_ANI_DIR + AnimName;
                            break;

                        default:
                            AnimName = Enum.GetName(typeof(Enums.Fighters), ochar) + "/" + Enum.GetName(typeof(Enums.Ani_IDs_Fighters), Anim_ID);
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



                    
                    //++
                    int File_Pos = 0;

                // JUMP BACK HERE IF ANIMATION HAS 2-PARTS WHICH ARE SEPARATED BY A TERMINATOR, IE WINPOSE.
                animation_continuation:;
                    Anim_Cont = false;
                    
                    // to save our original position after we follow a 0000 0001 animation jump flag.
                    //int File_Pos = 0;
                    
                    // if 0, done with animation, goto check for continuations.
                    while (Tools.Get_Long(Frame) != 0)
                    {
                        // restore original file position if we followed an animation jump flag.
                        //if (File_Pos != 0)
                        //{
                        //    Frame = File_Pos;
                        //}
                        
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


                        // DISABLED -- PREVIOUSLY USED FOR SEGMENT
                        // IF DIR NON-EXISTENT, CREATE IT FOR ANIMATION
/*                        if (!Directory.Exists(AnimPath + "/" + Frame_Num.ToString()))
                        {
                            Directory.CreateDirectory(AnimPath + "/" + Frame_Num.ToString());
                        }*/

                        // SET SPECIFIC PALETTE FOR SPRITE CREATIONS.
                        Choose_Palette(Frame_Num, Anim_ID, ochar);

                        

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
                                
                                bool create_palette = (Seg_Num + Frame_Num + Anim_ID > 0);

                                // MAKE PALETTE IF PROJECTILE
                                if ((Anim_ID == 39) && (Seg_Num == 0))
                                {
                                    Globals.PALETTE = Converters.Convert_Palette((int)((header.palloc / 8) & 0xfffff));
                                }

                                // DRAW SEGMENT
                                SKBitmap bitmap = Imaging.Draw_Image(header, create_palette);
                                
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
                        else
                        // DRAW FRAME
                        {
                            var header = Tools.Build_Header(Tools.Get_Pointer(Frame));
                            bool create_palette = (Frame_Num + Anim_ID > 0);
                            SKBitmap bitmap = Imaging.Draw_Image(header, create_palette);
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

                                // projectile
                                case 0x27:
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
                                case 0x27:
                                    Frame += 4;
                                    Part++;
                                    goto animation_continuation;

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
                                case 0x27:
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



                // if next long is 0, let's check the succeeding longs for matches
                // in our frame array, if so then we have an animation which will
                // be played in reverse with select frames.
                /*                    if(!Frames.Contains((int)Tools.Get_Long(Frame))) { continue; }

                                    var file = File.OpenWrite(AnimPath + "/0.rev");
                                    while (Frames.Contains((int)Tools.Get_Long(Frame)))
                                    {
                                        uint nextlong = Tools.Get_Long(Frame);
                                        if (nextlong == 0) { break;}
                                        if (Frames.Contains((int)nextlong))
                                        {
                                            file.Write()
                                        }
                                    }*/
                end_of_animation:;
                }

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
        static void Choose_Palette(int frame_num, int ani_id, int ochar)
        {

            // ONLY CREATE PALETTE IF ON FRAME 0
            if (frame_num != 0) { return; }

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
                        case 0:
                            {
                                Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 4 + PRIMARY_PAL));
                                break;
                            }
                        case 40:
                            {
                                Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 8 + PRIMARY_PAL));
                                break;
                            }
                        case 58:
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
                        case 65:
                            {
                                Globals.PALETTE = Converters.Convert_Palette(Tools.Get_Pointer(ochar * 8 + FATAL_PAL));
                                break;
                            }
                    }
                    break;
            }
        }

    }
}
