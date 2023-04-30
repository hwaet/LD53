using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// http://forum.unity3d.com/threads/eventtrigger-move-issue.325103/#post-2114563

public class test : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	// Called when the pointer enters our GUI component.
	// Start tracking the mouse
	public void OnPointerEnter(PointerEventData eventData)
	{
		StartCoroutine("TrackPointer");
	}

	// Called when the pointer exits our GUI component.
	// Stop tracking the mouse
	public void OnPointerExit(PointerEventData eventData)
	{
		StopCoroutine("TrackPointer");
	}

	IEnumerator TrackPointer()
	{
		var ray = GetComponentInParent<GraphicRaycaster>();
		var input = FindObjectOfType<StandaloneInputModule>();

		if (ray != null && input != null)
		{
			while (Application.isPlaying)
			{
				Vector2 localPos; // Mouse position  
				RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, Input.mousePosition, ray.eventCamera, out localPos);

				// local pos is the mouse position.
				Debug.Log(localPos);

				yield return 0;
			}
		}
		else
			UnityEngine.Debug.LogWarning("Could not find GraphicRaycaster and/or StandaloneInputModule");
	}
}