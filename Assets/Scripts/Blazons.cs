using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Blazons : ScriptableObject
{
	[TextArea(1, 3)]
	public List<string> distractorBlazons;


	[ContextMenu("Validate Blazons")]
	public void ValidateBlazons()
	{
		for (int i = 0; i < distractorBlazons.Count; i++)
		{
			try
			{
				Blazon test = Blazon.Parse(distractorBlazons[i]);
			}
			catch (BlazonError e)
			{
				Debug.LogErrorFormat("distractorBlazon[{0}] Error: {1}", i, e.Message);
			}
		}
	}
}