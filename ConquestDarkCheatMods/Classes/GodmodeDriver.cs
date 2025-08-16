using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using ConquestDarkCheatMods.Constants;
using ConquestDarkCheatMods.Helpers;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ConquestDarkCheatMods.Classes;

public class GodModeDriver : MonoBehaviour
{
    public static MelonLogger.Instance Log { get; set; }

    private Il2Cpp.GameMaster _gm;
    private Il2Cpp.Character _currentPlayer;
    private bool _scannedThisScene;

    private bool _godMode;
    private bool _showUi;

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private static bool Pressed(int vk) => (GetAsyncKeyState(vk) & 0x1) != 0;
    private const int VK_ESCAPE = 0x1B;

    private const int VK_NUMPAD1 = 0x61,
        VK_NUMPAD2 = 0x62,
        VK_NUMPAD3 = 0x63,
        VK_NUMPAD4 = 0x64,
        VK_NUMPAD5 = 0x65,
        VK_NUMPAD6 = 0x66;

    private ModSettings _settings = new ModSettings();
    private readonly ModSettings _defaults = new ModSettings();
    private PlayerSnapshot startingSnapshot;

    // UI instance
    private GodModeUI _ui;

    public GodModeDriver(System.IntPtr ptr) : base(ptr)
    {
    }

    void Awake()
    {
        try
        {
            _ui = new GodModeUI(Log);

            SceneManager.sceneLoaded += (UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;

            // Wire UI callbacks
            _ui.onApplyClicked = ApplyAllEditable;
            _ui.onResetClicked = () =>
            {
                _settings.CopyFrom(_defaults);
                _ui.SyncStringsFromValues(_settings);
            };
            _ui.onToggleGodModeClicked = () => _godMode = !_godMode;

            // Initialize UI text fields from current values
            _ui.SyncStringsFromValues(_settings);
        }
        catch (System.Exception ex)
        {
            MelonLogger.Warning(ex.ToString());
        }

        if (Log == null)
        {
            MelonLogger.Error("Logger is null!.");
            return;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= (UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        _gm = null;
        _currentPlayer = null;
        _scannedThisScene = false;
    }

    void Update()
    {
        try
        {
            if (!Application.isFocused) return;

            if (Pressed(VK_ESCAPE))
                _showUi = !_showUi;

            // Find the player once per scene
            if (!_scannedThisScene || _currentPlayer == null)
            {
                _scannedThisScene = true;
                var all = Resources.FindObjectsOfTypeAll<Il2Cpp.GameMaster>();
                if (all != null && all.Length > 0)
                {
                    _gm = all[0];
                    _currentPlayer = _gm?.gameLogic?.playerController?.currentPlayerCharacter;
                }
            }

            if (_currentPlayer == null) return;

            if (startingSnapshot is null)
            {
                var opts = new JsonSerializerOptions { WriteIndented = true };
                startingSnapshot = PlayerSnapshot.From(_currentPlayer); // Hotkeys
            }

            if (Pressed(VK_NUMPAD1))
            {
                // _godMode = !_godMode;
                // MelonLogger.Msg($"God mode → {(_godMode ? "ON" : "OFF")}");
                // _showUi = !_showUi;
            }

            if (Pressed(VK_NUMPAD2)) WithPlayer(p => p.attackSpeed = _settings.AttackSpeedBoost);
            if (Pressed(VK_NUMPAD3)) WithPlayer(p => p.blockChance = _settings.BlockChance);
            if (Pressed(VK_NUMPAD4)) WithPlayer(p => p.rareFind = _settings.RareFind);
            if (Pressed(VK_NUMPAD5))
                WithPlayer(p =>
                {
                    var aa = p.activeAutoAttackAbility;
                    if (aa != null) aa.abilityCooldown = _settings.AutoAttackCoolDown;
                });
            if (Pressed(VK_NUMPAD6)) WithPlayer(p => p.baseMovementSpeed = _settings.BaseMovementSpeed);

            if (_godMode)
            {
                WithPlayer(p =>
                {
                    try
                    {
                        p.maxLife = _settings.TargetHealth;
                        p.health = _settings.TargetHealth;
                    }
                    catch (System.Exception ex)
                    {
                        MelonLogger.Warning($"God-mode write failed: {ex.Message}");
                    }
                });
            }

            // Live apply from UI
            if (_ui.LiveApply)
                ApplyAllEditable();

            _ui.ClampToScreen();
        }
        catch (System.Exception ex)
        {
            MelonLogger.Warning(ex.ToString());
        }
    }

    void OnGUI()
    {
        try
        {
            if (!_showUi) return;
            bool canApply = _currentPlayer != null;
            _ui.DrawWindow(_settings, canApply, _godMode);
        }
        catch (System.Exception ex)
        {
            MelonLogger.Warning(ex.ToString());
        }
    }

    // ------- Logic helpers -------

    // Confirm we have player object before updating
    private void WithPlayer(System.Action<Il2Cpp.Character> fn)
    {
        try
        {
            if (_currentPlayer == null)
                _currentPlayer = _gm?.gameLogic?.playerController?.currentPlayerCharacter;

            if (_currentPlayer != null)
            {
                try
                {
                    fn(_currentPlayer);
                }
                catch (System.Exception ex)
                {
                    MelonLogger.Warning(ex.ToString());
                }
            }
        }
        catch (System.Exception ex)
        {
            MelonLogger.Warning(ex.ToString());
        }
    }

    private void ApplyAllEditable()
    {
        WithPlayer(p =>
        {
            try
            {
                p.attackSpeed = Mathf.Clamp(_settings.AttackSpeedBoost,
                    CheatUiConstants.AttackSpeed_Min, CheatUiConstants.AttackSpeed_Max);
                p.blockChance = Mathf.Clamp(_settings.BlockChance,
                    CheatUiConstants.BlockChance_Min, CheatUiConstants.BlockChance_Max);
                p.rareFind = Mathf.Clamp(_settings.RareFind,
                    CheatUiConstants.RareFind_Min, CheatUiConstants.RareFind_Max);
                p.baseMovementSpeed = Mathf.Clamp(_settings.BaseMovementSpeed,
                    CheatUiConstants.BaseMoveSpeed_Min, CheatUiConstants.BaseMoveSpeed_Max);

                var aa = p.activeAutoAttackAbility;
                if (aa != null)
                {
                    aa.abilityCooldown = Mathf.Clamp(_settings.AutoAttackCoolDown,
                        CheatUiConstants.AbilityCooldown_Min, CheatUiConstants.AbilityCooldown_Max);
                    var ss = aa.stats;
                    if (ss != null)
                    {
                        ss.criticalStrikeChance = Mathf.Clamp(_settings.CritChance,
                            CheatUiConstants.CritChance_Min, CheatUiConstants.CritChance_Max);
                        ss.criticalStrikeDamage = Mathf.Clamp(_settings.CritDamage,
                            CheatUiConstants.CritDamage_Min, CheatUiConstants.CritDamage_Max);
                        ss.piercingStrikeChance = Mathf.Clamp(_settings.PierceAmount,
                            CheatUiConstants.PierceAmount_Min, CheatUiConstants.PierceAmount_Max);
                        ss.projectileAmountMultiplier = Mathf.Clamp(_settings.ProjAmount,
                            CheatUiConstants.ProjAmount_Min, CheatUiConstants.ProjAmount_Max);
                        ss.chainedTargetsMultiplier = Mathf.Clamp(_settings.ChainTargets,
                            CheatUiConstants.ChainTargets_Min, CheatUiConstants.ChainTargets_Max);
                    }
                }

                if (_godMode)
                {
                    p.maxLife = Mathf.Clamp(_settings.TargetHealth,
                        CheatUiConstants.TargetHealth_Min, CheatUiConstants.TargetHealth_Max);
                    p.health = Mathf.Clamp(_settings.TargetHealth,
                        CheatUiConstants.TargetHealth_Min, CheatUiConstants.TargetHealth_Max);
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Warning(ex);
            }
        });
    }
}