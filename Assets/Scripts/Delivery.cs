using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Delivery : MonoBehaviour
{
    public Texture2D currentBlazon;
    Paintbrush paintbrush;

	public LayerMask layerMask;
    public BlazonGenerator targetBlazonGenerator;

	EvaluateGui evgui;

    // Start is called before the first frame update
    void Start()
    {
        paintbrush = FindObjectOfType<Paintbrush>();
        currentBlazon = new Texture2D(256,256,TextureFormat.RGBA32,false);
		evgui = FindObjectOfType<EvaluateGui>();

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
		float score = TestAccuracy();

		evgui.Evaluate(currentBlazon, targetBlazonGenerator.blazonTexture, score.ToString());
	}

	[ContextMenu("copy blazon")]
    public void CopyBlazon()
	{
		///GPU only, we can't compare pixels with this on the cpu
		////Graphics.CopyTexture(paintbrush.renderTex, currentBlazon);

		///Very Slow!
		//Rect regionToReadFrom = new Rect(0, 0, paintbrush.renderTex.width, paintbrush.renderTex.height);
		//currentBlazon.ReadPixels(regionToReadFrom, 0, 0);
		//currentBlazon.Apply();

		////Synchronously
		//var asyncAction = AsyncGPUReadback.Request(paintbrush.renderTex, 0);
		//asyncAction.WaitForCompletion();
		//currentBlazon.SetPixelData(asyncAction.GetData<byte>(), 0);
		//currentBlazon.Apply();

		//Asynchronously
		AsyncGPUReadback.Request(paintbrush.renderTex, 0, (AsyncGPUReadbackRequest asyncAction) =>
		{
			currentBlazon.SetPixelData(asyncAction.GetData<byte>(), 0);
			currentBlazon.Apply();
		});
	}

    [ContextMenu("Test Accuracy")]
    public float TestAccuracy()
    {
        
		float score = currentBlazon.MatchPercentage(targetBlazonGenerator.blazonTexture);

		Debug.Log(score);

        return score;
	}

	[ContextMenu("Reset Blazon")]
	public void ResetBlazon()
	{
        paintbrush.ResetRenderTexture();
	}



}
