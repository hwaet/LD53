using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class GameManager : MonoBehaviour
{
    public List<LevelSpecification> levels;

    public int currentLevel = 0;

	public Text GUI_Intro;
	public Text GUI_Dialog;
    public Text GUI_Blazon;

	public Text GUI_BlazonOrder;

    public GuiManager gameDialogue;
    public GuiManager gameIntro;

    public CinemachineVirtualCamera titleCam;

    public BlazonGenerator bz1;
    public BlazonGenerator bz2;
    public BlazonGenerator bz3;
    public BlazonGenerator bz4;
    public BlazonGenerator bz5;
    

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreaseLevel()
    {
        currentLevel++;
    }

    public string GetCurrentBlazon()
    {
        return levels[currentLevel].targetBlazon;
    }

	public List<string> GetDistractors()
	{
		return levels[currentLevel].distractorBlazons;
	}

	public string GetFlavorText()
	{
		return levels[currentLevel].narrativeText;
	}

	[ContextMenu("Activate Dialogue")]
    public void ActivateDialogue()
    {
        gameDialogue.OpenGUI();

        GUI_Dialog.text = GetFlavorText();
        GUI_Blazon.text = GetCurrentBlazon();
		GUI_BlazonOrder.text = GetCurrentBlazon();

		bz1.gameObject.SetActive(true);
		bz2.gameObject.SetActive(true);
		bz3.gameObject.SetActive(true);
		bz4.gameObject.SetActive(true);
		bz5.gameObject.SetActive(true);
		

		switch ((GetDistractors().Count)){
            case 0:
                break;
            case 1:
				break;
            case 2:
				bz1.blazonDescription = GetCurrentBlazon();
				bz2.blazonDescription = GetDistractors()[0];
				bz3.blazonDescription = GetDistractors()[1];
				bz4.gameObject.SetActive(false);
				bz5.gameObject.SetActive(false);
				break;
			case 3:
				bz1.gameObject.SetActive(false);
				bz2.blazonDescription = GetDistractors()[0];
				bz3.blazonDescription = GetDistractors()[1];
				bz4.blazonDescription = GetCurrentBlazon();
				bz5.blazonDescription = GetDistractors()[2];
				break;
			case 4:
				bz1.blazonDescription = GetDistractors()[0];
				bz2.blazonDescription = GetDistractors()[1];
				bz3.blazonDescription = GetCurrentBlazon();
				bz4.blazonDescription = GetDistractors()[2];
				bz5.blazonDescription = GetDistractors()[3];
				break;
			default:
				break;
		}

		bz1.Generate();
		bz2.Generate();
		bz3.Generate();
		bz4.Generate();
		bz5.Generate();

	}

    public void ActivateIntro()
    {
        if (currentLevel > 2)
        {
			ActivateDialogue();
			gameIntro.CloseGUI();
			//titleCam.enabled = false;
			return;
		}
            
		gameIntro.OpenGUI();
		GUI_Intro.text = GetFlavorText();
	}

	[ContextMenu("Close Dialogue")]
	public void CloseDialogue()
    {
		GuiManager gm = GetComponentInChildren<GuiManager>();
		gm.CloseGUI();
	}
}
