using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Delivery : MonoBehaviour
{
    public Texture2D currentBlazon;
    // Which tincture is at every pixel of the painting, what is scored against the blazon's map.
    public BlazonMap currentMap;
    Paintbrush paintbrush;

	public LayerMask layerMask;
    public BlazonGenerator targetBlazonGenerator;

	EvaluateGui evgui;
	GameManager gm;

	public FeedbackMessages feedbackMessages;

    // Start is called before the first frame update
    void Start()
    {
        paintbrush = FindObjectOfType<Paintbrush>();
        currentBlazon = new Texture2D(256,256,TextureFormat.RGBA32,false);
		evgui = FindObjectOfType<EvaluateGui>();
		gm = FindObjectOfType<GameManager>();
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
		{
			DeliverBlazon();
		}
    }

	private void DeliverBlazon()
	{
		
		Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		Debug.DrawRay(camRay.origin, camRay.direction*1000,Color.white,2f);

		RaycastHit hitInfo;
		bool hit = Physics.Raycast(camRay, out hitInfo, 9999f, layerMask);
		if (hit==false) return;

		targetBlazonGenerator = hitInfo.collider.gameObject.GetComponent<BlazonGenerator>();

		if (targetBlazonGenerator == null) return;
		float score = TestAccuracy(targetBlazonGenerator);

		evgui.Evaluate(currentBlazon, targetBlazonGenerator.blazonTexture, score.ToString());
		gm.CheckScore(targetBlazonGenerator, score);
	}

	[ContextMenu("copy blazon")]
    public void CopyBlazon()
	{
		// the brush keeps the tincture at every pixel as it paints, so there is nothing to read back from the canvas and snap to a tincture
		currentMap = paintbrush.map;

		// the picture shown is those tinctures, with a fur's color becoming its pattern as it does in the blazon
		currentBlazon.SetPixels32(currentMap.ToPixels());
		Furs.Resolve(currentBlazon);

		currentBlazon.Apply();

		////Synchronously
		//var asyncAction = AsyncGPUReadback.Request(paintbrush.renderTex, 0);
		//asyncAction.WaitForCompletion();
		//currentBlazon.SetPixelData(asyncAction.GetData<byte>(), 0);
		//currentBlazon.Apply();

		//Asynchronously
		//AsyncGPUReadback.Request(paintbrush.renderTex, 0, (AsyncGPUReadbackRequest asyncAction) =>
		//{
		//	currentBlazon.SetPixelData(asyncAction.GetData<byte>(), 0);
		//	currentBlazon.Apply();
		//});
	}

    [ContextMenu("Test Accuracy")]
    public float TestAccuracy(BlazonGenerator targetBlazonGenerator)
    {
		

		if (currentMap == null) {
			CopyBlazon();
		}
		float score = BlazonMap.Score(targetBlazonGenerator.blazonMap, currentMap);

		Debug.Log(score);

        return score;
	}

	[ContextMenu("Reset Blazon")]
	public void ResetBlazon()
	{
        paintbrush.ResetRenderTexture();
	}



}
