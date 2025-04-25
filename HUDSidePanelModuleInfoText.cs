using TMPro;
using UnityEngine;

public class HUDSidePanelModuleInfoText : HUDSidePanelModule
{
	protected string Text;

	public HUDSidePanelModuleInfoText(string text)
	{
		Text = text;
	}

	public override void Setup(GameObject container)
	{
		base.Setup(container);
		TMP_Text textObj = container.FindText("$Text");
		textObj.text = Text;
		float height = textObj.GetPreferredValues(Text, 300f, 10000f).y;
		container.GetComponent<RectTransform>().SetHeight(height);
		textObj.GetComponent<RectTransform>().SetHeight(height);
	}
}
