
using CustomWeaponBase;
using Harmony;

[HarmonyPatch(typeof(PlayerVehicleSetup), nameof(PlayerVehicleSetup.SetupForFlight))]
    public class Patch_PlayerVehicleSetup
    {
        public static void Postfix()
        {
            if (VehicleEquipper.loadoutSet)
            {
                CustomWeaponsBase.instance.Loadout = VehicleEquipper.loadout;
            }
        }
    }