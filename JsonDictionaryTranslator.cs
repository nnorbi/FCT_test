using System.Collections.Generic;
using Newtonsoft.Json;

public class JsonDictionaryTranslator : ITranslator
{
	[JsonProperty("Entries")]
	public Dictionary<string, string> Entries;

	[JsonProperty("LanguageTitle")]
	public string LanguageTitle { get; protected set; }

	[JsonProperty("LanguageCode")]
	public string LanguageCode { get; protected set; }

	public bool TryGetTranslation(string key, out string value)
	{
		return Entries.TryGetValue(key, out value);
	}
}
