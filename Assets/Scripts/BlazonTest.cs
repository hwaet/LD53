using System.Collections.Generic;
using System;
using UnityEngine;
//using UnityEditor.AssetImporters;
using static BlazonDrawers;

public partial class BlazonTest : MonoBehaviour {

    //if the first word is a color, then fill the field with that color
    //ex: Azue, a bend or

    private string blazonPrompt = "Azure, a bend or";
    string parseError = string.Empty;
    private List<string> warnings = new List<string>();
    private GUIStyle warningStyle;

    private Texture2D testTexture = null;
    public Texture2D shieldTemplate;
    private Blazon testBlazon;

    public Blazon systematicBlazon;

    private bool complexFields, ruleOfTincture, ordinary;
    private bool avoidFieldColorReuse = true, avoidElementColorReuse = true;
    private int numberOfElements = 1;

    private string[] shieldNames;
    private int shieldIndex;
    private bool clipToShield;


    [Header("Systematic Test Field")]
    public Field testField;

    [Header("Systematic Test Ordinary")]
    public bool includeOrdinary;
    public Ordinary testOrdinary;

    [Header("Systematic Test Charge")]
    public Charge[] charges;

    private Vector2 scrollPos = Vector2.zero;

    // Start is called before the first frame update
    void Start() {
        testTexture = GenerationExperiments();
        testField.tinctures = new Color[] { Tincture.Argent, Tincture.Sable, Tincture.Or};
        testOrdinary.tincture = Tincture.Gules;
    }

    // Update is called once per frame
    void Update() {

    }





    private Shield CurrentShield {
        get {
            if (shieldNames == null) {
                shieldNames = Shield.AllNames();
            }
            if (shieldNames.Length == 0) {
                return Shield.Default;
            }
            return Shield.Get(shieldNames[Mathf.Clamp(shieldIndex, 0, shieldNames.Length - 1)]);
        }
    }

    // Renders a blazon on the selected shield, clipped to its outline if asked.
    private Texture2D RenderOnShield(Blazon blazon) {
        Shield shield = CurrentShield;
        Texture2D texture = blazon.GenerateTexture(256, shield);
        if (clipToShield) {
            shield.ClipTexture(texture);
        }
        return texture;
    }

    // A roundel at every named point, to check where a shield puts them.
    private static Blazon ChargePointsBlazon() {
        Color[] tinctures = { Tincture.Gules, Tincture.Azure, Tincture.Vert, Tincture.Sable, Tincture.Purpure };
        List<Element> elements = new List<Element>();
        int i = 0;
        foreach (Blazon.Point point in Enum.GetValues(typeof(Blazon.Point))) {
            elements.Add(new Charge(Charge.ChargeShape.Roundel, tinctures[i % tinctures.Length], point));
            i++;
        }
        return new Blazon(new Field(Tincture.Argent), elements);
    }

    private static GUIStyle MakeWarningStyle() {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.normal.textColor = new Color(1f, 0.85f, 0.3f);
        style.wordWrap = true;
        return style;
    }

    private void OnGUI() {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Blazon:");
        blazonPrompt = GUILayout.TextField(blazonPrompt);

        parseError = "Blazon Valid!";
        warnings.Clear();
        try {
            testBlazon = Blazon.Parse(blazonPrompt);
            warnings = testBlazon.Validate();
        }
        catch (Exception e) {
            parseError = e.Message;
        }

        GUI.enabled = parseError == "Blazon Valid!";

        if (GUILayout.Button("Render")) {
            testTexture = RenderOnShield(testBlazon);
            Debug.Log("Creating:" + testBlazon.ToBlazonDescription());

            //Color[,] mask = shieldTemplate.GetColorArray();
            //testTexture = CutToStamp(testTexture, mask);

        }

        GUI.enabled = true;

        GUILayout.EndHorizontal();

       
        GUILayout.BeginHorizontal();

        GUILayout.Box(testTexture);

        GUILayout.BeginVertical();
        GUILayout.Label("Blazon Randomizer:");
        complexFields = GUILayout.Toggle(complexFields, "Complex Fields");
        ruleOfTincture = GUILayout.Toggle(ruleOfTincture, "Rule of Tincture");
        ordinary = GUILayout.Toggle(ordinary, "Add Ordinary");
        avoidFieldColorReuse = GUILayout.Toggle(avoidFieldColorReuse, "Avoid Field Color Reuse");
        avoidElementColorReuse = GUILayout.Toggle(avoidElementColorReuse, "Avoid Element Color Reuse");
        if (shieldNames == null) {
            shieldNames = Shield.AllNames();
        }
        if (shieldNames.Length > 1) {
            GUILayout.Label("Shield: " + shieldNames[Mathf.Clamp(shieldIndex, 0, shieldNames.Length - 1)]);
            shieldIndex = (int)GUILayout.HorizontalSlider(shieldIndex, 0, shieldNames.Length - 1);
        }
        clipToShield = GUILayout.Toggle(clipToShield, "Clip to Shield");
        if (GUILayout.Button("Show Charge Points")) {
            testBlazon = ChargePointsBlazon();
            blazonPrompt = testBlazon.ToBlazonDescription();
            testTexture = RenderOnShield(testBlazon);
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("Number of Charges:");
        GUI.enabled = false;
        GUILayout.TextField(numberOfElements.ToString());
        GUI.enabled = true;
        numberOfElements = (int)GUILayout.HorizontalSlider(numberOfElements, 0, 3);
        GUILayout.EndHorizontal();
        
        if(GUILayout.Button("Generate Random Blazon")) {
            testBlazon = Blazon.RandomBlazon(complexFields, ruleOfTincture, ordinary, numberOfElements, avoidFieldColorReuse, avoidElementColorReuse);
            blazonPrompt = testBlazon.ToBlazonDescription();
            testTexture = RenderOnShield(testBlazon);
        }


        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.Label("Systematic Test:");
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Field type:");
        GUI.enabled = false;
        GUILayout.TextField(testField.type.ToString());
        GUI.enabled = true;
        testField.type = (Field.FieldTypes)GUILayout.HorizontalSlider((int)testField.type, 0, 3);
        GUILayout.EndHorizontal();
        switch (testField.type) {
            case Field.FieldTypes.Simple:
                break;
            case Field.FieldTypes.Divided:
                GUILayout.BeginHorizontal();
                GUILayout.Label("Division Type:");
                GUI.enabled = false;
                GUILayout.TextField(testField.divison.ToString());
                GUI.enabled = true;
                testField.divison = (Field.Division)GUILayout.HorizontalSlider((int)testField.divison, 0, 6);
                GUILayout.EndHorizontal();

                if(testField.divison == Field.Division.Bend) {
                    testField.sinister = GUILayout.Toggle(testField.sinister, "Sinister");
                }

                break;
            case Field.FieldTypes.Varied:
                GUILayout.BeginHorizontal();
                GUILayout.Label("Variation Type:");
                GUI.enabled = false;
                GUILayout.TextField(testField.variation.ToString());
                GUI.enabled = true;
                testField.variation = (Field.Variation)GUILayout.HorizontalSlider((int)testField.variation, 0, 7);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Variation Count:");
                string check = testField.count.ToString();
                string bak = check;
                check = GUILayout.TextField(check);
                if(check != bak) {
                    if(int.TryParse(check, out int result)) {
                        testField.count = result;
                    }
                }
                GUILayout.EndHorizontal();

                if (testField.variation == Field.Variation.Bendy) {
                    testField.sinister = GUILayout.Toggle(testField.sinister, "Sinister");
                }
                break;
        }
        GUILayout.Space(10);
        includeOrdinary = GUILayout.Toggle(includeOrdinary, "Add Ordinary");
        if(includeOrdinary) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Ordinary Shape:");
            GUI.enabled = false;
            GUILayout.TextField(testOrdinary.shape.ToString());
            GUI.enabled = true;
            testOrdinary.shape = (Ordinary.OrdinaryShape)GUILayout.HorizontalSlider((int)testOrdinary.shape, 0, 7);
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        GUILayout.Label("Compiled:");
        List<Element> elements = new List<Element>();
        if(includeOrdinary) {
            elements.Add(testOrdinary);
        }
        elements.AddRange(charges);
        systematicBlazon = new Blazon(testField, elements);
        GUI.enabled = false;
        GUILayout.TextField(systematicBlazon.ToBlazonDescription());
        GUI.enabled = true;

        if(GUILayout.Button("Render Systematic Blazon")) {
            testTexture = RenderOnShield(systematicBlazon);
            blazonPrompt = systematicBlazon.ToBlazonDescription();
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.Label(parseError);
        foreach (string warning in warnings) {
            GUILayout.Label("Warning: " + warning, warningStyle ?? (warningStyle = MakeWarningStyle()));
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }



    private Texture2D GenerationExperiments() {
        Texture2D tex = new Texture2D(256, 256);
        tex = Fill(tex, Tincture.Argent);
        //tex = DrawSinWave(tex, new RectInt(0,128,256,256), Tincture.Or, 25, 10, 0);
        tex = DrawRect(tex, new RectInt(0, 128-12, 256, 24), Tincture.Gules);
        tex = RotateTexture(tex , 45);

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

        //tex = DrawRect(tex, new RectInt(64, 64, 128, 128), Tincture.Gules);

        tex.Apply();
        return tex;
    }


  

    



    

    

}
