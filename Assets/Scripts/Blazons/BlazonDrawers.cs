using System;
using System.Collections.Generic;
using UnityEngine;
using static Blazon;
using static Charge;
using static Field;
using static Ordinary;

public static class BlazonDrawers  {

    #region Main Drawing Functions

    public static Texture2D ApplyDivision(Texture2D field, Division div, Color[] tinctures, bool sinister = false, Line line = default, Shield shield = null) {
        Func<int, int, int> region = DivisionRegions(div, sinister, line, shield ?? Shield.Default, field.width);
        // a pall has three parts, the other divisions use the first two
        Color third = tinctures.Length > 2 ? tinctures[2] : tinctures[tinctures.Length - 1];
        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                int part = region(x, y);
                field.SetPixel(x, y, part == 0 ? tinctures[0] : part == 1 ? tinctures[1] : third);
            }
        }
        return field;
    }

    // Where the apex of a chevron division is, as a fraction of the shield's height.
    private const float ChevronApexHeight = 1f / 3f;

    // Which part of a division every pixel of a `size` texture belongs to: 0 for the first part named, 1 for the second and 2 for
    // the third (only a pall has one). The lines are placed with the shield's points, so they follow its shape, and are drawn in
    // the given line style.
    //
    // A line is described from its interior, the first part, and the drawers fill the right of a line's direction, so each line
    // below heads the way that leaves the first part on its right. Divisions with two lines (cross, saltire) are the exclusive
    // or of the two sides, which makes the quarters alternate.
    public static Func<int, int, int> DivisionRegions(Division div, bool sinister, Line line, Shield shield, int size) {
        Vector2 fessPoint = shield.PointPosition(Point.FessPoint, size);
        Vector2 dexter = shield.PointPosition(Point.Dexter, size);
        Vector2 sinisterSide = shield.PointPosition(Point.Sinister, size);
        Vector2 chief = shield.PointPosition(Point.Chief, size);
        Vector2 basePoint = shield.PointPosition(Point.Base, size);
        Vector2 dexterChief = shield.PointPosition(Point.DexterChief, size);
        Vector2 sinisterChief = shield.PointPosition(Point.SinisterChief, size);
        Vector2 dexterBase = shield.PointPosition(Point.DexterBase, size);
        Vector2 sinisterBase = shield.PointPosition(Point.SinisterBase, size);

        switch (div) {
            case Division.Fess: {
                // across the shield towards dexter, chief on the right
                WaveLine edge = line.ToWaveLine(sinisterSide, dexter);
                return (x, y) => edge.IsOnSide(x, y, LineSide.Right) ? 0 : 1;
            }
            case Division.Pale: {
                // down the shield, dexter on the right
                WaveLine edge = line.ToWaveLine(chief, basePoint);
                return (x, y) => edge.IsOnSide(x, y, LineSide.Right) ? 0 : 1;
            }
            case Division.Bend: {
                // from the sinister base up to the dexter chief, the chief side on the right. Sinister runs from the other chief.
                WaveLine edge = sinister ? line.ToWaveLine(sinisterChief, dexterBase) : line.ToWaveLine(sinisterBase, dexterChief);
                return (x, y) => edge.IsOnSide(x, y, LineSide.Right) ? 0 : 1;
            }
            case Division.Chevron: {
                Vector2 apex = shield.UnitToPixel(new Vector2(0.5f, ChevronApexHeight), size);
                float width = shield.Bounds(size).width;
                // two arms down from the apex, the chief above them is the first part and the base below the second
                return JunctionRegions(apex, line, new[] {
                    new Arm(dexterBase, 1, 0, width),
                    new Arm(sinisterBase, 0, 1, width) });
            }
            case Division.Cross: {
                float width = shield.Bounds(size).width, height = shield.Bounds(size).height;
                // four arms from the fess point, the quarters alternate: dexter chief and sinister base are the first part
                return JunctionRegions(fessPoint, line, new[] {
                    new Arm(sinisterSide, 1, 0, width),
                    new Arm(chief, 0, 1, height),
                    new Arm(dexter, 1, 0, width),
                    new Arm(basePoint, 0, 1, height) });
            }
            case Division.Saltire: {
                float diagonal = (sinisterBase - dexterChief).magnitude;
                // four diagonal arms: the chief and base are the first part, dexter and sinister the second
                return JunctionRegions(fessPoint, line, new[] {
                    new Arm(sinisterChief, 0, 1, diagonal),
                    new Arm(sinisterBase, 1, 0, diagonal),
                    new Arm(dexterBase, 0, 1, diagonal),
                    new Arm(dexterChief, 1, 0, diagonal) });
            }
            case Division.Pall: {
                Vector2 junction = shield.UnitToPixel(new Vector2(0.5f, PallJunctionHeight), size);
                float width = shield.Bounds(size).width, height = shield.Bounds(size).height;
                // the chief is the wedge between the two arms, the stem splits the rest into dexter (second) and sinister (third)
                return JunctionRegions(junction, line, new[] {
                    new Arm(dexterChief, 1, 0, width),
                    new Arm(sinisterChief, 0, 2, width),
                    new Arm(basePoint, 2, 1, height) });
            }
            default:
                return (x, y) => 0;
        }
    }

    // One arm of a division that meets others at a junction: the point it runs out to and the parts (tincture indices) on its left
    // and right looking away from the junction. `reference` is the length of the whole line the arm belongs to (a cross's fess line
    // is the width of the shield), so a line style's peaks come out the same size on a short arm as on the whole line.
    private struct Arm {
        public readonly Vector2 end;
        public readonly int left, right;
        public readonly float reference;

        public Arm(Vector2 end, int left, int right, float reference) {
            this.end = end;
            this.left = left;
            this.right = right;
            this.reference = reference;
        }
    }

    // Divisions whose lines meet at a point (cross, saltire, pall, chevron) are drawn as arms running out from the junction rather
    // than as whole lines crossing, so each arm starts its line style at the junction and the arms match. Every pixel is given to the
    // arm it points most towards, and that arm's line then decides which of the two parts beside it the pixel is in. As with a
    // single line the first part (the lower index) is the interior: an arm with the first part on its left is mirrored, so the line
    // style bends the same way into the first part on every arm.
    private static Func<int, int, int> JunctionRegions(Vector2 junction, Line line, Arm[] arms) {
        int count = arms.Length;
        Vector2[] directions = new Vector2[count];
        WaveLine[] edges = new WaveLine[count];
        bool[] interiorOnRight = new bool[count];
        int[] interior = new int[count];
        int[] other = new int[count];

        for (int i = 0; i < count; i++) {
            Vector2 along = arms[i].end - junction;
            directions[i] = along.normalized;
            int peaks = Mathf.RoundToInt(line.PeakCount * along.magnitude / arms[i].reference);
            // the arm starts straight at the junction and grows into its pattern over the first wavelength
            WaveLine edge = line.ToWaveLineWithPeaks(junction, arms[i].end, peaks, false).Tapered(along.magnitude / Mathf.Max(1, peaks));
            interiorOnRight[i] = arms[i].right < arms[i].left;
            interior[i] = Mathf.Min(arms[i].left, arms[i].right);
            other[i] = Mathf.Max(arms[i].left, arms[i].right);
            edges[i] = interiorOnRight[i] ? edge : edge.Mirrored();
        }

        return (x, y) => {
            Vector2 d = new Vector2(x + 0.5f, y + 0.5f) - junction;
            int nearest = 0;
            float best = float.MinValue;
            for (int i = 0; i < count; i++) {
                float alignment = Vector2.Dot(d, directions[i]);
                // compare directions, not distances: how much of the pixel's offset is along the arm, as a fraction of its length
                float cosine = d.sqrMagnitude > 0f ? alignment / d.magnitude : 0f;
                if (cosine > best) {
                    best = cosine;
                    nearest = i;
                }
            }
            bool inInterior = edges[nearest].IsOnSide(x, y, interiorOnRight[nearest] ? LineSide.Right : LineSide.Left);
            return inInterior ? interior[nearest] : other[nearest];
        };
    }

    // How much of the shield, as a fraction of its height, the ordinaries take up. All sizes are fractions of the shield's bounds so
    // they follow its shape, and the edges of an ordinary are drawn in the line style.
    private const float ChiefEdgeHeight = 2f / 3f;      // the chief covers everything above this
    private const float FessThickness = 1f / 4f;        // of the height
    private const float PaleThickness = 1f / 4f;        // of the width
    private const float CrossHeight = 0.7f;             // where the horizontal arm of a cross is, up the shield
    private const float CrossThickness = 0.2f;          // of the height or the width
    private const float DiagonalThickness = 0.18f;      // of the width, for a bend and a saltire
    private const float PallThickness = 1f / 6f;        // of the width
    private const float BordureThickness = 1f / 6f;     // of the width
    private const float ChevronApexUnit = 0.65f;        // the middle of the chevron's point, up the shield
    private const float ChevronEndsUnit = 0.15f;        // the middle of the chevron's ends
    private const float ChevronThickness = 0.2f;        // of the height, measured straight up
    private const float PileApexUnit = 0.1f;
    private const float PileTopUnit = 0.25f;            // the base of the pile runs from this far across to its mirror image

    // Which pixels of a `size` texture an ordinary covers, with the line style on its edges. Lines are described from the ordinary, so
    // the ordinary is the interior and a line style bends the same way into it on every edge. Edges that are copies of each other
    // (see Line.ParallelEdges) keep a band the same width. Embattled only varies the upper edge of a fess, chief, bend or chevron and
    // the dexter edge of a pale; on the others every edge is embattled.
    public static Func<int, int, bool> OrdinaryCoverage(OrdinaryShape shape, bool sinister, Line line, Shield shield, int size) {
        RectInt bounds = shield.Bounds(size);
        float width = bounds.width;
        float height = bounds.height;
        Vector2 fess = shield.PointPosition(Point.FessPoint, size);
        Vector2 dexterChief = shield.PointPosition(Point.DexterChief, size);
        Vector2 sinisterChief = shield.PointPosition(Point.SinisterChief, size);
        Vector2 dexterBase = shield.PointPosition(Point.DexterBase, size);
        Vector2 sinisterBase = shield.PointPosition(Point.SinisterBase, size);

        switch (shape) {
            case OrdinaryShape.Chief: {
                // only the lower edge is varied; heading towards dexter leaves the chief above, on the right
                float edgeY = bounds.y + height * ChiefEdgeHeight;
                WaveLine edge = line.ToWaveLine(new Vector2(bounds.xMax, edgeY), new Vector2(bounds.xMin, edgeY));
                return (x, y) => edge.IsOnSide(x, y, LineSide.Right);
            }
            case OrdinaryShape.Fess:
                return Band(new Vector2(bounds.xMin, fess.y), new Vector2(bounds.xMax, fess.y), height * FessThickness, line, true, width);
            case OrdinaryShape.Pale:
                return Band(new Vector2(fess.x, bounds.yMin), new Vector2(fess.x, bounds.yMax), width * PaleThickness, line, true, height);
            case OrdinaryShape.Cross: {
                float armY = bounds.y + height * CrossHeight;
                Func<int, int, bool> across = Band(new Vector2(bounds.xMin, armY), new Vector2(bounds.xMax, armY), height * CrossThickness, line, false, width);
                Func<int, int, bool> down = Band(new Vector2(fess.x, bounds.yMin), new Vector2(fess.x, bounds.yMax), width * CrossThickness, line, false, height);
                return (x, y) => across(x, y) || down(x, y);
            }
            case OrdinaryShape.Bend:
                return sinister
                    ? Band(dexterBase, sinisterChief, width * DiagonalThickness, line, true, width)
                    : Band(dexterChief, sinisterBase, width * DiagonalThickness, line, true, width);
            case OrdinaryShape.Saltire: {
                Func<int, int, bool> bend = Band(dexterChief, sinisterBase, width * DiagonalThickness, line, false, width);
                Func<int, int, bool> bendSinister = Band(dexterBase, sinisterChief, width * DiagonalThickness, line, false, width);
                return (x, y) => bend(x, y) || bendSinister(x, y);
            }
            case OrdinaryShape.Chevron: {
                Vector2 apex = shield.UnitToPixel(new Vector2(0.5f, ChevronApexUnit), size);
                Vector2 dexterEnd = shield.UnitToPixel(new Vector2(0f, ChevronEndsUnit), size);
                Vector2 sinisterEnd = shield.UnitToPixel(new Vector2(1f, ChevronEndsUnit), size);
                // the thickness is measured straight up, the band is thinner across a sloping arm
                float thickness = height * ChevronThickness * Mathf.Abs((apex - dexterEnd).normalized.x);
                // one band along the path dexter end, apex, sinister end, with the chief on its left. The two legs carry one pattern
                // through the point: each starts a cycle at the apex, so the pattern is continuous there.
                Func<int, int, bool> dexterLeg = Band(dexterEnd, apex, thickness, line, true, width, false);
                Func<int, int, bool> sinisterLeg = Band(apex, sinisterEnd, thickness, line, true, width, false);
                return (x, y) => (x + 0.5f < apex.x ? dexterLeg : sinisterLeg)(x, y);
            }
            case OrdinaryShape.Pile: {
                Vector2 apex = shield.UnitToPixel(new Vector2(0.5f, PileApexUnit), size);
                Vector2 topDexter = shield.UnitToPixel(new Vector2(PileTopUnit, 1f), size);
                Vector2 topSinister = shield.UnitToPixel(new Vector2(1f - PileTopUnit, 1f), size);
                int peaks = Mathf.Max(1, Mathf.RoundToInt(line.PeakCount * (topDexter - apex).magnitude / height));
                // both edges run out from the point, which is where the pile is narrowest, so they start straight and grow into the
                // line style; the pile is on the right going up the dexter edge and on the left going up the sinister edge, so that
                // edge is mirrored to bend the same way into the pile
                // the pile is only a few wavelengths wide near its point, so the amplitude grows over two of them
                float taper = 2f * (topDexter - apex).magnitude / peaks;
                WaveLine dexterEdge = line.ToWaveLineWithPeaks(apex, topDexter, peaks, false).Tapered(taper);
                WaveLine sinisterEdge = line.ToWaveLineWithPeaks(apex, topSinister, peaks, false).Tapered(taper).Mirrored();
                return (x, y) => dexterEdge.IsOnSide(x, y, LineSide.Right) && sinisterEdge.IsOnSide(x, y, LineSide.Left);
            }
            case OrdinaryShape.Bordure:
                return BordureCoverage(line, shield, size);
            case OrdinaryShape.Pall: {
                Vector2 junction = shield.UnitToPixel(new Vector2(0.5f, PallJunctionHeight), size);
                Func<int, int, bool> dexterArm = HalfBand(junction, dexterChief, width * PallThickness, line, width);
                Func<int, int, bool> sinisterArm = HalfBand(junction, sinisterChief, width * PallThickness, line, width);
                Func<int, int, bool> stem = HalfBand(junction, shield.PointPosition(Point.Base, size), width * PallThickness, line, height);
                return (x, y) => dexterArm(x, y) || sinisterArm(x, y) || stem(x, y);
            }
            default:
                Debug.LogFormat("Haven't implemented {0} yet", shape);
                return (x, y) => false;
        }
    }

    // A band of `thickness` pixels centred on the line from `start` towards `end` (it carries on past both), heading so that the left
    // of the line is the edge the line style is applied to when only one edge is. `reference` is the length of the whole line the
    // band belongs to, so the number of peaks scales with how much of it this band is. With `centred` the pattern is symmetrical about
    // the middle of the band, otherwise it starts a cycle at `start`.
    private static Func<int, int, bool> Band(Vector2 start, Vector2 end, float thickness, Line line, bool primaryEdgeOnly, float reference, bool centred = true) {
        Vector2 along = end - start;
        Vector2 direction = along.normalized;
        Vector2 offset = new Vector2(-direction.y, direction.x) * (thickness / 2f);
        int peaks = Mathf.Max(1, Mathf.RoundToInt(line.PeakCount * along.magnitude / reference));

        // the left edge, with the band on its right
        WaveLine first = line.ToWaveLineWithPeaks(start + offset, end + offset, peaks, centred);

        Line secondLine = primaryEdgeOnly && line.style == LineStyle.Embattled ? default(Line) : line;
        if (!secondLine.IsStraight && secondLine.ParallelEdges) {
            // the same line moved across, with the band on its left
            WaveLine second = first.Shifted(-thickness);
            return (x, y) => first.IsOnSide(x, y, LineSide.Right) && second.IsOnSide(x, y, LineSide.Left);
        }
        else {
            // the right edge heading back the other way, so the band is on its right too and the line bends into it the same way
            WaveLine second = secondLine.ToWaveLineWithPeaks(end - offset, start - offset, peaks, centred);
            return (x, y) => first.IsOnSide(x, y, LineSide.Right) && second.IsOnSide(x, y, LineSide.Right);
        }
    }

    // A band that starts at `start` and runs towards `end` and beyond, with every edge in the line style. The pattern starts a cycle
    // at the start so arms that meet there match.
    private static Func<int, int, bool> HalfBand(Vector2 start, Vector2 end, float thickness, Line line, float reference) {
        Vector2 direction = (end - start).normalized;
        Func<int, int, bool> band = Band(start, end, thickness, line, false, reference, false);
        return (x, y) => Vector2.Dot(new Vector2(x + 0.5f, y + 0.5f) - start, direction) >= 0f && band(x, y);
    }

    // The border: everything outside the shield's outline moved in by a sixth of its width. A line style is applied to that inner
    // edge. The outline is found by marching out from the middle of the shield, which suits the shields that are round about their
    // middle, and its length measured so the peaks are evenly spaced around it however the shield is shaped.
    private static Func<int, int, bool> BordureCoverage(Line line, Shield shield, int size) {
        RectInt bounds = shield.Bounds(size);
        float width = bounds.width * BordureThickness;
        if (line.IsStraight) {
            return (x, y) => shield.DistanceToEdge(x, y, size) <= width;
        }

        Vector2 centre = shield.Pole(size);
        const int steps = 720;
        float[] distanceAlong = new float[steps + 1];   // length of the inner edge from the chief to each angle, going round counter clockwise
        Vector2 previous = Vector2.zero;
        for (int i = 0; i <= steps; i++) {
            float angle = Mathf.PI / 2f + 2f * Mathf.PI * i / steps;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float radius = 0f;
            while (radius < size && shield.DistanceToEdge(Mathf.FloorToInt(centre.x + direction.x * radius), Mathf.FloorToInt(centre.y + direction.y * radius), size) > width) {
                radius += 0.5f;
            }
            Vector2 point = centre + direction * radius;
            distanceAlong[i] = i == 0 ? 0f : distanceAlong[i - 1] + (point - previous).magnitude;
            previous = point;
        }

        float perimeter = distanceAlong[steps];
        // peaks as big as the ones on a line across the shield, unless the blazon gives a number
        int peaks = line.count > 0 ? line.count : Mathf.Max(1, Mathf.RoundToInt(line.PeakCount * perimeter / bounds.width));
        float wavelength = perimeter / peaks;
        float amplitude = line.AmplitudeFor(wavelength);
        LineProfile profile = line.Profile();

        return (x, y) => {
            Vector2 d = new Vector2(x + 0.5f, y + 0.5f) - centre;
            float turns = Mathf.Repeat((Mathf.Atan2(d.y, d.x) - Mathf.PI / 2f) / (2f * Mathf.PI), 1f);
            float position = turns * steps;
            int index = Mathf.Min(Mathf.FloorToInt(position), steps - 1);
            float along = Mathf.Lerp(distanceAlong[index], distanceAlong[index + 1], position - index);
            // positive towards the middle of the shield, the border is the side towards the outline
            float across = shield.DistanceToEdge(x, y, size) - width;
            // a peak at the chief, so the pattern is symmetrical
            return profile.IsBelow(along / wavelength + profile.crestPhase, across / amplitude);
        };
    }

    // Which tincture each pixel of a `size` texture gets in paly, barry, bendy (dexter or sinister) and chevronny, or null for the other
    // variations. The shield's bounds are divided into `count` bars (bendy and chevronny measure along the width) separated by copies of
    // one line moved across the shield a bar at a time, so the bars are parallel and the line style repeats. Each line is described from
    // the bar before it (above, dexter, or the chief side) so the style bends the same way on every one. The tinctures go round in order.
    public static Func<int, int, int> VariationRegions(Variation variation, bool sinister, int count, Line line, Shield shield, int size, int tinctureCount) {
        RectInt bounds = shield.Bounds(size);
        float width = bounds.width;
        float height = bounds.height;
        int n = Mathf.Max(1, tinctureCount);
        count = Mathf.Max(1, count);
        float diagonal = Mathf.Sqrt(width * width + height * height);
        float root2 = Mathf.Sqrt(2f);

        switch (variation) {
            case Variation.Paly: {
                float step = width / count;
                // down the shield, so the bar to dexter is on the right; the first line is on the dexter edge
                WaveLine first = StripeLine(line, new Vector2(bounds.xMin, bounds.yMax), new Vector2(bounds.xMin, bounds.yMin), step, width);
                WaveLine[] lines = ShiftedCopies(first, step, count - 1);
                return (x, y) => (BarsBeyond(lines, x, y) + 1) % n;
            }
            case Variation.Barry: {
                float step = height / count;
                // across the shield towards dexter, so the bar above is on the right; the first line is on the chief edge
                WaveLine first = StripeLine(line, new Vector2(bounds.xMax, bounds.yMax), new Vector2(bounds.xMin, bounds.yMax), step, width);
                WaveLine[] lines = ShiftedCopies(first, step, count - 1);
                return (x, y) => BarsBeyond(lines, x, y) % n;
            }
            case Variation.Bendy: {
                // bars are a width / count apart along x + y (or x - y for sinister), so there are twice as many as `count` in the square
                float step = width / count / root2;
                Vector2 origin = sinister ? new Vector2(bounds.xMin, bounds.yMax) : new Vector2(bounds.xMin, bounds.yMin);
                // dexter: down and to sinister, the bar towards the dexter base on the right; sinister: down and to dexter, the bar towards the dexter chief on the right
                Vector2 direction = sinister ? new Vector2(-1f, -1f) / root2 : new Vector2(1f, -1f) / root2;
                WaveLine first = StripeLine(line, origin - direction * diagonal * 0.6f, origin + direction * diagonal * 0.6f, step, width);
                WaveLine[] lines = ShiftedCopies(first, step, 2 * count - 1);
                return (x, y) => (BarsBeyond(lines, x, y) + 1) % n;
            }
            case Variation.Chevronny: {
                // chevrons with legs at 45 degrees, `width / count` apart going down the shield. The first would have its legs through the
                // top corners of the shield, with its point above the shield on the middle line.
                float rise = width / count;
                float across = rise / root2;
                Vector2 apex = new Vector2(bounds.x + width / 2f, bounds.yMax + width / 2f);
                int chevrons = Mathf.CeilToInt((height + width) / rise);
                // both legs are measured out from the point, so they are mirror images of each other. The dexter leg heads down and to
                // dexter with the chief above it on the right. The sinister leg heads down and to sinister with the chief on the left, so it is mirrored.
                WaveLine dexterFirst = StripeLine(line, apex, apex + new Vector2(-1f, -1f) / root2 * diagonal, across, width);
                WaveLine sinisterFirst = StripeLine(line, apex, apex + new Vector2(1f, -1f) / root2 * diagonal, across, width);
                WaveLine[] dexterLegs = ShiftedCopies(dexterFirst, across, chevrons);
                WaveLine[] sinisterLegs = new WaveLine[chevrons];
                for (int i = 0; i < chevrons; i++) {
                    // the sinister leg's left is up, away from the chevron below it, so moving down is a negative shift
                    sinisterLegs[i] = sinisterFirst.Shifted(-(i + 1) * across).Mirrored();
                }
                float middle = apex.x;
                return (x, y) => {
                    int below = 0;
                    for (int i = 0; i < chevrons; i++) {
                        bool above = x + 0.5f < middle ? dexterLegs[i].IsOnSide(x, y, LineSide.Right) : sinisterLegs[i].IsOnSide(x, y, LineSide.Left);
                        if (!above) {
                            below++;
                        }
                    }
                    return (below + 1) % n;
                };
            }
            default:
                return null;
        }
    }

    // A line for the boundary between bars `spacing` apart, with peaks as big as on a line across the shield (`width` long with the
    // line's own number of peaks) and an amplitude that stops the neighbouring boundaries from crossing.
    private static WaveLine StripeLine(Line line, Vector2 start, Vector2 end, float spacing, float width) {
        float length = (end - start).magnitude;
        int peaks = Mathf.Max(1, Mathf.RoundToInt(line.PeakCount * length / width));
        float amplitude = line.IsStraight ? 0f : Mathf.Min(line.AmplitudeFor(length / peaks), 0.35f * spacing);
        return WaveLine.Between(start, end, peaks, line.Profile(), amplitude);
    }

    // `number` copies of a line, each moved `spacing` pixels further to its left than the last, starting one spacing from the original.
    private static WaveLine[] ShiftedCopies(WaveLine first, float spacing, int number) {
        WaveLine[] copies = new WaveLine[Mathf.Max(0, number)];
        for (int i = 0; i < copies.Length; i++) {
            copies[i] = first.Shifted((i + 1) * spacing);
        }
        return copies;
    }

    // How many of the lines the pixel is past, that is not on the right of: the number of the bar it is in.
    private static int BarsBeyond(WaveLine[] lines, int x, int y) {
        int beyond = 0;
        for (int i = 0; i < lines.Length; i++) {
            if (!lines[i].IsOnSide(x, y, LineSide.Right)) {
                beyond++;
            }
        }
        return beyond;
    }

    public static Texture2D ApplyVariation(Texture2D field, Variation variation, Color[] tinctures, int patternCount = 4, bool sinister = false, Line line = default, Shield shield = null) {
        int patternStep = field.width / patternCount;

        // paly, barry, bendy and chevronny are bars separated by lines, which can take a line style
        Func<int, int, int> bars = VariationRegions(variation, sinister, patternCount, line, shield ?? Shield.Default, field.width, tinctures.Length);
        if (bars != null) {
            for (int x = 0; x < field.width; x++) {
                for (int y = 0; y < field.height; y++) {
                    field.SetPixel(x, y, tinctures[bars(x, y)]);
                }
            }
            return field;
        }

        switch (variation) {
            case Variation.Chequy:
                for (int x = 0; x < field.width; x++) {
                    for (int y = 0; y < field.height; y++) {
                        if ((x * patternCount / field.width + y * patternCount / field.height) % 2 == 0) {
                            field.SetPixel(x, y, tinctures[1]);
                        }
                        else {
                            field.SetPixel(x, y, tinctures[0]);
                        }
                    }
                }
                return field;
            case Variation.Gyronny:
                field = Fill(field, tinctures[0]);
                Vector2Int center = new Vector2Int(field.width / 2, field.height / 2);
                field = DrawLineSegment(field, center, new Vector2Int(0, field.height / 2), tinctures[1]);
                field = DrawLineSegment(field, center, new Vector2Int(0, field.height - 1), tinctures[1]);

                field = DrawLineSegment(field, center, new Vector2Int(field.width / 2, field.height - 1), tinctures[1]);
                field = DrawLineSegment(field, center, new Vector2Int(field.width - 1, field.height - 1), tinctures[1]);

                field = DrawLineSegment(field, center, new Vector2Int(field.width - 1, field.height / 2), tinctures[1]);
                field = DrawLineSegment(field, center, new Vector2Int(field.width - 1, 0), tinctures[1]);

                field = DrawLineSegment(field, center, new Vector2Int(field.width / 2, 0), tinctures[1]);
                field = DrawLineSegment(field, center, new Vector2Int(0, 0), tinctures[1]);

                field = FloodFill(field, new Vector2Int(0, field.height * 3 / 4), tinctures[1], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width * 3 / 4, field.height - 1), tinctures[1], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width - 1, field.height / 4), tinctures[1], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width / 4, 0), tinctures[1], FloodFillMode.UntilTargetColor);

                return field;

            case Variation.Lozengy:
                field = Fill(field, tinctures[0]);
                for (int x = 0; x < patternCount; x++) {
                    for (int y = 0; y < patternCount+1; y++) {
                        field = DrawLozenge(field, new RectInt(x * patternStep, y * patternStep, patternStep, patternStep), tinctures[1]);

                    }
                }
                return field;
            case Variation.Fusilly:
                field = Fill(field, tinctures[0]);
                for (int x = 0; x < patternCount * 2; x++) {
                    for (int y = 0; y < patternCount +1; y++) {
                        field = DrawLozenge(field, new RectInt(x * patternStep / 2, y * patternStep, patternStep / 2, patternStep), tinctures[1]);

                    }
                }
                return field;
            default:
                Debug.LogFormat("Haven't implemented {0} yet", variation);
                return field;


        }
    }

    public static Texture2D ApplyOrdinary(Texture2D field, OrdinaryShape shape, Color tincture, bool sinister = false, Shield shield = null, Line line = default) {
        Func<int, int, bool> covers = OrdinaryCoverage(shape, sinister, line, shield ?? Shield.Default, field.width);
        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                if (covers(x, y)) {
                    field.SetPixel(x, y, tincture);
                }
            }
        }
        return field;
    }

    public static Texture2D ApplyCharge(Texture2D field, Charge charge, Shield shield = null) {
        shield = shield ?? Shield.Default;
        int shieldWidth = shield.Bounds(field.width).width;
        switch (charge.type) {
            case ChargeType.Single:
                charge.point = Point.FessPoint;
                return ApplyChargeAtPoint(field, charge, shieldWidth / 2, shield);

            case ChargeType.Positioned:
                return ApplyChargeAtPoint(field, charge, shieldWidth / 3, shield);

            case ChargeType.Multiple:
                RectInt[] placements = GetChargeLayout(field, charge.count, shield);
                return ApplyChargesAtPlacements(field, charge, placements);

            case ChargeType.LaidOut:
            default:
                Debug.LogFormat("Haven't implemented {0} yet", charge.type);
                return field;
        }
    }

    private static Texture2D ApplyChargeAtPoint(Texture2D field, Charge charge, int scale, Shield shield) {
        RectInt outerRect = RectFromPoint(field, charge.point, scale, shield);
        outerRect = outerRect.ConcentricRectInt(field.width / 32);
        switch (charge.shape) {
            case ChargeShape.Roundel:
                field = DrawCircle(field, outerRect, charge.tincture);
                return field;
            case ChargeShape.Lozenge:
                field = DrawLozenge(field, outerRect, charge.tincture);
                return field;
            case ChargeShape.Fusil:
                field = DrawFusil(field, outerRect, charge.tincture);
                return field;
            case ChargeShape.Billet:
                field = DrawBillet(field, outerRect, charge.tincture);
                return field;
            case ChargeShape.Annulet:
                RectInt innerRect = outerRect.ConcentricRectInt(outerRect.width / 4);
                field = DrawAnnulet(field, outerRect, innerRect, charge.tincture);
                return field;
            case ChargeShape.Mascle:
                RectInt innerRect2 = outerRect.ConcentricRectInt(outerRect.width / 4);
                field = DrawMascle(field, outerRect, innerRect2, charge.tincture);
                return field;
            case ChargeShape.Rustre:
                field = DrawRustre(field, outerRect, charge.tincture);
                return field;
            case ChargeShape.Mullet:
                field = DrawMullet(field, outerRect, charge.tincture, 5);
                return field;
            default:
                Debug.LogFormat("Haven't implemented {0} yet", charge.shape);
                return field;
        }
    }

    public static Texture2D ApplyChargesAtPlacements(Texture2D field, Charge charge, RectInt[] placements) {
        switch (charge.shape) {
            case ChargeShape.Roundel:
                foreach (RectInt place in placements) {
                    field = DrawCircle(field, place, charge.tincture);
                }
                return field;
            case ChargeShape.Lozenge:
                foreach (RectInt place in placements) {
                    field = DrawLozenge(field, place, charge.tincture);
                }
                return field;
            case ChargeShape.Fusil:
                foreach (RectInt place in placements) {
                    field = DrawFusil(field, place, charge.tincture);
                }
                return field;
            case ChargeShape.Billet:
                foreach (RectInt place in placements) {
                    field = DrawBillet(field, place, charge.tincture);
                }
                return field;
            case ChargeShape.Annulet:
                foreach (RectInt place in placements) {
                    RectInt innerRect = place.ConcentricRectInt(place.width / 4);
                    field = DrawAnnulet(field, place, innerRect, charge.tincture);
                }
                return field;
            case ChargeShape.Mascle:
                foreach (RectInt place in placements) {
                    RectInt innerRect2 = place.ConcentricRectInt(place.width / 4);
                    field = DrawMascle(field, place, innerRect2, charge.tincture);
                }
                return field;
            case ChargeShape.Rustre:
                foreach (RectInt place in placements) {
                    field = DrawRustre(field, place, charge.tincture);
                }
                return field;
            case ChargeShape.Mullet:
                foreach (RectInt place in placements) {
                    field = DrawMullet(field, place, charge.tincture, 5);
                }
                return field;
            default:
                Debug.LogFormat("Haven't implemented {0} yet", charge.shape);
                return field;
        }
    }

    #endregion

    #region Helper Functions

    // Where a charge of `size` pixels goes at a named point. The points belong to the shield, see Shield.PlaceCharge.
    public static RectInt RectFromPoint(Texture2D field, Point point, int size, Shield shield = null) {
        return (shield ?? Shield.Default).PlaceCharge(point, size, field.width);
    }

    // Where each of `number` charges goes, laid out within the shield, see Shield.PlaceCharges.
    public static RectInt[] GetChargeLayout(Texture2D field, int number, Shield shield = null) {
        return (shield ?? Shield.Default).PlaceCharges(number, field.width);
    }

    public static Texture2D TextureFromColorsArray(Color[,] colors) {
        Texture2D tex = new Texture2D(colors.GetLength(0), colors.GetLength(1));
        Color[] pix = new Color[colors.GetLength(0) * colors.GetLength(1)];
        for (int x = 0; x < colors.GetLength(0); x++) {
            for (int y = 0; y < colors.GetLength(1); y++) {
                pix[x + y * colors.GetLength(0)] = colors[x, y];
            }
        }
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }

    #endregion

    #region Primitive Drawing Functions

    public static Texture2D Fill(Texture2D field, Color color) {
        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                field.SetPixel(x, y, color);
            }
        }
        return field;
    }

    public static Texture2D Replace(Texture2D field, Color oldColor, Color targetColor) {
        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                if (field.GetPixel(x, y) == oldColor) {
                    field.SetPixel(x, y, targetColor);
                }
            }
        }
        return field;
    }

    public static Texture2D StampToTexture (Texture2D field, Color[,] stamp) {
        if(field.width != stamp.GetLength(0) || field.height != stamp.GetLength(1)) {
            throw new ArgumentException("Stamp and field must be the same size");
        }

        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                if (stamp[x, y] != Color.clear) {
                    field.SetPixel(x, y, stamp[x, y]);
                }
            }
        }
        return field;
    }

    public static Texture2D CutToStamp(Texture2D field, Color[,] stamp) {
        if (field.width != stamp.GetLength(0) || field.height != stamp.GetLength(1)) {
            throw new ArgumentException("Stamp and field must be the same size");
        }
        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                if (stamp[x, y].a == 0) {
                    field.SetPixel(x, y, Color.clear);
                }
            }
        }
        return field;
    }

    public static Texture2D DrawRect(Texture2D field, RectInt rect, Color color) {
        for (int x = rect.x; x < rect.x + rect.width; x++) {
            for (int y = rect.y; y < rect.y + rect.height; y++) {
                field.SetPixel(x, y, color);
            }
        }
        return field;
    }

    public static Color[,] DrawRect(Color[,] field, RectInt rect, Color color) {
        for (int x = rect.x; x < rect.x + rect.width; x++) {
            for (int y = rect.y; y < rect.y + rect.height; y++) {
                field[x, y] = color;
            }
        }
        return field;
    }

    public static Texture2D DrawBillet(Texture2D field, RectInt rect, Color color) {
        rect.x += rect.width / 4;
        rect.width = rect.width / 2;
        rect.y -= rect.height;
        return DrawRect(field, rect, color);
    }

    public static Color[,] DrawBillet(Color[,] field, RectInt rect, Color color) {
        rect.x += rect.width / 4;
        rect.width = rect.width / 2;
        rect.y -= rect.height;
        return DrawRect(field, rect, color);
    }

    public static Texture2D DrawLozenge(Texture2D field, RectInt rect, Color color) {
        Color[,] stamp = field.GetStampTemplate();
        stamp = DrawLozenge(stamp, rect, color);
        return StampToTexture(field, stamp);
    }

    public static Color[,] DrawLozenge(Color[,] field, RectInt rect, Color color) {
        float slope = (float)rect.height / rect.width;
        for(int x = 0; x < rect.width; x++) {
            for(int y = 0; y < rect.height; y++) {                
                
                if(x < rect.width/2) {
                    //we're left
                    if( y > rect.height / 2) {
                        //Up left
                        if(y <= x * slope + rect.height / 2 && 
                            x + rect.x < field.GetLength(0) && y + rect.y - rect.height < field.GetLength(1) && 
                            x + rect.x >= 0 && y + rect.y - rect.height >= 0 ) { 
                            field[x + rect.x, y + rect.y - rect.height] = color;
                        }
                    }
                    else {
                        //Down Left
                        if(y >= x * -slope + rect.height /2 &&
                            x + rect.x < field.GetLength(0) && y + rect.y - rect.height < field.GetLength(1) &&
                            x + rect.x >= 0 && y + rect.y - rect.height >= 0) {
                            field[x + rect.x, y + rect.y - rect.height] = color;
                        }
                    }
                }
                else {
                    if (y > rect.height / 2) {
                        // Up right
                        if (y <= x * -slope + rect.height * 3 / 2 &&
                            x + rect.x < field.GetLength(0) && y + rect.y - rect.height < field.GetLength(1) &&
                            x + rect.x >= 0 && y + rect.y - rect.height >= 0) {
                            field[x + rect.x, y + rect.y - rect.height] = color;
                        }
                    }
                    else {
                        // Down right
                        if (y >= x * slope - rect.height / 2 &&
                            x + rect.x < field.GetLength(0) && y + rect.y - rect.height < field.GetLength(1) &&
                            x + rect.x >= 0 && y + rect.y - rect.height >= 0) {
                            field[x + rect.x, y + rect.y - rect.height] = color;
                        }
                    }
                }
            }
        }
        return field;
    }

    public static Texture2D DrawFusil(Texture2D field, RectInt rect, Color color) {
        rect.x += rect.width / 4;
        rect.width = rect.width / 2;
        return DrawLozenge(field, rect, color);
    }

    public static Color[,] DrawFusil(Color[,] field, RectInt rect, Color color) {
        rect.x += rect.width / 4;
        rect.width = rect.width / 2;
        return DrawLozenge(field, rect, color);
    }

    public static Texture2D DrawMascle(Texture2D field, RectInt outerRect, RectInt innerRect, Color color) {
        Color[,] stamp = field.GetStampTemplate();
        stamp = DrawLozenge(stamp, outerRect, color);
        stamp = DrawLozenge(stamp, innerRect, Color.clear);
        return StampToTexture(field, stamp);
    }

    public static Texture2D DrawRustre(Texture2D field, RectInt outerRect, Color color) {
        Color[,] stamp = field.GetStampTemplate();
        stamp = DrawFusil(stamp, outerRect, color);
        stamp = DrawCircle(stamp, new Vector2Int(outerRect.x + outerRect.width / 2,
                                                 outerRect.y - outerRect.height / 2),
                                                 outerRect.width / 8, Color.clear);
        return StampToTexture(field, stamp);
    }

    public static Texture2D DrawMullet(Texture2D field, RectInt outerRect, Color color, int points=5) {
        Color[,] stamp = field.GetStampTemplate();

        Vector2Int rectCenter = new Vector2Int(outerRect.x + outerRect.width / 2, 
                                               outerRect.y - outerRect.height / 2);

        int outerRadius = outerRect.width / 2;
        int innerRadius = outerRadius * 3 / 8;
        int angle = 360 / points / 2;
        bool innerPoint = true;

        Vector2Int lastPoint = rectCenter + new Vector2Int(0, outerRadius);
        Vector2Int currPoint;
        for(int i = 1; i < points * 2; i++) {
            if (innerPoint) {
                currPoint = rectCenter + new Vector2Int((int)(innerRadius * Mathf.Sin((angle * i) * Mathf.Deg2Rad)),
                                                        (int)(innerRadius * Mathf.Cos((angle * i) * Mathf.Deg2Rad)));
                stamp = DrawLineSegment(stamp, lastPoint, currPoint, color);
                lastPoint = currPoint;
                innerPoint = false;
            }
            else {
                currPoint = rectCenter + new Vector2Int((int)(outerRadius * Mathf.Sin((angle * i) * Mathf.Deg2Rad)),
                                                        (int)(outerRadius * Mathf.Cos((angle * i) * Mathf.Deg2Rad)));
                stamp = DrawLineSegment(stamp, lastPoint, currPoint, color);
                lastPoint = currPoint;
                innerPoint = true;
            }
        }
        stamp = DrawLineSegment(stamp, lastPoint, rectCenter + new Vector2Int(0, outerRadius), color);
        stamp = FloodFill(stamp, rectCenter, color, FloodFillMode.UntilTargetColor);
        return StampToTexture(field, stamp);
    }

    public static Texture2D DrawCircle(Texture2D field, RectInt rect, Color color) {
        return DrawCircle(field, new Vector2Int(rect.x + rect.width / 2, rect.y - rect.height / 2), rect.width / 2, color);
    }

    public static Texture2D DrawCircle(Texture2D field, Vector2Int center, int radius, Color color) {
        for (int x = center.x - radius; x < center.x + radius; x++) {
            for (int y = center.y - radius; y < center.y + radius; y++) {
                if (Vector2Int.Distance(center, new Vector2Int(x, y)) <= radius) {
                    field.SetPixel(x, y, color);
                }
            }
        }
        return field;
    }

    public static Color[,] DrawCircle(Color[,] field, Vector2Int center, int radius, Color color) {
        for (int x = center.x - radius; x < center.x + radius; x++) {
            for (int y = center.y - radius; y < center.y + radius; y++) {
                if (Vector2Int.Distance(center, new Vector2Int(x, y)) <= radius) {
                    field[x, y] = color;
                }
            }
        }
        return field;
    }

    public static Texture2D DrawAnnulet(Texture2D field, RectInt outerRect, RectInt innerRect, Color color) {
        return DrawAnnulet(field, new Vector2Int(outerRect.x + outerRect.width / 2, outerRect.y - outerRect.height / 2),  innerRect.width / 2, outerRect.width / 2, color);
    }

    public static Texture2D DrawAnnulet(Texture2D field, Vector2Int center, int minRadius, int maxRadius, Color color) {
        Color[,] stamp = field.GetStampTemplate();
        stamp = DrawCircle(stamp, center, maxRadius, color);
        stamp = DrawCircle(stamp, center, minRadius, Color.clear);
        field = StampToTexture(field, stamp);
        return field;
    }
    //Given a Color[,] field and starting point, draw a line in the field of a given slope using Bresenham's line algorithm
    
    public static Texture2D DrawSinWave(Texture2D field, RectInt rect, Color color, float amplitude, float frequency, float phase) {
        for (int x = 0; x < rect.width; x++) {
            for(int y = 0; y < rect.height; y++) {
                if(y < (int)(rect.height / 2 + amplitude * Mathf.Sin((x + phase) * frequency * Mathf.Deg2Rad))) {
                    field.SetPixel(rect.x + x, rect.height + y, color);
                }
            }
            //field.SetPixel(x, (int)(rect.y - amplitude * Mathf.Sin((x + phase) * frequency * Mathf.Deg2Rad)), color);
        }
        return field;
    }

    // Rotates the pixels of the texture about its centre, counter clockwise for positive angles.
    // Corners that rotate in from outside the texture become Color.clear. To rotate just one shape, draw it into a
    // blank stamp (GetStampTemplate), RotateStamp that and StampToTexture it instead.
    public static Texture2D RotateTexture(Texture2D field, float angle) {
        Color[,] rotated = RotateStamp(field.GetColorArray(), angle);
        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                field.SetPixel(x, y, rotated[x, y]);
            }
        }
        return field;
    }

    // Rotates a stamp about its centre, counter clockwise for positive angles. Pixels with nothing behind them are Color.clear.
    // Works backwards from each destination pixel to the source pixel it came from, so the result has no holes at any angle.
    public static Color[,] RotateStamp(Color[,] stamp, float angle) {
        int width = stamp.GetLength(0);
        int height = stamp.GetLength(1);
        Color[,] ret = new Color[width, height];

        float ang = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(ang);
        float sin = Mathf.Sin(ang);
        float cx = width / 2f;
        float cy = height / 2f;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                // centre of the destination pixel relative to the middle, rotated back by the angle
                float dx = x + 0.5f - cx;
                float dy = y + 0.5f - cy;
                int sx = Mathf.FloorToInt(cos * dx + sin * dy + cx);
                int sy = Mathf.FloorToInt(-sin * dx + cos * dy + cy);

                if (sx >= 0 && sx < width && sy >= 0 && sy < height) {
                    ret[x, y] = stamp[sx, sy];
                }
            }
        }
        return ret;
    }

    public static Texture2D DrawScallop(Texture2D field, RectInt rect, Color color, float amplitude, float frequency, float phase, bool up) {
        for (int x = 0; x < rect.width; x++) {
            for (int y = 0; y < rect.height; y++) {
                if (up) {
                    if (y < (int)(rect.height / 2 + amplitude * Mathf.Abs(Mathf.Sin((x + phase) * frequency * Mathf.Deg2Rad)))) {
                        field.SetPixel(rect.x + x, rect.height + y, color);
                    }
                }
                else {
                    if (y < (int)(rect.height / 2 - amplitude * Mathf.Abs(Mathf.Sin((x + phase) * frequency * Mathf.Deg2Rad)))) {
                        field.SetPixel(rect.x + x, rect.height + y, color);
                    }
                }
            }
            //field.SetPixel(x, (int)(rect.y - amplitude * Mathf.Sin((x + phase) * frequency * Mathf.Deg2Rad)), color);
        }
        return field;
    }

    #region Wave Drawing Functions

    // Fills everything on `side` of a line bent by a profile. The line types are in Line.cs.
    public static Texture2D DrawWave(Texture2D field, WaveLine line, Color color, LineSide side = LineSide.Right) {
        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                if (line.IsOnSide(x, y, side)) {
                    field.SetPixel(x, y, color);
                }
            }
        }
        return field;
    }

    // Same, into a stamp. Draw with Color.clear to cut a wavy edge out of a stamp.
    public static Color[,] DrawWave(Color[,] field, WaveLine line, Color color, LineSide side = LineSide.Right) {
        for (int x = 0; x < field.GetLength(0); x++) {
            for (int y = 0; y < field.GetLength(1); y++) {
                if (line.IsOnSide(x, y, side)) {
                    field[x, y] = color;
                }
            }
        }
        return field;
    }

    public static Texture2D DrawSinWave(Texture2D field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Sine(), amplitude, wavelength, phase), color, side);
    }

    public static Color[,] DrawSinWave(Color[,] field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Sine(), amplitude, wavelength, phase), color, side);
    }

    public static Texture2D DrawSquareWave(Texture2D field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Square(), amplitude, wavelength, phase), color, side);
    }

    public static Color[,] DrawSquareWave(Color[,] field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Square(), amplitude, wavelength, phase), color, side);
    }

    public static Texture2D DrawTriangleWave(Texture2D field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Triangle(), amplitude, wavelength, phase), color, side);
    }

    public static Color[,] DrawTriangleWave(Color[,] field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Triangle(), amplitude, wavelength, phase), color, side);
    }

    public static Texture2D DrawScallopWave(Texture2D field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Scallop(), amplitude, wavelength, phase), color, side);
    }

    public static Color[,] DrawScallopWave(Color[,] field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Scallop(), amplitude, wavelength, phase), color, side);
    }

    #endregion

    public static Texture2D DrawLine(Texture2D field, Vector2Int intercept, Vector2Int slope, Color color, int thickness) {

        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                if (slope.x * (y - intercept.y) == slope.y * (x - intercept.x)) {
                    if (thickness == 1) {
                        field.SetPixel(x, y, color);
                    }
                    else {
                        for (int i = -thickness / 2; i < thickness / 2; i++) {
                            for (int j = -thickness / 2; j < thickness / 2; j++) {
                                if (x + i < field.width && y + j < field.height &&
                                    x + i >= 0 && y + j >= 0) {
                                    field.SetPixel(x + i, y + j, color);
                                }
                            }
                        }
                    }
                }
            }
        }
        return field;
    }

    public static Color[,] DrawLine(Color[,] field, Vector2Int intercept, Vector2Int slope, Color color, int thickness) {

        for (int x = 0; x < field.GetLength(0); x++) {
            for (int y = 0; y < field.GetLength(1); y++) {
                if (slope.x * (y - intercept.y) == slope.y * (x - intercept.x)) {
                    if (thickness == 1) {
                        field[x, y] = color;
                    }
                    else {
                        for (int i = -thickness / 2; i < thickness / 2; i++) {
                            for (int j = -thickness / 2; j < thickness / 2; j++) {
                                if (x + i < field.GetLength(0) && y + j < field.GetLength(1) &&
                                    x + i >= 0 && y + j >= 0) {
                                    field[x + i, y + j] = color;
                                }
                            }
                        }
                    }
                }
            }
        }
        return field;
    }

    // Height of the pall's junction as a fraction of the shield's height, same as the pall division uses.
    private const float PallJunctionHeight = 2f / 3f;

    public static Texture2D DrawLineSegment(Texture2D field, Vector2Int start, Vector2Int end, Color color) {
        int dx = Mathf.Abs(end.x - start.x);
        int sx = start.x < end.x ? 1 : -1;
        int dy = -Mathf.Abs(end.y - start.y);
        int sy = start.y < end.y ? 1 : -1;
        int err = dx + dy;

        while (true) {
            field.SetPixel(start.x, start.y, color);
            if (start.x == end.x && start.y == end.y) {
                break;
            }

            int e2 = 2 * err;

            if (e2 >= dy) {
                if (start.x == end.x) {
                    break;
                }
                err += dy;
                start.x += sx;
            }
            if (e2 <= dx) {
                if (start.y == end.y) {
                    break;
                }
                err += dx;
                start.y += sy;
            }
        }
        return field;
    }

    public static Color[,] DrawLineSegment(Color[,] field, Vector2Int start, Vector2Int end, Color color) {
        int dx = Mathf.Abs(end.x - start.x);
        int sx = start.x < end.x ? 1 : -1;
        int dy = -Mathf.Abs(end.y - start.y);
        int sy = start.y < end.y ? 1 : -1;
        int err = dx + dy;

        while (true) {
            field[start.x, start.y] = color;
            if (start.x == end.x && start.y == end.y) {
                break;
            }

            int e2 = 2 * err;

            if (e2 >= dy) {
                if (start.x == end.x) {
                    break;
                }
                err += dy;
                start.x += sx;
            }
            if (e2 <= dx) {
                if (start.y == end.y) {
                    break;
                }
                err += dx;
                start.y += sy;
            }
        }
        return field;
    }

    public enum FloodFillMode {
        AllSeedColor,
        UntilTargetColor
    }

    public static Texture2D FloodFill(Texture2D field, Vector2Int seed, Color color, FloodFillMode fillMode) {
        color = (Color32)color;   // compare against what the 8-bit texture actually stores
        Color seedColor = field.GetPixel(seed.x, seed.y);
        if (fillMode == FloodFillMode.AllSeedColor && seedColor == color) {
            return field;
        }
        Stack<Vector2Int> pixelStack = new Stack<Vector2Int>();
        pixelStack.Push(seed);
        while (pixelStack.Count > 0) {
            Vector2Int node = pixelStack.Pop();
            field.SetPixel(node.x, node.y, color);

            if(pixelStack.Count > field.width * field.height) {
                Debug.LogWarningFormat("FloodFill aborted: stack exceeded {0} entries (seed {1}, color {2}, mode {3}). The fill is probably not terminating.", field.width * field.height, seed, color, fillMode);
                return field;
            }

            switch (fillMode) {
                case FloodFillMode.AllSeedColor:
                    if (node.x > 0 && field.GetPixel(node.x - 1, node.y) == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x - 1, node.y));
                    }
                    if (node.x < field.width - 1 && field.GetPixel(node.x + 1, node.y) == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x + 1, node.y));
                    }
                    if (node.y > 0 && field.GetPixel(node.x, node.y - 1) == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x, node.y - 1));
                    }
                    if (node.y < field.height - 1 && field.GetPixel(node.x, node.y + 1) == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x, node.y + 1));
                    }
                    break;
                case FloodFillMode.UntilTargetColor:
                    if (node.x > 0 && field.GetPixel(node.x - 1, node.y) != color) {
                        pixelStack.Push(new Vector2Int(node.x - 1, node.y));
                    }
                    if (node.x < field.width - 1 && field.GetPixel(node.x + 1, node.y) != color) {
                        pixelStack.Push(new Vector2Int(node.x + 1, node.y));
                    }
                    if (node.y > 0 && field.GetPixel(node.x, node.y - 1) != color) {
                        pixelStack.Push(new Vector2Int(node.x, node.y - 1));
                    }
                    if (node.y < field.height - 1 && field.GetPixel(node.x, node.y + 1) != color) {
                        pixelStack.Push(new Vector2Int(node.x, node.y + 1));
                    }
                    break;
                default:
                    goto case FloodFillMode.AllSeedColor;
            }
        }
        return field;

    }

    public static Color[,] FloodFill(Color[,] field, Vector2Int seed, Color color, FloodFillMode fillMode) {
        Color seedColor = field[seed.x, seed.y];
        if (fillMode == FloodFillMode.AllSeedColor && seedColor == color) {
            return field;
        }
        Stack<Vector2Int> pixelStack = new Stack<Vector2Int>();
        pixelStack.Push(seed);
        while (pixelStack.Count > 0) {
            Vector2Int node = pixelStack.Pop();
            field[node.x, node.y] = color;

            switch (fillMode) {
                case FloodFillMode.AllSeedColor:
                    if (node.x > 0 && field[node.x - 1, node.y] == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x - 1, node.y));
                    }
                    if (node.x < field.GetLength(0) - 1 && field[node.x + 1, node.y] == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x + 1, node.y));
                    }
                    if (node.y > 0 && field[node.x, node.y - 1] == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x, node.y - 1));
                    }
                    if (node.y < field.GetLength(1) - 1 && field[node.x, node.y + 1] == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x, node.y + 1));
                    }
                    break;
                case FloodFillMode.UntilTargetColor:
                    if (node.x > 0 && field[node.x - 1, node.y] != color) {
                        pixelStack.Push(new Vector2Int(node.x - 1, node.y));
                    }
                    if (node.x < field.GetLength(0) - 1 && field[node.x + 1, node.y] != color) {
                        pixelStack.Push(new Vector2Int(node.x + 1, node.y));
                    }
                    if (node.y > 0 && field[node.x, node.y - 1] != color) {
                        pixelStack.Push(new Vector2Int(node.x, node.y - 1));
                    }
                    if (node.y < field.GetLength(1) - 1 && field[node.x, node.y + 1] != color) {
                        pixelStack.Push(new Vector2Int(node.x, node.y + 1));
                    }
                    break;
                default:
                    goto case FloodFillMode.AllSeedColor;
            }
        }
        return field;

    }

    #endregion

}
