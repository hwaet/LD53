using System.Collections;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting.Antlr3.Runtime;
using static BlazonTest;
using UnityEditor.AssetImporters;
using static BlazonDrawers;

public partial class BlazonTest : MonoBehaviour {

    //if the first word is a color, then fill the field with that color
    //ex: Azue, a bend or

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



    



    

    

}
