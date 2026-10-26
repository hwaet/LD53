using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaletteButton : MonoBehaviour
{
    public Paintbrush paintbrush;
    // The tincture this button is for, set by the palette. If it isn't, it is the tincture whose color the button is.
    public byte tinctureId;
    Image image;

    // Start is called before the first frame update
    void Start()
    {
        paintbrush = FindObjectOfType<Paintbrush>();
        image = GetComponent<Image>();
    }

    public void setCurrentColorToPaintbrush()
    {
        paintbrush.SetTincture(tinctureId != Tincture.Empty ? tinctureId : Tincture.IdOf(image.color));


	}
}
