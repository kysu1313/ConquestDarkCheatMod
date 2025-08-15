using System.Runtime.InteropServices;
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

    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);
    private static bool Pressed(int vk) => (GetAsyncKeyState(vk) & 0x1) != 0;
    private const int VK_ESCAPE = 0x1B;
    private const int VK_NUMPAD1 = 0x61, VK_NUMPAD2 = 0x62, VK_NUMPAD3 = 0x63, VK_NUMPAD4 = 0x64, VK_NUMPAD5 = 0x65, VK_NUMPAD6 = 0x66;

    private ModSettings _settings = new ModSettings();
    private readonly ModSettings _defaults = new ModSettings();

    // UI instance
    private GodModeUI _ui;

    public GodModeDriver(System.IntPtr ptr) : base(ptr) {}

    void Awake()
    {
        if (Log == null)
        {
            MelonLogger.Error("Logger is null!.");
            return;
        }
            
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

        // Hotkeys
        if (Pressed(VK_NUMPAD1))
        {
            _godMode = !_godMode;
            MelonLogger.Msg($"God mode → {(_godMode ? "ON" : "OFF")}");
        }
        if (Pressed(VK_NUMPAD2)) WithPlayer(p => p.attackSpeed = _settings.AttackSpeedBoost);
        if (Pressed(VK_NUMPAD3)) WithPlayer(p => p.blockChance = _settings.BlockChance);
        if (Pressed(VK_NUMPAD4)) WithPlayer(p => p.rareFind = _settings.RareFind);
        if (Pressed(VK_NUMPAD5)) WithPlayer(p => { var aa = p.activeAutoAttackAbility; if (aa != null) aa.abilityCooldown = _settings.AutoAttackCoolDown; });
        if (Pressed(VK_NUMPAD6)) WithPlayer(p => p.baseMovementSpeed = _settings.BaseMovementSpeed);

        if (_godMode)
        {
            WithPlayer(p =>
            {
                try
                {
                    p.maxLife = _settings.TargetHealth;
                    p.health  = _settings.TargetHealth;
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

    void OnGUI()
    {
        if (!_showUi) return;
        bool canApply = _currentPlayer != null;
        _ui.DrawWindow(_settings, canApply, _godMode);
    }

    // ------- Logic helpers -------

    // Confirm we have player object before updating
    private void WithPlayer(System.Action<Il2Cpp.Character> fn)
    {
        if (_currentPlayer == null)
            _currentPlayer = _gm?.gameLogic?.playerController?.currentPlayerCharacter;

        if (_currentPlayer != null)
        {
            try { fn(_currentPlayer); }
            catch (System.Exception ex) { MelonLogger.Warning(ex.ToString()); }
        }
    }

    private void ApplyAllEditable()
    {
        WithPlayer(p =>
        {
            try
            {
                p.attackSpeed = _settings.AttackSpeedBoost;
                p.blockChance = _settings.BlockChance;
                p.rareFind = _settings.RareFind;
                p.baseMovementSpeed = _settings.BaseMovementSpeed;
                    
                var aa = p.activeAutoAttackAbility;
                if (aa != null)
                {
                    aa.abilityCooldown = _settings.AutoAttackCoolDown;
                    var ss = aa.stats;
                    if (ss != null)
                    {
                        ss.criticalStrikeChance = _settings.CritChance;
                        ss.criticalStrikeDamage = _settings.CritDamage;
                        ss.piercingStrikeChance = _settings.PierceAmount;
                        ss.projectileAmountMultiplier = _settings.ProjAmount;
                        ss.chainedTargetsMultiplier = _settings.ChainTargets;
                    }
                }

                if (_godMode)
                {
                    p.maxLife = _settings.TargetHealth;
                    p.health  = _settings.TargetHealth;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Warning($"Apply failed: {ex.Message}");
            }
        });
    }
}