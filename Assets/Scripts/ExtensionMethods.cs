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

    public static float MatchPercentage(this Texture2D tex, Texture2D other) {
        float total = 0f;

        if(tex.width != other.width || tex.height != other.height) {
            return 0f;
        }

        for(int x = 0; x < tex.width; x++) {
            for(int y = 0; y < tex.height; y++) {
                if(tex.GetPixel(x, y) == other.GetPixel(x, y)) {
                    total++;
                }
            }
        }
        return total/(tex.width*tex.height);
    }

}
