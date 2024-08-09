using HarmonyLib;


[HarmonyPatch(typeof(FlightSceneManager), nameof(FlightSceneManager.Awake))]
public class Patch_FlightSceneManager
{
    public static void Postfix()
    {
        if (CustomWeaponsBase.instance)
        {
            FlightSceneManager.instance.OnExitScene += CustomWeaponsBase.instance.DestroyDetachedObjects; // Don't know if FSM is ever destroyed so doing this..
        }
    }
}