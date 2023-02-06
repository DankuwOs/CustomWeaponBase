using VTNetworking;


public class BurstMissileSync : VTNetSyncRPCOnly
{
    public BurstMissile burstMissile;
    
    protected override void OnNetInitialized()
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
        burstMissile.OnBurst.Invoke();
    }
}