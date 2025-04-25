using System;
using System.Collections.Generic;
using Tayx.Graphy;
using Tayx.Graphy.Utils;
using UnityEngine;

public class GeneralCommands
{
	public static void RegisterCommands(DebugConsole console)
	{
		console.Register("debug.print-build-args", delegate(DebugConsole.CommandContext ctx)
		{
			ctx.Output("Build parameters:");
			ctx.Output(" - DEVELOPMENT_BUILD: ON");
			ctx.Output(" - UNITY_EDITOR: OFF");
			ctx.Output(" - SPZ_SLOW_EDITOR_CHECKS: OFF");
			ctx.Output(" - Platform: " + Application.platform);
			ctx.Output(" - Unity version: " + Application.unityVersion);
			ctx.Output(" - Version: " + Application.version);
			ctx.Output(" - Game Environment: " + GameEnvironmentManager.ENVIRONMENT);
			ctx.Output(" - Game Store: " + GameEnvironmentManager.STORE);
		});
		console.Register("debug.sysinfo", delegate(DebugConsole.CommandContext ctx)
		{
			ctx.Output("System info (Game version " + GameEnvironmentManager.VERSION + "):");
			ctx.Output("- maxComputeBufferInputsVertex: " + SystemInfo.maxComputeBufferInputsVertex);
			ctx.Output("- maxCubemapSize: " + SystemInfo.maxCubemapSize);
			ctx.Output("- maxTextureArraySlices: " + SystemInfo.maxTextureArraySlices);
			ctx.Output("- maxTexture3DSize: " + SystemInfo.maxTexture3DSize);
			ctx.Output("- maxTextureSize: " + SystemInfo.maxTextureSize);
			ctx.Output("- npotSupport: " + SystemInfo.npotSupport);
			ctx.Output("- usesReversedZBuffer: " + SystemInfo.usesReversedZBuffer);
			ctx.Output("- supportsMultisampleAutoResolve: " + SystemInfo.supportsMultisampleAutoResolve);
			ctx.Output("- supportsMultisampledTextures: " + SystemInfo.supportsMultisampledTextures);
			ctx.Output("- supportedRandomWriteTargetCount: " + SystemInfo.supportedRandomWriteTargetCount);
			ctx.Output("- supportedRenderTargetCount: " + SystemInfo.supportedRenderTargetCount);
			ctx.Output("- supportsSparseTextures: " + SystemInfo.supportsSparseTextures);
			ctx.Output("- supports32bitsIndexBuffer: " + SystemInfo.supports32bitsIndexBuffer);
			ctx.Output("- supportsHardwareQuadTopology: " + SystemInfo.supportsHardwareQuadTopology);
			ctx.Output("- supportsInstancing: " + SystemInfo.supportsInstancing);
			ctx.Output("- supportsTessellationShaders: " + SystemInfo.supportsTessellationShaders);
			ctx.Output("- supportsGeometryShaders: " + SystemInfo.supportsGeometryShaders);
			ctx.Output("- hdrDisplaySupportFlags: " + SystemInfo.hdrDisplaySupportFlags);
			ctx.Output("- supportsMipStreaming: " + SystemInfo.supportsMipStreaming);
			ctx.Output("- hasMipMaxLevel: " + SystemInfo.hasMipMaxLevel);
			ctx.Output("- maxGraphicsBufferSize: " + SystemInfo.maxGraphicsBufferSize);
			ctx.Output("- supportsGraphicsFence: " + SystemInfo.supportsGraphicsFence);
			ctx.Output("- supportsGpuRecorder: " + SystemInfo.supportsGpuRecorder);
			ctx.Output("- supportsAsyncCompute: " + SystemInfo.supportsAsyncCompute);
			ctx.Output("- supportsComputeShaders: " + SystemInfo.supportsComputeShaders);
			ctx.Output("- deviceType: " + SystemInfo.deviceType);
			ctx.Output("- supportsAudio: " + SystemInfo.supportsAudio);
			ctx.Output("- supportsMultisampleResolveDepth: " + SystemInfo.supportsMultisampleResolveDepth);
			ctx.Output("- deviceName: " + SystemInfo.deviceName);
			ctx.Output("- deviceModel: " + SystemInfo.deviceModel);
			ctx.Output("- graphicsMemorySize: " + SystemInfo.graphicsMemorySize);
			ctx.Output("- processorCount: " + SystemInfo.processorCount);
			ctx.Output("- processorFrequency: " + SystemInfo.processorFrequency);
			ctx.Output("- processorType: " + SystemInfo.processorType);
			ctx.Output("- operatingSystemFamily: " + SystemInfo.operatingSystemFamily);
			ctx.Output("- operatingSystem: " + SystemInfo.operatingSystem);
			ctx.Output("- batteryStatus: " + SystemInfo.batteryStatus);
			ctx.Output("- systemMemorySize: " + SystemInfo.systemMemorySize);
			ctx.Output("- graphicsDeviceName: " + SystemInfo.graphicsDeviceName);
			ctx.Output("- graphicsDeviceID: " + SystemInfo.graphicsDeviceID);
			ctx.Output("- graphicsDeviceVendor: " + SystemInfo.graphicsDeviceVendor);
			ctx.Output("- supportsRawShadowDepthSampling: " + SystemInfo.supportsRawShadowDepthSampling);
			ctx.Output("- supportsShadows: " + SystemInfo.supportsShadows);
			ctx.Output("- graphicsDeviceType: " + SystemInfo.graphicsDeviceType);
			ctx.Output("- graphicsUVStartsAtTop: " + SystemInfo.graphicsUVStartsAtTop);
			ctx.Output("- graphicsShaderLevel: " + SystemInfo.graphicsShaderLevel);
			ctx.Output("- graphicsMultiThreaded: " + SystemInfo.graphicsMultiThreaded);
			ctx.Output("- graphicsDeviceVersion: " + SystemInfo.graphicsDeviceVersion);
			ctx.Output("- renderingThreadingMode: " + SystemInfo.renderingThreadingMode);
			ctx.Output("- hasHiddenSurfaceRemovalOnGPU: " + SystemInfo.hasHiddenSurfaceRemovalOnGPU);
		});
		console.Register("quit", delegate
		{
			Application.Quit(0);
		});
		console.Register("screen.set-resolution", new DebugConsole.IntOption("width", 512, 8192), new DebugConsole.IntOption("height", 512, 8192), delegate(DebugConsole.CommandContext ctx)
		{
			Screen.SetResolution(ctx.GetInt(0), ctx.GetInt(1), Screen.fullScreen);
			ctx.Output("Resolution has been applied.");
		});
		console.Register("screen.move-to-display", new DebugConsole.IntOption("display-index", 0, 32), delegate(DebugConsole.CommandContext ctx)
		{
			List<DisplayInfo> list = new List<DisplayInfo>();
			Screen.GetDisplayLayout(list);
			int num = ctx.GetInt(0);
			if (list.Count < num)
			{
				ctx.Output("Invalid display index: " + num);
			}
			else
			{
				Screen.MoveMainWindowTo(list[num], new Vector2Int(0, 0));
			}
		});
		console.Register("debug.show-charts", new DebugConsole.BoolOption("enabled"), delegate(DebugConsole.CommandContext ctx)
		{
			if (ctx.GetBool(0))
			{
				G_Singleton<GraphyManager>.Instance.Enable();
			}
			else
			{
				G_Singleton<GraphyManager>.Instance.Disable();
			}
		});
		console.Register("debug.statistics", delegate(DebugConsole.CommandContext ctx)
		{
			GameMap currentMap = Singleton<GameCore>.G.LocalPlayer.CurrentMap;
			ctx.Output("There are " + currentMap.Islands.Count + " islands.");
			int num = 0;
			foreach (Island current in currentMap.Islands)
			{
				num += current.Buildings.Buildings.Count;
			}
			ctx.Output("There are " + num + " buildings.");
		});
		console.Register("debug.clear-player-prefs", delegate(DebugConsole.CommandContext ctx)
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
			ctx.Output("Player prefs have been cleared");
		});
		console.Register("debug.matrix-test", delegate(DebugConsole.CommandContext ctx)
		{
			Perftest.Run(ctx);
		});
		console.Register("debug.highlight-material", new DebugConsole.StringOption("material"), delegate(DebugConsole.CommandContext ctx)
		{
			string[] array = new string[9] { "none", "accent", "fake-emissive", "metal-brushed", "metal-noise", "metal-coat", "plastic", "belt-rubber", "greeble" };
			int num = Array.IndexOf(array, ctx.GetString(0).ToLower().Trim());
			if (num < 0)
			{
				ctx.Output("Material not found. Valid values: ");
				string[] array2 = array;
				foreach (string text in array2)
				{
					ctx.Output(" - " + text);
				}
			}
			else
			{
				Shader.SetGlobalFloat("_G_MaterialDebugIndex", num);
			}
		});
		console.Register("debug.clear-bp-cache", delegate(DebugConsole.CommandContext ctx)
		{
			foreach (MetaBuilding current in Singleton<GameCore>.G.Mode.Buildings)
			{
				foreach (MetaBuildingVariant current2 in current.Variants)
				{
					MetaBuildingInternalVariant[] internalVariants = current2.InternalVariants;
					foreach (MetaBuildingInternalVariant metaBuildingInternalVariant in internalVariants)
					{
						metaBuildingInternalVariant.ClearBlueprintMeshCache();
					}
				}
			}
			ctx.Output("Cleared BP mesh cache");
		});
		console.Register("debug.export-buildings-metadata", delegate
		{
			BuildingMetadataExporter.ExportMetadata();
		});
		console.Register("debug.export-research-metadata", delegate
		{
			ResearchMetadataExporter.ExportMetadata();
		});
	}
}
