using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaletteButton : MonoBehaviour
{
    public Paintbrush paintbrush;
    Image image;

    // Start is called before the first frame update
    void Start()
    {
        paintbrush = FindObjectOfType<Paintbrush>();
        image = GetComponent<Image>();
    }

    public void setCurrentColorToPaintbrush()
    {
        paintbrush.setColor(image.color);


	}
}
