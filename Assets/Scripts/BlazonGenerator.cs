using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlazonGenerator : MonoBehaviour
{
    BlazonParser bp;
    public string blazonDescription = "Azure, a bend Or";
    public Texture2D blazonTexture;

    public SkinnedMeshRenderer bannerRenderer;

	// Start is called before the first frame update
	void OnValidate()
    {
        bp = GetComponent<BlazonParser>();

	}

    private void Start()
    {
        Generate();
    }

    [ContextMenu("Generate")]
    // Update is called once per frame
    void Generate()
    {
        BlazonParser.Blazon newBlazon;
		newBlazon = BlazonParser.Blazon.Parse(blazonDescription);
		blazonTexture = newBlazon.GenerateTexture(256);

		bannerRenderer.sharedMaterial.SetTexture("_Blazon_Texture", blazonTexture);

	}

}
