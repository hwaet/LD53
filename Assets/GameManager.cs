using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
    public CinemachineVirtualCamera workshopCam;

    public BlazonGenerator bz1;
    public BlazonGenerator bz2;
    public BlazonGenerator bz3;
    public BlazonGenerator bz4;
    public BlazonGenerator bz5;

	public Text responseText;
	public FeedbackMessages feedback;

	public Button SuccessButton;
	public Button FailButton;

	public float tierS = .9f;
	public float tierA = .8f;
	public float tierB = .7f;
	public float tierC = .6f;
	public float tierD = .5f;
	public float tierF = 0f;

	public Blazons blazons;

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
		if (currentLevel > levels.Count) currentLevel = 3;

	}

    public string GetCurrentBlazon()
    {
		if (levels[currentLevel].targetBlazon == "")
		{
			int id = Random.Range(0, blazons.distractorBlazons.Count);
			levels[currentLevel].targetBlazon = blazons.distractorBlazons[id];
		}
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
				List<string> distractors = new List<string>();
				while (distractors.Count<=5)
				{
					int id = Random.Range(0, blazons.distractorBlazons.Count);
					string tempBlazon = blazons.distractorBlazons[id];
					if (tempBlazon != GetCurrentBlazon()) distractors.Add(tempBlazon);
				}
				int correctID = Random.Range(0, 4);
				distractors[correctID] = GetCurrentBlazon();

				bz1.blazonDescription = distractors[0];
				bz2.blazonDescription = distractors[1];
				bz3.blazonDescription = distractors[2];
				bz4.blazonDescription = distractors[3];
				bz5.blazonDescription = distractors[4];
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

	public IEnumerator DelayDialog() {

		yield return new WaitForSeconds(2.5f);

		ActivateDialogue();
	}
	

    public void ActivateIntro()
    {
        if (currentLevel > 2)
        {
			StartCoroutine(DelayDialog());
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



	internal void CheckScore(BlazonGenerator bz, float score)
	{
		string test = GetCurrentBlazon();
		if (bz.blazonDescription != GetCurrentBlazon())
		{
			int id = Random.Range(0, feedback.wrongHouseReactions.Count);
			responseText.text = feedback.wrongHouseReactions[id];
			DeliverySuccess(false);
			return;
		}

		else if (score > tierS)
		{
			int id = Random.Range(0, feedback.SReactions.Count);
			responseText.text = feedback.SReactions[id];
			DeliverySuccess(true);
			return;
		}
		else if (score > tierA)
		{
			int id = Random.Range(0, feedback.AReactions.Count);
			responseText.text = feedback.AReactions[id];
			DeliverySuccess(true);
			return;
		}
		else if (score > tierB)
		{
			int id = Random.Range(0, feedback.BReactions.Count);
			responseText.text = feedback.BReactions[id];
			DeliverySuccess(true);
			return;
		}
		else if (score > tierC)
		{
			int id = Random.Range(0, feedback.CReactions.Count);
			responseText.text = feedback.CReactions[id];
			DeliverySuccess(true);
			return;
		}
		else if(score > tierD)
		{
			int id = Random.Range(0, feedback.DReactions.Count);
			responseText.text = feedback.DReactions[id];
			DeliverySuccess(true);
			return;
		}
		else
		{
			int id = Random.Range(0, feedback.FReactions.Count);
			responseText.text = feedback.FReactions[id];
			DeliverySuccess(true);
			return;
		}
	}

	public void DeliverySuccess(bool correctAddress)
	{
		if (correctAddress)
		{
			SuccessButton.gameObject.SetActive(true);
			FailButton.gameObject.SetActive(false);
		}
		else
		{
			SuccessButton.gameObject.SetActive(false);
			FailButton.gameObject.SetActive(true);
		}
	}


	public void OnDeliverySuccessful()
	{
		IncreaseLevel();
		BackToWorkshop();
		StartCoroutine(DelayDialog());
	}

	public void BackToWorkshop()
	{
		workshopCam.enabled = true;
	}
}
