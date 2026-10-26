using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlazonGenerator : MonoBehaviour
{
    BlazonTest bp;
    public string blazonDescription = "Azure, a bend Or";
    // Name of a PNG in Resources/Shields. Empty uses the default shield.
    public string shieldName = "";
    public Texture2D blazonTexture;
    // Which tincture is at every pixel of the blazon and how much each counts, what a painting is scored against.
    public BlazonMap blazonMap;

    public SkinnedMeshRenderer bannerRenderer;

	// Start is called before the first frame update
	void OnValidate()
    {
        bp = GetComponent<BlazonTest>();

	}

    private void Start()
    {
        Generate();
    }

    [ContextMenu("Generate")]
    // Update is called once per frame
    public void Generate()
    {
        Blazon newBlazon;
		newBlazon = Blazon.Parse(blazonDescription);
		Shield shield = string.IsNullOrEmpty(shieldName) ? Shield.Default : Shield.Get(shieldName);
		BlazonRender render = newBlazon.Render(256, shield);
		blazonTexture = render.texture;
		blazonMap = render.map;

		bannerRenderer.sharedMaterial.SetTexture("_Blazon_Texture", blazonTexture);
		if (!string.IsNullOrEmpty(shieldName) && shield.source != null) {
			// the banner clips the blazon to the shield with the same PNG
			bannerRenderer.sharedMaterial.SetTexture("_Blazon_Mask", shield.source);
		}

	}

}
