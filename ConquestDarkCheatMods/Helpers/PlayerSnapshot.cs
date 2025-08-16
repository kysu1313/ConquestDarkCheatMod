namespace ConquestDarkCheatMods.Helpers;

public sealed class PlayerSnapshot
{
    public string name;
    public int    health;
    public int    maxLife;
    public float  attackSpeed;
    public float  baseMovementSpeed;
    public float  blockChance;
    public float  rareFind;
    public float critChance;
    public float critDamage;
    public float    projAmount;
    public float    pierceAmount;
    public float    targetAmount;
    public float    chainTargets;
    public float  abilityCooldown;

    public V3 position;
    public V3 velocity;

    public static PlayerSnapshot From(Il2Cpp.Character p)
    {
        var snap = new PlayerSnapshot();
        snap.name             = p?.name?.ToString();
        snap.health           = (int)(p?.health ?? 0);
        snap.maxLife          = (int)(p?.maxLife ?? 0);
        snap.attackSpeed      = p?.attackSpeed      ?? 0f;
        snap.baseMovementSpeed= p?.baseMovementSpeed?? 0f;
        snap.blockChance      = p?.blockChance      ?? 0f;
        snap.rareFind         = p?.rareFind         ?? 0f;
        snap.abilityCooldown = p?.activeAutoAttackAbility.abilityCooldown ?? 0f;
        snap.critChance = p?.activeAutoAttackAbility.stats.criticalStrikeChance ?? 0f;
        snap.critDamage = p?.activeAutoAttackAbility.stats.criticalStrikeDamage ?? 0f;
        snap.projAmount = p?.activeAutoAttackAbility.stats.projectileAmountMultiplier ?? 0f;
        snap.pierceAmount = p?.activeAutoAttackAbility.stats.piercingStrikeChance ?? 0f;
        snap.targetAmount = p?.activeAutoAttackAbility.stats.targetAmountMultiplier ?? 0f;
        snap.chainTargets = p?.activeAutoAttackAbility.stats.chainedTargetsMultiplier ?? 0f;

        return snap;
    }
}

// Tiny vector DTO; no properties, no methods — just data.
public readonly struct V3
{
    public readonly float x, y, z;
    public V3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    public static V3 From(UnityEngine.Vector3 v) => new V3(v.x, v.y, v.z);
}