using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PromptGUI : MonoBehaviour
{

    public Text messageText;

    GuiManager guiManager;

    private void OnValidate() {
        guiManager = GetComponent<GuiManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        OnValidate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
