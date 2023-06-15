using Steamworks;
using Steamworks.Data;
using System;
using System.Threading.Tasks;
using UnityEngine;

public static class SteamAvatarTest
{
    public static async Task<Image?> GetAvatar(SteamId id)
    {
        try
        {
            // Get Avatar using await
            return await SteamFriends.GetLargeAvatarAsync(id);
        }
        catch (Exception e)
        {
            // If something goes wrong, log it
            Debug.Log(e);
            return null;
        }
    }

    public static Texture2D Convert(Image? image)
    {
        if (image == null)
            return null;

        // Create a new Texture2D
        var avatar = new Texture2D((int)image.Value.Width, (int)image.Value.Height, TextureFormat.ARGB32, false);

        // Set filter type, or else it's really blurry
        avatar.filterMode = FilterMode.Trilinear;

        // Flip image
        for (int x = 0; x < image.Value.Width; x++)
        {
            for (int y = 0; y < image.Value.Height; y++)
            {
                var p = image.Value.GetPixel(x, y);
                avatar.SetPixel(x, (int)image.Value.Height - y, new UnityEngine.Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
            }
        }

        avatar.Apply();
        return avatar;
    }
}
