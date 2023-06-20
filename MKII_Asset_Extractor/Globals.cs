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
            ANI_65_SCORPION_SLICED_ME			// 65 F
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
