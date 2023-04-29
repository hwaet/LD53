using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System.Runtime.CompilerServices;

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
        Bendy, // diagonal bars (top-left to bottom-right)
        Chevronny, // chevrons pointing up

        Chequy, // checkerboard
        Lozengy, // diamond pattern (using squares)
        Fusilly, // diamond pattern (using steeper diamonds)

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
        Bend, // diagonal line (top-left to bottom-right)
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

    public enum Ordinary {
        Cross, // cross
        Pale, // vertical line
        Fess, // horizontal line
        Bend, // diagonal line (top-left to bottom-right)
        BendSinister, // diagonal line (top-right to bottom-left)
        Chevron, // chevron pointing up
        Saltire, // diagonal cross
        Cheif, // horizontal line at top
        Bordure, // border
        Pile, // triangle pointing down
        Pall, // Y shape
    }

    public class BlazonError: System.Exception {
        public BlazonError(string message) : base(message) { }
    }

    public class Blazon {
        Field field;
        List<Ordinary> ordinaries;
        //Charge[] charges; 

        public Blazon(Field field, List<Ordinary> ordinaries) {
            this.field = field;
            this.ordinaries = ordinaries;
        }
        
        public Blazon(Color tinc) {
            field = new Field(tinc);
            ordinaries  = new List<Ordinary>();
        }

        public Blazon(Variation variation, Color[] tinctures, bool sinister=false) {
            field = new Field(variation, tinctures, sinister);
            ordinaries = new List<Ordinary>();
        }

        public Blazon(Division divsion, Color[] tinctures, bool sinister= false) {
            field = new Field(divsion, tinctures, sinister);
            ordinaries = new List<Ordinary>();
        }
       
        public Texture2D GenerateTexture(int size) {
            Texture2D tex = new Texture2D(size, size);
            switch(field.type) {
                case FieldTypes.Simple:
                    tex = Fill(tex, field.tinctures[0]);
                    break;
                case FieldTypes.Varied:
                    tex = ApplyVariation(tex, field.variation, field.tinctures, 4, field.sinister);
                    break;
                case FieldTypes.Divided:
                    tex = ApplyDivision(tex, field.divison, field.tinctures, field.sinister);
                    break;
            }

            tex.Apply();
            return tex;
        }

        public void AddOrdinary(string ordinary) {
            //No op for now.
        }

        public static Blazon Parse(string blazonDescription) {
            if (!blazonDescription.Contains(',')) {
                throw new BlazonError("Blazon elements must be seperated by commas with the field comming first.");
            }
            string[] blazonElements = blazonDescription.ToLower().Split(',');
            string[] field = blazonElements[0].Split(' ');
            bool sinister = false;
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
                }
                if(tinctures.Count < 2) {
                    throw new BlazonError("A Variation must be composed of at least 2 tinctures.");
                }

                blazon = new Blazon(Variations[field[0]], tinctures.ToArray(), sinister);
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

            // Once we've done the field parse the rest of the blazon elements as ordinaries... TODO
            for(int i = 1; i <blazonElements.Length; i++) {
                blazon.AddOrdinary(blazonElements[i]);
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
            return ret;
        }
    }

    public class Field {
        public FieldTypes type;
        public Variation variation;
        public Division divison;
        public Color[] tinctures;
        public bool sinister;

        public Field(Color tincture) {
            type = FieldTypes.Simple;
            tinctures = new Color[] { tincture };
        }

        public Field(Variation variation, Color[] tinctures, bool sinister) {
            type = FieldTypes.Varied;
            this.variation = variation;
            this.tinctures = tinctures;
            this.sinister = sinister;
        }

        public Field(Division divsion, Color[] tinctures, bool sinister) {
            type = FieldTypes.Divided;
            this.divison = divsion;
            this.tinctures = tinctures;
            this.sinister = sinister;
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
                field = DrawLineSegment(field, new Vector2Int(0, 0), new Vector2Int(field.width, field.height), tinctures[1]);
                field = DrawLineSegment(field, new Vector2Int(field.width, 0), new Vector2Int(0, field.height), tinctures[1]);
                field = FloodFill(field, new Vector2Int(0, field.height/2), tinctures[1], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width, field.height / 2), tinctures[1], FloodFillMode.UntilTargetColor);
                return field;
            case Division.Pall: 
                field = DrawLineSegment(field, new Vector2Int(0, field.height), new Vector2Int(field.width/2, field.height/3*2), tinctures[0]);
                field = DrawLineSegment(field, new Vector2Int(field.width / 2, field.height / 3 * 2), new Vector2Int(field.width, field.height), tinctures[0]);
                field = DrawLineSegment(field, new Vector2Int(field.width / 2, field.height / 3 * 2), new Vector2Int(field.width/2,0), tinctures[1]);

                field = FloodFill(field, new Vector2Int(field.width / 2, field.height), tinctures[0], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(0, 0), tinctures[1], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width, 0), tinctures[2], FloodFillMode.AllSeedColor);
                return field;
            default: 
                break;
        }
        return field;

    }

    public static Texture2D ApplyVariation(Texture2D field, Variation variation, Color[] tinctures, int patternCount = 4, bool sinister = false) {
        Vector2Int p1, p2;
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

            

                //field = Fill(field, tinctures[0]);
                //for (int i = patternStep; i < field.height; i += patternStep) {
                //    for (int j = i % 2 == 0 ? patternStep : 0; j < field.width; j += patternStep) {
                //        field = DrawRect(field, new RectInt(j, i, patternStep, patternStep), tinctures[1]);
                //    }
                //}
                return field;
            default:
                Debug.LogFormat("Haven't implemented {0} yet", variation);
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

    public static Texture2D DrawRect(Texture2D field, RectInt rect, Color color) {
        for (int x = rect.x; x < rect.x + rect.width; x++) {
            for (int y = rect.y; y < rect.y + rect.height; y++) {
                field.SetPixel(x, y, color);
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
        Color seedColor = field.GetPixel(seed);
        Stack<Vector2Int> pixelStack = new Stack<Vector2Int>();
        pixelStack.Push(seed);
        while (pixelStack.Count > 0) {
            Vector2Int node = pixelStack.Pop();
            field.SetPixel(node, color);

            switch (fillMode) {
                case FloodFillMode.AllSeedColor:
                    if (node.x > 0 && field.GetPixel(node + Vector2Int.left) == seedColor) {
                        pixelStack.Push(node + Vector2Int.left);
                    }
                    if (node.x < field.width - 1 && field.GetPixel(node + Vector2Int.right) == seedColor) {
                        pixelStack.Push(node + Vector2Int.right);
                    }
                    if (node.y > 0 && field.GetPixel(node + Vector2Int.down) == seedColor) {
                        pixelStack.Push(node + Vector2Int.down);
                    }
                    if (node.y < field.height - 1 && field.GetPixel(node + Vector2Int.up) == seedColor) {
                        pixelStack.Push(node + Vector2Int.up);
                    }
                    break;
                case FloodFillMode.UntilTargetColor:
                    if (node.x > 0 && field.GetPixel(node + Vector2Int.left) != color) {
                        pixelStack.Push(node + Vector2Int.left);
                    }
                    if (node.x < field.width - 1 && field.GetPixel(node + Vector2Int.right) != color) {
                        pixelStack.Push(node + Vector2Int.right);
                    }
                    if (node.y > 0 && field.GetPixel(node + Vector2Int.down) != color) {
                        pixelStack.Push(node + Vector2Int.down);
                    }
                    if (node.y < field.height - 1 && field.GetPixel(node + Vector2Int.up) != color) {
                        pixelStack.Push(node + Vector2Int.up);
                    }
                    break;
                default:
                    goto case FloodFillMode.AllSeedColor;
            }
        }
        return field;

    }

}
