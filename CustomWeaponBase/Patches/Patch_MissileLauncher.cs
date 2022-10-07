using Harmony;

[HarmonyPatch(typeof(MissileLauncher), nameof(MissileLauncher.FinallyFire))]
public class Patch_MissileLauncher
{
    public static void Postfix(MissileLauncher __instance)
    {
        var rotary = __instance.GetComponent<RotaryComponent>();
        if (!rotary)
            return;

        rotary.fireCount++;
        
        rotary.StartRotating();
    }
}