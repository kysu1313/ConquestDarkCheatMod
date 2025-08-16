using UnityEngine.Windows.WebCam;

namespace ConquestDarkCheatMods.Constants;
public static class CheatUiConstants
    {
        // ---------- Core ----------
        public const int   TargetHealth_Default   = 200;     // close to your 163
        public const int   TargetHealth_Min       = 0;
        public const int   TargetHealth_Max       = 1_000;   // avoid giant numbers while debugging
        public const int   TargetHealth_Step      = 10;
        public const int   TargetHealth_BigStep   = 50;

        public const float AttackSpeed_Default    = 1f;      // snapshot
        public const float AttackSpeed_Min        = 0.1f;
        public const float AttackSpeed_Max        = 10f;     // plenty fast, but safe
        public const float AttackSpeed_Step       = 0.1f;
        public const float AttackSpeed_BigStep    = 0.5f;

        public const float BaseMoveSpeed_Default  = 2f;      // snapshot
        public const float BaseMoveSpeed_Min      = 0f;
        public const float BaseMoveSpeed_Max      = 20f;     // keep sane
        public const float BaseMoveSpeed_Step     = 0.1f;
        public const float BaseMoveSpeed_BigStep  = 0.5f;

        public const float AbilityCooldown_Default= 0.35f;   // snapshot
        public const float AbilityCooldown_Min    = 0f;
        public const float AbilityCooldown_Max    = 10f;
        public const float AbilityCooldown_Step   = 0.05f;
        public const float AbilityCooldown_Big    = 0.25f;

        public const float BlockChance_Default    = 0f;      // treat as 0..1 (not %)
        public const float BlockChance_Min        = 0f;
        public const float BlockChance_Max        = 1f;
        public const float BlockChance_Step       = 0.05f;
        public const float BlockChance_BigStep    = 0.20f;

        public const float RareFind_Default       = 0.2f;    // snapshot
        public const float RareFind_Min           = 0f;
        public const float RareFind_Max           = 1f;
        public const float RareFind_Step          = 0.05f;
        public const float RareFind_BigStep       = 0.20f;

        // ---------- Extras ----------
        public const float CritChance_Default     = 0f;      // 0..1
        public const float CritChance_Min         = 0f;
        public const float CritChance_Max         = 1f;
        public const float CritChance_Step        = 0.01f;
        public const float CritChance_BigStep     = 0.10f;

        public const float CritDamage_Default     = 0f;      // multiplier-ish
        public const float CritDamage_Min         = 0f;
        public const float CritDamage_Max         = 5f;      // = +500%
        public const float CritDamage_Step        = 0.1f;
        public const float CritDamage_BigStep     = 0.5f;

        public const int   ProjAmount_Default     = 0;
        public const int   ProjAmount_Min         = 0;
        public const int   ProjAmount_Max         = 50;
        public const int   ProjAmount_Step        = 1;
        public const int   ProjAmount_BigStep     = 5;

        public const int   PierceAmount_Default   = 0;
        public const int   PierceAmount_Min       = 0;
        public const int   PierceAmount_Max       = 50;
        public const int   PierceAmount_Step      = 1;
        public const int   PierceAmount_BigStep   = 5;

        public const int   TargetAmount_Default   = 0;
        public const int   TargetAmount_Min       = 0;
        public const int   TargetAmount_Max       = 25;
        public const int   TargetAmount_Step      = 1;
        public const int   TargetAmount_BigStep   = 5;

        public const int   ChainTargets_Default   = 0;
        public const int   ChainTargets_Min       = 0;
        public const int   ChainTargets_Max       = 25;
        public const int   ChainTargets_Step      = 1;
        public const int   ChainTargets_BigStep   = 5;
    }