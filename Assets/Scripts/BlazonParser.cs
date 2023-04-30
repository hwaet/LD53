using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class BlazonParser : MonoBehaviour {

    //if the first word is a color, then fill the field with that color
    //ex: Azue, a bend or

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
        Lozengy, // diamond pattern (using squares)          << NOT IMPLMENTED
        Fusilly, // diamond pattern (using steeper diamonds) << NOT IMPLMENTED

        Gyronny // pie slices
    }

    public static readonly Dictionary<string, Variation> Variations = new Dictionary<string, Variation>() {
        {"barry", Variation.Barry },
        {"paly", Variation.Paly },
        {"bendy", Variation.Bendy },
        {"chevronny", Variation.Chevronny },
        {"chequy", Variation.Chequy },
        {"lozengy", Variation.Lozengy },
        {"fusilly", Variation.Fusilly },
        {"gyronny", Variation.Gyronny }
    };

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

    public static readonly Dictionary<string, Division> Divisions = new Dictionary<string, Division>() {
        {"fess", Division.Fess },
        {"pale", Division.Pale },
        {"bend", Division.Bend },
        {"chevron", Division.Chevron },
        {"cross", Division.Cross },
        {"saltire", Division.Saltire },
        {"pall", Division.Pall }
    };

    public static readonly HashSet<string> DivisionCues = new HashSet<string>() {
        "party"," parted", "per"
    };

    public static readonly Dictionary<string, int> NumberWords = new Dictionary<string, int>() {
        {"one", 1 },
        {"two", 2},
        {"three", 3},
        {"four", 4},
        {"five", 5 },
        {"six", 6 },
        {"seven", 7 },
        {"eight", 8 },
        {"nine", 9 },
        {"ten", 10 },
        {"eleven", 11 },
        {"twelve", 12 },
        {"thirteen", 13 },
        {"fourteen", 14 },
        {"fifteen", 15 }
    };

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

    public static readonly Dictionary<string, OrdinaryShape> OrdinaryShapes = new Dictionary<string, OrdinaryShape>() {
        {"cross", OrdinaryShape.Cross },
        {"pale", OrdinaryShape.Pale },
        {"fess", OrdinaryShape.Fess },
        {"bend", OrdinaryShape.Bend },
        {"chevron", OrdinaryShape.Chevron },
        {"saltire", OrdinaryShape.Saltire },
        {"chief", OrdinaryShape.Chief },
        {"bordure", OrdinaryShape.Bordure },
        {"pile", OrdinaryShape.Pile },
        {"pall", OrdinaryShape.Pall }
    };

    public class BlazonError: System.Exception {
        public BlazonError(string message) : base(message) { }
    }

    public class Blazon {
        Field field;
        List<Element> elements;
        //Charge[] charges; 

        public Blazon(Field field, List<Element> ordinaries) {
            this.field = field;
            this.elements = ordinaries;
        }
        
        public Blazon(Color tinc) {
            field = new Field(tinc);
            elements  = new List<Element>();
        }

        public Blazon(Variation variation, Color[] tinctures, bool sinister=false, int count=0) {
            field = new Field(variation, tinctures, sinister, count);
            elements = new List<Element>();
        }

        public Blazon(Division divsion, Color[] tinctures, bool sinister= false) {
            field = new Field(divsion, tinctures, sinister);
            elements = new List<Element>();
        }
       
        public Texture2D GenerateTexture(int size) {
            Texture2D tex = new Texture2D(size, size);
            switch(field.type) {
                case FieldTypes.Simple:
                    tex = Fill(tex, field.tinctures[0]);
                    break;
                case FieldTypes.Varied:
                    if (field.count > 0) {
                        tex = ApplyVariation(tex, field.variation, field.tinctures, field.count, field.sinister);
                    }
                    else {
                        tex = ApplyVariation(tex, field.variation, field.tinctures, 4, field.sinister);
                    }
                    break;
                case FieldTypes.Divided:
                    tex = ApplyDivision(tex, field.divison, field.tinctures, field.sinister);
                    break;
            }

            foreach(Element elem in elements) {
                if (elem is Ordinary ordinary) {
                    tex = ApplyOrdinary(tex, ordinary.shape, ordinary.tincture, ordinary.sinister);
                }
                else if(elem is Charge charge) {
                    tex = ApplyCharge(tex, charge);
                }
            }

            tex.Apply();
            return tex;
        }

        public void AddElement(string elementDescription) {
            string[] elementProperties = elementDescription.Trim().Split(" ");
            Color tincture = Color.clear;

            
            if (elementProperties[0] == "a" || elementProperties[0] == "an") {
                Debug.Log("Hit the Single condition");
                if (ChargeShapes.ContainsKey(elementProperties[1])) {
                    Debug.Log("Hit the Charge condition");
                    //we have a single charge 
                    Charge charge = new Charge(ChargeShapes[elementProperties[1]], Tincture.Tinctures[elementProperties[2]]);
                    elements.Add(charge);
                }
                else if (OrdinaryShapes.ContainsKey(elementProperties[1])) {
                    Debug.Log("Hit the Ordinary condition");
                    //we have a single ordinary
                    Ordinary ord = new Ordinary(OrdinaryShapes[elementProperties[1]]);
                    if (elementProperties[2] == "sinister") {
                        ord.sinister = true;
                        ord.tincture = Tincture.Tinctures[elementProperties[3]];
                    }
                    else {
                        ord.tincture = Tincture.Tinctures[elementProperties[2]];
                    }
                    elements.Add(ord);
                }
            }
            else if (elementProperties[0] == "in") {
                Point point = PointWords[elementProperties[1]];
                //elementProperties[2] should be "a" or "an"
                ChargeShape shape = ChargeShapes[elementProperties[3]];
                tincture = Tincture.Tinctures[elementProperties[4]];
                Charge charge = new Charge(shape, tincture, point);
                elements.Add(charge);
            }
            else if (NumberWords.ContainsKey(elementProperties[0])) {
                int number = NumberWords[elementProperties[0]];
                ChargeShape shape = ChargeShapes[elementProperties[1]];
                tincture = Tincture.Tinctures[elementProperties[2]];
                Charge charge = new Charge(shape, tincture, number);
                elements.Add(charge);
            }
            else {
                //We have a problem
                throw new BlazonError("Blazon Element hit the fall through: " + elementProperties[0] + ", "+ elementDescription);
            }
            //}
            //catch(KeyNotFoundException) {
            //    throw new BlazonError("Blazon Element not recognized: " + elementDescription +" because of Key Not Found Exception");
            //}
        }

        public static Blazon Parse(string blazonDescription) {
            if (!blazonDescription.Contains(',')) {
                throw new BlazonError("Blazon elements must be seperated by commas with the field comming first.");
            }
            string[] blazonElements = blazonDescription.ToLower().Split(',');
            string[] field = blazonElements[0].Split(' ');
            bool sinister = false;
            string count = string.Empty;
            Blazon blazon;

            //If it just starts with a color then its a simple field
            if (Tincture.Tinctures.ContainsKey(field[0])) {
                blazon = new Blazon(Tincture.Tinctures[field[0]]);
            }
            //If it starts with the name of a variation then its a varied field
            //This could technically have numbers following it but not handling that for now.
            else if (Variations.ContainsKey(field[0])){
                List<Color> tinctures = new List<Color>();
                for (int i = 1; i < field.Length; i++) {
                    if (Tincture.Tinctures.ContainsKey(field[i])) {
                        tinctures.Add(Tincture.Tinctures[field[i]]);
                    }
                    if (field[i] == "sinister") {
                        sinister = true;
                    }
                    if (field[i] == "of") {
                        if(i+1 < field.Length && NumberWords.ContainsKey(field[i+1])) {
                            count = field[i + 1];
                        }
                        else {
                            throw new BlazonError("Varation looks like it's specifying a number but the number doens't make sense.");
                        }
                    }
                }
                if(tinctures.Count < 2) {
                    throw new BlazonError("A Variation must be composed of at least 2 tinctures.");
                }
                if(count != string.Empty) {
                    blazon = new Blazon(Variations[field[0]], tinctures.ToArray(), sinister, NumberWords[count]);
                }
                else {
                    blazon = new Blazon(Variations[field[0]], tinctures.ToArray(), sinister, -1);
                }   
            }
            //If it starts with Pary, Parted, or Per then its a Divided field
            else if (DivisionCues.Contains(field[0])){
                string divString = string.Empty;
                List<Color> tinctures = new List<Color>();

                for(int i =0; i< field.Length; i++) {
                    if (Divisions.ContainsKey(field[i])) {
                        divString = field[i];
                    }
                    if (Tincture.Tinctures.ContainsKey(field[i])) {
                        tinctures.Add(Tincture.Tinctures[field[i]]);
                    }
                    if (field[i] == "sinister") {
                        sinister = true;
                    }
                }
                if(divString == string.Empty) {
                    throw new BlazonError("Field description resembles a Divsion but not recognizable division type is specified.");
                }
                if (tinctures.Count < 2) {
                    throw new BlazonError("A Divsion must be composed of at least 2 tinctures.");
                }
                blazon =  new Blazon(Divisions[divString], tinctures.ToArray(), sinister);
            }
            else {
                throw new BlazonError("Blazon does not have a reconizable field descriptor.");
            }

            if (blazonElements[1].Length > 0) {
                // Once we've done the field parse the rest of the blazon elements as elements... TODO
                for (int i = 1; i < blazonElements.Length; i++) {
                    blazon.AddElement(blazonElements[i]);
                }
            }
            return blazon;
        }

        public String ToBlazonDescription() {
            string ret = string.Empty;
            switch (field.type) {
                case FieldTypes.Simple:
                    ret = Tincture.Tinctures.FirstOrDefault(x => x.Value == field.tinctures[0]).Key +", ";
                    break;
                case FieldTypes.Varied:
                    ret = field.variation.ToString();
                    if(field.count > 0) {
                        ret += " of" + NumberWords.FirstOrDefault(x => x.Value == field.count).Key;
                    }

                    for (int i =0; i < field.tinctures.Length; i++) {
                        if (i == field.tinctures.Length - 1)
                            ret += " and " + Tincture.Tinctures.FirstOrDefault(x => x.Value == field.tinctures[i]).Key;
                        else
                            ret += " " + Tincture.Tinctures.FirstOrDefault(x => x.Value == field.tinctures[i]).Key;
                    }
                    ret += ", ";
                    break;
                case FieldTypes.Divided:
                    ret = "Party per";
                    ret += " " + Divisions.FirstOrDefault(x => x.Value == field.divison).Key;
                    for (int i = 0; i < field.tinctures.Length; i++) {
                        if (i == field.tinctures.Length - 1)
                            ret += " and " + Tincture.Tinctures.FirstOrDefault(x => x.Value == field.tinctures[i]).Key;
                        else
                            ret += " " + Tincture.Tinctures.FirstOrDefault(x => x.Value == field.tinctures[i]).Key;
                    }
                    break;
            }

            foreach(Element elem in elements) {
                ret += ", " + elem.ToBlazonDescription();
            }
            return ret;
        }
    }

    public class Field {
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

    public abstract class Element {
        public Color tincture;

        public abstract string ToBlazonDescription();
    }

    public class Ordinary: Element {
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

    public static readonly Dictionary<string, ChargeShape> ChargeShapes = new Dictionary<string, ChargeShape>() {
        {"roundel", ChargeShape.Roundel},
        {"lozenge", ChargeShape.Lozenge },
        {"fusil", ChargeShape.Fusil },
        {"billet", ChargeShape.Billet },
        {"mascle", ChargeShape.Mascle },
        {"annulet", ChargeShape.Annulet },
        {"rustre", ChargeShape.Rustre },
        {"mullet", ChargeShape.Mullet },

        {"roundels", ChargeShape.Roundel},
        {"lozenges", ChargeShape.Lozenge },
        {"fusils", ChargeShape.Fusil },
        {"billets", ChargeShape.Billet },
        {"mascles", ChargeShape.Mascle },
        {"annulets", ChargeShape.Annulet },
        {"rustres", ChargeShape.Rustre },
        {"mullets", ChargeShape.Mullet }
    };

    public enum Point {
        Chief,
        Dexter,
        Sinister,
        Base,
        DexterChief,
        MiddleChief,
        SinisterChief,
        HonourPoint,
        FessPoint,
        NombrilPoint,
        DexterBase,
        MiddleBase,
        SinisterBase
    }

    public static readonly Dictionary<string, Point> PointWords = new Dictionary<string, Point>() {
        { "chief", Point.Chief},
        {"dexter", Point.Dexter },
        {"sinister", Point.Sinister },
        {"base", Point.Base },
        {"dexter chief", Point.DexterChief },
        {"middle chief", Point.MiddleChief },
        {"sinister chief", Point.SinisterChief },
        {"honour point", Point.HonourPoint },
        {"fess point", Point.FessPoint },
        {"nombril point", Point.NombrilPoint },
        {"dexter base", Point.DexterBase },
        {"middle base", Point.MiddleBase },
        {"sinister base", Point.SinisterBase }
    };

    public enum ChargeType {
        Single,
        Multiple,
        Positioned,
        LaidOut
    }

    public class Charge: Element {
        public ChargeType type;
        public ChargeShape shape;
        public Point point;
        public OrdinaryShape layout;
        public int count;

        public Charge(ChargeShape shape, Color tincture) {
            type = ChargeType.Single;
            point = Point.FessPoint;
            this.shape = shape;
            this.tincture = tincture;
        }

        public Charge(ChargeShape shape, Color tincture, int count) {
            type = ChargeType.Multiple;
            point = Point.FessPoint;
            this.shape = shape;
            this.tincture = tincture;
        }

        public Charge(ChargeShape shape, Color tincture, Point point) {
            type = ChargeType.Positioned;
            this.point = point;
            this.shape = shape;
            this.tincture = tincture;
        }

        public Charge(ChargeShape shape, Color tincture, OrdinaryShape layout) {
            type = ChargeType.LaidOut;
            this.layout = layout;
            this.shape = shape;
            this.tincture = tincture;
        }

        public override string ToBlazonDescription() {
            string ret = string.Empty;
            switch(type){
                case ChargeType.Single:
                    ret = "a " + shape.ToString() + " " + Tincture.Tinctures.FirstOrDefault(x => x.Value == tincture).Key;
                    break;
                case ChargeType.Multiple:
                    ret = NumberWords.FirstOrDefault(x => x.Value == count).Key + " " + shape.ToString() + "s " + Tincture.Tinctures.FirstOrDefault(x => x.Value == tincture).Key;
                    break;
                case ChargeType.Positioned:
                    ret = "in " + PointWords.FirstOrDefault(x => x.Value == point).Key + " a " + shape.ToString() + " " + Tincture.Tinctures.FirstOrDefault(x => x.Value == tincture).Key;
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


    public static Texture2D FromColorsArray(Color[,] colors) {
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



    private static Texture2D ApplyDivision(Texture2D field, Division div, Color [] tinctures, bool sinister=false) {
        switch (div) {
            case Division.Fess:
                field = Fill(field, tinctures[0]);
                field = DrawRect(field, new RectInt(0, 0, field.width, field.height / 2), tinctures[1]);
                return field;
            case Division.Pale:
                field = Fill(field, tinctures[0]);
                field = DrawRect(field, new RectInt(field.width / 2, 0, field.width / 2, field.height), tinctures[1]);
                return field;
            case Division.Bend:
                if (!sinister) {
                    field = Fill(field, tinctures[0]);
                    field = DrawLineSegment(field, new Vector2Int(0, field.height-1), new Vector2Int(field.width - 1, 0), tinctures[1]);
                    field = FloodFill(field, new Vector2Int(0, 0), tinctures[1], FloodFillMode.AllSeedColor);
                }
                else {
                    field = Fill(field, tinctures[0]);
                    field = DrawLineSegment(field, new Vector2Int(field.width - 1, field.height - 1), new Vector2Int(0, 0), tinctures[1]);
                    field = FloodFill(field, new Vector2Int(field.width-1, 0), tinctures[1], FloodFillMode.AllSeedColor);
                }
                return field;
            case Division.Chevron:
                field = Fill(field, tinctures[0]);
                field = DrawLineSegment(field, new Vector2Int(0, 0), new Vector2Int(field.width / 2, field.height/3), tinctures[1]);
                field = DrawLineSegment(field, new Vector2Int(field.width / 2, field.height / 3), new Vector2Int(field.width, 0), tinctures[1]);
                field = FloodFill(field, new Vector2Int(1, 0), tinctures[1], FloodFillMode.UntilTargetColor);
                return field;
            case Division.Cross: 
                field = Fill(field, tinctures[0]);
                field = DrawRect(field, new RectInt(field.width/2,field.height/2,field.width/2,field.height/2), tinctures[1]);
                field = DrawRect(field, new RectInt(0, 0, field.width / 2, field.height / 2), tinctures[1]);
                return field;
            case Division.Saltire: 
                field = Fill(field, tinctures[0]);
                field = DrawLineSegment(field, new Vector2Int(0, 0), new Vector2Int(field.width-1, field.height-1), tinctures[1]);
                field = DrawLineSegment(field, new Vector2Int(field.width-1, 0), new Vector2Int(0, field.height-1), tinctures[1]);
                field = FloodFill(field, new Vector2Int(0, field.height/2), tinctures[1], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width-1, field.height / 2), tinctures[1], FloodFillMode.UntilTargetColor);
                return field;
            case Division.Pall: 
                field = DrawLineSegment(field, new Vector2Int(field.width / 2, field.height * 2 / 3), new Vector2Int(0, field.height - 1), tinctures[0]);
                field = DrawLineSegment(field, new Vector2Int(field.width / 2, field.height * 2 / 3), new Vector2Int(field.width - 1, field.height - 1), tinctures[0]);
                field = FloodFill(field, new Vector2Int(field.width / 2, field.height - 1), tinctures[0], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(0, 0), tinctures[1], FloodFillMode.AllSeedColor);
                field = DrawLineSegment(field, new Vector2Int(field.width / 2, field.height * 2 / 3), new Vector2Int(field.width/2, 0), tinctures[2]);
                field = FloodFill(field, new Vector2Int(field.width-1, 0), tinctures[2], FloodFillMode.AllSeedColor);
                
                
                //field = FloodFill(field, new Vector2Int(field.width - 1, 0), tinctures[2], FloodFillMode.AllSeedColor);
                return field;
            default: 
                break;
        }
        return field;

    }

    public static Texture2D ApplyVariation(Texture2D field, Variation variation, Color[] tinctures, int patternCount = 4, bool sinister = false) {
        int patternStep = field.width / patternCount;
        int colorDex;

        switch (variation) {
            case Variation.Paly:
                field = Fill(field, tinctures[0]);
                colorDex = 0;
                for(int x = 0; x < field.width; x ++) {
                    if (x % patternStep == 0) {
                        colorDex = (colorDex + 1) % tinctures.Length;
                    }
                    for (int y = 0; y < field.height; y++) {
                        field.SetPixel(x, y, tinctures[colorDex]);   
                    }
                }
                return field;
            case Variation.Barry:
                field = Fill(field, tinctures[0]);
                colorDex = 0;
                for(int y = field.height-1; y >= 0; y --) {
                    if (y % patternStep == 0) {
                        colorDex = (colorDex + 1) % tinctures.Length;
                    }
                    for (int x = 0; x < field.width; x++) {
                        field.SetPixel(x, y, tinctures[colorDex]);   
                    }
                }
                return field;
            case Variation.Bendy:
                colorDex = 0;
                int offset = 0;
                if (sinister) {
                    
                    for (int y = field.height - 1; y >= 0; y--) {
                        for (int x = 0; x < field.width; x++) {
                            if ((x + offset) % patternStep == 0) {
                                colorDex = (colorDex + 1) % tinctures.Length;
                            }
                            field.SetPixel(x, y, tinctures[colorDex]);
                        }
                        offset++;
                        if((y + 1) % patternStep == 0) {
                            colorDex--;
                            if (colorDex < 0) {
                                colorDex = tinctures.Length - 1;
                            }
                        }
                    }
                }
                else {
                    colorDex = 0;
                    offset = field.width;
                    for (int y = field.height - 1; y >= 0; y--) {
                        for (int x = 0; x < field.width; x++) {
                            if ((x + offset) % patternStep == 0) {
                                colorDex = (colorDex + 1) % tinctures.Length;
                            }
                            field.SetPixel(x, y, tinctures[colorDex]);
                        }
                        offset--;
                        if (y % patternStep == 0) {
                            colorDex--;
                            if (colorDex < 0) {
                                colorDex = tinctures.Length - 1;
                            }
                        }
                    }
                }
                return field;
            case Variation.Chequy:
                for(int x = 0; x < field.width; x++) {
                    for (int y = 0; y < field.height; y++) {
                        if ((x / patternStep + y / patternStep) % 2 == 0) {
                            field.SetPixel(x, y, tinctures[1]);
                        }
                        else {
                            field.SetPixel(x, y, tinctures[0]);
                        }
                    }
                }
                return field;
            case Variation.Chevronny:
                field = Fill(field, tinctures[0]);
                int colorDexLeft = 0;
                int colorDexRight = 1;
                int offsetLeft = 0;
                int offsetRight = field.width;

                for(int y = field.height-1; y >= 0; y--) {
                    for(int x = 0; x < field.width/2; x++) {
                        if ((x + offsetLeft) % patternStep == 0) {
                            colorDexLeft = (colorDexLeft + 1) % tinctures.Length;
                        }
                        field.SetPixel(x, y, tinctures[colorDexLeft]);
                    }

                    for (int x = field.width / 2; x < field.width; x++) {
                        if ((x + offsetRight) % patternStep == 0) {
                            colorDexRight = (colorDexRight + 1) % tinctures.Length;
                        }
                        field.SetPixel(x, y, tinctures[colorDexRight]);
                    }
                    if ((y + 1) % patternStep == 0) {
                        colorDexLeft--;
                        if (colorDexLeft < 0) {
                            colorDexLeft = tinctures.Length - 1;
                        }
                    }
                    if(y % patternStep == 0) {
                        colorDexRight--;
                        if (colorDexRight < 0) {
                            colorDexRight = tinctures.Length - 1;
                        }
                    }
                    offsetLeft++;
                    offsetRight--;

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

                field = FloodFill(field, new Vector2Int(0, field.height *3 /4), tinctures[1], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width * 3 / 4, field.height - 1), tinctures[1], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width - 1, field.height / 4), tinctures[1], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width / 4, 0), tinctures[1], FloodFillMode.UntilTargetColor);
                
                return field;

            case Variation.Lozengy:
                field = Fill(field, tinctures[0]);
                for(int x = 0; x < patternCount; x++) {
                    for(int y = 0; y < patternCount; y++) {
                        field = DrawLozenge(field, new RectInt(x * patternStep, y * patternStep, patternStep, patternStep), tinctures[1]);

                    }
                }
                return field;
            case Variation.Fusilly:
                field = Fill(field, tinctures[0]);
                for (int x = 0; x < patternCount*2; x++) {
                    for (int y = 0; y < patternCount; y++) {
                        field = DrawLozenge(field, new RectInt(x * patternStep / 2, y * patternStep, patternStep/2, patternStep), tinctures[1]);

                    }
                }
                return field;
            default:
                Debug.LogFormat("Haven't implemented {0} yet", variation);
                return field;


        }
    }

    public static Texture2D ApplyOrdinary(Texture2D field, OrdinaryShape shape, Color tincture, bool sinister = false) {
        switch (shape) {
            case OrdinaryShape.Chief:
                field = DrawRect(field, new RectInt(0, field.height*2/3, field.width - 1, field.height -1), tincture);
                return field;
            case OrdinaryShape.Fess:
                field = DrawRect(field, new RectInt(0, field.height / 2 - field.height / 8, field.width - 1, field.height / 4), tincture);
                return field;
            case OrdinaryShape.Pale:
                field = DrawRect(field, new RectInt(field.width / 2 - field.width / 8, 0, field.width / 4, field.height), tincture);
                return field;
            case OrdinaryShape.Cross:
                field = DrawRect(field, new RectInt(0, field.height/ 2 + field.height / 10, field.width, field.height / 5), tincture);
                field = DrawRect(field, new RectInt(field.width / 2 - field.width / 10, 0, field.width / 5, field.height), tincture);
                return field;
            case OrdinaryShape.Bend:
                if (sinister) {
                    field = DrawLine(field, new Vector2Int(0, 0), new Vector2Int(1, 1), tincture, field.width / 8);
                }
                else {
                    field = DrawLine(field, new Vector2Int(0, field.height), new Vector2Int(1, -1), tincture, field.width / 8);
                }
                return field;
            case OrdinaryShape.Saltire:
                field = DrawLineSegment(field, new Vector2Int(field.width / 8, 0), new Vector2Int(field.width - 1, field.height * 7 / 8), tincture);
                field = DrawLineSegment(field, new Vector2Int(0, field.height / 8), new Vector2Int(field.width * 7 / 8, field.height - 1), tincture);

                field = DrawLineSegment(field, new Vector2Int(field.width / 8, field.height-1), new Vector2Int(field.width - 1, field.height / 8), tincture);
                field = DrawLineSegment(field, new Vector2Int(0, field.width * 7 / 8), new Vector2Int(field.width * 7 / 8, 0), tincture);

                field = FloodFill(field, new Vector2Int(field.width / 2, field.height / 2), tincture, FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(0, 0), tincture, FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(0, field.height-1), tincture, FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width-1, field.height - 1), tincture, FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width - 1, 0), tincture, FloodFillMode.UntilTargetColor);
                return field;

            case OrdinaryShape.Pile:
                field = DrawLineSegment(field, new Vector2Int(field.width / 4, field.height-1), new Vector2Int(field.width / 2, field.height / 10), tincture);
                field = DrawLineSegment(field, new Vector2Int(field.width * 3 / 4, field.height - 1), new Vector2Int(field.width / 2, field.height / 10), tincture);
                field = FloodFill(field, new Vector2Int(field.width /2, field.height-1), tincture, FloodFillMode.UntilTargetColor);
                return field;
            
            case OrdinaryShape.Chevron:
                field = DrawLineSegment(field, new Vector2Int(field.width / 2, field.height * 3 / 4), new Vector2Int(0, field.height / 4), tincture);
                field = DrawLineSegment(field, new Vector2Int(field.width / 2, field.height * 3 / 4), new Vector2Int(field.width-1, field.height / 4), tincture);
                field = DrawLineSegment(field, new Vector2Int(field.width / 2, field.height * 3 / 4 - field.height / 5), new Vector2Int(0, field.height / 4 - field.height / 5), tincture);
                field = DrawLineSegment(field, new Vector2Int(field.width / 2, field.height * 3 / 4 - field.height / 5), new Vector2Int(field.width-1, field.height / 4 - field.height / 5), tincture);
                field = FloodFill(field, new Vector2Int(field.width / 2, field.height * 3 / 4 - 1), tincture, FloodFillMode.UntilTargetColor);
                return field;

            case OrdinaryShape.Pall:
            case OrdinaryShape.Bordure:
            default:
                Debug.LogFormat("Haven't implemented {0} yet", shape);
                return field;

        }
    }

    public static Texture2D ApplyCharge(Texture2D field, Charge charge) {
        switch (charge.type) {
            case ChargeType.Single:
                return ApplySingleCharge(field, charge);

            case ChargeType.Positioned:
            case ChargeType.Multiple:
            case ChargeType.LaidOut:
            default:
                Debug.LogFormat("Haven't implemented {0} yet", charge.type);
                return field;
        }
    }

   public static Texture2D ApplySingleCharge(Texture2D field, Charge charge) {
        switch (charge.shape) {
            case ChargeShape.Roundel:
                field = DrawCircle(field, new Vector2Int(field.width / 2, field.height / 2), field.width/4, charge.tincture);
                return field;
            case ChargeShape.Lozenge:
                field = DrawLozenge(field, new RectInt(field.width/4,field.height*3/4,field.width/2,field.height/2), charge.tincture);
                return field;
            case ChargeShape.Fusil:
                field = DrawLozenge(field, new RectInt(field.width * 3 / 8, field.height * 3 / 4, field.width / 4, field.height / 2), charge.tincture);
                return field;
            case ChargeShape.Billet:
                field = DrawRect(field, new RectInt(field.width * 3 / 8, field.height / 4, field.width / 4, field.height / 2), charge.tincture);
                return field;
            case ChargeShape.Annulet:
                field = DrawRing(field, new Vector2Int(field.width / 2, field.height / 2), field.width / 6, field.width / 4, charge.tincture);
                return field;
            case ChargeShape.Mascle:
                field = DrawMascle(field,
                    new RectInt(field.width / 4, field.height * 3 / 4, field.width / 2, field.height / 2),
                    new RectInt(field.width * 3/8, field.height * 5/8, field.width / 4, field.height / 4),
                    charge.tincture);
                return field;
            case ChargeShape.Mullet:
            case ChargeShape.Rustre:
            default:
                Debug.LogFormat("Haven't implemented {0} yet", charge.shape);
                return field;
        }
    }

    private GUIStyle testTextureStyle = null;

    private string blazonPrompt = "Azure, a bend or";
    string parseError = string.Empty;

    private Texture2D testTexture = null;
    private Blazon testBlazon;

    private void OnGUI() {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Blazon:");
        blazonPrompt = GUILayout.TextField(blazonPrompt);

        parseError = "Blazon Valid!";
        try {
            testBlazon = Blazon.Parse(blazonPrompt);
        }
        catch (Exception e) {
            parseError = e.Message;
        }

        GUI.enabled = parseError == "Blazon Valid!";

        if (GUILayout.Button("Generate")) {
            testTexture = testBlazon.GenerateTexture(256);
            Debug.Log("Generating:" + testBlazon.ToBlazonDescription());
        }

        GUI.enabled = true;

        GUILayout.EndHorizontal();

        GUILayout.Label(parseError);


        GUILayout.Box(testTexture);

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private Texture2D GenerationExperiments() {
        Texture2D tex = new Texture2D(256, 256);
        tex = Fill(tex, Tincture.Azure);
        //tex = DrawLine(tex, new Vector2Int(0, 256 / 4), new Vector2Int(1, 0), Tincture.Or, 10);
        //tex = DrawLine(tex, new Vector2Int(0, 256 / 4 * 3), new Vector2Int(1, 0), Tincture.Or, 10);
        //tex = DrawLine(tex, new Vector2Int(256 / 4, 0), new Vector2Int(0, 1), Tincture.Or, 10);
        //tex = DrawLine(tex, new Vector2Int(256 / 4 * 3, 0), new Vector2Int(0, 1), Tincture.Or, 10);
        //tex = FloodFill(tex, new Vector2Int(128, 128), Tincture.Vert, FloodFillMode.AllSeedColor);

        //tex = DrawLineSegment(tex, Vector2Int.zero, new Vector2Int(128, 128), Tincture.Gules);
        //tex = DrawLineSegment(tex, new Vector2Int(128, 128), new Vector2Int(256, 0), Tincture.Gules);


        //tex = DrawLineSegment(tex, new Vector2Int( 20,0), new Vector2Int(128, 108), Tincture.Gules);
        //tex = DrawLineSegment(tex, new Vector2Int(128, 108), new Vector2Int(256-20, 0), Tincture.Gules);
        //tex = FloodFill(tex, new Vector2Int(128, 126), Tincture.Gules, FloodFillMode.UntilTargetColor);

        tex = DrawRect(tex, new RectInt(64, 64, 128, 128), Tincture.Gules);

        tex.Apply();
        return tex;
    }


    // Start is called before the first frame update
    void Start() {
        testTexture = GenerationExperiments();
    }

    // Update is called once per frame
    void Update() {

    }

    public enum FloodFillMode {
        AllSeedColor,
        UntilTargetColor
    }

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

    public static Texture2D DrawRect(Texture2D field, RectInt rect, Color color) {
        for (int x = rect.x; x < rect.x + rect.width; x++) {
            for (int y = rect.y; y < rect.y + rect.height; y++) {
                field.SetPixel(x, y, color);
            }
        }
        return field;
    }

    public static Texture2D DrawLozenge(Texture2D field, RectInt rect, Color color) {
        Vector2Int left = new Vector2Int(rect.x, rect.y - rect.height / 2);
        Vector2Int right = new Vector2Int(rect.x + rect.width, rect.y - rect.height / 2);
        Vector2Int top = new Vector2Int(rect.x + rect.width / 2, rect.y);
        Vector2Int bottom = new Vector2Int(rect.x + rect.width / 2, rect.y - rect.height);
        field = DrawLineSegment(field, left, top, color);
        field = DrawLineSegment(field, top, right, color);
        field = DrawLineSegment(field, right, bottom, color);
        field = DrawLineSegment(field, bottom, left, color);
        field = FloodFill(field, new Vector2Int(rect.x + rect.width / 2, rect.y - rect.height / 2), color, FloodFillMode.UntilTargetColor);
        return field;
    }

    public static Texture2D DrawMascle(Texture2D field, RectInt outerRect, RectInt innerRect, Color color) {
        Vector2Int outLeft = new Vector2Int(outerRect.x, outerRect.y - outerRect.height / 2);
        Vector2Int outRight = new Vector2Int(outerRect.x + outerRect.width, outerRect.y - outerRect.height / 2);
        Vector2Int outTop = new Vector2Int(outerRect.x + outerRect.width / 2, outerRect.y);
        Vector2Int outBottom = new Vector2Int(outerRect.x + outerRect.width / 2, outerRect.y - outerRect.height);

        Vector2Int inLeft = new Vector2Int(innerRect.x, innerRect.y - innerRect.height / 2);
        Vector2Int inRight = new Vector2Int(innerRect.x + innerRect.width, innerRect.y - innerRect.height / 2);
        Vector2Int inTop = new Vector2Int(innerRect.x + innerRect.width / 2, innerRect.y);
        Vector2Int inBottom = new Vector2Int(innerRect.x + innerRect.width / 2, innerRect.y - innerRect.height);

        field = DrawLineSegment(field, outLeft, outTop, color);
        field = DrawLineSegment(field, outTop, outRight, color);
        field = DrawLineSegment(field, outRight, outBottom, color);
        field = DrawLineSegment(field, outBottom, outLeft, color);

        field = DrawLineSegment(field, inLeft, inTop, color);
        field = DrawLineSegment(field, inTop, inRight, color);
        field = DrawLineSegment(field, inRight, inBottom, color);
        field = DrawLineSegment(field, inBottom, inLeft, color);


        field = FloodFill(field, new Vector2Int(outLeft.x +1, outLeft.y) , color, FloodFillMode.UntilTargetColor);
        return field;
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

    public static Texture2D DrawRing(Texture2D field, Vector2Int center, int minRadius, int maxRadius, Color color) {
        for (int x = center.x - maxRadius; x < center.x + maxRadius; x++) {
            for (int y = center.y - maxRadius; y < center.y + maxRadius; y++) {
                if (Vector2Int.Distance(center, new Vector2Int(x, y)) <= maxRadius && Vector2Int.Distance(center, new Vector2Int(x,y)) > minRadius) {
                    field.SetPixel(x, y, color);
                }
            }
        }
        return field;
    }


    //Given a Color[,] field and starting point, draw a line in the field of a given slope using Bresenham's line algorithm
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

    public static Texture2D FloodFill(Texture2D field, Vector2Int seed, Color color, FloodFillMode fillMode) {
        Color seedColor = field.GetPixel(seed.x, seed.y);
        if(fillMode == FloodFillMode.AllSeedColor && seedColor == color) {
            return field;
        }
        Stack<Vector2Int> pixelStack = new Stack<Vector2Int>();
        pixelStack.Push(seed);
        while (pixelStack.Count > 0) {
            Vector2Int node = pixelStack.Pop();
            field.SetPixel(node.x, node.y, color);

            switch (fillMode) {
                case FloodFillMode.AllSeedColor:
                    if (node.x > 0 && field.GetPixel(node.x - 1, node.y) == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x - 1, node.y));
                    }
                    if (node.x < field.width - 1 && field.GetPixel(node.x + 1, node.y) == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x+1, node.y));
                    }
                    if (node.y > 0 && field.GetPixel(node.x, node.y-1) == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x, node.y-1));
                    }
                    if (node.y < field.height - 1 && field.GetPixel(node.x, node.y+1) == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x, node.y+1));
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

}
