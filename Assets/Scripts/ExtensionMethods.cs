using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods {
    
    public static Color GetPixel(this Texture2D tex, Vector2Int point) {
        return tex.GetPixel(point.x, point.y);
    }

    public static void SetPixel(this Texture2D tex, Vector2Int point, Color color) {
        tex.SetPixel(point.x, point.y, color);
    }

}
