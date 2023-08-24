
using VTOLVR.Multiplayer;

public class HPEquipGunTurretCombined : HPEquipGunTurret
{
    public Gun gun2;

    public bool useCollective;

    private Gun _originalGun;

    protected override void OnEquip()
    {
        base.OnEquip();
        
        _originalGun = gun;

        var vcm = weaponManager.GetComponent<VehicleControlManifest>();
        if (!vcm)
            return;

        if (useCollective)
        {
            foreach (var vcmCollective in vcm.collectives)
            {
                vcmCollective.OnStickPressDown.AddListener(delegate
                {
                    if (itemActivated)
                        ToggleGun();
                });
            }
        }
        else
        {
            foreach (var vcmJoystick in vcm.joysticks)
            {
                vcmJoystick.OnThumbstickButtonDown.AddListener(delegate
                {
                    if (itemActivated)
                        ToggleGun();
                });
            }
        }
    }

    public void ToggleGun()
    {
        gun = gun == _originalGun ? gun2 : _originalGun;
    }
}