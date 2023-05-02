using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvaluateGui : MonoBehaviour
{
    public RawImage srcImg;
    public RawImage targetImg;

    public Text textDisplay;

    GuiManager guiManager;

    // Start is called before the first frame update
    void OnValidate()
    {
        guiManager = GetComponent<GuiManager>();
	}

    // Update is called once per frame
    void Start()
    {
        OnValidate();
    }

    public void Evaluate(Texture2D src, Texture2D target, string inText)
    {
        guiManager.OpenGUI();


		srcImg.texture = src;
        targetImg.texture = target;

        textDisplay.text = inText;

	}
}
