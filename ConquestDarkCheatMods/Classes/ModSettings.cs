using ConquestDarkCheatMods.Constants;

namespace ConquestDarkCheatMods;

public class ModSettings
{
    public int   TargetHealth        = CheatUiConstants.TargetHealth_Default;
    public float AttackSpeedBoost    = CheatUiConstants.AttackSpeed_Default;
    public float BaseMovementSpeed   = CheatUiConstants.BaseMoveSpeed_Default;
    public float AutoAttackCoolDown  = CheatUiConstants.AbilityCooldown_Default;
    public float BlockChance         = CheatUiConstants.BlockChance_Default;
    public float RareFind            = CheatUiConstants.RareFind_Default;
    public float CritChance          = CheatUiConstants.CritChance_Default;
    public float CritDamage          = CheatUiConstants.CritDamage_Default;
    public int   ProjAmount          = CheatUiConstants.ProjAmount_Default;
    public int   PierceAmount        = CheatUiConstants.PierceAmount_Default;
    public int   TargetAmount        = CheatUiConstants.TargetAmount_Default;
    public int   ChainTargets        = CheatUiConstants.ChainTargets_Default;

    public void CopyFrom(ModSettings s) { /* unchanged */ }
}