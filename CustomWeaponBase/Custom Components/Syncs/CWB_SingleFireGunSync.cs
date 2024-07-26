using UnityEngine;
using VTNetworking;
using VTOLVR.Multiplayer;

namespace CustomWeaponBase.Custom_Components.Syncs;

public class CWB_SingleFireGunSync : VTNetSyncRPCOnly
{
    public Gun gun;

    [Tooltip("Used for getting the name of the weapon for kill credits")]
    public HPEquippable equip;

    private MultiUserVehicleSync muvs;
    
    private bool useMuvs;
    
    private int remoteFireTfIdx;
    
    
    private bool listenedGun;

    private bool listenedGunMuvs;
    
    private bool destroyed;

    private bool isGunController => useMuvs ? muvs.IsLocalWeaponController() : base.isMine;


    public override void OnNetInitialized()
    {
        if (muvs)
            useMuvs = true;
        gun.isLocal = isMine;
        if (isMine || useMuvs)
        {
            if (useMuvs)
            {
                gun.isLocal = isGunController;
                ListenMuvs();
            }
            listenedGun = true;
            gun.OnFiredBullet += OnFiredBullet;
        }

        gun.weaponEntityID = id;
        if (equip)
        {
            VTOLMPSceneManager.instance.RegisterWeaponName(base.id, equip.shortName);
            return;
        }
        VTOLMPSceneManager.instance.RegisterWeaponName(base.id, "Gun");

    }

    private void OnFiredBullet(Ray ray, float speed)
    {
        if (!isGunController)
            return;

        int nsv;
        Vector3 offset;
        
        FloatingOrigin.WorldToNetPoint(ray.origin, out nsv, out offset);
        
        SendRPC("RPC_OnFiredBullet", offset, nsv, speed * ray.direction, VTNetworkManager.GetNetworkTimestamp());
    }

    private void OnDestroy()
    {
        destroyed = true;
        
        if (!useMuvs || !muvs) return;
        
        muvs.OnSetWeaponControllerId -= Muvs_OnSetWeaponControllerId;
        muvs.OnOccupantEntered -= Muvs_OnOccupantEntered;
    }
    
    private void Gun_OnInfReloaded()
    {
        if (muvs)
        {
            muvs.SendRPCToCopilots(this, "RPC_SetAmmo", gun.currentAmmo);
        }
    }


    public void SetMuvs(MultiUserVehicleSync muvs)
    {
        this.muvs = muvs;
        useMuvs = muvs != null;
        if (!listenedGun)
        {
            listenedGun = true;
        }
        if (useMuvs)
        {
            ListenMuvs();
        }
        gun.isLocal = isGunController;
    }
    
    private void Muvs_OnSetWeaponControllerId(ulong obj)
    {
        gun.isLocal = isGunController;
    }

    private void Muvs_OnOccupantEntered(int seatIdx, ulong userID)
    {
        if (userID != BDSteamClient.mySteamID)
        {
            base.SendDirectedRPC(userID, "RPC_SetAmmo", gun.currentAmmo);
        }
    }
    
    private void ListenMuvs()
    {
        if (listenedGunMuvs) return;
        
        listenedGunMuvs = true;
        muvs.OnSetWeaponControllerId += Muvs_OnSetWeaponControllerId;
        if (isMine)
        {
            muvs.OnOccupantEntered += Muvs_OnOccupantEntered;
        }
        
        gun.OnInfReloaded += Gun_OnInfReloaded;
    }

    
    [VTRPC]
    private void RPC_SetAmmo(int ct)
    {
        gun.currentAmmo = ct;
        if (equip && equip.weaponManager)
        {
            equip.weaponManager.RefreshWeapon();
        }
    }

    [VTRPC]
    private void RPC_OnFiredBullet(Vector3 offset, int nsv, Vector3 vel, float t)
    {
        if (destroyed)
            return;
        
        gun.FireBullet();
        
        var wPos = FloatingOrigin.NetToWorldPoint(offset, nsv);
        var timestamp = VTNetworkManager.GetNetworkTimestamp() - t;
        var origin = wPos + timestamp * vel + 0.5f * timestamp * timestamp * Physics.gravity;
        var speed = gun.bulletInfo.speed;
        var dir = vel.normalized * speed;
        var inheritVel = vel - dir;
        var origPos = gun.fireTransforms[remoteFireTfIdx].position;
        
        Bullet.FireBulletWithOrigin(origin, dir, speed, gun.bulletInfo.tracerWidth, 0, 0, 0, inheritVel, gun.bulletInfo.color, gun.actor, origPos, gun.bulletInfo.detonationRange, gun.bulletInfo.maxLifetime - timestamp, gun.bulletInfo.lifetimeVariance);

        remoteFireTfIdx = (remoteFireTfIdx + 1) % gun.fireTransforms.Length;
    }

}