using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Unity.VisualScripting;

public class Paintbrush : MonoBehaviour
{
	public RectTransform RenderTexturePanel;

	[Header("Brush Config")]
	public int resolution = 256;
	public float brushSize;
	public Texture2D brushTexture;

	public Color brushColor;

	Texture2D whiteMap;
	public RenderTexture renderTex;
	public RawImage rawImage;

	[Header("debug")]
	public Vector2 screenPos;
	public Vector2 localCoordinate = new Vector2();

	void Start()
	{
		CreateClearTexture();// clear white texture to draw on
	}

	private void OnValidate()
	{
		rawImage = RenderTexturePanel.GetComponent<RawImage>();
		renderTex = (RenderTexture) rawImage.texture;

		//Color[] texturePixels = brushTexture.GetPixels();
		//for (int i = 0; i < texturePixels.Length; i++)
		//{
		//	texturePixels[i] = texturePixels[i] * brushColor;
		//}
		//brushTexture.SetPixels(texturePixels);
		//brushTexture.Apply();
	}

	void Update()
	{
		if (Input.GetMouseButton(0))
		{
			OnDraw();
		}

	}

	private void OnDraw()
	{
		Resolution res = Screen.currentResolution;

		float x = Mathf.Clamp(Input.mousePosition.x, 0, Screen.width);
		float y = Mathf.Clamp(Input.mousePosition.y, 0, Screen.height);
		screenPos = new Vector2(x, y);

		RectTransformUtility.ScreenPointToLocalPointInRectangle(RenderTexturePanel, screenPos, null, out localCoordinate);

		DrawTexture(renderTex, localCoordinate.x, localCoordinate.y);
	}

	void DrawTexture(RenderTexture rt, float posX, float posY)
	{
		Vector2 size = RenderTexturePanel.rect.size;

		posX = math.remap(-size.x / 2, size.x / 2, 0, rt.width, posX) - brushTexture.width * brushSize / 2;
		posY = math.remap(size.y / 2, -size.y / 2, 0, rt.height, posY) - brushTexture.height * brushSize / 2;

		RenderTexture.active = rt;				// activate rendertexture for drawtexture;
		GL.PushMatrix();						// save matrixes
		GL.LoadPixelMatrix(0, resolution, resolution, 0);      // setup matrix for correct size

		

		// draw brushtexture
		Rect newDrawing = new Rect(posX, posY, brushTexture.width*brushSize, brushTexture.height * brushSize);
		Rect sourceRect = new Rect(0, 0, brushTexture.width, brushTexture.height);
		Graphics.DrawTexture(newDrawing, brushTexture, sourceRect, 0, 0, 0, 0, brushColor);

		GL.PopMatrix();
		RenderTexture.active = null;// turn off rendertexture


	}

	[ContextMenu("Get White Render Texture")]
	RenderTexture getWhiteRT()
	{
		RenderTexture rt = new RenderTexture(resolution, resolution, 32);
		Graphics.Blit(whiteMap, rt);
		return rt;
	}

	[ContextMenu("Clear")]
	void CreateClearTexture()
	{
		//whiteMap = new Texture2D(1, 1);
		//whiteMap.SetPixel(0, 0, Color.white);
		//whiteMap.Apply();
		renderTex.Release();
	}

	public void setColor (Color newColor)
	{
		brushColor = newColor; 
	}
}