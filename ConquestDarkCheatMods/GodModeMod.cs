using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using Il2CppInterop.Runtime.Injection;
using System.Runtime.InteropServices;
using ConquestDarkCheatMods.Classes;
using Il2Cpp;
using UnityEngine.Events;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(ConquestDarkCheatMods.GodModeMod), "ConquestDarkCheatMods", "1.0.0", "ksups")]
[assembly: MelonGame(null, null)]

namespace ConquestDarkCheatMods;

public class GodModeMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        try
        {
            ClassInjector.RegisterTypeInIl2Cpp<GodModeDriver>();

            var go = new GameObject("ConquestDark_GodModeDriver");
            Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            GodModeDriver.Log = LoggerInstance;
            go.AddComponent<GodModeDriver>();

            LoggerInstance.Msg("God mode driver injected.");
        }
        catch (System.Exception ex)
        {
            LoggerInstance.Error(ex.ToString());
        }
    }

}
