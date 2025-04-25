using System.Linq;

public static class BlueprintExceptionTranslationExtensions
{
	public static string tr(this BlueprintException exception)
	{
		if (!(exception is BlueprintSerializationSyntaxException e))
		{
			if (!(exception is BlueprintSerializationParsingException e2))
			{
				if (!(exception is BlueprintSerializationBlueprintVersionException e3))
				{
					if (!(exception is BlueprintSerializationSavegameVersionException e4))
					{
						if (!(exception is BlueprintSerializationConvertBase64Exception))
						{
							if (!(exception is BlueprintSerializationZipException))
							{
								if (!(exception is BlueprintSerializationJsonException))
								{
									if (!(exception is BlueprintSerializationUnknownTypeException e5))
									{
										if (!(exception is BlueprintEmptyException))
										{
											if (exception is AggregateBlueprintException ae)
											{
												return ae.InnerExceptions.Distinct().Aggregate(string.Empty, (string aggregated, BlueprintException exception2) => aggregated + exception2.tr() + "\n\n");
											}
											return "blueprint-serializer.unexpected-error".tr();
										}
										return "blueprint-serializer.empty-blueprint".tr();
									}
									return "blueprint-serializer.unknown-type".tr(("<type>", e5.Type));
								}
								return "blueprint-serializer.json-error".tr();
							}
							return "blueprint-serializer.gzip-error".tr();
						}
						return "blueprint-serializer.base64-error".tr();
					}
					return "blueprint-serializer.data-version-mismatch".tr(("<minimum>", e4.MinimumSupportedVersion.ToString()), ("<maximum>", e4.MaximumSupportedVersion.ToString()), ("<actual>", e4.SerializedVersion.ToString()));
				}
				return "blueprint-serializer.version-mismatch".tr(("<minimum>", e3.MinimumSupportedVersion.ToString()), ("<maximum>", e3.MaximumSupportedVersion.ToString()), ("<actual>", e3.SerializedVersion.ToString()));
			}
			string tokenId = "blueprint-serializer.token." + e2.TokenName;
			string tokenName = tokenId.tr();
			return "blueprint-serializer.parsing-failed".tr(("<token>", tokenName), ("<value>", e2.TokenValue));
		}
		return "blueprint-serializer.missing-token".tr(("<token>", e.MissingToken));
	}
}
