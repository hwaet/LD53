using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery : MonoBehaviour
{
    public Texture2D currentBlazon;
    public Paintbrush paintbrush;

    // Start is called before the first frame update
    void Start()
    {
        paintbrush = FindObjectOfType<Paintbrush>();
        currentBlazon = new Texture2D(256,256,TextureFormat.ARGB32,false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("copy blazon")]
    public void CopyBlazon()
    {
        Graphics.CopyTexture(paintbrush.renderTex, currentBlazon);
        float score = currentBlazon.MatchPercentage(currentBlazon);
        Debug.Log(score);
	}
}
