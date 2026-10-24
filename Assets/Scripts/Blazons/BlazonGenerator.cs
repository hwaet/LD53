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
		blazonTexture = newBlazon.GenerateTexture(256, shield);

		bannerRenderer.sharedMaterial.SetTexture("_Blazon_Texture", blazonTexture);
		if (!string.IsNullOrEmpty(shieldName) && shield.source != null) {
			// the banner clips the blazon to the shield with the same PNG
			bannerRenderer.sharedMaterial.SetTexture("_Blazon_Mask", shield.source);
		}

	}

}
