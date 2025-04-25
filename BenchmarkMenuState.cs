using System;
using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using Crosstales.Common.Util;
using Crosstales.FB;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BenchmarkMenuState : MainMenuState
{
	private static BenchmarkResults LastSessionBenchmarkResults;

	public Button UIBtnBack;

	[SerializeField]
	private BenchmarkEntryBehavior UIBenchmarkEntryPrefab;

	[SerializeField]
	private RectTransform UIBenchmarkEntriesParent;

	private HUDDialogSimpleConfirm Dialog;

	[Construct]
	private void Construct()
	{
		UIBtnBack.onClick.AddListener(GoBack);
		if (LastSessionBenchmarkResults != null)
		{
			Menu.SwitchToState<BenchmarkMenuState>();
			Dialog = DialogStack.ShowUIDialog<HUDDialogSimpleConfirm>();
			Dialog.InitDialogContents("Benchmark results", LastSessionBenchmarkResults.ToString(), "Export", "Cancel");
			Dialog.OnConfirmed.AddListener(ExportResults);
			Dialog.CloseRequested.AddListener(DismissResults);
		}
	}

	protected override void OnDispose()
	{
		UIBtnBack.onClick.RemoveListener(GoBack);
		Dialog.OnConfirmed.RemoveListener(ExportResults);
		Dialog.CloseRequested.RemoveListener(DismissResults);
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
	}

	private static void ExportResults()
	{
		string path = Crosstales.Common.Util.Singleton<FileBrowser>.Instance.SaveFile("Save benchmark results", null, "Benchmark", "csv");
		if (!string.IsNullOrEmpty(path))
		{
			BenchmarkCsvExporter.Export(path, LastSessionBenchmarkResults);
		}
	}

	private static void DismissResults()
	{
		LastSessionBenchmarkResults.Dispose();
		LastSessionBenchmarkResults = null;
	}

	public override void OnMenuEnterState(object payload)
	{
		AssemblyTypesCache typesCache = new AssemblyTypesCache(ReflectionUtils.GetCurrentDomainUserAssemblies());
		UIBenchmarkEntriesParent.RemoveAllChildren();
		foreach (IBenchmark benchmark in from benchmark2 in typesCache.CreateInstancesForInterfaceImplementations<IBenchmark>()
			orderby benchmark2.Priority
			select benchmark2)
		{
			CreateEntryForBenchmark(benchmark);
		}
	}

	public override void GoBack()
	{
		Menu.SwitchToState<MenuMenuState>();
	}

	private void CreateEntryForBenchmark(IBenchmark benchmark)
	{
		BenchmarkEntryBehavior entryInstance = UnityEngine.Object.Instantiate(UIBenchmarkEntryPrefab, UIBenchmarkEntriesParent);
		entryInstance.Init(benchmark.Title, benchmark.Description, delegate
		{
			RunBenchmark(benchmark);
		});
		IBenchmarkConfiguration[] configurations = benchmark.Configurations;
		foreach (IBenchmarkConfiguration configuration in configurations)
		{
			entryInstance.AddConfiguration(configuration.Title, configuration.Settings);
		}
	}

	private void RunBenchmark(IBenchmark benchmark)
	{
		WarnAboutSettingChanges((IReadOnlyCollection<IBenchmarkConfiguration>)(object)benchmark.Configurations, OnContinue);
		void OnContinue()
		{
			BenchmarkRunner runner = new BenchmarkRunner(benchmark, OnCompleteBenchmark);
			runner.Run();
		}
	}

	private static void OnCompleteBenchmark(BenchmarkResults results)
	{
		LastSessionBenchmarkResults = results;
	}

	private void WarnAboutSettingChanges(IReadOnlyCollection<IBenchmarkConfiguration> configurations, UnityAction continueCallback)
	{
		if (configurations.Count == 0)
		{
			return;
		}
		GraphicSettings graphicsWorkingSet = Globals.Settings.Graphics;
		string diff = "";
		foreach (IBenchmarkConfiguration t in configurations)
		{
			GraphicSettings requiredGraphicSettings = t.RequiredGraphicSettings();
			if (!graphicsWorkingSet.Equals(requiredGraphicSettings))
			{
				diff = diff + "Configuration [" + t.Title + "] require changes: " + Environment.NewLine;
				diff += ComputeSettingsDiff(graphicsWorkingSet, requiredGraphicSettings);
				diff += Environment.NewLine;
				graphicsWorkingSet = requiredGraphicSettings;
			}
		}
		if (!string.IsNullOrEmpty(diff))
		{
			HUDDialogSimpleConfirm dialog = DialogStack.ShowUIDialog<HUDDialogSimpleConfirm>();
			dialog.InitDialogContents("menu.benchmarks.change-settings-confirm-dialog.title".tr(), "menu.benchmarks.change-settings-confirm-dialog.description".tr() + "\n" + diff, "global.btn-confirm".tr(), "global.btn-cancel".tr());
			dialog.OnConfirmed.AddListener(continueCallback);
		}
	}

	private static string ComputeSettingsDiff(GameSettingsGroup currentGraphicsSettings, GameSettingsGroup requiredGraphicSettings)
	{
		string diff = "";
		for (int i = 0; i < currentGraphicsSettings.Settings.Count; i++)
		{
			if (!currentGraphicsSettings.Settings[i].Equals(requiredGraphicSettings.Settings[i]))
			{
				string settingName = ("menu.setting." + currentGraphicsSettings.Settings[i].Id).tr();
				string currentState = currentGraphicsSettings.Settings[i].GetValueText();
				string requiredState = requiredGraphicSettings.Settings[i].GetValueText();
				diff = diff + settingName + ": " + currentState + " => " + requiredState + " " + Environment.NewLine;
			}
		}
		return diff;
	}
}
