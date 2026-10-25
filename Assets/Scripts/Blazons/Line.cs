using System;
using System.Linq;
using UnityEngine;
using static BlazonParserHelpers;

// Lines of partition and variation, the shapes the edge of a division, an ordinary or a bordure can take.
// https://en.wikipedia.org/wiki/Line_(heraldry)
//
// A line is described from its interior: for a partition line the interior is the part of the field named first, and the
// drawers fill the interior on the right of the line's direction (LineSide.Right).
public enum LineStyle {
    Straight,       // A plain straight line, the default for anything that doesn't name a line.
    Wavy,           // A sine wave, often for water. Also called undy.
    Engrailed,      // A series of circular arcs meeting at points. The points face away from the interior.
    Invected,       // Like engrailed but the points face into the interior.
    Embattled,      // A square wave, for castle battlements. Blazons can give the number of battlements.
    Indented,       // Small zigzags like a triangle wave. Across a band the peaks of one edge match the peaks of the other.
    Dancetty,       // Like indented but across a band peaks match troughs so the band keeps a constant width. Usually fewer points, three by default.
    Nebuly,         // Like wavy with more exaggerated meanders, for clouds. Blazons can give the number of undulations.
    Raguly,         // Embattled with oblique rather than right angled sides, like the stumps of limbs on a tree trunk.
    Dovetailed      // Like a carpentry dovetail joint: merlons wider at the top than at the base.
}

// How a division or an ordinary's edge is drawn. A line is a property of a Field (for its division) or an Ordinary, not an
// Element: it has no tincture of its own. The default value is a straight line, so anything that doesn't set one is plain.
[Serializable]
public struct Line {
    public LineStyle style;
    // Number of peaks along the line (crests, merlons, points, lobes, bulbs), or 0 for the default of the style.
    public int count;

    public Line(LineStyle style, int count = 0) {
        this.style = style;
        this.count = count;
    }

    public bool IsStraight {
        get { return style == LineStyle.Straight; }
    }

    // Peaks to draw across a whole edge of the shield when the blazon doesn't say how many.
    public int PeakCount {
        get { return count > 0 ? count : DefaultCount(style); }
    }

    // Whether the two edges of a band are copies of each other moved apart (a wavy fess is a ribbon of constant width) rather than
    // mirror images of each other (an indented fess has its points opposite each other and swells and narrows).
    public bool ParallelEdges {
        get { return style == LineStyle.Wavy || style == LineStyle.Nebuly || style == LineStyle.Dancetty; }
    }

    // How far the line wanders from straight for a given wavelength, in pixels.
    public float AmplitudeFor(float wavelength) {
        return AmplitudeFactor(style) * wavelength;
    }

    public static int DefaultCount(LineStyle style) {
        switch (style) {
            case LineStyle.Engrailed:
            case LineStyle.Invected:
                return 6;
            case LineStyle.Indented:
                return 7;
            case LineStyle.Dancetty:
                return 3;
            default:
                return 5;
        }
    }

    #region Blazon

    // The name of the style as a blazon says it, "embattled". An empty string for a straight line.
    public string StyleName {
        get {
            if (IsStraight) {
                return string.Empty;
            }
            // lambdas in a struct can't use its fields directly
            LineStyle current = style;
            return LineStyles.FirstOrDefault(x => x.Value == current).Key;
        }
    }

    // "embattled", or "embattled of five" with a number of peaks. An empty string for a straight line.
    public string ToBlazonDescription() {
        if (IsStraight) {
            return string.Empty;
        }
        // lambdas in a struct can't use its fields directly
        int peaks = count;
        string ret = StyleName;
        if (peaks > 0) {
            ret += " of " + Numbers.FirstOrDefault(x => x.Value == peaks).Key;
        }
        return ret;
    }

    // Reads a line starting at words[index]: a style name, optionally followed by "of" and a number of peaks. `consumed` is how many
    // words it used. Where "of" and a number belong to something else (the number of bars in "barry wavy of six") pass allowCount false.
    public static bool TryParse(string[] words, int index, out Line line, out int consumed, bool allowCount = true) {
        line = default;
        consumed = 0;
        LineStyle style;
        if (index < 0 || index >= words.Length || !LineStyles.TryGetValue(words[index], out style)) {
            return false;
        }

        consumed = 1;
        int count = 0;
        int number;
        if (allowCount && index + 2 < words.Length && words[index + 1] == "of" && Numbers.TryGetValue(words[index + 2], out number)) {
            count = number;
            consumed = 3;
        }
        line = new Line(style, count);
        return true;
    }

    #endregion

    #region Drawing

    // The primitive profile for this style, see LineProfile.
    public LineProfile Profile() {
        switch (style) {
            case LineStyle.Wavy:
                return LineProfile.Sine();
            case LineStyle.Engrailed:
                // concave arcs into the interior, points facing out
                return LineProfile.Scallop(true);
            case LineStyle.Invected:
                return LineProfile.Scallop();
            case LineStyle.Embattled:
                return LineProfile.Square();
            case LineStyle.Indented:
            case LineStyle.Dancetty:
                return LineProfile.Triangle();
            case LineStyle.Nebuly:
                return LineProfile.OverhangingSine();
            case LineStyle.Raguly:
                return LineProfile.SlantedSquare();
            case LineStyle.Dovetailed:
                return LineProfile.FlaredSquare();
            default:
                return LineProfile.Straight();
        }
    }

    // How far the line wanders from straight, as a fraction of its wavelength.
    static float AmplitudeFactor(LineStyle style) {
        switch (style) {
            case LineStyle.Wavy:
                return 0.2f;
            case LineStyle.Engrailed:
            case LineStyle.Invected:
                return 0.3f;
            case LineStyle.Nebuly:
                return 0.32f;
            case LineStyle.Straight:
                return 0f;
            default:
                return 0.25f;
        }
    }

    // The line from `start` to `end` with this line's number of peaks, drawn with the amplitude given in pixels,
    // or a good looking one for the style if it is left out.
    public WaveLine ToWaveLine(Vector2 start, Vector2 end, bool centred = true) {
        float wavelength = (end - start).magnitude / PeakCount;
        return ToWaveLine(start, end, AmplitudeFactor(style) * wavelength, centred);
    }

    public WaveLine ToWaveLine(Vector2 start, Vector2 end, float amplitude, bool centred = true) {
        return WaveLine.Between(start, end, PeakCount, Profile(), amplitude, centred);
    }

    // The line from `start` to `end` with an exact number of peaks and the usual amplitude for the style, for one part of a
    // longer line (an arm of a cross) so its peaks are the same size as the whole line's.
    public WaveLine ToWaveLineWithPeaks(Vector2 start, Vector2 end, int peaks, bool centred = true) {
        peaks = Mathf.Max(1, peaks);
        float wavelength = (end - start).magnitude / peaks;
        return WaveLine.Between(start, end, peaks, Profile(), AmplitudeFactor(style) * wavelength, centred);
    }

    #endregion
}

// Which side of a line to fill, looking along the direction the line travels.
public enum LineSide {
    Left,
    Right
}

// The shape of a line over one period, written in terms of the graphics rather than the heraldry; Line maps each LineStyle to one.
// Heights are in units of the amplitude with up the left of the line, and a point is "below" the profile if it's on the right of
// the line once the line has been bent by the profile. The drawers fill one side of the bent line.
public abstract class LineProfile {
    public readonly float crestPhase;   // where the middle of a peak sits within a cycle

    protected LineProfile(float crestPhase) {
        this.crestPhase = crestPhase;
    }

    public abstract bool IsBelow(float cycles, float height);

    #region Profiles

    public static LineProfile Straight() {
        return new FunctionProfile(cycles => 0f, 0.25f);
    }

    // A sine wave, from -1 to 1, starting at 0 and rising.
    public static LineProfile Sine() {
        return new FunctionProfile(cycles => Mathf.Sin(Mathf.Repeat(cycles, 1f) * 2f * Mathf.PI), 0.25f);
    }

    // A triangle wave, from -1 to 1, starting at 0 and rising.
    public static LineProfile Triangle() {
        return new FunctionProfile(cycles => 1f - 4f * Mathf.Abs(Mathf.Repeat(cycles + 0.25f, 1f) - 0.5f), 0.25f);
    }

    // One half sine arch per cycle, from 0 to 1 with the cusps on the line and the arcs bulging left, or from 0 to -1 when flipped.
    public static LineProfile Scallop(bool flipped = false) {
        float sign = flipped ? -1f : 1f;
        return new FunctionProfile(cycles => sign * Mathf.Abs(Mathf.Sin(Mathf.Repeat(cycles, 1f) * Mathf.PI)), 0.5f);
    }

    // A square wave: `duty` of each cycle is up at +1 (starting the cycle), the rest is down at -1.
    public static LineProfile Square(float duty = 0.5f) {
        return new OutlineProfile(new[] { new Vector2(0, 1), new Vector2(duty, 1), new Vector2(duty, -1), new Vector2(1, -1), new Vector2(1, 1) }, duty / 2f);
    }

    // A square wave whose sides lean. `slant` is how far, in cycles, the top of a step is shifted along the line from its
    // middle height (so the top is shifted 2 * slant relative to the bottom).
    public static LineProfile SlantedSquare(float duty = 0.5f, float slant = 0.12f) {
        return new OutlineProfile(new[] { new Vector2(slant, 1), new Vector2(duty + slant, 1), new Vector2(duty - slant, -1), new Vector2(1 - slant, -1), new Vector2(1 + slant, 1) }, duty / 2f + slant);
    }

    // A square wave whose upper steps are wider at the top than at the base. `duty` is the width of the top as a fraction of the
    // cycle, each side of the base is pulled in by `flare` cycles.
    public static LineProfile FlaredSquare(float duty = 0.45f, float flare = 0.12f) {
        return new OutlineProfile(new[] { new Vector2(0, 1), new Vector2(duty, 1), new Vector2(duty - flare, -1), new Vector2(1 + flare, -1), new Vector2(1, 1) }, duty / 2f);
    }

    // A sine wave that overhangs: its along-position is warped by a second harmonic so the crests lean out over the troughs on both
    // sides. `overhang` 0 is a plain sine wave, about 1 gives bulbs on necks, beyond about 2 they get very bulbous.
    public static LineProfile OverhangingSine(float overhang = 1f, int samples = 48) {
        Vector2[] points = new Vector2[samples + 1];
        for (int i = 0; i <= samples; i++) {
            float t = (float)i / samples;
            points[i] = new Vector2(t - overhang * Mathf.Sin(4f * Mathf.PI * t) / (2f * Mathf.PI), Mathf.Sin(2f * Mathf.PI * t));
        }
        return new OutlineProfile(points, 0.25f);
    }

    #endregion

    // One height for every position along the line, so it can't overhang.
    class FunctionProfile : LineProfile {
        readonly Func<float, float> height;

        public FunctionProfile(Func<float, float> height, float crestPhase) : base(crestPhase) {
            this.height = height;
        }

        public override bool IsBelow(float cycles, float h) {
            return h <= height(cycles);
        }
    }

    // One period of the outline as a polyline, for shapes that overhang. The points run along the period as (cycles, height) and the
    // last point must be the first point plus one cycle so periods join up. A point is below the outline if a ray straight up
    // from it crosses the outline an odd number of times.
    class OutlineProfile : LineProfile {
        readonly Vector2[] points;
        readonly float minCycles, maxCycles, minHeight, maxHeight;

        public OutlineProfile(Vector2[] points, float crestPhase) : base(crestPhase) {
            this.points = points;
            minCycles = maxCycles = points[0].x;
            minHeight = maxHeight = points[0].y;
            foreach (Vector2 p in points) {
                minCycles = Mathf.Min(minCycles, p.x);
                maxCycles = Mathf.Max(maxCycles, p.x);
                minHeight = Mathf.Min(minHeight, p.y);
                maxHeight = Mathf.Max(maxHeight, p.y);
            }
        }

        public override bool IsBelow(float cycles, float height) {
            if (height > maxHeight) {
                return false;
            }
            if (height < minHeight) {
                return true;
            }

            float u = Mathf.Repeat(cycles, 1f);
            int crossings = 0;
            // copies of the period whose points can be at u
            int first = Mathf.FloorToInt(u - maxCycles);
            int last = Mathf.CeilToInt(u - minCycles);
            for (int k = first; k <= last; k++) {
                float uk = u - k;
                for (int i = 0; i < points.Length - 1; i++) {
                    Vector2 a = points[i];
                    Vector2 b = points[i + 1];
                    if ((a.x <= uk) != (b.x <= uk)) {
                        float t = (uk - a.x) / (b.x - a.x);
                        if (a.y + t * (b.y - a.y) > height) {
                            crossings++;
                        }
                    }
                }
            }
            return (crossings & 1) == 1;
        }
    }
}

// A straight line through `origin` heading `angle` degrees counter clockwise from +x, bent sideways by a profile.
// Amplitude and wavelength are in pixels, phase is in cycles (0.25 shifts the pattern a quarter of a wavelength along the line).
// Between places a line from one point to another with a given number of peaks.
public struct WaveLine {
    readonly Vector2 origin, direction, normal;
    readonly LineProfile profile;
    readonly float amplitude, wavelength, phase;
    readonly float taper;   // distance along the line over which the amplitude grows from nothing, 0 for none

    public WaveLine(Vector2 origin, float angle, LineProfile profile, float amplitude, float wavelength, float phase = 0f)
        : this(origin, angle, profile, amplitude, wavelength, phase, 0f) { }

    WaveLine(Vector2 origin, float angle, LineProfile profile, float amplitude, float wavelength, float phase, float taper) {
        float radians = angle * Mathf.Deg2Rad;
        this.origin = origin;
        direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        normal = new Vector2(-direction.y, direction.x);
        this.profile = profile;
        this.amplitude = amplitude;
        this.wavelength = wavelength;
        this.phase = phase;
        this.taper = taper;
    }

    // A line from `start` to `end` with `count` peaks (one per wavelength: crests, merlons, points, lobes or bulbs).
    // With `centred` a peak sits in the middle of the line so the pattern is symmetrical, otherwise a cycle starts at `start`.
    public static WaveLine Between(Vector2 start, Vector2 end, int count, LineProfile profile, float amplitude, bool centred = true) {
        Vector2 along = end - start;
        float angle = Mathf.Atan2(along.y, along.x) * Mathf.Rad2Deg;
        float phase = centred ? profile.crestPhase - count / 2f : 0f;
        return new WaveLine(start, angle, profile, amplitude, along.magnitude / count, phase);
    }

    public bool IsOnSide(int x, int y, LineSide side) {
        Vector2 d = new Vector2(x + 0.5f, y + 0.5f) - origin;
        float along = Vector2.Dot(d, direction);
        float across = Vector2.Dot(d, normal);
        float size = amplitude;
        if (size < 0f) {
            // a negative amplitude is the same outline seen from the other side of the line
            across = -across;
            size = -size;
            side = side == LineSide.Left ? LineSide.Right : LineSide.Left;
        }
        if (taper > 0f) {
            size *= Mathf.Clamp01(along / taper);
        }
        // no amplitude (a straight line, or the very start of a tapered one) is a flat line
        bool below = size < 0.0001f ? across <= 0f : profile.IsBelow(along / wavelength + phase, across / size);
        return side == LineSide.Right ? below : !below;
    }

    // The same line bent the other way: filling the left of a mirrored line is the mirror image of filling the right of the original.
    public WaveLine Mirrored() {
        return new WaveLine(origin, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, profile, -amplitude, wavelength, phase, taper);
    }

    // The same line with its amplitude growing from nothing over the first `length` pixels, so it starts straight. Used where
    // several lines meet at a point so their waves don't swing into each other's side.
    public WaveLine Tapered(float length) {
        return new WaveLine(origin, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, profile, amplitude, wavelength, phase, length);
    }

    // The same line moved `distance` pixels to the left of the line (negative for right). Two shifted copies make the
    // edges of a band: fill one side of the first, then clear one side of the second.
    public WaveLine Shifted(float distance) {
        return new WaveLine(origin + normal * distance, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, profile, amplitude, wavelength, phase, taper);
    }
}
