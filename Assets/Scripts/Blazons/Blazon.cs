using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Field;
using static BlazonDrawers;
using static BlazonParserHelpers;
using static Charge;
using static Ordinary;
using TMPro;

[System.Serializable]
public class Blazon {
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


    public Field field;
    public List<Element> elements;

    public Blazon(Field field, List<Element> elements) {
        this.field = field;
        this.elements = elements;
    }
        
    public Blazon(Color tinc) {
        field = new Field(tinc);
        elements  = new List<Element>();
    }

    public Blazon(Variation variation, Color[] tinctures, bool sinister=false, int count=0, Line line = default) {
        field = new Field(variation, tinctures, sinister, count, line);
        elements = new List<Element>();
    }

    public Blazon(Division divsion, Color[] tinctures, bool sinister= false, Line line = default) {
        field = new Field(divsion, tinctures, sinister, line);
        elements = new List<Element>();
    }
       
    public Texture2D GenerateTexture(int size, Shield shield = null) {
        Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
		switch (field.type) {
            case FieldTypes.Simple:
                tex = Fill(tex, field.tinctures[0]);
                break;
            case FieldTypes.Varied:
                if (field.count > 0) {
                    tex = ApplyVariation(tex, field.variation, field.tinctures, field.count, field.sinister, field.line, shield);
                }
                else {
                    tex = ApplyVariation(tex, field.variation, field.tinctures, 4, field.sinister, field.line, shield);
                }
                break;
            case FieldTypes.Divided:
                tex = ApplyDivision(tex, field.divison, field.tinctures, field.sinister, field.line, shield);
                break;
        }

        // a fur's marker color becomes its pattern as soon as the field or element that used it is drawn, so the pattern is scaled to that
        // shape, and an element drawn over it isn't taken for part of it
        tex = Furs.Resolve(tex, shield);

        foreach(Element elem in elements) {
            if (elem is Ordinary ordinary) {
                tex = ApplyOrdinary(tex, ordinary.shape, ordinary.tincture, ordinary.sinister, shield, ordinary.line);
            }
            else if(elem is Charge charge) {
                tex = ApplyCharge(tex, charge, shield);
            }
            tex = Furs.Resolve(tex, shield);
        }

        tex.Apply();
        return tex;
    }

    public void AddElement(string elementDescription) {
        string[] elementProperties = elementDescription.Trim().Split(" ");
            
        if (elementProperties[0] == "a" || elementProperties[0] == "an") {
            if (ChargeShapes.ContainsKey(elementProperties[1])) {
                //we have a single charge 
                Charge charge = new Charge(ChargeShapes[elementProperties[1]], Tincture.Tinctures[elementProperties[2]]);
                elements.Add(charge);
            }
            else if (OrdinaryShapes.ContainsKey(elementProperties[1])) {
                //we have a single ordinary
                Ordinary ord = new Ordinary(OrdinaryShapes[elementProperties[1]]);
                int next = 2;
                if (elementProperties[next] == "sinister") {
                    ord.sinister = true;
                    next++;
                }
                Line parsedLine;
                int used;
                if (Line.TryParse(elementProperties, next, out parsedLine, out used)) {
                    ord.line = parsedLine;
                    next += used;
                }
                ord.tincture = Tincture.Tinctures[elementProperties[next]];
                elements.Add(ord);
            }
            else {
                //We have a problem
                throw new BlazonError("Blazon Element looks like it's trying to be an ordinary or simple charge but the type wasn't recognized: " + elementDescription);
            }
        }
        else if (elementProperties[0] == "in") {
            
            if (Points.ContainsKey(elementProperties[2]) || elementProperties[2] == "point") {
                // then we have a 2 word point name
                Point point = Points[elementProperties[1]+" "+elementProperties[2]];

                //elementProperties[3] should be "a" or "an"
                ChargeShape shape = ChargeShapes[elementProperties[4]];
                Charge charge = new Charge(shape, Tincture.Tinctures[elementProperties[5]], point);
                elements.Add(charge);
            }
            else {
                Point point = Points[elementProperties[1]];

                //elementProperties[2] should be "a" or "an"
                ChargeShape shape = ChargeShapes[elementProperties[3]];
                Charge charge = new Charge(shape, Tincture.Tinctures[elementProperties[4]], point);
                elements.Add(charge);
            }
        }
        else if (Numbers.ContainsKey(elementProperties[0])) {
            Charge charge = new Charge(ChargeShapes[elementProperties[1]], Tincture.Tinctures[elementProperties[2]], Numbers[elementProperties[0]]);
            elements.Add(charge);
        }
        else {
            //We have a problem
            throw new BlazonError("Blazon Element doesn't think it's an ordinary, simple charge, positioned charge, or multiple charge: " + elementDescription);
        }
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
        else if (Variations.ContainsKey(field[0])){
            List<Color> tinctures = new List<Color>();
            Line variationLine = default;
            for (int i = 1; i < field.Length; i++) {
                // the number after "of" is how many bars there are, not how many peaks the line has
                Line parsedLine;
                int used;
                if (Line.TryParse(field, i, out parsedLine, out used, false)) {
                    variationLine = parsedLine;
                    continue;
                }
                if (Tincture.Tinctures.ContainsKey(field[i])) {
                    tinctures.Add(Tincture.Tinctures[field[i]]);
                }
                if (field[i] == "sinister") {
                    sinister = true;
                }
                if (field[i] == "of") {
                    if(i+1 < field.Length && Numbers.ContainsKey(field[i+1])) {
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
                blazon = new Blazon(Variations[field[0]], tinctures.ToArray(), sinister, Numbers[count], variationLine);
            }
            else {
                blazon = new Blazon(Variations[field[0]], tinctures.ToArray(), sinister, -1, variationLine);
            }   
        }
        //If it starts with Pary, Parted, or Per then its a Divided field
        else if (DivisionCues.Contains(field[0])){
            string divString = string.Empty;
            List<Color> tinctures = new List<Color>();
            Line divisionLine = default;

            for(int i =0; i< field.Length; i++) {
                Line parsedLine;
                int used;
                if (Line.TryParse(field, i, out parsedLine, out used)) {
                    divisionLine = parsedLine;
                    i += used - 1;
                    continue;
                }
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
            blazon =  new Blazon(Divisions[divString], tinctures.ToArray(), sinister, divisionLine);
        }
        else {
            throw new BlazonError("Blazon does not have a reconizable field descriptor.");
        }

        if (blazonElements[1].Length > 0) {
            // Once we've done the field, parse the rest of the blazon elements as elements
            for (int i = 1; i < blazonElements.Length; i++) {
                blazon.AddElement(blazonElements[i]);
            }
        }
        return blazon;
    }

    public string ToBlazonDescription() {
        string ret = string.Empty;
        switch (field.type) {
            case FieldTypes.Simple:
                ret = Tincture.Tinctures.FirstOrDefault(x => x.Value == field.tinctures[0]).Key;
                break;
            case FieldTypes.Varied:
                ret = field.variation.ToString();

                if(field.variation == Variation.Bendy && field.sinister) {
                    ret += " sinister";
                }

                if (!field.line.IsStraight) {
                    ret += " " + field.line.StyleName;
                }

                if(field.count > 0) {
                    ret += " of " + Numbers.FirstOrDefault(x => x.Value == field.count).Key;
                }

                for (int i =0; i < field.tinctures.Length; i++) {
                    if (i == field.tinctures.Length - 1)
                        ret += " and " + Tincture.Tinctures.FirstOrDefault(x => x.Value == field.tinctures[i]).Key;
                    else
                        ret += " " + Tincture.Tinctures.FirstOrDefault(x => x.Value == field.tinctures[i]).Key;
                }
                break;
            case FieldTypes.Divided:
                ret = "Party per";
                ret += " " + Divisions.FirstOrDefault(x => x.Value == field.divison).Key;
                if(field.divison == Division.Bend && field.sinister) {
                    ret += " sinister";
                }
                if (!field.line.IsStraight) {
                    ret += " " + field.line.ToBlazonDescription();
                }

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

        ret = ret.ToLower();

        ret = ret.Replace("argent", "Argent");
        ret = ret.Replace("or", "Or");
        ret = ret.Replace("gules", "Gules");
        ret = ret.Replace("sable", "Sable");
        ret = ret.Replace("azure", "Azure");
        ret = ret.Replace("vert", "Vert");
        ret = ret.Replace("purpure", "Purpure");
        
        ret = ret.Replace("tenne", "Tenne");
        ret = ret.Replace("sanguine", "Sanguine");
        ret = ret.Replace("murrey", "Murrey");
        ret = ret.Replace("carnation", "Carnation");
        ret = ret.Replace("celeste", "Celeste");
        ret = ret.Replace("rose", "Rose");
        ret = ret.Replace("bleuceleste", "Bleu Celeste");
        ret = ret.Replace("bleu-celeste", "Bleu-Celeste");
        ret = ret.Replace("brunatre", "Brunatre");
        ret = ret.Replace("orange", "Orange");

        ret = ret.Replace("erminois", "Erminois");
        ret = ret.Replace("counter-vair", "Counter-Vair");
        ret = ret.Replace("vair", "Vair");
        ret = ret.Replace("pean", "Pean");
        ret = ret.Replace("ermine", "Ermine");
        
        return ret;
    }

    // Warnings about blazons that parse fine but contain elements that can't be seen.
    // These are warnings, not errors: an element sharing a tincture with the field is legitimate when it is
    // countercharged, which isn't modelled yet. Once it is, exempt those elements from the field check below.
    public List<string> Validate() {
        List<string> warnings = new List<string>();

        if (field.type == FieldTypes.Varied && !field.line.IsStraight && !LineApplies(field.variation)) {
            warnings.Add("A line style only applies to paly, barry, bendy and chevronny, so \"" + field.line.StyleName + "\" is ignored on " + field.variation.ToString().ToLower() + ".");
        }

        for (int i = 0; i < elements.Count; i++) {
            Element elem = elements[i];
            string what = elem.ToBlazonDescription();
            string name = TinctureName(elem.tincture);

            if (System.Array.Exists(field.tinctures, t => t == elem.tincture)) {
                if (field.type == FieldTypes.Simple) {
                    warnings.Add("\"" + what + "\" is the same colour as the field (" + name + ") so it will be invisible.");
                }
                else {
                    warnings.Add("\"" + what + "\" shares " + name + " with the field, so it will be partly or fully hidden unless it is countercharged.");
                }
            }

            for (int j = 0; j < i; j++) {
                if (elements[j].tincture == elem.tincture) {
                    warnings.Add("\"" + what + "\" is the same colour (" + name + ") as the earlier \"" + elements[j].ToBlazonDescription() + "\" so they may blend together.");
                    break;
                }
            }
        }

        return warnings;
    }

    // Whether a variation of the field has lines between its bars that a line style can be applied to.
    public static bool LineApplies(Variation variation) {
        return variation == Variation.Paly || variation == Variation.Barry || variation == Variation.Bendy || variation == Variation.Chevronny;
    }

    private static string TinctureName(Color tincture) {
        string name = Tincture.Tinctures.FirstOrDefault(x => x.Value == tincture).Key;
        return name ?? tincture.ToString();
    }

    // avoidFieldColorReuse: ordinaries and charges may not use any of the field's tinctures.
    // avoidElementColorReuse: an ordinary or charge may not reuse the tincture of an earlier element (the field is not considered).
    // If the rules can't be satisfied (pool exhausted) the element rule is relaxed first, then the field rule, with a warning logged.
    public static Blazon RandomBlazon(bool complexfields, bool ruleOfTincture, bool ordinary, int charges, bool avoidFieldColorReuse = true, bool avoidElementColorReuse = true) {
        Field field = null;
        List<Color> colours = new List<Color>();
        List<Color> metals = new List<Color>();

        if (ruleOfTincture) {
            colours = new List<Color>(Tincture.Colours.Values);
            metals = new List<Color>(Tincture.Metals.Values);
        }
        else {
            colours = new List<Color>(Tincture.Tinctures.Values);
        }

        if (!complexfields) {
            var dex = Random.Range(0, colours.Count);
            var tincture = colours[dex];
            colours.RemoveAt(dex);
            field = new Field(tincture);
        }
        else {
            float fate = Random.Range(0f, 1f);
            if(fate < .2f) {
                field = new Field(colours[Random.Range(0, colours.Count)]);
            }
            else if(fate < .6f) {
                //varied field 
                var variation = Variations.Values.ElementAt(Random.Range(0, Variations.Values.Count));
                List<Color> tinctures = new List<Color>();
                var dex = Random.Range(0, colours.Count);
                tinctures.Add(colours[dex]);
                colours.RemoveAt(dex);
                if (ruleOfTincture) {
                    dex = Random.Range(0, metals.Count);
                    tinctures.Add(metals[dex]);
                    metals.RemoveAt(dex);
                }
                else {
                    dex = Random.Range(0, colours.Count);
                    tinctures.Add(colours[dex]);
                    colours.RemoveAt(dex);
                }
                int count;
                if (Random.value > .65) {
                    count = Numbers.Values.ElementAt(Random.Range(0, Numbers.Values.Count));
                }
                else {
                    count = 4;
                }
                field = new Field(variation, tinctures.ToArray(), Random.value > .5, count);
                //need the type of variation and 2 tinctures
            }
            else {
                //need the division type and 2 (possibly 3) tinctures
                var division = Divisions.Values.ElementAt(Random.Range(0, Divisions.Values.Count));
                List<Color> tinctures = new List<Color>();

                var dex = Random.Range(0, colours.Count);
                tinctures.Add(colours[dex]);
                colours.RemoveAt(dex);

                if(ruleOfTincture) {
                    dex = Random.Range(0, metals.Count);
                    tinctures.Add(metals[dex]);
                    metals.RemoveAt(dex);
                }
                else {
                    dex = Random.Range(0, colours.Count);
                    tinctures.Add(colours[dex]);
                    colours.RemoveAt(dex);
                }

                if(division == Division.Pall) {
                    dex = Random.Range(0, colours.Count);
                    tinctures.Add(colours[dex]);
                    colours.RemoveAt(dex);
                    field = new Field(division, tinctures.ToArray(), false);
                }
                else {
                    field = new Field(division, tinctures.ToArray(), Random.value > .5);
                }
            }
        }

        List<Element> elements = new List<Element>();
        HashSet<Color> usedByElements = new HashSet<Color>();

        List<Color> Candidates(IEnumerable<Color> source, bool skipFieldColors, bool skipElementColors) {
            return source.Where(c => !(skipFieldColors && System.Array.Exists(field.tinctures, t => t == c))
                                  && !(skipElementColors && usedByElements.Contains(c))).ToList();
        }

        // Picks a tincture for an ordinary or charge. metalChance only matters under the rule of tincture.
        Color PickTincture(float metalChance) {
            IEnumerable<Color> metalSource = ruleOfTincture ? Tincture.Metals.Values : new Color[0];
            IEnumerable<Color> colourSource = ruleOfTincture ? Tincture.Colours.Values : Tincture.Tinctures.Values;

            // strictest first; relax the element rule, then the field rule, if the pool runs dry
            for (int relax = 0; relax < 3; relax++) {
                bool skipFieldColors = avoidFieldColorReuse && relax < 2;
                bool skipElementColors = avoidElementColorReuse && relax < 1;
                List<Color> metalPool = Candidates(metalSource, skipFieldColors, skipElementColors);
                List<Color> colourPool = Candidates(colourSource, skipFieldColors, skipElementColors);
                if (metalPool.Count + colourPool.Count == 0) {
                    continue;
                }

                if ((avoidFieldColorReuse && relax >= 2) || (avoidElementColorReuse && relax >= 1)) {
                    Debug.LogWarning("RandomBlazon: ran out of tinctures that satisfy the color reuse rules, relaxing them.");
                }

                bool useMetal = metalPool.Count > 0 && (colourPool.Count == 0 || Random.value < metalChance);
                List<Color> pool = useMetal ? metalPool : colourPool;
                Color picked = pool[Random.Range(0, pool.Count)];
                usedByElements.Add(picked);
                return picked;
            }

            // nothing in the palette at all, can't happen with the current tinctures
            return Tincture.Tinctures.Values.First();
        }

        if (ordinary) {
            var ordinaryType = OrdinaryShapes.Values.ElementAt(Random.Range(0, OrdinaryShapes.Values.Count));
            Color ordinaryTincture = PickTincture(.5f);

            if (ordinaryType == OrdinaryShape.Bend) {
                elements.Add(new Ordinary(ordinaryType, ordinaryTincture, Random.value > .5f));
            }
            else {
                elements.Add(new Ordinary(ordinaryType, ordinaryTincture, false));
            }
        }

        for(int i = 0; i < charges; i++) {
            Color tincture = PickTincture(.2f);

            var fate = Random.Range(0f, 1f);
            var shape = ChargeShapes.Values.ElementAt(Random.Range(0, ChargeShapes.Values.Count));
            if(fate < .4f) {
                elements.Add(new Charge(shape, tincture));
            }
            else if( fate < .7f) {
                var point = Points.Values.ElementAt(Random.Range(0, Points.Values.Count));
                elements.Add(new Charge(shape, tincture, point));
            }
            else {
                var count = Random.Range(2, 9);
                elements.Add(new Charge(shape, tincture, count));
            }
        }

        return new Blazon(field, elements);

    }
}

public class BlazonError : System.Exception {
    public BlazonError(string message) : base(message) { }
}

public abstract class Element {
    public Color tincture;

    public abstract string ToBlazonDescription();
}
