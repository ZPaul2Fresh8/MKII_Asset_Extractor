using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKII_Asset_Extractor
{
    public static class Globals
    {
        public const string FILE_PROGRAM = "mk2.program";
        public const string FILE_GRAPHICS = "mk2.graphics";
        public const string FILE_SOUNDS = "mk2.sounds";
        public const string FILE_HEADERS = "gfx_headers.txt";
        public const string PATH_IMAGES = "Images/";
        public const string PATH_SOUNDS = "Sounds/";

        public static List<byte> PRG = new List<byte>();
        public static List<byte> GFX = new List<byte>();
        public static List<SKColor> PALETTE = new List<SKColor>();

        public static int GFX_BYTES_EXTRACTED = 0;
    }

    public class Constants
    {
        public const int M_PALS = 987712;   // MK2+ Specific
        public const int A_PALS = 987824;   // MK2+ Specific
        public const int ANI_PITFALLS = 48342;

        public static int[] m_palettes =
        {
            M_HH = Tools.Get_Pointer(M_PALS),
            M_LK = Tools.Get_Pointer(M_PALS + 4),
            M_JC = Tools.Get_Pointer(M_PALS + 8),
            M_SA = Tools.Get_Pointer(M_PALS + 12),
            M_KT = Tools.Get_Pointer(M_PALS + 16),
            M_ML = Tools.Get_Pointer(M_PALS + 20),
            M_ST = Tools.Get_Pointer(M_PALS + 24),
            M_RD = Tools.Get_Pointer(M_PALS + 28),
            M_SZ = Tools.Get_Pointer(M_PALS + 32),
            M_RP = Tools.Get_Pointer(M_PALS + 36),
            M_SC = Tools.Get_Pointer(M_PALS + 40),
            M_JX = Tools.Get_Pointer(M_PALS + 44),
            M_GO = Tools.Get_Pointer(M_PALS + 48),
            M_SK = Tools.Get_Pointer(M_PALS + 52),
            M_SM = Tools.Get_Pointer(M_PALS + 56),
            M_NS = Tools.Get_Pointer(M_PALS + 60),
            M_JD = Tools.Get_Pointer(M_PALS + 64),
            M_HB = Tools.Get_Pointer(M_PALS + 68)
        };

        // palettes
        public static int M_HH;
        public static int M_LK;
        public static int M_JC;
        public static int M_SA;
        public static int M_KT;
        public static int M_ML;
        public static int M_ST;
        public static int M_RD;
        public static int M_SZ;
        public static int M_RP;
        public static int M_SC;
        public static int M_JX;
        public static int M_GO;
        public static int M_SK;
        public static int M_SM;
        public static int M_NS;
        public static int M_JD;
        public static int M_HB;

        // alt palettes
        public static int A_HH = Tools.Get_Pointer(A_PALS);
        public static int A_LK = Tools.Get_Pointer(A_PALS + 4);
        public static int A_JC = Tools.Get_Pointer(A_PALS + 8);
        public static int A_SA = Tools.Get_Pointer(A_PALS + 12);
        public static int A_KT = Tools.Get_Pointer(A_PALS + 16);
        public static int A_ML = Tools.Get_Pointer(A_PALS + 20);
        public static int A_ST = Tools.Get_Pointer(A_PALS + 24);
        public static int A_RD = Tools.Get_Pointer(A_PALS + 28);
        public static int A_SZ = Tools.Get_Pointer(A_PALS + 32);
        public static int A_RP = Tools.Get_Pointer(A_PALS + 36);
        public static int A_SC = Tools.Get_Pointer(A_PALS + 40);
        public static int A_JX = Tools.Get_Pointer(A_PALS + 44);
        public static int A_GO = Tools.Get_Pointer(A_PALS + 48);
        public static int A_SK = Tools.Get_Pointer(A_PALS + 52);
        public static int A_SM = Tools.Get_Pointer(A_PALS + 56);
        public static int A_NS = Tools.Get_Pointer(A_PALS + 60);
        public static int A_JD = Tools.Get_Pointer(A_PALS + 64);
        public static int A_HB = Tools.Get_Pointer(A_PALS + 68);
    }

    /// <summary>
    /// struct for manual frame extraction option
    /// </summary>
    public struct ME_Table
    {
        public string folder;
        public string subfolder;
        public int frameloc;
        public int pal_loc;
        public bool multi_frame;
    }

	public static class Enums
	{
        public enum Fighters
        {
            KUNG_LAO,
            LIU_KANG,
            CAGE,
            BARAKA,
            KITANA,
            MILEENA,
            SHANG_TSUNG,
            RAIDEN,
            SUBZERO,
            REPTILE,
            SCORPION,
            JAX,
            KINTARO,
            SHAO_KAHN,
            SMOKE,
            NOOB_SAIBOT,
            JADE
        }

        public enum Ani_IDs_Fighters
        {
            ANI_00_STANCE,                      // 0
            ANI_01_WALK_FWD,                    // 1
            ANI_02_SKIP_FWD,                    // 2
            ANI_03_WALK_BWD,                    // 3
            ANI_04_SKIP_BWD,                    // 4
            ANI_05_DUCK,                        // 5
            ANI_06_JUMP_UP,                     // 6
            ANI_07_FLIP_FORWARD,                // 7
            ANI_08_FLIP_BACKWARD,               // 8
            ANI_09_TURN_AROUND,                 // 9
            ANI_10_TURN_AROUND_CROUCHED,        // 10
            ANI_11_BLOCKING,                    // 11
            ANI_12_BLOCKING_CROUCHED,           // 12
            ANI_13_HIGH_KICK,                   // 13
            ANI_14_LOW_KICK,                    // 14
            ANI_15_KNOCKED_DOWN,                // 15
            ANI_16_HIT_HIGH,                    // 16
            ANI_17_HIT_LOW,                     // 17
            ANI_18_NORMAL_GET_UP,               // 18
            ANI_19_HIGH_PUNCH,                  // 19
            ANI_20_LOW_PUNCH,                   // 20
            ANI_21_SWEEPING,                    // 21
            ANI_22_SWEPT,                       // 22
            ANI_23_GET_UP_FROM_SWEPT,           // 23
            ANI_24_LOW_PUNCH_CROUCHED,          // 24
            ANI_25_HIGH_KICK_CROUCHED,          // 25
            ANI_26_LOW_KICK_CROUCHED,           // 26
            ANI_27_TAKING_HIT_CROUCHED,         // 27
            ANI_28_UPPERCUT,                    // 28
            ANI_29_JUMP_UP_KICK,                // 29
            ANI_30_FLYING_KICK,                 // 30
            ANI_31_FLYING_PUNCH,                // 31
            ANI_32_ROUND_HOUSE,                 // 32
            ANI_33_KNEE_TO_MID_SECTION,         // 33
            ANI_34_ELBOW_TO_FACE,               // 34
            ANI_35_STUMBLE_BACKWARDS,           // 35
            ANI_36_GRAB_OPPONENT_TO_THROW,      // 36
            ANI_37_SHREDDED,                    // 37
            ANI_38_THROW_PROJECTILE,            // 38
            ANI_39_PROJECTILE_OBJECT,           // 39
            ANI_40_STUNNED,                     // 40
            ANI_41_VICTORY_POSE,                // 41
            ANI_42_THROWN_BY_LAO,               // 42
            ANI_43_THROWN_BY_KANG,              // 43
            ANI_44_THROWN_BY_CAGE,              // 44
            ANI_45_THROWN_BY_BARAKA,            // 45
            ANI_46_THROWN_BY_KITANA,            // 46
            ANI_47_THROWN_BY_MILEENA,           // 47
            ANI_48_THROWN_BY_SHANG,             // 48
            ANI_49_THROWN_BY_RAIDEN,            // 49
            ANI_50_THROWN_BY_SUBZERO,           // 50
            ANI_51_THROWN_BY_REPTILE,           // 51
            ANI_52_THROWN_BY_SCORPION,          // 52
            ANI_53_THROWN_BY_JAX,               // 53
            ANI_54_LOW_BLOWED,                  // 54
            ANI_55_BICYCLE_KICKED,              // 55
            ANI_56_SOUL_DRAINED,                // 56
            ANI_57_TORSO_GETTING_RIPPED,        // 57 F
            ANI_58_SLOW_PROJ_BANG,              // 58
            ANI_59_GETTING_IMPALED,             // 59 F
            ANI_60_FALLING_FROM_DECAPITATION,   // 60 F
            ANI_61_DECAPITATED_HEAD_ROTATING,   // 61 F
            ANI_62_THROWN_BY_KINTARO,           // 62
            ANI_63_BACK_BREAKER,                // 63
            ANI_64_CHANGE,                      // 64
            ANI_65_SCORPION_SLICED_ME,			// 65 F
            //SPEC_ANI_00,			            // 
            //SPEC_ANI_01,            			// 
            //SPEC_ANI_02,			            // 
            //SPEC_ANI_03,			            // 
            //SPEC_ANI_04,			            // 
            //SPEC_ANI_05, 			            // 
            ANI_SPLIT_IN_2,                     // 66 F
            ANI_FALLING_FROM_TORSO_DECAP,       // 67
            ANI_PITFALL                         // 68
        }

        public enum Ani_IDs_Fighters2
        {
            SPEC_ANI_0,
            SPEC_ANI_1,
            SPEC_ANI_2,
            SPEC_ANI_3,
            SPEC_ANI_4,
            SPEC_ANI_5
        }

        public enum Ani_IDs_Kintaro
        {
            A_GSTANCE,                  // 0 - STANCE
            A_GWALK,                    // 1 - WALK
            A_GTURN,                    // 2 - TURN AROUND
            A_GORO_PUNCH,               // 3 - PUNCH
            A_GORO_KICK,                // 4 - KICK
            A_GORO_UPPERCUT,            // 5 - UPPERCUT
            A_GORO_SLAM,                // 6 - BODY SLAM
            A_GORO_ROAR,                // 7 - GORO ROAR !!!
            A_GORO_BLOCK,               // 8 - GORO BLOCK
            A_GORO_KDOWN,               // 9 - GORO KNOCKED DOWN
            A_GORO_STOMP,               // A - GORO STOMP
            A_GORO_GETUP,               // B - GORO GETUP
            A_GORO_HIT,                 // C - GORO HIT
            A_GORO_VICTORY,             // D - VICTORY
            A_GORO_UPCUTTED,            // E - GORO UPPERCUTTED
            A_GORO_ZAP,                 // F - PROJECTILE THROW
            A_GORO_STUMBLE,             // 10 - STUMBLE
            A_BIKE_KICKED,              // 11 - BIKE KICKED
            A_GORO_STUNNED,             // 12 - STUNNED
            A_GORO_NOOGIED,             // 13 -
            GORO_DUMMY,                 // 14 -
            A_SHREDDED,                 // 15 -
            A_GORO_ZAP_DUPE,            // 16 - PUKE A FIREBALL
            A_GORO_FIREBALL             // 17 - FIREBALL ANIMATION
        }

        public enum Ani_IDs_Kahn
        {
            A_SKSTANCE,                 // 0 = STANCE
            A_SKWALKF,                  // 1 = WALK FORWARD
            A_SKTURN,                   // 2 = TURNAROUND
            A_SKPUNCH,                  // 3 = PUNCH
            A_SKKICK,                   // 4 = KICK
            A_SKUPPERCUT,               // 5 = UPPERCUT
            A_SK_SPEAR,                 // 6 = SPEAR
            A_SK_SWEPT,                 // 7 = SWEPT
            A_SKBLOCK,                  // 8 = BLOCK
            A_SKKDOWN,                  // 9 = KNOCKED DOWN
            A_SKWALKB,                  // A = WALK BACKWARD
            A_SKGETUP,                  // B = GETUP
            A_SKHIT,                    // C = GETTING HIT
            A_SK_LAUGH,                 // D = VICTORY	
            KAHN_DUMMY,                 // E = 
            A_SK_ZAP,                   // F = PROJECTILE THROW
            A_SK_STUMBLE,               // 10 = STUMBLE
            A_BIKE_KICKED,              // 11 = BIKE KICKED
            KAHN_DUMMY_DUPE,            // 12 =
            A_SK_NOOGIED,               // 13 = NOOGY BY JAX
            A_SKCHARGE,                 // 14 = CHARGE ATTACK
            A_SHREDDED,                 // 15 =
            A_STONE_CRACK,              // 16 = STONE CRACKING !!
            STONE_EXPLODE,              // 17 =
            A_SK_TALKUP,                // 18 =
            A_SK_TALKDOWN,              // 19 =
        }
    }
}
