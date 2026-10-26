using UnityEngine;

// What a blazon is made of, pixel by pixel, for scoring: which tincture is at every pixel and how much that pixel counts. Scoring compares
// these maps, not the textures, so how a tincture looks (a fur's pattern, say) doesn't have to match exactly, only which tincture it is.
// Both maps are in the order of a texture's pixels, bottom row first.
public class BlazonMap {
    public readonly int size;
    // Tincture.IdOf for every pixel, or Tincture.Empty (0) where nothing was drawn. A fur is its own id, not the colors of its pattern.
    public readonly byte[] tinctures;
    // How much each pixel counts, from 0 to 1. A target's weights say what matters in it; a painted map's weights aren't used.
    public readonly float[] weights;

    public BlazonMap(int size) {
        this.size = size;
        tinctures = new byte[size * size];
        weights = new float[size * size];
    }

    // The map of a painting: the nearest tincture to every pixel, each counting 1.
    public static BlazonMap FromPixels(Color32[] pixels, int size) {
        BlazonMap map = new BlazonMap(size);
        for (int i = 0; i < pixels.Length; i++) {
            map.tinctures[i] = Tincture.IdOf(pixels[i]);
            map.weights[i] = 1f;
        }
        return map;
    }

    // Every pixel the same tincture with the same weight, like a blank canvas.
    public void Fill(byte tincture, float weight) {
        for (int i = 0; i < tinctures.Length; i++) {
            tinctures[i] = tincture;
            weights[i] = weight;
        }
    }

    // Sets the tincture of every pixel a brush covers, where it was just drawn. The brush is hard edged: a pixel is covered where the brush
    // texture (bottom row first, like any texture) is at least half opaque. `stamp` is where the brush was drawn, in the pixels of the canvas
    // counting down from the top, and the map's rows count up from the bottom.
    public void Stamp(Rect stamp, Color32[] brush, int brushWidth, int brushHeight, byte tincture) {
        int x0 = Mathf.Max(0, Mathf.FloorToInt(stamp.xMin));
        int x1 = Mathf.Min(size, Mathf.CeilToInt(stamp.xMax));
        int y0 = Mathf.Max(0, Mathf.FloorToInt(stamp.yMin));
        int y1 = Mathf.Min(size, Mathf.CeilToInt(stamp.yMax));
        for (int py = y0; py < y1; py++) {
            float v = (py + 0.5f - stamp.yMin) / stamp.height;      // 0 at the top of the brush
            int row = Mathf.Clamp(Mathf.FloorToInt((1f - v) * brushHeight), 0, brushHeight - 1);
            for (int px = x0; px < x1; px++) {
                float u = (px + 0.5f - stamp.xMin) / stamp.width;
                int column = Mathf.Clamp(Mathf.FloorToInt(u * brushWidth), 0, brushWidth - 1);
                if (brush[column + row * brushWidth].a >= 128) {
                    tinctures[px + (size - 1 - py) * size] = tincture;
                }
            }
        }
    }

    // Records a layer drawn on top of what is already here: every pixel the layer covers (it isn't transparent) takes the layer's
    // tincture, and counts `weight` more than it did, to a most of 1. So the pixels under the last thing drawn count the most.
    public void AddLayer(Color32[] layer, float weight) {
        for (int i = 0; i < layer.Length; i++) {
            if (layer[i].a != 0) {
                tinctures[i] = Tincture.IdOf(layer[i]);
                weights[i] = Mathf.Min(1f, weights[i] + weight);
            }
        }
    }

    // Pixels off the shield can't be seen, so they don't count.
    public void ClearWeightsOutside(Shield shield) {
        for (int i = 0; i < weights.Length; i++) {
            if (!shield.Contains(i % size, i / size, size)) {
                weights[i] = 0f;
            }
        }
    }

    // The colors of the tinctures, to show the map as a picture.
    public Color32[] ToPixels() {
        Color32[] pixels = new Color32[tinctures.Length];
        for (int i = 0; i < pixels.Length; i++) {
            pixels[i] = Tincture.FromId(tinctures[i]);
        }
        return pixels;
    }

    // How well a painting matches a target, from 0 to 1: the weight of the pixels with the right tincture over the weight of all of them,
    // by the target's weights.
    public static float Score(BlazonMap target, BlazonMap painted) {
        if (target.size != painted.size) {
            Debug.Log("Blazon map size mismatch");
            return 0f;
        }

        double total = 0.0;
        double matching = 0.0;
        for (int i = 0; i < target.weights.Length; i++) {
            float weight = target.weights[i];
            if (weight <= 0f) {
                continue;
            }
            total += weight;
            if (target.tinctures[i] == painted.tinctures[i]) {
                matching += weight;
            }
        }
        return total > 0.0 ? (float)(matching / total) : 0f;
    }
}

// How much the layers of a blazon count for when it is scored: a pixel starts at the field's weight and gains the layer's weight for each
// ordinary or charge drawn over it.
public class ScoringWeights {
    public float field = 0.3f;
    public float layer = 0.2f;
}

// What drawing a blazon makes: the picture to show and the map to score with.
public class BlazonRender {
    public readonly Texture2D texture;
    public readonly BlazonMap map;

    public BlazonRender(Texture2D texture, BlazonMap map) {
        this.texture = texture;
        this.map = map;
    }
}
