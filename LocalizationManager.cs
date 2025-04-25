using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public class LocalizationManager
{
	public const string LANGUAGE_KEY_AUTODETECT = "autodetect";

	public const string FALLBACK_LANGUAGE = "en-US";

	private static Dictionary<SystemLanguage, string> LANGUAGE_TO_ISO_CODE = new Dictionary<SystemLanguage, string>
	{
		{
			SystemLanguage.Unknown,
			"en-US"
		},
		{
			SystemLanguage.English,
			"en-US"
		},
		{
			SystemLanguage.German,
			"de-DE"
		},
		{
			SystemLanguage.Spanish,
			"es-ES"
		},
		{
			SystemLanguage.French,
			"fr-FR"
		},
		{
			SystemLanguage.Japanese,
			"ja-JP"
		},
		{
			SystemLanguage.Korean,
			"ko-KR"
		},
		{
			SystemLanguage.Polish,
			"pl-PL"
		},
		{
			SystemLanguage.Portuguese,
			"pt-BR"
		},
		{
			SystemLanguage.Russian,
			"ru-RU"
		},
		{
			SystemLanguage.Thai,
			"th-TH"
		},
		{
			SystemLanguage.Turkish,
			"tr-TR"
		},
		{
			SystemLanguage.Chinese,
			"zh-CN"
		},
		{
			SystemLanguage.ChineseSimplified,
			"zh-CN"
		},
		{
			SystemLanguage.ChineseTraditional,
			"zh-TW"
		}
	};

	public Action OnLanguageSetFirstTime;

	protected Dictionary<string, string> LanguageCodeToNameLookup = new Dictionary<string, string>();

	private ITranslator _CurrentTranslator;

	public IReadOnlyList<ITranslator> AvailableTranslators { get; protected set; }

	public ITranslator CurrentTranslator
	{
		get
		{
			return _CurrentTranslator;
		}
		private set
		{
			_CurrentTranslator = value;
			Debug.Log("Loaded translator: " + value.LanguageCode);
			CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(value.LanguageCode);
			OverrideFrenchPercentageSymbol(value.LanguageCode);
		}
	}

	private static void OverrideFrenchPercentageSymbol(string languageCode)
	{
		if (languageCode.StartsWith("fr"))
		{
			CultureInfo.DefaultThreadCurrentCulture.NumberFormat.PercentSymbol = "%";
		}
	}

	public static ITranslator CreateEditorOnlyTranslatorUncached()
	{
		if (!Application.isEditor || Application.isPlaying)
		{
			throw new Exception("Not available outside of editor");
		}
		LocalizationManager localizer = new LocalizationManager();
		localizer.LoadTranslators();
		localizer.TryLoadLanguage("en-US");
		return localizer.CurrentTranslator;
	}

	public void LoadTranslators()
	{
		if (GameEnvironmentManager.FLAG_CUSTOM_TRANSLATION)
		{
			Debug.Log("Loading custom translator");
			AvailableTranslators = new ITranslator[1] { LoadCustomTranslator() };
			return;
		}
		TextAsset[] translationFiles = Resources.LoadAll<TextAsset>("Translations");
		List<ITranslator> translators = new List<ITranslator>();
		TextAsset[] array = translationFiles;
		foreach (TextAsset translationFile in array)
		{
			try
			{
				translators.Add(JsonConvert.DeserializeObject<JsonDictionaryTranslator>(translationFile.text));
			}
			catch (Exception arg)
			{
				Debug.LogWarning($"Failed to load translation {translationFile}. Error: {arg}");
			}
		}
		AvailableTranslators = translators.ToArray();
		if (AvailableTranslators.Count == 0)
		{
			CurrentTranslator = new ErrorTranslator();
			Debug.LogWarning("Could not load any language");
		}
		foreach (ITranslator availableTranslation in AvailableTranslators)
		{
			LanguageCodeToNameLookup.Add(availableTranslation.LanguageCode, availableTranslation.LanguageTitle);
		}
	}

	private ITranslator LoadCustomTranslator()
	{
		string path = Path.Join(GameEnvironmentManager.DATA_PATH, "translations-override.json");
		Debug.Log("Loading custom translations from " + path);
		if (!File.Exists(path))
		{
			throw new Exception("Could not find translation override at '" + path + "'.");
		}
		string json = File.ReadAllText(path, Encoding.UTF8);
		return JsonConvert.DeserializeObject<JsonDictionaryTranslator>(json);
	}

	private string AutoDetectLanguage()
	{
		if (LANGUAGE_TO_ISO_CODE.TryGetValue(Application.systemLanguage, out var code))
		{
			Debug.Log("Language auto detected: " + code);
			return code;
		}
		Debug.Log("Language detected, but not available: " + Application.systemLanguage.ToString() + " -> fallback to en-US");
		return "en-US";
	}

	public bool TryLoadLanguage(string languageCode)
	{
		if (AvailableTranslators == null)
		{
			throw new Exception("LocalizationManager: Call LoadTranslators first");
		}
		Debug.Log("Loading translations, preferred = " + languageCode);
		if (GameEnvironmentManager.FLAG_CUSTOM_TRANSLATION)
		{
			CurrentTranslator = AvailableTranslators[0];
			return true;
		}
		if (languageCode == "autodetect")
		{
			languageCode = AutoDetectLanguage();
			Debug.Log("Auto detected language to " + languageCode);
		}
		ITranslator matchingTranslator = AvailableTranslators.FirstOrDefault((ITranslator x) => x.LanguageCode == languageCode);
		if (matchingTranslator != null)
		{
			CurrentTranslator = matchingTranslator;
			return true;
		}
		if (languageCode == "en-US")
		{
			Debug.LogError("Failed to load fallback language '" + languageCode + "' -> Translations will fail");
			CurrentTranslator = new ErrorTranslator();
			return false;
		}
		Debug.LogWarning("Failed to find matching translator for language '" + languageCode + "' -> falling back to en-US");
		TryLoadLanguage("en-US");
		return false;
	}

	public string GetDisplayLanguageFromCode(string code)
	{
		if (LanguageCodeToNameLookup.TryGetValue(code, out var result))
		{
			return result;
		}
		Debug.LogWarning("No mapping defined for " + code);
		return code + " (?)";
	}

	public void SetLanguagePreloadIntroOnly(ITranslator translator)
	{
		if ((bool)Globals.Settings.General.PreloadIntroShown)
		{
			throw new Exception("This should only be called in the preload");
		}
		CurrentTranslator = translator;
		OnLanguageSetFirstTime?.Invoke();
	}
}
