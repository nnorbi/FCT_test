using System;
using Core.Dependency;
using TMPro;
using UnityEngine;

public class HUDKeySetRenderer : HUDComponent
{
	[Serializable]
	private class GroupSet
	{
		[SerializeField]
		private GameObject GameObject;

		[SerializeField]
		private TMP_Text Text;

		[SerializeField]
		private RectTransform Transform;

		public void Render(KeyCode code)
		{
			if (code == KeyCode.None)
			{
				GameObject.SetActiveSelfExt(active: false);
				return;
			}
			GameObject.SetActiveSelfExt(active: true);
			Text.text = KeyCodeFormatter.Resolve(code);
			float width = Text.GetPreferredValues(Text.text, 10000f, 25f).x;
			Transform.SetWidth(width + 12f);
		}
	}

	[SerializeField]
	private GroupSet UIBaseSet;

	[SerializeField]
	private GroupSet UIModifier0Set;

	[SerializeField]
	private GroupSet UIModifier1Set;

	private KeySet _CurrentSet;

	public KeySet CurrentSet
	{
		set
		{
			_CurrentSet = value;
			Rerender();
		}
	}

	[Construct]
	private void Construct()
	{
		UIBaseSet.Render(KeyCode.None);
		UIModifier0Set.Render(KeyCode.None);
		UIModifier1Set.Render(KeyCode.None);
	}

	private void Rerender()
	{
		UIBaseSet.Render(_CurrentSet.Code);
		UIModifier0Set.Render(_CurrentSet.Modifier0);
		UIModifier1Set.Render(_CurrentSet.Modifier1);
	}

	protected override void OnDispose()
	{
	}
}
