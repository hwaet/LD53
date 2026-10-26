using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Field {

    public enum FieldTypes {
        Simple,
        Varied,
        Divided
    }

    public enum Variation {
        // https://en.wikipedia.org/wiki/Variation_of_the_field
        Barry, // horizontal bars even number of alternating colors first color is what starts
        Paly, // vertical bars
        Bendy, // diagonal bars (outTop-outLeft to outBottom-outRight)
        Chevronny, // chevrons pointing up

        Chequy, // checkerboard
        Lozengy, // diamond pattern (using squares)          
        Fusilly, // diamond pattern (using steeper diamonds) 

        Gyronny // pie slices
    }

    public enum Division {
        // https://en.wikipedia.org/wiki/Division_of_the_field
        Fess, // horizontal line
        Pale, // vertical line
        Bend, // diagonal line (outTop-outLeft to outBottom-outRight)
        Chevron, // chevron pointing up
        Cross, // cross
        Saltire, // diagonal cross
        Pall, // Y shape            
    }

    public FieldTypes type;
    public Variation variation;
    public Division divison;
    public Color[] tinctures;
    public bool sinister;
    public int count;
    // How the division is drawn, a straight line unless the blazon names one. Divided fields use it for their division, and the
    // varied fields paly, barry, bendy and chevronny for the lines between their bars.
    public Line line;

    // How many of its tinctures the field uses: a pall has three parts, a cross or a saltire four, and every other field two.
    public int TinctureCount {
        get {
            if (type == FieldTypes.Simple) {
                return 1;
            }
            if (type == FieldTypes.Divided) {
                if (divison == Division.Pall) {
                    return 3;
                }
                if (divison == Division.Cross || divison == Division.Saltire) {
                    return 4;
                }
            }
            return 2;
        }
    }

    // The tinctures a counterchanged element changes between, in the order the field names them: each part of the element takes the tincture
    // after the one under it, and the last goes round to the first. With two that is a swap, and with more they rotate. A plain field has
    // nothing to change to.
    public bool TryGetCounterchangeCycle(out Color[] cycle) {
        cycle = new Color[0];
        if (type == FieldTypes.Simple) {
            return false;
        }
        int used = Mathf.Min(TinctureCount, tinctures.Length);
        List<Color> distinct = new List<Color>();
        for (int i = 0; i < used; i++) {
            if (!distinct.Contains(tinctures[i])) {
                distinct.Add(tinctures[i]);
            }
        }
        if (distinct.Count < 2) {
            return false;
        }
        cycle = distinct.ToArray();
        return true;
    }

    public Field(Color tincture) {
        type = FieldTypes.Simple;
        tinctures = new Color[] { tincture };
        count = 0;
    }

    public Field(Variation variation, Color[] tinctures, bool sinister, int count, Line line = default) {
        type = FieldTypes.Varied;
        this.variation = variation;
        this.tinctures = tinctures;
        this.sinister = sinister;
        this.count = count;
        this.line = line;
    }

    public Field(Division divsion, Color[] tinctures, bool sinister, Line line = default) {
        type = FieldTypes.Divided;
        this.divison = divsion;
        this.tinctures = tinctures;
        this.sinister = sinister;
        this.line = line;
        count = 0;
    }
}
