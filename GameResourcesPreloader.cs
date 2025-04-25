using System.Collections.Generic;
using UnityEngine;

public class GameResourcesPreloader : MonoBehaviour
{
	public ShaderVariantCollection PreloadShaderVariants;

	public GameResources GameResources;

	public VisualThemeBaseResources BaseThemeResources;

	public SpaceThemeResources SpaceThemeResources;

	public MetaGameMode[] GameModes;

	public IEnumerable<string> Preload()
	{
		yield return "Preloading shaders ...";
		Debug.LogWarning("DEV HINT: If you get any errors related to MSAA / antialiasing DURING shader preload, make sure shaders like CopyDepth are NOT in the ShaderVariantsCollection! Just search for 'MSAA' in the raw file.");
		Debug.LogWarning("DEV HINT: If you get FLICKERING during shader preload, remove the UI shaders and generally all internal shaders from the shader variant collection");
		int totalCount = PreloadShaderVariants.variantCount;
		int batchSize = (Application.isEditor ? 1 : 5);
		do
		{
			yield return "Preloaded " + PreloadShaderVariants.warmedUpVariantCount + " / " + totalCount + " shaders";
		}
		while (!PreloadShaderVariants.WarmUpProgressively(batchSize));
		yield return "Preloaded " + PreloadShaderVariants.warmedUpVariantCount + " / " + totalCount + " shaders";
		yield return "Resources preloaded.";
	}
}
