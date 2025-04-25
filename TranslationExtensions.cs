using System.Collections.Generic;
using UnityEngine;

public static class TranslationExtensions
{
	private static string KEYBINDING_PREFIX = "<hotkey:";

	private static string KEYBINDING_SUFFIX = ">";

	private static string KEYBINDING_PREFIX_REPLACE = "<color=#FFA036>";

	private static string KEYBINDING_SUFFIX_REPLACE = "</color>";

	private static string HIGHLIGHT_PREFIX = "<highlight>";

	private static string HIGHLIGHT_SUFFIX = "</highlight>";

	private static string HIGHLIGHT_REPLACE_PREFIX = "<color=#FFA036>";

	private static string HIGHLIGHT_REPLACE_SUFFIX = "</color>";

	private static string GLOSSARY_PREFIX = "<glossary>";

	private static string GLOSSARY_SUFFIX = "</glossary>";

	private static string GLOSSARY_REPLACE_PREFIX = "<color=#BBBBBA>";

	private static string GLOSSARY_REPLACE_SUFFIX = "</color>";

	private static string CROSS_REFERENCE_TRANSLATION_PREFIX = "<copy-from:";

	private static string CROSS_REFERENCE_TRANSLATION_SUFFIX = ">";

	private static string LINK_START = "<link=";

	private static string LINK_START_REPLACEMENT = "<color=#" + HUDTheme.LinkColorHex + "><b><u>" + LINK_START;

	private static string LINK_END = "</link>";

	private static string LINK_END_REPLACEMENT = LINK_END + "</u></b></color>";

	private static string WIP_WARNING_TAG = "<wip-warning-hint>";

	private static string WIP_WARNING_REPLACEMENT = "<color=#FF2C63>This content is work in progress and might get reworked at any time. Please give feedback in the shapez 2 discord!</color>\n\n";

	public static string tr(this string id)
	{
		return Tr(id);
	}

	public static string tr(this string id, params (string token, string content)[] replacements)
	{
		string translatedText = Tr(id);
		for (int i = 0; i < replacements.Length; i++)
		{
			(string, string) replacement = replacements[i];
			translatedText = translatedText.Replace(replacement.Item1, replacement.Item2);
		}
		return translatedText;
	}

	public static string tr(this string id, IEnumerable<(string token, string content)> replacements)
	{
		string translatedText = Tr(id);
		foreach (var replacement in replacements)
		{
			translatedText = translatedText.Replace(replacement.token, replacement.content);
		}
		return translatedText;
	}

	public static bool HasTranslation(this string id)
	{
		string value;
		return GetTranslator().TryGetTranslation(id, out value);
	}

	private static string Tr(string id)
	{
		if (!GetTranslator().TryGetTranslation(id, out var result))
		{
			Debug.LogWarning("Missing translation for: " + id);
			return "[[" + id + "]]";
		}
		if (result.StartsWith(CROSS_REFERENCE_TRANSLATION_PREFIX) && result.EndsWith(CROSS_REFERENCE_TRANSLATION_SUFFIX))
		{
			string translationId = result.Substring(CROSS_REFERENCE_TRANSLATION_PREFIX.Length, result.Length - CROSS_REFERENCE_TRANSLATION_PREFIX.Length - CROSS_REFERENCE_TRANSLATION_SUFFIX.Length);
			return Tr(translationId);
		}
		if (result.Contains(KEYBINDING_PREFIX))
		{
			foreach (KeybindingsLayer layer in Globals.Keybindings.Layers)
			{
				foreach (Keybinding keybinding in layer.Bindings)
				{
					string keyReplace = KEYBINDING_PREFIX + keybinding.Id + KEYBINDING_SUFFIX;
					while (result.Contains(keyReplace))
					{
						result = result.Replace(keyReplace, KEYBINDING_PREFIX_REPLACE + KeyCodeFormatter.Resolve(keybinding) + KEYBINDING_SUFFIX_REPLACE);
					}
				}
			}
		}
		if (result.Contains(WIP_WARNING_TAG))
		{
			result = result.Replace(WIP_WARNING_TAG, WIP_WARNING_REPLACEMENT);
		}
		if (result.Contains(HIGHLIGHT_PREFIX))
		{
			result = result.Replace(HIGHLIGHT_PREFIX, HIGHLIGHT_REPLACE_PREFIX);
			result = result.Replace(HIGHLIGHT_SUFFIX, HIGHLIGHT_REPLACE_SUFFIX);
		}
		if (result.Contains(GLOSSARY_PREFIX))
		{
			result = result.Replace(GLOSSARY_PREFIX, GLOSSARY_REPLACE_PREFIX);
			result = result.Replace(GLOSSARY_SUFFIX, GLOSSARY_REPLACE_SUFFIX);
		}
		if (result.Contains(LINK_START))
		{
			result = result.Replace(LINK_START, LINK_START_REPLACEMENT);
			result = result.Replace(LINK_END, LINK_END_REPLACEMENT);
		}
		return result;
	}

	private static ITranslator GetTranslator()
	{
		return Globals.Localization.CurrentTranslator;
	}
}
