using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class ExtensionMethods {
    
    public static Color GetPixel(this Texture2D tex, Vector2Int point) {
        return tex.GetPixel(point.x, point.y);
    }

    public static void SetPixel(this Texture2D tex, Vector2Int point, Color color) {
        tex.SetPixel(point.x, point.y, color);
    }

    public static Color[,] GetStampTemplate(this Texture2D tex) {
        Color[,] ret = new Color[tex.width, tex.height];
        for(int x = 0; x < tex.width; x++) {
            for(int y = 0; y < tex.height; y++) {
                ret[x, y] = Color.clear;
            }
        }
        return ret;
    }

    public static Color[,] GetColorArray(this Texture2D tex) {
        Color[,] ret = new Color[tex.width, tex.height];
        for(int x = 0; x < tex.width; x++) {
            for(int y = 0; y < tex.height; y++) {
                ret[x, y] = tex.GetPixel(x, y);
            }
        }
        return ret;
    }

    public static float MatchPercentage(this Texture2D tex, Texture2D other) {
        float total = 0f;

        if(tex.width != other.width || tex.height != other.height) {
            Debug.Log("Texture Dimension Mistmatch");
            return 0f;
        }

        for(int x = 0; x < tex.width; x++) {
            for(int y = 0; y < tex.height; y++) {
                Color a = tex.GetPixel(x, y);
                Color b = other.GetPixel(x, y);

				if (tex.GetPixel(x, y) == other.GetPixel(x, y)) {
                    total++;
                }
            }
        }
        return total/(tex.width*tex.height);
    }



    public static RectInt ConcentricRectInt (this RectInt rect, int padding) {
        return new RectInt(rect.x + padding, rect.y - padding, rect.width - padding * 2, rect.height - padding * 2);
    }
}
