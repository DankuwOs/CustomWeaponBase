public class HPEquipGunTurretCombined : HPEquipGunTurret
{
    
    public Gun gun2;

    public bool useCollective;

    private Gun originalGun;

    protected override void OnEquip()
    {
        base.OnEquip();
        originalGun = gun;

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
        gun = gun == originalGun ? gun2 : originalGun;
    }
}