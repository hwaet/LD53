using UnityEngine;

public static class TextureReadback {
    // The pixels of any texture, bottom row first, whether or not it is readable or compressed, by drawing it into a render texture.
    public static Color32[] Read(Texture2D texture) {
        RenderTexture previous = RenderTexture.active;
        RenderTexture temp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        Graphics.Blit(texture, temp);
        RenderTexture.active = temp;
        Texture2D readable = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false, true);
        readable.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        readable.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(temp);

        Color32[] pixels = readable.GetPixels32();
        if (Application.isPlaying) {
            Object.Destroy(readable);
        }
        else {
            Object.DestroyImmediate(readable);
        }
        return pixels;
    }
}
