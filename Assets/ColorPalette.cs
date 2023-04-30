using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPalette : MonoBehaviour
{
    public RectTransform colorPaletteRect;
    public GameObject button;

    // Start is called before the first frame update
    void Start()
    {
        LoadColors();

	}

    [ContextMenu("populate")]
    void LoadColors ()
    {
        foreach (KeyValuePair<string,Color> item in Tincture.Tinctures)
        {
            Debug.Log(item.Key);
            GameObject newGO = GameObject.Instantiate(button, colorPaletteRect);
			newGO.GetComponent<Image>().color = item.Value;

        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
