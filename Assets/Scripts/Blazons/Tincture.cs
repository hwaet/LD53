using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tincture {
    // https://en.wikipedia.org/wiki/Tincture_(heraldry)
    // Metal
    public static readonly Color Argent = new Color(1, 1, 1);
    public static readonly Color Or = new Color(1, .8f, 0);

    // Color
    public static readonly Color Gules = new Color(1, 0, 0);
    public static readonly Color Sable = new Color(0, 0, 0);
    public static readonly Color Azure = new Color(0, 0, 1);
    public static readonly Color Vert = new Color(0, .5f, 0);
    public static readonly Color Purpure = new Color(.5f, 0, .5f);

    // Stain
    public static readonly Color Tenne = new Color(.8f, .4f, 0);
    public static readonly Color Sanguine = new Color(178f / 255, 34f / 255, 34f / 255);
    public static readonly Color Murrey = new Color(139f / 255, 0, 75f / 255);

    // Other colors
    public static readonly Color Carnation = new Color(1, .6f, .6f);
    public static readonly Color Celeste = new Color(0, 1, 1);
    public static readonly Color Rose = new Color(1, .5f, .5f);
    public static readonly Color BleuCeleste = new Color(0, .5f, 1);
    public static readonly Color Brunatre = new Color(.5f, .2f, 0);
    public static readonly Color Orange = new Color(1, .5f, 0);

    public static readonly Dictionary<string, Color> Tinctures = new Dictionary<string, Color>() {
        { "argent",  Argent },
        { "or",  Or },
        { "gules",  Gules },
        { "sable",  Sable },
        { "azure",  Azure },
        { "vert",  Vert },
        { "purpure",  Purpure },
        { "tenne",  Tenne },
        { "sanguine",  Sanguine },
        { "murrey",  Murrey },
        {"carnation", Carnation },
        {"celeste", Celeste },
        {"rose", Rose },
        {"bleuceleste", BleuCeleste },
        {"brunatre", Brunatre },
        {"orange", Orange }
    };

    public static readonly Dictionary<string, Color> Metals = new Dictionary<string, Color>() {
        { "argent",  Argent },
        { "or",  Or }
    };

    public static readonly Dictionary<string, Color> Colours = new Dictionary<string, Color> {
        {"gules", Gules },
        {"sable", Sable },
        {"azure", Azure },
        {"vert", Vert },
        {"purpure", Purpure }
    };
    
    public static readonly Dictionary<string, Color> Stains = new Dictionary<string, Color> {
        {"tenne", Tenne },
        {"sanguine", Sanguine },
        {"murrey", Murrey }
    };

    public static readonly Dictionary<string, Color> OtherColors = new Dictionary<string, Color> {
        { "carnation", Carnation },
        {"celeste", Celeste },
        {"rose", Rose },
        {"bleuceleste", BleuCeleste },
        {"brunatre", Brunatre },
        {"orange", Orange }
    };
}
