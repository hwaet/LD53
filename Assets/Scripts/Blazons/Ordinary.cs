using System.Linq;
using UnityEngine;

public class Ordinary: Element {
    public enum OrdinaryShape {
        Cross, // cross
        Pale, // vertical line
        Fess, // horizontal line
        Bend, // diagonal line (outTop-outLeft to outBottom-outRight)
        Chevron, // chevron pointing up
        Saltire, // diagonal cross
        Chief, // horizontal line at outTop
        Bordure, // border
        Pile, // triangle pointing down
        Pall, // Y shape
    }

    public OrdinaryShape shape;
    public bool sinister;

    public Ordinary(OrdinaryShape shape) {
        this.shape = shape;
        tincture = Color.clear;
        sinister = false;
    }

    public Ordinary(OrdinaryShape shape, Color tincture, bool sinister=false) {
        this.shape = shape;
        this.tincture = tincture;
        this.sinister = sinister;
    }

    public override string ToBlazonDescription() {
        string ret = "a " + shape.ToString();
        if (sinister) {
            ret += " sinister";
        }
        ret += " " + Tincture.Tinctures.FirstOrDefault(x => x.Value == tincture).Key;
        return ret;
    }
}


