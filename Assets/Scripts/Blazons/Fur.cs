using System.Collections.Generic;
using UnityEngine;

public enum Fur {
    Ermine,         // argent with sable ermine spots
    Ermines,        // sable with argent spots
    Erminois,       // or with sable spots
    Pean,           // sable with or spots
    Vair,           // rows of azure and argent bells, each row staggered half a bell from the last
    CounterVair     // vair with alternate rows turned over, so bells of one color meet base to base
}

// The fur tinctures. A fur is a pattern, not a color, so the drawers fill with a marker color for it (Tincture.Ermine and the rest) like any
// other tincture, and Resolve turns those pixels into the pattern. Resolve is called after each element is drawn, so what is left of a marker
// belongs to that element. Each separate patch of a marker is a shape to the fur: the pattern is scaled to the patch's bounding box and laid
// out from its top and middle, so an ermine roundel has small spots and an ermine field big ones. A painting resolves the same way, from the
// patches of marker color the player painted.
public static class Furs {
    // The first ermine spot of Madboy74's "Coa Illustration Ermine spots", released under CC0 1.0
    // (https://commons.wikimedia.org/wiki/File:Coa_Illustration_Ermine_spots.svg). Three discs over a splayed tail, y pointing down.
    const string ErmineSpotPath =
        "m 43.00302,11.9269 c 0,5.40625 -4.3877,9.79394 -9.794,9.79394 -5.4062,0 -9.7939,-4.38769 -9.7939,-9.79394 0,-5.40626 4.3877,-9.79393 9.7939,-9.79393 5.4063,0 9.794,4.38767 9.794,9.79393 z " +
        "m -14.1033,17.23734 c 0,5.40625 -4.3877,9.79394 -9.7939,9.79394 -5.4063,0 -9.794,-4.38769 -9.794,-9.79394 0,-5.40626 4.3877,-9.79394 9.794,-9.79394 5.4062,0 9.7939,4.38768 9.7939,9.79394 z " +
        "m 28.2066,0 c 0,5.40625 -4.3877,9.79394 -9.794,9.79394 -5.4062,0 -9.7939,-4.38769 -9.7939,-9.79394 0,-5.40626 4.3877,-9.79394 9.7939,-9.79394 5.4063,0 9.794,4.38768 9.794,9.79394 z " +
        "m -26.7629,-5.19236 4.6259,0.31903 c 0.1595,30.14784 23.1293,77.84205 23.1293,77.84205 L 43.66272,88.33514 c -3.9878,4.62586 -11.0861,13.79782 -11.0861,13.79782 0,0 -5.1044,-9.25172 -8.7732,-13.87758 " +
        "l -16.4298,13.71807 c 0,0 22.6508,-47.21567 22.9698,-78.00157 z";

    // Sizes are for a shape the size of the shield on a 256 pixel texture, and scale with the texture and with the shape.
    const float ErmineSpotHeight = 34f;
    const float ErmineAcross = 3.3f;    // from one spot to the next in a row, in spot widths
    const float ErmineDown = 1.25f;     // from one row to the next, in spot heights
    const float VairUnit = 14f;         // one unit of the vair bell's outline
    const float VairFlare = 1f;         // the bell flares out at 45 degrees for this many units, runs straight for the waist, then flares again
    const float VairWaist = 2.5f;
    const float MinimumScale = 0.2f;    // a pattern is never smaller than this fraction of its size on the whole shield, so tiny shapes still show it

    // The marker color of each fur, in the order of the enum.
    static readonly Color[] markers = { Tincture.Ermine, Tincture.Ermines, Tincture.Erminois, Tincture.Pean, Tincture.Vair, Tincture.CounterVair };
    static readonly Dictionary<int, Fur> markerKeys = BuildMarkerKeys();

    static Dictionary<int, Fur> BuildMarkerKeys() {
        Dictionary<int, Fur> keys = new Dictionary<int, Fur>();
        for (int i = 0; i < markers.Length; i++) {
            keys[Key((Color32)markers[i])] = (Fur)i;
        }
        return keys;
    }

    static int Key(Color32 c) {
        return c.r << 16 | c.g << 8 | c.b;
    }

    public static Color Marker(Fur fur) {
        return markers[(int)fur];
    }

    // Whether the color is a fur's marker.
    public static bool TryGetFur(Color color, out Fur fur) {
        return markerKeys.TryGetValue(Key((Color32)color), out fur);
    }

    // Replaces every pixel of a fur's marker color with the fur's pattern. Call Apply on the texture afterwards.
    public static Texture2D Resolve(Texture2D texture, Shield shield = null) {
        Color32[] pixels = texture.GetPixels32();
        if (Resolve(pixels, texture.width, shield ?? Shield.Default)) {
            texture.SetPixels32(pixels);
        }
        return texture;
    }

    // The same on an array of pixels, bottom row first, for a square texture. Returns whether there was anything to resolve.
    // Each patch of a marker (pixels of the same marker touching along an edge) is resolved on its own, scaled to the bounding box of the part of
    // it on the shield, or all of it if none is.
    public static bool Resolve(Color32[] pixels, int size, Shield shield) {
        int count = pixels.Length;
        sbyte[] furAt = null;
        for (int i = 0; i < count; i++) {
            Color32 p = pixels[i];
            Fur fur;
            if (p.a == 255 && markerKeys.TryGetValue(Key(p), out fur)) {
                if (furAt == null) {
                    furAt = new sbyte[count];
                    for (int j = 0; j < count; j++) {
                        furAt[j] = -1;
                    }
                }
                furAt[i] = (sbyte)fur;
            }
        }
        if (furAt == null) {
            return false;
        }

        int shieldWidth = shield.Bounds(size).width;
        bool[] done = new bool[count];
        List<int> patch = new List<int>();
        Stack<int> pending = new Stack<int>();
        for (int first = 0; first < count; first++) {
            if (furAt[first] < 0 || done[first]) {
                continue;
            }

            Fur fur = (Fur)furAt[first];
            patch.Clear();
            pending.Push(first);
            done[first] = true;
            int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
            int shieldMinX = int.MaxValue, shieldMinY = int.MaxValue, shieldMaxX = -1, shieldMaxY = -1;
            while (pending.Count > 0) {
                int i = pending.Pop();
                patch.Add(i);
                int x = i % size;
                int y = i / size;
                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
                if (shield.Contains(x, y, size)) {
                    shieldMinX = Mathf.Min(shieldMinX, x);
                    shieldMinY = Mathf.Min(shieldMinY, y);
                    shieldMaxX = Mathf.Max(shieldMaxX, x);
                    shieldMaxY = Mathf.Max(shieldMaxY, y);
                }
                if (x > 0) Visit(i - 1, fur, furAt, done, pending);
                if (x < size - 1) Visit(i + 1, fur, furAt, done, pending);
                if (y > 0) Visit(i - size, fur, furAt, done, pending);
                if (y < size - 1) Visit(i + size, fur, furAt, done, pending);
            }

            RectInt area = shieldMaxX >= 0
                ? new RectInt(shieldMinX, shieldMinY, shieldMaxX - shieldMinX + 1, shieldMaxY - shieldMinY + 1)
                : new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
            float relative = Mathf.Max(MinimumScale, Mathf.Sqrt((float)area.width * area.height) / shieldWidth);
            Layout layout = new Layout(area, size, relative);
            foreach (int i in patch) {
                pixels[i] = ColorAt(fur, i % size, i / size, layout);
            }
        }
        return true;
    }

    static void Visit(int index, Fur fur, sbyte[] furAt, bool[] done, Stack<int> pending) {
        if (!done[index] && furAt[index] == (sbyte)fur) {
            done[index] = true;
            pending.Push(index);
        }
    }

    public static Color ColorAt(Fur fur, int x, int y, Layout layout) {
        switch (fur) {
            case Fur.Ermine:
                return layout.OnErmineSpot(x, y) ? Tincture.Sable : Tincture.Argent;
            case Fur.Ermines:
                return layout.OnErmineSpot(x, y) ? Tincture.Argent : Tincture.Sable;
            case Fur.Erminois:
                return layout.OnErmineSpot(x, y) ? Tincture.Sable : Tincture.Or;
            case Fur.Pean:
                return layout.OnErmineSpot(x, y) ? Tincture.Or : Tincture.Sable;
            case Fur.Vair:
                return layout.OnVairBell(x, y, false) ? Tincture.Azure : Tincture.Argent;
            default:
                return layout.OnVairBell(x, y, true) ? Tincture.Azure : Tincture.Argent;
        }
    }

    // The patterns laid out in an area of a texture: where they start and how big they are. Cheap to make, and the ermine spot's mask is
    // shared between layouts of the same size.
    public class Layout {
        readonly float centreX;     // the middle of the shape, where the patterns are centred
        readonly float top;         // the top of the shape, where the first row starts
        readonly float scale;
        readonly SpotMask spot;
        readonly float spotAcross, spotDown;

        // the vair bell, in units, as a polygon with y pointing down: a point at the top and a base at the bottom
        static readonly Vector2[] bell = {
            new Vector2(0f, 0f),
            new Vector2(VairFlare, VairFlare),
            new Vector2(VairFlare, VairFlare + VairWaist),
            new Vector2(2f * VairFlare, 2f * VairFlare + VairWaist),
            new Vector2(-2f * VairFlare, 2f * VairFlare + VairWaist),
            new Vector2(-VairFlare, VairFlare + VairWaist),
            new Vector2(-VairFlare, VairFlare)
        };
        const float VairRow = 2f * VairFlare + VairWaist;   // height of a row in units
        const float VairPeriod = 4f * VairFlare;            // a bell and an upside down bell

        // `area` is the bounding box of the shape the fur fills. `relative` is how big that shape is compared to the whole shield, which the
        // patterns are scaled by, and `size` the size of the texture.
        public Layout(RectInt area, int size, float relative) {
            centreX = area.x + area.width / 2f;
            top = area.yMax;
            scale = size / 256f * relative;
            spot = SpotMask.For(Mathf.Max(4, Mathf.RoundToInt(ErmineSpotHeight * scale)));
            spotAcross = spot.width * ErmineAcross;
            spotDown = spot.height * ErmineDown;
        }

        // Rows of spots staggered by half a spot, with a spot in the middle of the shape in the first row.
        public bool OnErmineSpot(int x, int y) {
            float down = top - (y + 0.5f);
            if (down < 0f) {
                return false;
            }
            int row = Mathf.FloorToInt(down / spotDown);
            float localY = down - row * spotDown - (spotDown - spot.height) / 2f;
            float across = x + 0.5f - centreX + spotAcross / 2f - ((row & 1) == 1 ? spotAcross / 2f : 0f);
            float localX = across - Mathf.Floor(across / spotAcross) * spotAcross - (spotAcross - spot.width) / 2f;
            int mx = Mathf.FloorToInt(localX);
            int my = Mathf.FloorToInt(localY);
            return mx >= 0 && my >= 0 && mx < spot.width && my < spot.height && spot.mask[mx + my * spot.width];
        }

        // Azure bells point up in rows from the top, with argent bells between them upside down. Plain vair staggers alternate rows by half a
        // period so azure bases meet argent bases; counter vair turns alternate rows over so bases meet bases of the same color.
        public bool OnVairBell(int x, int y, bool counter) {
            float u = VairUnit * scale;
            float ux = (x + 0.5f - centreX) / u;
            float uy = (top - (y + 0.5f)) / u;
            int row = Mathf.FloorToInt(uy / VairRow);
            float ry = uy - row * VairRow;
            if (counter && (row & 1) == 1) {
                ry = Mathf.Min(VairRow - ry, VairRow - 0.0001f);
            }
            else if (!counter && (row & 1) == 1) {
                ux += VairPeriod / 2f;
            }
            float rx = Mathf.Repeat(ux, VairPeriod);
            // the bell next door can reach over the edge of the period
            for (int k = -1; k <= 1; k++) {
                if (InBell(rx - k * VairPeriod, ry)) {
                    return true;
                }
            }
            return false;
        }

        static bool InBell(float x, float y) {
            bool inside = false;
            for (int i = 0, j = bell.Length - 1; i < bell.Length; j = i++) {
                Vector2 a = bell[i];
                Vector2 b = bell[j];
                if ((a.y > y) != (b.y > y) && x < (b.x - a.x) * (y - a.y) / (b.y - a.y) + a.x) {
                    inside = !inside;
                }
            }
            return inside;
        }
    }

    // The ermine spot drawn at one height, row by row from the top.
    class SpotMask {
        public readonly int width, height;
        public readonly bool[] mask;

        static readonly Dictionary<int, SpotMask> cache = new Dictionary<int, SpotMask>();
        static List<Vector2[]> outline;
        static Rect outlineBounds;

        SpotMask(int spotHeight) {
            if (outline == null) {
                outline = SvgPath.Parse(ErmineSpotPath);
                outlineBounds = SvgPath.Bounds(outline);
            }
            height = spotHeight;
            width = Mathf.Max(2, Mathf.RoundToInt(height * outlineBounds.width / outlineBounds.height));
            mask = new bool[width * height];
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    float sx = outlineBounds.xMin + (x + 0.5f) / width * outlineBounds.width;
                    float sy = outlineBounds.yMin + (y + 0.5f) / height * outlineBounds.height;
                    mask[x + y * width] = SvgPath.InPolygons(outline, sx, sy);
                }
            }
        }

        public static SpotMask For(int spotHeight) {
            SpotMask spot;
            if (!cache.TryGetValue(spotHeight, out spot)) {
                spot = new SpotMask(spotHeight);
                cache[spotHeight] = spot;
            }
            return spot;
        }
    }
}
