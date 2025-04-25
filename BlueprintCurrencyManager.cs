using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BlueprintCurrencyManager
{
	public class SerializedData
	{
		public BlueprintCurrency BlueprintCurrency;
	}

	public UnityEvent<BlueprintCurrency> BlueprintCurrencyChanged = new UnityEvent<BlueprintCurrency>();

	private ResearchShapeStorage ResearchShapeStorage;

	private Dictionary<string, ShapesStoredDelegate> ShapeStoredHooksByShape = new Dictionary<string, ShapesStoredDelegate>();

	private GameModeHandle Mode;

	public BlueprintCurrency BlueprintCurrency { get; protected set; } = Application.isEditor ? BlueprintCurrency.FromMain(1000000.0) : BlueprintCurrency.Zero;

	public BlueprintCurrencyManager(GameModeHandle mode)
	{
		Mode = mode;
	}

	public void Initialize(ResearchShapeStorage researchShapeStorage)
	{
		ResearchShapeStorage = researchShapeStorage;
		UpdateResearchRegistrations();
		Singleton<GameCore>.G.Research.Progress.OnChanged.AddListener(UpdateResearchRegistrations);
		foreach (GameModeBlueprintCurrencyShape blueprintCurrencyShape in Mode.ResearchConfig.BlueprintCurrencyShapes)
		{
			researchShapeStorage.AddShapeStoredHook(blueprintCurrencyShape.Shape, delegate(int shapesAmount)
			{
				AddBlueprintCurrency(shapesAmount * blueprintCurrencyShape.Amount);
			});
		}
	}

	private void UpdateResearchRegistrations()
	{
		foreach (var (shape, onShapesStoredDelegate) in ShapeStoredHooksByShape)
		{
			ResearchShapeStorage.RemoveShapeStoredHook(shape, onShapesStoredDelegate);
		}
		ShapeStoredHooksByShape.Clear();
		ResearchLevelHandle[] levels = Singleton<GameCore>.G.Mode.ResearchTree.Levels;
		foreach (ResearchLevelHandle researchLevel in levels)
		{
			if (!Singleton<GameCore>.G.Research.Progress.IsUnlocked(researchLevel))
			{
				continue;
			}
			if (researchLevel.Cost.Type != ResearchUnlockCost.CostType.Free && researchLevel.Cost.CurrencyValue > BlueprintCurrency.Zero)
			{
				RegisterForShapesStored(researchLevel.Cost);
			}
			foreach (ResearchSideGoalHandle sideGoal in researchLevel.SideGoals)
			{
				if (sideGoal.Cost.Type != ResearchUnlockCost.CostType.Free && sideGoal.Cost.CurrencyValue > BlueprintCurrency.Zero)
				{
					RegisterForShapesStored(sideGoal.Cost);
				}
			}
		}
	}

	private void RegisterForShapesStored(ResearchUnlockCost cost)
	{
		ResearchShapeStorage.AddShapeStoredHook(cost.DefinitionHash, OnShapesStored);
		StoreDelegate(cost.DefinitionHash, OnShapesStored);
		void OnShapesStored(int shapesAmount)
		{
			AddBlueprintCurrency(shapesAmount * cost.CurrencyValue);
		}
		void StoreDelegate(string shape, ShapesStoredDelegate newDelegate)
		{
			if (ShapeStoredHooksByShape.ContainsKey(shape))
			{
				Dictionary<string, ShapesStoredDelegate> shapeStoredHooksByShape = ShapeStoredHooksByShape;
				shapeStoredHooksByShape[shape] = (ShapesStoredDelegate)Delegate.Combine(shapeStoredHooksByShape[shape], newDelegate);
			}
			else
			{
				ShapeStoredHooksByShape.Add(shape, newDelegate);
			}
		}
	}

	public void InitializeExisting(ResearchShapeStorage researchShapeStorage, SerializedData data)
	{
		Initialize(researchShapeStorage);
		BlueprintCurrency = data.BlueprintCurrency;
	}

	public SerializedData Serialize()
	{
		return new SerializedData
		{
			BlueprintCurrency = BlueprintCurrency
		};
	}

	public void SetBlueprintCurrency(BlueprintCurrency amount)
	{
		if (!(BlueprintCurrency == amount))
		{
			BlueprintCurrency = amount;
			BlueprintCurrencyChanged.Invoke(amount);
		}
	}

	public void AddBlueprintCurrency(BlueprintCurrency amount)
	{
		if (amount < BlueprintCurrency.Zero)
		{
			throw new ArgumentException($"Cannot add negative blueprint currency ({amount}).", "amount");
		}
		SetBlueprintCurrency(BlueprintCurrency + amount);
	}

	public bool TryTakeBlueprintCurrency(BlueprintCurrency amount)
	{
		if (amount < BlueprintCurrency.Zero)
		{
			throw new ArgumentException($"Cannot take negative blueprint currency ({amount}).", "amount");
		}
		if (BlueprintCurrency >= amount)
		{
			SetBlueprintCurrency(BlueprintCurrency - amount);
			return true;
		}
		return false;
	}

	public bool CanAfford(BlueprintCurrency amount)
	{
		if (amount == BlueprintCurrency.Zero)
		{
			return true;
		}
		return BlueprintCurrency >= amount;
	}

	public void RegisterCommands(DebugConsole console)
	{
		console.Register("blueprints.set-currency", new DebugConsole.LongOption("amount", 0L), delegate(DebugConsole.CommandContext ctx)
		{
			long num = ctx.GetLong(0);
			ctx.Output("Updated blueprint currency to " + num);
			SetBlueprintCurrency(BlueprintCurrency.FromMain(num));
		}, isCheat: true);
	}
}
