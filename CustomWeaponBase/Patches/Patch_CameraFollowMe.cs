using System.IO;
using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(CameraFollowMe), nameof(CameraFollowMe.Screenshot))]
public class Patch_CameraFollowMe
{
    public static bool Prefix(CameraFollowMe __instance)
    {
        int num = 3840;
        int num2 = num / 16 * 9;
        RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 32);
        temporary.antiAliasing = 8;
            Camera camera = __instance.cam;
            if (camera.gameObject.activeSelf)
            {
                RenderTexture targetTexture = camera.targetTexture;
                camera.targetTexture = temporary;
                camera.Render();
                camera.targetTexture = targetTexture;
            }

            Texture2D texture2D = new Texture2D(num, num2, TextureFormat.RGB24, false);
        RenderTexture.active = temporary;
        texture2D.ReadPixels(new Rect(0f, 0f, (float)num, (float)num2), 0, 0);
        RenderTexture.ReleaseTemporary(temporary);
        string dir = Path.Combine(VTResources.gameRootDirectory, "Screenshots");
        byte[] png = texture2D.EncodeToPNG();
        
        UnityEngine.Object.Destroy(texture2D);
        
        File.WriteAllBytes(FlybyCameraMFDPage.GetNewScreenshotFilepath(dir), png);
        
        return false;
    }
}