using System;
using System.Linq;
using UnityEngine;
using static BlazonParserHelpers;

[Serializable]
public class Charge : Element {
    public enum ChargeType {
        Single,
        Multiple,
        Positioned,
        LaidOut
    }

    public enum ChargeShape {
        Roundel, //Circle
        Lozenge, //Diamond
        Fusil,   //Thin Diamond
        Billet,  //Square (or rectangle)
        Mascle,  // Square with a hole in the middle
        Annulet, // Ring
        Rustre,  // Square with a hole in the middle with rounded corners
        Mullet,  // Star
    }

    public ChargeType type;
    public ChargeShape shape;
    public Blazon.Point point;
    public Ordinary.OrdinaryShape layout;
    public int count;

    public Charge(ChargeShape shape, Color tincture) {
        type = ChargeType.Single;
        point = Blazon.Point.FessPoint;
        this.shape = shape;
        this.tincture = tincture;
    }

    public Charge(ChargeShape shape, Color tincture, int count) {
        type = ChargeType.Multiple;
        point = Blazon.Point.FessPoint;
        this.shape = shape;
        this.tincture = tincture;
        this.count = count;
    }

    public Charge(ChargeShape shape, Color tincture, Blazon.Point point) {
        type = ChargeType.Positioned;
        this.point = point;
        this.shape = shape;
        this.tincture = tincture;
    }

    public Charge(ChargeShape shape, Color tincture, Ordinary.OrdinaryShape layout) {
        type = ChargeType.LaidOut;
        this.layout = layout;
        this.shape = shape;
        this.tincture = tincture;
    }

    public override string ToBlazonDescription() {
        string ret = string.Empty;
        switch (type) {
            case ChargeType.Single:
                ret = "a " + shape.ToString() + " " + Tincture.Tinctures.FirstOrDefault(x => x.Value == tincture).Key;
                break;
            case ChargeType.Multiple:
                ret = Numbers.FirstOrDefault(x => x.Value == count).Key + " " + shape.ToString() + "s " + Tincture.Tinctures.FirstOrDefault(x => x.Value == tincture).Key;
                break;
            case ChargeType.Positioned:
                ret = "in " + Points.FirstOrDefault(x => x.Value == point).Key + " a " + shape.ToString() + " " + Tincture.Tinctures.FirstOrDefault(x => x.Value == tincture).Key;
                break;
            case ChargeType.LaidOut:
                ret = "A more complex description of the ordinary this is on";
                break;
            default:
                break;
        }
        return ret;
    }
}

