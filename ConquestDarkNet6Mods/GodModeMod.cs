using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using Il2CppInterop.Runtime.Injection;
using System.Runtime.InteropServices;
using Il2Cpp;
using UnityEngine.Events;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(ConquestDarkNet6Mods.GodModeMod), "ConquestDarkNet6Mods", "1.0.0", "ksups")]
[assembly: MelonGame(null, null)]

namespace ConquestDarkNet6Mods;

public class GodModeMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        ClassInjector.RegisterTypeInIl2Cpp<GodModeDriver>();

        var go = new GameObject("ConquestDark_GodModeDriver");
        Object.DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        go.AddComponent<GodModeDriver>();

        LoggerInstance.Msg("God mode driver injected.");
    }
}
