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

    public Field(Color tincture) {
        type = FieldTypes.Simple;
        tinctures = new Color[] { tincture };
        count = 0;
    }

    public Field(Variation variation, Color[] tinctures, bool sinister, int count) {
        type = FieldTypes.Varied;
        this.variation = variation;
        this.tinctures = tinctures;
        this.sinister = sinister;
        this.count = count;
    }

    public Field(Division divsion, Color[] tinctures, bool sinister) {
        type = FieldTypes.Divided;
        this.divison = divsion;
        this.tinctures = tinctures;
        this.sinister = sinister;
        count = 0;
    }
}
