using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// Reads SVG path data, the "d" attribute of a path (https://www.w3.org/TR/SVG11/paths.html), into polygons.
//
// Handles move, line, horizontal and vertical line, cubic and quadratic curves (and their smooth forms) and close, in their absolute
// (capital) and relative (lower case) forms. Curves are flattened into straight segments. Arcs aren't supported. Every subpath comes back as
// a polygon whether or not it was closed, which is what filling a shape needs. Coordinates are as written, so y points down in SVG.
// Use InPolygons to fill: it fills the way SVG's even odd rule does, so a shape inside another is a hole.
public static class SvgPath {
    public static List<Vector2[]> Parse(string d, int curveSteps = 16) {
        List<Vector2[]> polygons = new List<Vector2[]>();
        List<Vector2> points = new List<Vector2>();
        Vector2 current = Vector2.zero;
        Vector2 start = Vector2.zero;
        Vector2 lastCubicControl = Vector2.zero;
        Vector2 lastQuadraticControl = Vector2.zero;
        bool cubicBefore = false;
        bool quadraticBefore = false;
        char command = ' ';
        int i = 0;

        while (true) {
            SkipSeparators(d, ref i);
            if (i >= d.Length) {
                break;
            }

            if (char.IsLetter(d[i])) {
                command = d[i];
                i++;
                if (command == 'Z' || command == 'z') {
                    Flush(polygons, points);
                    current = start;
                    cubicBefore = quadraticBefore = false;
                    continue;
                }
            }
            else if (command == ' ') {
                throw new FormatException("SVG path data has to start with a command: " + d);
            }

            bool relative = char.IsLower(command);
            char upper = char.ToUpperInvariant(command);
            Vector2 origin = relative ? current : Vector2.zero;
            bool nextCubic = false;
            bool nextQuadratic = false;

            switch (upper) {
                case 'M': {
                    Vector2 p = origin + ReadPoint(d, ref i);
                    Flush(polygons, points);
                    current = start = p;
                    points.Add(p);
                    // numbers after a move are implicit lines
                    command = relative ? 'l' : 'L';
                    break;
                }
                case 'L': {
                    Vector2 p = origin + ReadPoint(d, ref i);
                    Begin(points, current);
                    points.Add(p);
                    current = p;
                    break;
                }
                case 'H': {
                    float x = ReadNumber(d, ref i) + (relative ? current.x : 0f);
                    Begin(points, current);
                    current = new Vector2(x, current.y);
                    points.Add(current);
                    break;
                }
                case 'V': {
                    float y = ReadNumber(d, ref i) + (relative ? current.y : 0f);
                    Begin(points, current);
                    current = new Vector2(current.x, y);
                    points.Add(current);
                    break;
                }
                case 'C': {
                    Vector2 c1 = origin + ReadPoint(d, ref i);
                    Vector2 c2 = origin + ReadPoint(d, ref i);
                    Vector2 p = origin + ReadPoint(d, ref i);
                    Begin(points, current);
                    AddCubic(points, current, c1, c2, p, curveSteps);
                    current = p;
                    lastCubicControl = c2;
                    nextCubic = true;
                    break;
                }
                case 'S': {
                    Vector2 c1 = cubicBefore ? 2f * current - lastCubicControl : current;
                    Vector2 c2 = origin + ReadPoint(d, ref i);
                    Vector2 p = origin + ReadPoint(d, ref i);
                    Begin(points, current);
                    AddCubic(points, current, c1, c2, p, curveSteps);
                    current = p;
                    lastCubicControl = c2;
                    nextCubic = true;
                    break;
                }
                case 'Q': {
                    Vector2 c = origin + ReadPoint(d, ref i);
                    Vector2 p = origin + ReadPoint(d, ref i);
                    Begin(points, current);
                    AddQuadratic(points, current, c, p, curveSteps);
                    current = p;
                    lastQuadraticControl = c;
                    nextQuadratic = true;
                    break;
                }
                case 'T': {
                    Vector2 c = quadraticBefore ? 2f * current - lastQuadraticControl : current;
                    Vector2 p = origin + ReadPoint(d, ref i);
                    Begin(points, current);
                    AddQuadratic(points, current, c, p, curveSteps);
                    current = p;
                    lastQuadraticControl = c;
                    nextQuadratic = true;
                    break;
                }
                default:
                    throw new NotSupportedException("SVG path command '" + command + "' isn't supported");
            }

            cubicBefore = nextCubic;
            quadraticBefore = nextQuadratic;
        }

        Flush(polygons, points);
        return polygons;
    }

    // Whether a point is inside the shape, by the even odd rule: a ray from the point crosses the outlines an odd number of times.
    public static bool InPolygons(List<Vector2[]> polygons, float x, float y) {
        bool inside = false;
        foreach (Vector2[] polygon in polygons) {
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++) {
                Vector2 a = polygon[i];
                Vector2 b = polygon[j];
                if ((a.y > y) != (b.y > y) && x < (b.x - a.x) * (y - a.y) / (b.y - a.y) + a.x) {
                    inside = !inside;
                }
            }
        }
        return inside;
    }

    // The smallest rectangle round every point.
    public static Rect Bounds(List<Vector2[]> polygons) {
        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        foreach (Vector2[] polygon in polygons) {
            foreach (Vector2 p in polygon) {
                minX = Mathf.Min(minX, p.x);
                minY = Mathf.Min(minY, p.y);
                maxX = Mathf.Max(maxX, p.x);
                maxY = Mathf.Max(maxY, p.y);
            }
        }
        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    // A command that carries on from the end of a close starts at the start of the subpath it closed.
    static void Begin(List<Vector2> points, Vector2 current) {
        if (points.Count == 0) {
            points.Add(current);
        }
    }

    static void Flush(List<Vector2[]> polygons, List<Vector2> points) {
        if (points.Count >= 3) {
            polygons.Add(points.ToArray());
        }
        points.Clear();
    }

    static void AddCubic(List<Vector2> points, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int steps) {
        for (int s = 1; s <= steps; s++) {
            float t = (float)s / steps;
            float u = 1f - t;
            points.Add(u * u * u * p0 + 3f * u * u * t * p1 + 3f * u * t * t * p2 + t * t * t * p3);
        }
    }

    static void AddQuadratic(List<Vector2> points, Vector2 p0, Vector2 p1, Vector2 p2, int steps) {
        for (int s = 1; s <= steps; s++) {
            float t = (float)s / steps;
            float u = 1f - t;
            points.Add(u * u * p0 + 2f * u * t * p1 + t * t * p2);
        }
    }

    static Vector2 ReadPoint(string d, ref int i) {
        float x = ReadNumber(d, ref i);
        float y = ReadNumber(d, ref i);
        return new Vector2(x, y);
    }

    static void SkipSeparators(string d, ref int i) {
        while (i < d.Length && (char.IsWhiteSpace(d[i]) || d[i] == ',')) {
            i++;
        }
    }

    // Numbers can run together: "1.5.5" is 1.5 and .5, and "3-2" is 3 and -2.
    static float ReadNumber(string d, ref int i) {
        SkipSeparators(d, ref i);
        int begin = i;
        if (i < d.Length && (d[i] == '-' || d[i] == '+')) {
            i++;
        }
        while (i < d.Length && char.IsDigit(d[i])) {
            i++;
        }
        if (i < d.Length && d[i] == '.') {
            i++;
            while (i < d.Length && char.IsDigit(d[i])) {
                i++;
            }
        }
        if (i < d.Length && (d[i] == 'e' || d[i] == 'E')) {
            int exponent = i;
            i++;
            if (i < d.Length && (d[i] == '-' || d[i] == '+')) {
                i++;
            }
            if (i < d.Length && char.IsDigit(d[i])) {
                while (i < d.Length && char.IsDigit(d[i])) {
                    i++;
                }
            }
            else {
                i = exponent;
            }
        }
        if (i == begin) {
            throw new FormatException("Expected a number at position " + begin + " of SVG path data: " + d);
        }
        return float.Parse(d.Substring(begin, i - begin), CultureInfo.InvariantCulture);
    }
}
