﻿using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using static Field;
using static BlazonDrawers;
using static BlazonParserHelpers;
using static Charge;
using static Ordinary;


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


    Field field;
    List<Element> elements;

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
        Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
		switch (field.type) {
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
            
        if (elementProperties[0] == "a" || elementProperties[0] == "an") {
            if (ChargeShapes.ContainsKey(elementProperties[1])) {
                //we have a single charge 
                Charge charge = new Charge(ChargeShapes[elementProperties[1]], Tincture.Tinctures[elementProperties[2]]);
                elements.Add(charge);
            }
            else if (OrdinaryShapes.ContainsKey(elementProperties[1])) {
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
                blazon = new Blazon(Variations[field[0]], tinctures.ToArray(), sinister, Numbers[count]);
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
                ret = Tincture.Tinctures.FirstOrDefault(x => x.Value == field.tinctures[0]).Key +", ";
                break;
            case FieldTypes.Varied:
                ret = field.variation.ToString();
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

public class BlazonError : System.Exception {
    public BlazonError(string message) : base(message) { }
}

public abstract class Element {
    public Color tincture;

    public abstract string ToBlazonDescription();
}
