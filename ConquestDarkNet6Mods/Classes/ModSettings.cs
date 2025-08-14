namespace ConquestDarkNet6Mods;

public class ModSettings
{
    public int   TargetHealth        = 99999;
    public float AttackSpeedBoost    = 100f;
    public float BlockChance         = 100f;
    public float RareFind            = 100f;
    public float AutoAttackCoolDown  = 0f;
    public float BaseMovementSpeed   = 100f;

    // Extras
    public float CritChance          = 1f;
    public float CritDamage          = 10f;
    public int   ProjAmount          = 50;
    public int   PierceAmount        = 99;
    public int   TargetAmount        = 25;
    public int   ChainTargets        = 25;

    public void CopyFrom(ModSettings other)
    {
        TargetHealth       = other.TargetHealth;
        AttackSpeedBoost   = other.AttackSpeedBoost;
        BlockChance        = other.BlockChance;
        RareFind           = other.RareFind;
        AutoAttackCoolDown = other.AutoAttackCoolDown;
        BaseMovementSpeed  = other.BaseMovementSpeed;
        CritChance         = other.CritChance;
        CritDamage         = other.CritDamage;
        ProjAmount         = other.ProjAmount;
        PierceAmount       = other.PierceAmount;
        TargetAmount       = other.TargetAmount;
        ChainTargets       = other.ChainTargets;
    }
}