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

	// What is painted as a logical picture: the tincture at every pixel, which is what a painting is scored by. The brush sets it as it paints,
	// knowing which tincture it is painting with, so the pixels the canvas shows (soft edges and all) never have to be matched back to a tincture.
	[Header("Scoring")]
	public BlazonMap map;
	public byte brushTincture;
	Color32[] brushPixels;
	int brushWidth, brushHeight;

	Texture2D whiteMap;
	public RenderTexture renderTex;
	public RawImage rawImage;
	public CanvasGroup canvasGroup;

	[Header("debug")]
	public Vector2 screenPos;
	public Vector2 localCoordinate = new Vector2();

	void Start()
	{
		brushPixels = TextureReadback.Read(brushTexture);
		brushWidth = brushTexture.width;
		brushHeight = brushTexture.height;
		brushTincture = Tincture.IdOf(brushColor);
		ResetRenderTexture();// clear white texture to draw on
	}

	private void OnValidate()
	{
		rawImage = RenderTexturePanel.GetComponent<RawImage>();
		renderTex = (RenderTexture) rawImage.texture;
		canvasGroup = GetComponent<CanvasGroup>();
	}

	void Update()
	{
		if (canvasGroup.interactable && Input.GetMouseButton(0))
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
		Rect sourceRect = new Rect(0, 0, 1, 1);
		Graphics.DrawTexture(newDrawing, brushTexture, sourceRect, 0, 0, 0, 0, brushColor.linear * 0.5f);

		GL.PopMatrix();
		RenderTexture.active = null;// turn off rendertexture

		StampMap(newDrawing);
	}

	[ContextMenu("Get White Render Texture")]
	RenderTexture getWhiteRT()
	{
		RenderTexture rt = new RenderTexture(resolution, resolution, 32);
		Graphics.Blit(whiteMap, rt);
		return rt;
	}

	[ContextMenu("Clear")]
	public void ResetRenderTexture()
	{
		//whiteMap = new Texture2D(1, 1);
		//whiteMap.SetPixel(0, 0, Color.white);
		//whiteMap.Apply();
		renderTex.Release();

		// the blank canvas is white, which is argent
		map = new BlazonMap(resolution);
		map.Fill(Tincture.IdOf(Color.white), 1f);
	}

	// Records on the map what was just drawn, in the tincture being painted with.
	void StampMap(Rect stamp)
	{
		map.Stamp(stamp, brushPixels, brushWidth, brushHeight, brushTincture);
	}

	// Checks the map against what is on the canvas, to see that the two line up (and aren't upside down): they should agree except along the
	// soft edges of strokes.
	[ContextMenu("Compare map to canvas")]
	void CompareMapToCanvas()
	{
		RenderTexture previous = RenderTexture.active;
		RenderTexture.active = renderTex;
		Texture2D canvas = new Texture2D(renderTex.width, renderTex.height, TextureFormat.RGBA32, false);
		canvas.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
		RenderTexture.active = previous;
		Color32[] pixels = canvas.GetPixels32();
		DestroyImmediate(canvas);

		int size = map.size;
		int same = 0, flipped = 0;
		for (int i = 0; i < pixels.Length; i++)
		{
			byte shown = Tincture.IdOf(pixels[i]);
			if (shown == map.tinctures[i]) same++;
			if (shown == map.tinctures[i % size + (size - 1 - i / size) * size]) flipped++;
		}
		Debug.LogFormat("The map matches the canvas on {0:P1} of pixels, and {1:P1} if it were upside down.", (float)same / pixels.Length, (float)flipped / pixels.Length);
	}

	public void setColor (Color newColor)
	{
		brushColor = newColor;
		brushTincture = Tincture.IdOf(newColor);
	}

	// Paints with a tincture, which is how the palette sets the brush: it knows which tincture each of its buttons is.
	public void SetTincture(byte tinctureId)
	{
		brushTincture = tinctureId;
		brushColor = Tincture.FromId(tinctureId);
	}

	public void SetBrushSize(float size)
	{
		brushSize = size;
	}
}