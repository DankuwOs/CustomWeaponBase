using VTNetworking;

namespace CustomWeaponBase
{
    public class VehicleCompatibility
    {
        // This code is from Temperz87's NotBDArmory: https://github.com/Temperz87/NotBDArmory
        public static bool CompareTo(VehicleCompat compatability, VTOLVehicles vehicle) // is this overengineered? yes. Do I care? Yes ;(
        {
            VehicleCompat bitMask = convert(vehicle);
            bool
                compatFlag = ((int) (compatability & bitMask) !=
                              0); // we compare & and if it isn't 0 we can assume that one of our selected masks is true, and since don't EVER want to flag and exclude to be the same so xor 
            return compatFlag;
        }

        public static VehicleCompat convert(VTOLVehicles v)
        {
            if (v == VTOLVehicles.AV42C)
                return VehicleCompat.AV42C;
            else if (v == VTOLVehicles.FA26B)
                return VehicleCompat.FA26B;
            else if (v == VTOLVehicles.F45A)
                return VehicleCompat.F45A;
            else if (v == VTOLVehicles.AH94)
                return VehicleCompat.AH94;
            return VehicleCompat.None;
        }
    }

    public enum VehicleCompat // I'm just gonna leave this here, yes this is bad practice but come on
    {
        None = 0,
        AV42C = 1,
        FA26B = 2,
        F45A = 4,
        AH94 = 8
    }
}