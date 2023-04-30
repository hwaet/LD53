using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GuiManager : MonoBehaviour
{
    CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void OnValidate()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        OnValidate();
    }

    [ContextMenu("Close")]
    // Update is called once per frame
    public void CloseGUI()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

	}

    [ContextMenu("Open")]
	public void OpenGUI()
	{
		canvasGroup.alpha = 1;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;

	}
}
