using System;
using System.Globalization;
using Core.Dependency;
using TMPro;
using UnityEngine;

public class ModeConfigMenuState : MainMenuState
{
	[SerializeField]
	private HUDMenuBackButton UIBtnBack;

	[SerializeField]
	private HUDButton UIBtnStart;

	[SerializeField]
	private TMP_Text UIModeTitle;

	[SerializeField]
	private HUDInputField UISeedInput;

	[SerializeField]
	private HUDIconButton UIBtnGenerateSeed;

	private MetaGameMode Mode;

	[Construct]
	private void Construct()
	{
		AddChildView(UIBtnBack);
		AddChildView(UIBtnStart);
		AddChildView(UISeedInput);
		AddChildView(UIBtnGenerateSeed);
		UIBtnBack.Clicked.AddListener(GoBack);
		UIBtnStart.Clicked.AddListener(StartNewGame);
		UIBtnGenerateSeed.Clicked.AddListener(GenerateNewSeed);
	}

	public override void OnMenuEnterState(object payload)
	{
		if (!(payload is MetaGameMode mode))
		{
			throw new Exception("No game mode passed");
		}
		Mode = mode;
		UIModeTitle.text = mode.Title;
		GenerateNewSeed();
	}

	protected override void OnDispose()
	{
		UIBtnBack.Clicked.RemoveListener(GoBack);
		UIBtnStart.Clicked.RemoveListener(StartNewGame);
		UIBtnGenerateSeed.Clicked.RemoveListener(GenerateNewSeed);
	}

	private void GenerateNewSeed()
	{
		UISeedInput.Value = RandomUtils.NextInt(100000).ToString(CultureInfo.InvariantCulture);
	}

	private void StartNewGame()
	{
		if (!(Mode == null))
		{
			string seedText = UISeedInput.Value;
			if (!int.TryParse(seedText, out var seed))
			{
				Debug.LogWarning("Invalid seed.");
				GenerateNewSeed();
			}
			else
			{
				Debug.Log("Start game of mode " + Mode.Title + " with seed " + seed);
				GameModeConfig config = new GameModeConfig(Mode, seed);
				Menu.StartNewGame(config);
			}
		}
	}

	public override void GoBack()
	{
		Mode = null;
		Menu.SwitchToState<SelectModeMenuState>();
	}
}
