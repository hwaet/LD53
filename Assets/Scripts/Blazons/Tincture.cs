//using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tincture {
    // https://en.wikipedia.org/wiki/Tincture_(heraldry)
    // Metal
    public static readonly Color Argent = new Color(1, 1, 1);
    public static readonly Color Or = new Color(1, .8f, 0);

    // Color
    public static readonly Color Gules = new Color(1, 0, 0);
    public static readonly Color Sable = new Color(0, 0, 0);
    public static readonly Color Azure = new Color(0, 0, 1);
    public static readonly Color Vert = new Color(0, .5f, 0);
    public static readonly Color Purpure = new Color(.5f, 0, .5f);

    // Stain
    public static readonly Color Tenne = new Color(.8f, .4f, 0);
    public static readonly Color Sanguine = new Color(178f / 255, 34f / 255, 34f / 255);
    public static readonly Color Murrey = new Color(139f / 255, 0, 75f / 255);

    // Other colors
    public static readonly Color Carnation = new Color(1, .6f, .6f);
    public static readonly Color Celeste = new Color(0, 1, 1);
    public static readonly Color Rose = new Color(1, .5f, .5f);
    public static readonly Color BleuCeleste = new Color(0, .5f, 1);
    public static readonly Color Brunatre = new Color(.5f, .2f, 0);
    public static readonly Color Orange = new Color(1, .5f, 0);

    // Furs. A fur is a pattern, so these are marker colors: drawing fills with one like any tincture and Furs.Resolve turns the pixels into
    // the pattern. They are exact 8 bit values so they survive being stored in a texture, and are well away from the other tinctures so a
    // painted color doesn't snap to one by accident. The swatch suggests the fur.
    public static readonly Color Ermine = new Color32(226, 226, 238, 255);
    public static readonly Color Ermines = new Color32(58, 58, 84, 255);
    public static readonly Color Erminois = new Color32(222, 188, 70, 255);
    public static readonly Color Pean = new Color32(84, 70, 26, 255);
    public static readonly Color Vair = new Color32(122, 150, 224, 255);
    public static readonly Color CounterVair = new Color32(66, 98, 178, 255);

    public static readonly Dictionary<string, Color> Tinctures = new Dictionary<string, Color>() {
        { "argent",  Argent },
        { "or",  Or },
        { "gules",  Gules },
        { "sable",  Sable },
        { "azure",  Azure },
        { "vert",  Vert },
        { "purpure",  Purpure },
        { "tenne",  Tenne },
        { "sanguine",  Sanguine },
        { "murrey",  Murrey },
        {"carnation", Carnation },
        {"celeste", Celeste },
        {"rose", Rose },
        {"bleuceleste", BleuCeleste },
        {"brunatre", Brunatre },
        {"orange", Orange },
        {"ermine", Ermine },
        {"ermines", Ermines },
        {"erminois", Erminois },
        {"pean", Pean },
        {"vair", Vair },
        {"counter-vair", CounterVair }
    };

    public static readonly Dictionary<string, Color> Metals = new Dictionary<string, Color>() {
        { "argent",  Argent },
        { "or",  Or }
    };

    public static readonly Dictionary<string, Color> Colours = new Dictionary<string, Color> {
        {"gules", Gules },
        {"sable", Sable },
        {"azure", Azure },
        {"vert", Vert },
        {"purpure", Purpure }
    };
    
    public static readonly Dictionary<string, Color> Stains = new Dictionary<string, Color> {
        {"tenne", Tenne },
        {"sanguine", Sanguine },
        {"murrey", Murrey }
    };

    public static readonly Dictionary<string, Color> OtherColors = new Dictionary<string, Color> {
        { "carnation", Carnation },
        {"celeste", Celeste },
        {"rose", Rose },
        {"bleuceleste", BleuCeleste },
        {"brunatre", Brunatre },
        {"orange", Orange }
    };

    // Ids for scoring: every tincture has a number, in the order of Tinctures starting at 1, and 0 is for nothing. A fur is one id.
    public const byte Empty = 0;
    static readonly Color32[] ordered = BuildOrdered();
    static readonly bool[] isFur = BuildIsFur();
    static readonly Dictionary<int, byte> idCache = new Dictionary<int, byte>();
    // how far a color can be from a fur's marker, squared, and still be it: a painted color may be a step or two off after going through a texture
    const int FurTolerance = 12;

    static Color32[] BuildOrdered() {
        List<Color32> colors = new List<Color32>();
        foreach (Color color in Tinctures.Values) {
            colors.Add(color);
        }
        return colors.ToArray();
    }

    static bool[] BuildIsFur() {
        Color[] furs = { Ermine, Ermines, Erminois, Pean, Vair, CounterVair };
        bool[] flags = new bool[ordered.Length];
        int i = 0;
        foreach (Color color in Tinctures.Values) {
            foreach (Color fur in furs) {
                if (color == fur) {
                    flags[i] = true;
                }
            }
            i++;
        }
        return flags;
    }

    // The id of the tincture nearest a color, so a pixel doesn't have to be exact. A fur only if the color is close to its marker, so the
    // in between colors at the edge of a stroke go to the ordinary tinctures and not to a fur that happens to be near them. Transparent is Empty.
    public static byte IdOf(Color32 color) {
        if (color.a == 0) {
            return Empty;
        }
        int key = color.r << 16 | color.g << 8 | color.b;
        byte id;
        if (idCache.TryGetValue(key, out id)) {
            return id;
        }
        int nearest = int.MaxValue;
        for (int i = 0; i < ordered.Length; i++) {
            int dr = color.r - ordered[i].r;
            int dg = color.g - ordered[i].g;
            int db = color.b - ordered[i].b;
            int distance = dr * dr + dg * dg + db * db;
            if (isFur[i]) {
                if (distance <= FurTolerance) {
                    id = (byte)(i + 1);
                    nearest = -1;
                    break;
                }
            }
            else if (distance < nearest) {
                nearest = distance;
                id = (byte)(i + 1);
            }
        }
        idCache[key] = id;
        return id;
    }

    public static byte IdOf(Color color) {
        return IdOf((Color32)color);
    }

    // The color of a tincture id, transparent for Empty.
    public static Color32 FromId(byte id) {
        return id == Empty || id > ordered.Length ? new Color32(0, 0, 0, 0) : ordered[id - 1];
    }

    public static Color ClosestTincture(Color color) {
        float minDistance = float.MaxValue;
        Color closestColor = color;
        foreach (KeyValuePair<string, Color> item in Tinctures) {
            Vector3 colorVec = new Vector3(color.r, color.g, color.b);
            Vector3 itemVec = new Vector3(item.Value.r, item.Value.g, item.Value.b);
            var distance = Vector3.Distance(colorVec, itemVec);
            if (distance < minDistance) {
                minDistance = distance;
                closestColor = item.Value;
            }
        }
        return closestColor;
    }
}
