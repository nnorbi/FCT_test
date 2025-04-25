#define UNITY_ASSERTIONS
using System.Collections.Generic;
using UnityEngine;

public struct SimulationPredictionInputPredictionRange
{
	private enum InputType
	{
		NoInput,
		Degenerated,
		Shape
	}

	private InputType CurrentInputType;

	private HashSet<ShapeItem> PredictedItemsPerInput;

	public static SimulationPredictionInputPredictionRange NoPredictionInput => new SimulationPredictionInputPredictionRange
	{
		CurrentInputType = InputType.NoInput,
		PredictedItemsPerInput = null
	};

	public static SimulationPredictionInputPredictionRange FromShape(ShapeItem shapeItem)
	{
		if (shapeItem == null)
		{
			return new SimulationPredictionInputPredictionRange
			{
				CurrentInputType = InputType.Degenerated,
				PredictedItemsPerInput = null
			};
		}
		return new SimulationPredictionInputPredictionRange
		{
			CurrentInputType = InputType.Shape,
			PredictedItemsPerInput = new HashSet<ShapeItem> { shapeItem }
		};
	}

	public bool HasInput()
	{
		return CurrentInputType != InputType.NoInput;
	}

	public bool IsDegenerated()
	{
		return CurrentInputType == InputType.Degenerated;
	}

	public void AddShape(ShapeItem shapeItem)
	{
		if (shapeItem == null)
		{
			if (CurrentInputType == InputType.NoInput)
			{
				CurrentInputType = InputType.Degenerated;
			}
			return;
		}
		CurrentInputType = InputType.Shape;
		if (PredictedItemsPerInput == null)
		{
			PredictedItemsPerInput = new HashSet<ShapeItem> { shapeItem };
		}
		else
		{
			PredictedItemsPerInput.Add(shapeItem);
		}
	}

	public IReadOnlyCollection<ShapeItem> GetShapes()
	{
		Debug.Assert(CurrentInputType != InputType.NoInput, "Shape has no input");
		Debug.Assert(CurrentInputType != InputType.Degenerated, "Shape is null");
		return PredictedItemsPerInput;
	}
}
