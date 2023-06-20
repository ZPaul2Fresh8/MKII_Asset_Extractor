using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKII_Asset_Extractor
{
    static public class Extract
    {
        const int PRIMARY_PAL       = 0x20F22;	// PRIMARY PALETTE ARRAY
        const int FATAL_PAL         = 0x21920;	// FATALITY PALETTE ARRAY
        const int STONE_PAL         = 0x7CE34;	// SK STONE PALETTE
        const int FONT8_CHARS_LOC   = 0x4Aefa;

        static public void Fonts()
        {
            // Small 8 point font. These are normally created with a blitter
            //# operation within original hardware

            string FONT_SMALL_DIR = "assets/gfx/fonts/small/";

            if (!Directory.Exists(FONT_SMALL_DIR))
            {
                Directory.CreateDirectory(FONT_SMALL_DIR);
            }

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
        }

        static public void Animations()
        {
            int FIGHTER_ANIMS_LOC   = 0x20c2a;  // FIGHTERS ANIMATION ARRAYS
            string FIGHTER_ANI_DIR = "assets/gfx/fighters/";

            // MAKE FIGHTER DIR & GET ANIMATION PTR
            for (int ochar = 0; ochar < Enum.GetNames(typeof(Enums.Fighters)).Length; ochar++)
            {
                // CREATE FIGHTER DIRECTORY
                if(!Directory.Exists(FIGHTER_ANI_DIR + Enum.GetName(typeof(Enums.Fighters), ochar)))
                {
                    Directory.CreateDirectory(FIGHTER_ANI_DIR + Enum.GetName(typeof(Enums.Fighters), ochar));
                }

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
                    switch (ochar)
                    {
                        case (int)Enums.Fighters.KINTARO:
                            AnimPath = FIGHTER_ANI_DIR + Enum.GetName(typeof(Enums.Fighters), ochar) + "/" + Enum.GetName(typeof(Enums.Ani_IDs_Kintaro), Anim_ID);
                            if (!Directory.Exists(AnimPath))
                            {
                                Directory.CreateDirectory(AnimPath);
                            }
                            break;

                        case (int)Enums.Fighters.SHAO_KAHN:
                            AnimPath = FIGHTER_ANI_DIR + Enum.GetName(typeof(Enums.Fighters), ochar) + "/" + Enum.GetName(typeof(Enums.Ani_IDs_Kahn), Anim_ID);
                            if (!Directory.Exists(AnimPath))
                            {
                                Directory.CreateDirectory(AnimPath);
                            }
                            break;

                        default:
                            AnimPath = FIGHTER_ANI_DIR + Enum.GetName(typeof(Enums.Fighters), ochar) + "/" + Enum.GetName(typeof(Enums.Ani_IDs_Fighters), Anim_ID);
                            if (!Directory.Exists(AnimPath))
                            {
                                Directory.CreateDirectory(AnimPath);
                            }
                            break;
                    }

                    // GET ANIMATION POINTER
                    int Ani_Ptr = Tools.Get_Pointer(animations + (Anim_ID * 4));

                    // IF 0 GOTO NEXT ANIMATION
                    if (Ani_Ptr == 0) { continue; }

                    // GET FRAME PTRS HERE
                    int Frame = Ani_Ptr;
                    int Frame_Num = 0;
                    var Frames = new List<int>();

                    while (Tools.Get_Long(Frame) != 0)
                    {
                        // 0x7780 ANIMATION ROUTINE ARRAY LOC
                        int Frame_Ptr = Tools.Get_Pointer(Frame);
                        Frames.Add(Frame_Ptr);
                        switch (Frame_Ptr)
                        {
                            case 0:
                                break;
                            
                            case 1:
                                // NEXT LONG = WHERE TO JUMP FOR ANIMATION LOOP
                                int Ani_Command = ((int)((Tools.Get_Long(Frame+4) / 8) & 0xfffff));
                                int Frame_Jump = 0;

                                if (Ani_Ptr == Ani_Command)
                                {
                                    File.Open(AnimPath + "/1.0.end", FileMode.OpenOrCreate, FileAccess.Write);
                                }
                                else
                                {
                                    Frame_Jump = Frame_Num - ((Frame - Ani_Command) / 4);
                                    var file = File.OpenWrite(AnimPath + "/1." + Frame_Jump.ToString() + ".end");
                                }
                                break;

                            case 2:
                                // FLIP X
                                Frame += 4;
                                continue;

                            case 3:
                                // ADJUST POSITION
                                Frame+= 6;
                                continue;

                            case 4:
                                // ADJUST X AND Y POSITIONS
                                Frame+= 8;
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
                                continue;

                            case 8:
                                // NEXT DATA = (WORD)CHAR ID COMPARE FOR SHARED SPRITES IN NINJAS
                                // GET NEXT WORD (CHAR ID)
                                while (Tools.Get_Long(Frame) == 8)
                                {
                                    Frame+= 4;
                                    if (Tools.Get_Word(Frame) == ochar)
                                    {
                                        Frame = Tools.Get_Pointer(Frame + 2);
                                        break;
                                    }
                                    Frame+= 6;
                                }
                                break;

                            case 9:
                                break;
                        }
                    }

                    // IF DIR NON-EXISTENT, CREATE IT FOR ANIMATION
                    if (!Directory.Exists(AnimPath + "/" + Frame_Num.ToString()))
                    {
                        Directory.CreateDirectory(AnimPath + "/" + Frame_Num.ToString());
                    }

                    // SET SPECIFIC PALETTE FOR SPRITE CREATIONS
                    Choose_Palette(Frame_Num, Anim_ID, ochar);

                    // CHECK IF MULTISEGMENTED FRAME BY LOOKING AT *PTR
                    if(Imaging.Is_Frame_MultiSegmented(Frame))
                    {
                        // GET SEGMENT PTRS HERE
                        int Segment = Tools.Get_Pointer(Frame);
                        int Seg_Num = 0;
                        while (Tools.Get_Long(Segment) != 0)
                        {
                            // DRAW SEGMENT
                            SKBitmap image 
                        }
                    }

                }

            }
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
            if (frame_num== 0) { return; }

            switch (ochar)
            {
                case (int)Enums.Fighters.SHAO_KAHN:
                    switch (ani_id)
                    {
                        case (int)Enums.Ani_IDs_Kahn.A_STONE_CRACK | (int)Enums.Ani_IDs_Kahn.STONE_EXPLODE:
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
