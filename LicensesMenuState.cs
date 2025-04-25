using System;
using System.Collections.Generic;
using System.Globalization;
using Core.Dependency;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LicensesMenuState : MainMenuState
{
	[Serializable]
	public struct Licenses
	{
		public Category[] Categories;
	}

	[Serializable]
	public struct Category
	{
		public string Title;

		public LicenseEntry[] Entries;
	}

	[Serializable]
	public struct LicenseEntry
	{
		public string AssetName;

		public string CreatorName;

		public string LicenseType;

		public string CustomLicenseLink;
	}

	public struct LicenseTypeData
	{
		public string Name;

		public string Url;

		public LicenseTypeData(string name, string url)
		{
			Name = name;
			Url = url;
		}
	}

	protected static readonly Dictionary<string, LicenseTypeData> LicenseTypeLookup = new Dictionary<string, LicenseTypeData>
	{
		{
			"MIT",
			new LicenseTypeData("MIT", "https://opensource.org/license/mit/")
		},
		{
			"OFL",
			new LicenseTypeData("OFL", "https://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL")
		},
		{
			"UnityAssetStore",
			new LicenseTypeData("Standard Unity Asset Store EULA", "https://unity.com/legal/as-terms")
		}
	};

	protected static JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings
	{
		Culture = CultureInfo.InvariantCulture,
		MissingMemberHandling = MissingMemberHandling.Ignore,
		ObjectCreationHandling = ObjectCreationHandling.Replace,
		StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
	};

	[SerializeField]
	private HUDMenuBackButton UIBtnBack;

	[SerializeField]
	private Button UIReloadButtonEditorOnly;

	[SerializeField]
	private RectTransform UILicensesParent;

	[SerializeField]
	private TMP_Text UILicensesCategoryHeaderPrefab;

	[SerializeField]
	private LicensesMenuEntry UIEntryPrefab;

	[Construct]
	private void Construct()
	{
		AddChildView(UIBtnBack);
		UIBtnBack.Clicked.AddListener(GoBack);
		if (Application.isEditor)
		{
			UIReloadButtonEditorOnly.onClick.AddListener(LoadLicenses);
		}
		else
		{
			UIReloadButtonEditorOnly.gameObject.SetActive(value: false);
		}
		LoadLicenses();
	}

	protected override void OnDispose()
	{
		UIBtnBack.Clicked.RemoveListener(GoBack);
		if (Application.isEditor)
		{
			UIReloadButtonEditorOnly.onClick.RemoveListener(LoadLicenses);
		}
	}

	private void LoadLicenses()
	{
		try
		{
			UILicensesParent.RemoveAllChildren();
			TextAsset licensesJson = Resources.Load<TextAsset>("Licenses");
			Category[] categories = JsonConvert.DeserializeObject<Licenses>(licensesJson.text, JSON_SETTINGS).Categories;
			for (int i = 0; i < categories.Length; i++)
			{
				Category category = categories[i];
				TMP_Text headerInstance = UnityEngine.Object.Instantiate(UILicensesCategoryHeaderPrefab, UILicensesParent);
				headerInstance.text = category.Title;
				LicenseEntry[] entries = category.Entries;
				foreach (LicenseEntry entry in entries)
				{
					LicensesMenuEntry entryInstance = UnityEngine.Object.Instantiate(UIEntryPrefab, UILicensesParent);
					entryInstance.EntryText.text = EntryText(entry);
					entryInstance.EntryText.AddLinkClickHandler(Application.OpenURL);
				}
			}
		}
		catch (Exception arg)
		{
			base.Logger.Exception?.Log($"Failed to load licenses. Error {arg}");
		}
	}

	protected static string EntryText(LicenseEntry entry)
	{
		LicenseTypeData licenseData = default(LicenseTypeData);
		if (string.IsNullOrEmpty(entry.LicenseType) || !LicenseTypeLookup.TryGetValue(entry.LicenseType, out licenseData))
		{
			if (string.IsNullOrEmpty(entry.CustomLicenseLink))
			{
				Debug.LogWarning("License type is not identified and no custom license found in the ");
				return "Error reading license for " + entry.AssetName;
			}
			licenseData.Name = "a custom license";
			licenseData.Url = entry.CustomLicenseLink;
		}
		if (string.IsNullOrEmpty(entry.CreatorName))
		{
			return entry.AssetName + " is licensed under " + TextUtils.CreateLinkText(licenseData.Url, licenseData.Name) + "\n";
		}
		return entry.AssetName + " by " + entry.CreatorName + " is licensed under " + TextUtils.CreateLinkText(licenseData.Url, licenseData.Name) + "\n";
	}

	public override void GoBack()
	{
		Menu.SwitchToState<AboutMenuState>();
	}
}
