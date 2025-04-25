using System;
using UnityEngine;

public class VisualThemeFeature
{
	[Serializable]
	public class BaseExtraResources
	{
		public bool Draw = true;

		[NonSerialized]
		[HideInInspector]
		public bool NeedsRegeneration = true;

		private void Regenerate()
		{
			NeedsRegeneration = true;
		}
	}
}
