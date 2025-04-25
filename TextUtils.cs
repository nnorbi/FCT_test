using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public static class TextUtils
{
	private class CustomTextLinkEventListener : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
	{
		public UnityEvent<PointerEventData> PointerClick = new UnityEvent<PointerEventData>();

		public void OnPointerClick(PointerEventData data)
		{
			PointerClick.Invoke(data);
		}
	}

	public static string CreateLinkText(string link, string content)
	{
		return "<link=\"" + link + "\"><color=#" + HUDTheme.LinkColorHex + "><b><u>" + content + "</u></b></color></link>";
	}

	public static void AddLinkClickHandler(this TMP_Text text, Action<string> handler, Camera camera = null)
	{
		CustomTextLinkEventListener eventTrigger = text.gameObject.GetOrAddComponent<CustomTextLinkEventListener>();
		eventTrigger.PointerClick.AddListener(OnTextClick);
		void OnTextClick(PointerEventData eventData)
		{
			int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, camera);
			if (linkIndex != -1)
			{
				TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
				handler(linkInfo.GetLinkID());
			}
		}
	}
}
