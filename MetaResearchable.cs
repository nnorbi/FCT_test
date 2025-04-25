using System;
using System.Collections.Generic;
using System.Linq;
using Core.Collections.Scoped;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "RNExample", menuName = "Metadata/Research/Researchable", order = 3)]
public class MetaResearchable : ScriptableObject, IResearchUnlock
{
	[Serializable]
	public class SpeedAdjustmentData
	{
		public MetaResearchSpeed Speed;

		[Tooltip("How much % to add to the base speed. I.e. a value of 30 means a 30% speed increase. Speed increases are additive and not multiplicative.")]
		public int AdditiveSpeed = 30;
	}

	[Header("Config")]
	[Space(20f)]
	[SerializeField]
	[FormerlySerializedAs("Icon")]
	private Sprite _Icon;

	[SerializeField]
	[Tooltip("Whether this is a recurring upgrade like Belt Speed 1, Belt Speed 2 ... The filename should then be RNBeltSpeed1, RNBeltSpeed2 for example, and the translation will be shared")]
	private bool NameIncludesLevel = false;

	[SerializeField]
	[Tooltip("Whether unlocking this research shows a fullscreen unlock")]
	private bool _ShowFullUnlockNotification = true;

	[Header("Unlocks")]
	[Space(20f)]
	[SerializeField]
	private MetaWikiEntry[] UnlockWikiEntries = new MetaWikiEntry[0];

	[SerializeField]
	[Space(20f)]
	private MetaBuildingVariant[] UnlockBuildingVariants = new MetaBuildingVariant[0];

	[SerializeField]
	[Space(20f)]
	private MetaGenericResearchUnlock[] UnlockGenerics = new MetaGenericResearchUnlock[0];

	[SerializeField]
	[Space(20f)]
	private MetaIslandLayout[] UnlockLayouts = new MetaIslandLayout[0];

	[Header("Speed Adjustments")]
	[Space(20f)]
	[SerializeField]
	private SpeedAdjustmentData[] _SpeedAdjustments = new SpeedAdjustmentData[0];

	[FormerlySerializedAs("_BlueptintDiscount")]
	[Header("Blueprint Discount")]
	[Space(20f)]
	[SerializeField]
	private uint _BlueprintDiscount;

	[NonSerialized]
	private List<IResearchUnlock> _Unlocks = new List<IResearchUnlock>();

	private string EffectiveTranslationId = null;

	private int Tier = -1;

	public IReadOnlyList<IResearchUnlock> Unlocks => _Unlocks;

	public string Description
	{
		get
		{
			using ScopedList<(string, string)> replacements = ScopedList<(string, string)>.Get(SpeedAdjustments.Select((SpeedAdjustmentData speedOverride, int index) => ($"<speed{index}>", StringFormatting.FormatGeneralPercentage((float)speedOverride.AdditiveSpeed / 100f))));
			return ("research." + EffectiveTranslationId + ".description").tr(replacements);
		}
	}

	public IReadOnlyList<SpeedAdjustmentData> SpeedAdjustments => _SpeedAdjustments;

	public uint BlueprintDiscount => _BlueprintDiscount;

	public bool ShowFullUnlockNotification => _ShowFullUnlockNotification;

	public Sprite Icon => _Icon;

	public string Title
	{
		get
		{
			string baseTitle = ("research." + EffectiveTranslationId + ".title").tr();
			if (Tier >= 0)
			{
				return baseTitle + " " + StringFormatting.FormatResearchNodeTier(Tier);
			}
			return baseTitle;
		}
	}

	public void Init()
	{
		_Unlocks.Clear();
		MetaBuildingVariant[] unlockBuildingVariants = UnlockBuildingVariants;
		foreach (MetaBuildingVariant variant in unlockBuildingVariants)
		{
			_Unlocks.Add(variant);
		}
		MetaGenericResearchUnlock[] unlockGenerics = UnlockGenerics;
		foreach (MetaGenericResearchUnlock generic in unlockGenerics)
		{
			_Unlocks.Add(generic);
		}
		MetaIslandLayout[] unlockLayouts = UnlockLayouts;
		foreach (MetaIslandLayout layout in unlockLayouts)
		{
			_Unlocks.Add(layout);
		}
		MetaWikiEntry[] unlockWikiEntries = UnlockWikiEntries;
		foreach (MetaWikiEntry wikiEntry in unlockWikiEntries)
		{
			_Unlocks.Add(wikiEntry);
		}
		EffectiveTranslationId = base.name;
		Tier = -1;
		if (!NameIncludesLevel)
		{
			return;
		}
		string parsedTier = "";
		while (EffectiveTranslationId.Length > 0)
		{
			string effectiveTranslationId = EffectiveTranslationId;
			if (effectiveTranslationId[effectiveTranslationId.Length - 1] <= '0')
			{
				break;
			}
			string effectiveTranslationId2 = EffectiveTranslationId;
			if (effectiveTranslationId2[effectiveTranslationId2.Length - 1] > '9')
			{
				break;
			}
			string effectiveTranslationId3 = EffectiveTranslationId;
			parsedTier = effectiveTranslationId3[effectiveTranslationId3.Length - 1] + parsedTier;
			EffectiveTranslationId = EffectiveTranslationId.Substring(0, EffectiveTranslationId.Length - 1);
		}
		if (!int.TryParse(parsedTier, out Tier))
		{
			throw new Exception("Failed to parse tier of research node: " + parsedTier);
		}
	}
}
