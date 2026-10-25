using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// The shape of the shield blazons are drawn on, built from a square PNG whose alpha is the shield
// (opaque inside, transparent outside, top of the image is the chief). Everything shape dependent
// (the edge distance used by a bordure, where the charge points sit) comes from here, so a new shield
// is just a new PNG in Assets/Resources/Shields.
//
// All public queries take the size of the texture being drawn on (blazons are normally 256) and work in
// that texture's pixels, y up, like the rest of the drawers.
public class Shield {
    public const string DefaultName = "shieldTemplate";
    public const string ResourceFolder = "Shields";
    const float AlphaThreshold = 0.5f;

    // How much of a charge has to be inside the shield, as a fraction of its half size. 1 keeps the
    // circle inscribed in the charge's square inside the shield.
    const float ChargeClearance = 1f;

    public readonly string name;
    public readonly Texture2D source; // the PNG this was built from, null for the built in fallback
    readonly int resolution;        // the mask is resolution x resolution
    readonly bool[] inside;         // x + y * resolution, y up
    readonly float[] edgeDistance;  // pixels to the nearest outside pixel, 0 outside
    readonly RectInt maskBounds;    // tight bounds of the inside pixels, in mask pixels
    readonly Vector2 pole;          // the pixel furthest from the edge, in mask pixels

    #region Construction

    // alpha is row major, y up (like Texture2D.GetPixels), values 0..1.
    public Shield(string name, float[] alpha, int resolution, Texture2D source = null) {
        if (resolution < 2 || alpha.Length != resolution * resolution) {
            throw new ArgumentException("Shield mask must be a square, " + resolution + "x" + resolution + " needs " + resolution * resolution + " values but got " + alpha.Length);
        }

        this.name = name;
        this.source = source;
        this.resolution = resolution;
        inside = new bool[alpha.Length];

        int minX = resolution, minY = resolution, maxX = -1, maxY = -1;
        for (int y = 0; y < resolution; y++) {
            for (int x = 0; x < resolution; x++) {
                if (alpha[x + y * resolution] >= AlphaThreshold) {
                    inside[x + y * resolution] = true;
                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            }
        }
        if (maxX < 0) {
            throw new ArgumentException("Shield " + name + " has no opaque pixels, the shield should be opaque on a transparent background.");
        }
        maskBounds = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);

        edgeDistance = ComputeEdgeDistance(inside, resolution);
        pole = FindPole(edgeDistance, resolution, maskBounds);
    }

    // Builds a shield from the alpha channel of a texture. The texture doesn't need to be readable or uncompressed.
    public static Shield FromTexture(Texture2D texture) {
        if (texture.width != texture.height) {
            throw new ArgumentException("Shield texture " + texture.name + " must be square but is " + texture.width + "x" + texture.height);
        }

        // Go through a render texture so compressed or non readable imports work too
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
            UnityEngine.Object.Destroy(readable);
        }
        else {
            UnityEngine.Object.DestroyImmediate(readable);
        }

        float[] alpha = new float[pixels.Length];
        for (int i = 0; i < pixels.Length; i++) {
            alpha[i] = pixels[i].a / 255f;
        }
        return new Shield(texture.name, alpha, texture.width, texture);
    }

    // The original shield, rasterised from its measurements. Only used if the PNG can't be loaded.
    public static Shield CreateFallback() {
        const int size = 256;
        float[] alpha = new float[size * size];
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                bool inShield;
                if (y >= 127) {
                    inShield = x >= 4 && x <= 250 && y <= 251;
                }
                else {
                    float dx = x - 127f, dy = y - 127f;
                    inShield = dx * dx + dy * dy <= 123.5f * 123.5f;
                }
                alpha[x + y * size] = inShield ? 1f : 0f;
            }
        }
        return new Shield(DefaultName, alpha, size);
    }

    #endregion

    #region Library

    static readonly Dictionary<string, Shield> cache = new Dictionary<string, Shield>();

    public static Shield Default {
        get { return Get(DefaultName); }
    }

    // Looks for a PNG called `shieldName` in Resources/Shields. Falls back to the default shield if it's missing.
    public static Shield Get(string shieldName) {
        Shield shield;
        if (cache.TryGetValue(shieldName, out shield)) {
            return shield;
        }

        Texture2D texture = Resources.Load<Texture2D>(ResourceFolder + "/" + shieldName);
        if (texture != null) {
            shield = FromTexture(texture);
        }
        else if (shieldName != DefaultName) {
            Debug.LogWarningFormat("No shield called '{0}' in Resources/{1}, using the default.", shieldName, ResourceFolder);
            return Default;
        }
        else {
            Debug.LogWarningFormat("Default shield '{0}' not found in Resources/{1}, using a built in copy of it.", DefaultName, ResourceFolder);
            shield = CreateFallback();
        }
        cache[shieldName] = shield;
        return shield;
    }

    // Names of every shield PNG in Resources/Shields, the default first.
    public static string[] AllNames() {
        return Resources.LoadAll<Texture2D>(ResourceFolder)
                        .Select(t => t.name)
                        .Distinct()
                        .OrderBy(n => n == DefaultName ? 0 : 1)
                        .ThenBy(n => n)
                        .ToArray();
    }

    #endregion

    #region Shape queries

    // Maps a pixel of a `size` texture to the mask.
    int MaskIndex(int x, int y, int size) {
        int mx = Mathf.Clamp((int)((x + 0.5f) * resolution / size), 0, resolution - 1);
        int my = Mathf.Clamp((int)((y + 0.5f) * resolution / size), 0, resolution - 1);
        return mx + my * resolution;
    }

    public bool Contains(int x, int y, int size) {
        if (x < 0 || y < 0 || x >= size || y >= size) {
            return false;
        }
        return inside[MaskIndex(x, y, size)];
    }

    // Pixels from the centre of (x, y) to the edge of the shield, 0 if it's outside.
    public float DistanceToEdge(int x, int y, int size) {
        if (x < 0 || y < 0 || x >= size || y >= size) {
            return 0f;
        }
        float d = edgeDistance[MaskIndex(x, y, size)];
        // the edge is half a pixel closer than the nearest outside pixel's centre
        return d <= 0f ? 0f : (d - 0.5f) * size / resolution;
    }

    // Tight rectangle around the shield. y is the bottom edge, like a normal RectInt (not the top edge convention charges use).
    public RectInt Bounds(int size) {
        float s = (float)size / resolution;
        int x0 = Mathf.RoundToInt(maskBounds.xMin * s);
        int y0 = Mathf.RoundToInt(maskBounds.yMin * s);
        int x1 = Mathf.RoundToInt(maskBounds.xMax * s);
        int y1 = Mathf.RoundToInt(maskBounds.yMax * s);
        return new RectInt(x0, y0, x1 - x0, y1 - y0);
    }

    // The point furthest from the edge of the shield, the middle of the largest circle that fits.
    public Vector2 Pole(int size) {
        return pole * size / resolution;
    }

    // Maps a position in the shield's bounds, (0,0) bottom dexter to (1,1) top sinister, to a pixel.
    public Vector2 UnitToPixel(Vector2 unit, int size) {
        RectInt b = Bounds(size);
        return new Vector2(b.x + unit.x * b.width, b.y + unit.y * b.height);
    }

    // Where a named point of the shield is, as a pixel position. The fess point is the middle of the shield's bounds, the corner
    // points are the corners of its bounds (which may be outside the outline, as at the base of a rounded shield) and the
    // edge points are the middle of each side. Use PlaceCharge to put a charge at a point.
    public Vector2 PointPosition(Blazon.Point point, int size) {
        return UnitToPixel(PointUnit(point), size);
    }

    // Clears everything outside the shield, handy for seeing a blazon the way the player will.
    public Texture2D ClipTexture(Texture2D texture) {
        for (int x = 0; x < texture.width; x++) {
            for (int y = 0; y < texture.height; y++) {
                if (!Contains(x, y, texture.width)) {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();
        return texture;
    }

    #endregion

    #region Charge points

    // Where each named point sits in the shield's bounds as (across, up), 0 to 1, with dexter (the left of the picture) at 0
    // and the chief at 1. The honour and nombril points are positioned the way the old fixed layout did.
    public static Vector2 PointUnit(Blazon.Point point) {
        switch (point) {
            case Blazon.Point.Chief: return new Vector2(0.5f, 1f);
            case Blazon.Point.Dexter: return new Vector2(0f, 0.5f);
            case Blazon.Point.Sinister: return new Vector2(1f, 0.5f);
            case Blazon.Point.Base: return new Vector2(0.5f, 0f);

            case Blazon.Point.DexterChief: return new Vector2(0f, 1f);
            case Blazon.Point.MiddleChief: return new Vector2(0.5f, 1f);
            case Blazon.Point.SinisterChief: return new Vector2(1f, 1f);

            case Blazon.Point.DexterBase: return new Vector2(0f, 0f);
            case Blazon.Point.MiddleBase: return new Vector2(0.5f, 0f);
            case Blazon.Point.SinisterBase: return new Vector2(1f, 0f);

            case Blazon.Point.FessPoint: return new Vector2(0.5f, 0.5f);
            // Half way between the middle chief and the fess point
            case Blazon.Point.HonourPoint: return new Vector2(0.5f, 0.75f);
            // Half way between the base and the honour point
            case Blazon.Point.NombrilPoint: return new Vector2(0.5f, 0.375f);

            default: return new Vector2(0.5f, 0.5f);
        }
    }

    // The square a charge of `chargeSize` pixels occupies at a named point. The square sits against the matching edge of the
    // shield's bounds and is then pulled towards the middle of the shield until the charge is inside the outline.
    // Uses the same RectInt convention as the drawers: y is the top edge, the centre is (x + width / 2, y - height / 2).
    public RectInt PlaceCharge(Blazon.Point point, int chargeSize, int size) {
        Vector2 unit = PointUnit(point);
        RectInt b = Bounds(size);
        float freeX = b.width - chargeSize;
        float freeY = b.height - chargeSize;
        Vector2 centre = new Vector2(b.x + chargeSize / 2f + unit.x * freeX,
                                     b.y + chargeSize / 2f + unit.y * freeY);
        return RectAround(FitInside(centre, chargeSize, size), chargeSize);
    }

    // Placements for several charges of one kind, laid out the way the old fixed layouts did but within the shield's bounds.
    public RectInt[] PlaceCharges(int number, int size) {
        if (number < 1) {
            throw new ArgumentException("Number of charges must be greater than 0");
        }

        RectInt b = Bounds(size);
        int width;
        // each layout is a list of centres in the shield's bounds
        List<Vector2> centres = new List<Vector2>();
        switch (number) {
            case 1:
                return new RectInt[] { PlaceCharge(Blazon.Point.FessPoint, b.width / 2, size) };
            case 2:
                width = b.width / 4;
                centres.Add(new Vector2(0.2679f, 0.75f));
                centres.Add(new Vector2(0.6964f, 0.75f));
                break;
            case 4:
                width = b.width / 4;
                centres.Add(new Vector2(1f / 3f, 0.4f));
                centres.Add(new Vector2(1f / 3f, 0.8f));
                centres.Add(new Vector2(2f / 3f, 0.4f));
                centres.Add(new Vector2(2f / 3f, 0.8f));
                break;
            default:
                // around a circle, starting at the bottom
                width = b.width / Mathf.Max(number, 4);
                int angle = 360 / number;
                for (int i = 0; i < number; i++) {
                    centres.Add(new Vector2(0.5f + 0.25f * Mathf.Sin((angle * i + 180) * Mathf.Deg2Rad),
                                            0.5f + 0.25f * Mathf.Cos((angle * i + 180) * Mathf.Deg2Rad)));
                }
                break;
        }

        RectInt[] rects = new RectInt[centres.Count];
        for (int i = 0; i < rects.Length; i++) {
            rects[i] = RectAround(FitInside(UnitToPixel(centres[i], size), width, size), width);
        }
        return rects;
    }

    // Moves a charge centre towards the middle of the shield until the charge fits inside the outline.
    Vector2 FitInside(Vector2 centre, int chargeSize, int size) {
        // a little slack so charges sitting flush against a straight edge aren't pushed in by rounding
        float needed = Mathf.Max(0f, chargeSize / 2f * ChargeClearance - 1.5f);
        Vector2 target = Pole(size);
        Vector2 toTarget = target - centre;
        float length = toTarget.magnitude;
        if (length < 0.001f) {
            return centre;
        }
        Vector2 step = toTarget / length;

        for (float travelled = 0f; travelled <= length; travelled += 1f) {
            Vector2 candidate = centre + step * travelled;
            if (DistanceToEdge(Mathf.FloorToInt(candidate.x), Mathf.FloorToInt(candidate.y), size) >= needed) {
                return candidate;
            }
        }
        return target;
    }

    static RectInt RectAround(Vector2 centre, int chargeSize) {
        return new RectInt(Mathf.RoundToInt(centre.x - chargeSize / 2f),
                           Mathf.RoundToInt(centre.y + chargeSize / 2f),
                           chargeSize, chargeSize);
    }

    #endregion

    #region Distance transform

    // For every pixel, the distance to the nearest pixel that isn't inside. Exact euclidean distance (Felzenszwalb and Huttenlocher).
    // The grid is padded by a pixel of "outside" so a shield that touches the texture edge still has an edge there.
    static float[] ComputeEdgeDistance(bool[] inside, int n) {
        const double Far = 1e12;
        int m = n + 2;
        double[] d = new double[m * m];
        for (int y = 0; y < n; y++) {
            for (int x = 0; x < n; x++) {
                d[(x + 1) + (y + 1) * m] = inside[x + y * n] ? Far : 0.0;
            }
        }

        double[] f = new double[m];
        double[] g = new double[m];
        int[] v = new int[m];
        double[] z = new double[m + 1];

        for (int x = 0; x < m; x++) {
            for (int y = 0; y < m; y++) {
                f[y] = d[x + y * m];
            }
            DistanceTransform1D(f, g, v, z, m);
            for (int y = 0; y < m; y++) {
                d[x + y * m] = g[y];
            }
        }
        for (int y = 0; y < m; y++) {
            for (int x = 0; x < m; x++) {
                f[x] = d[x + y * m];
            }
            DistanceTransform1D(f, g, v, z, m);
            for (int x = 0; x < m; x++) {
                d[x + y * m] = g[x];
            }
        }

        float[] result = new float[n * n];
        for (int y = 0; y < n; y++) {
            for (int x = 0; x < n; x++) {
                result[x + y * n] = (float)Math.Sqrt(d[(x + 1) + (y + 1) * m]);
            }
        }
        return result;
    }

    static void DistanceTransform1D(double[] f, double[] d, int[] v, double[] z, int n) {
        int k = 0;
        v[0] = 0;
        z[0] = -1e30;
        z[1] = 1e30;
        for (int q = 1; q < n; q++) {
            double s;
            while (true) {
                int p = v[k];
                s = ((f[q] + (double)q * q) - (f[p] + (double)p * p)) / (2.0 * q - 2.0 * p);
                if (s <= z[k]) {
                    k--;
                }
                else {
                    break;
                }
            }
            k++;
            v[k] = q;
            z[k] = s;
            z[k + 1] = 1e30;
        }
        k = 0;
        for (int q = 0; q < n; q++) {
            while (z[k + 1] < q) {
                k++;
            }
            int p = v[k];
            d[q] = (double)(q - p) * (q - p) + f[p];
        }
    }

    // The pixel furthest from the edge. Ties are broken towards the middle of the bounds so symmetric shields get a centred pole.
    static Vector2 FindPole(float[] distance, int n, RectInt bounds) {
        float best = 0f;
        for (int i = 0; i < distance.Length; i++) {
            best = Mathf.Max(best, distance[i]);
        }

        Vector2 middle = new Vector2(bounds.x + bounds.width / 2f, bounds.y + bounds.height / 2f);
        Vector2 pole = middle;
        float nearest = float.MaxValue;
        for (int y = 0; y < n; y++) {
            for (int x = 0; x < n; x++) {
                if (distance[x + y * n] >= best - 1f) {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                    float m = (p - middle).sqrMagnitude;
                    if (m < nearest) {
                        nearest = m;
                        pole = p;
                    }
                }
            }
        }
        return pole;
    }

    #endregion
}
