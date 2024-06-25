using VTNetworking;


public class BurstMissileSync : VTNetSyncRPCOnly
{
    public BurstMissile burstMissile;

    public override void OnNetInitialized()
    {
        base.OnNetInitialized();
        if (netEntity == null)
            return;
        
        burstMissile.OnBurst.AddListener(OnBurst);
    }

    private void OnBurst()
    {
        if (!isMine)
            return;
        
        SendRPC("BurstMissile_Burst");
    }

    [VTRPC]
    private void BurstMissile_Burst()
    {
        if (isMine)
            return;
        
        burstMissile.OnBurst.Invoke();
    }
}