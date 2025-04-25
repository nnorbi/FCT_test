public static class PathBuildingAutoReplacements
{
	public static PathBuildingAutoReplacement[] Belts = new PathBuildingAutoReplacement[18]
	{
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultForwardInternalVariant",
			IfInputs = new Grid.Direction[1] { Grid.Direction.Bottom },
			ThenInternalVariantName = "Splitter1To2RInternalVariant"
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultForwardInternalVariant",
			IfInputs = new Grid.Direction[1] { Grid.Direction.Top },
			ThenInternalVariantName = "Splitter1To2LInternalVariant"
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultLeftInternalVariant",
			IfInputs = new Grid.Direction[1],
			ThenInternalVariantName = "Splitter1To2LInternalVariant"
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultRightInternalVariant",
			IfInputs = new Grid.Direction[1],
			ThenInternalVariantName = "Splitter1To2RInternalVariant"
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultLeftInternalVariant",
			IfInputs = new Grid.Direction[1] { Grid.Direction.Bottom },
			ThenInternalVariantName = "SplitterTShapeInternalVariant"
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultRightInternalVariant",
			IfInputs = new Grid.Direction[1] { Grid.Direction.Top },
			ThenInternalVariantName = "SplitterTShapeInternalVariant"
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultForwardInternalVariant",
			IfOutputs = new Grid.Direction[2]
			{
				Grid.Direction.Top,
				Grid.Direction.Bottom
			},
			ThenInternalVariantName = "Merger3To1InternalVariant"
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultRightInternalVariant",
			IfOutputs = new Grid.Direction[2]
			{
				Grid.Direction.Top,
				Grid.Direction.Right
			},
			ThenInternalVariantName = "Merger3To1InternalVariant",
			ThenRotateDirection = Grid.Direction.Bottom
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultLeftInternalVariant",
			IfOutputs = new Grid.Direction[2]
			{
				Grid.Direction.Bottom,
				Grid.Direction.Right
			},
			ThenInternalVariantName = "Merger3To1InternalVariant",
			ThenRotateDirection = Grid.Direction.Top
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultForwardInternalVariant",
			IfOutputs = new Grid.Direction[1] { Grid.Direction.Bottom },
			ThenInternalVariantName = "Merger2To1RInternalVariant"
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultForwardInternalVariant",
			IfOutputs = new Grid.Direction[1] { Grid.Direction.Top },
			ThenInternalVariantName = "Merger2To1LInternalVariant"
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultLeftInternalVariant",
			IfOutputs = new Grid.Direction[1] { Grid.Direction.Bottom },
			ThenInternalVariantName = "Merger2To1LInternalVariant",
			ThenRotateDirection = Grid.Direction.Top
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultRightInternalVariant",
			IfOutputs = new Grid.Direction[1] { Grid.Direction.Top },
			ThenInternalVariantName = "Merger2To1RInternalVariant",
			ThenRotateDirection = Grid.Direction.Bottom
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultLeftInternalVariant",
			IfOutputs = new Grid.Direction[1],
			ThenInternalVariantName = "MergerTShapeInternalVariant",
			ThenRotateDirection = Grid.Direction.Top
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "BeltDefaultRightInternalVariant",
			IfOutputs = new Grid.Direction[1],
			ThenInternalVariantName = "MergerTShapeInternalVariant",
			ThenRotateDirection = Grid.Direction.Bottom
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "Merger2To1LInternalVariant",
			IfOutputs = new Grid.Direction[1] { Grid.Direction.Bottom },
			ThenInternalVariantName = "Merger3To1InternalVariant"
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "Merger2To1RInternalVariant",
			IfOutputs = new Grid.Direction[1] { Grid.Direction.Top },
			ThenInternalVariantName = "Merger3To1InternalVariant"
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "MergerTShapeInternalVariant",
			IfOutputs = new Grid.Direction[1] { Grid.Direction.Left },
			ThenInternalVariantName = "Merger3To1InternalVariant"
		}
	};

	public static PathBuildingAutoReplacement[] Pipes = new PathBuildingAutoReplacement[9]
	{
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "PipeForwardInternalVariant",
			IfInputs = new Grid.Direction[2]
			{
				Grid.Direction.Bottom,
				Grid.Direction.Top
			},
			ThenInternalVariantName = "PipeCrossInternalVariant"
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "PipeRightInternalVariant",
			IfInputs = new Grid.Direction[2]
			{
				Grid.Direction.Right,
				Grid.Direction.Top
			},
			ThenInternalVariantName = "PipeCrossInternalVariant"
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "PipeLeftInternalVariant",
			IfInputs = new Grid.Direction[2]
			{
				Grid.Direction.Right,
				Grid.Direction.Bottom
			},
			ThenInternalVariantName = "PipeCrossInternalVariant"
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "PipeForwardInternalVariant",
			IfInputs = new Grid.Direction[1] { Grid.Direction.Bottom },
			ThenInternalVariantName = "PipeJunctionInternalVariant",
			ThenRotateDirection = Grid.Direction.Bottom
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "PipeForwardInternalVariant",
			IfInputs = new Grid.Direction[1] { Grid.Direction.Top },
			ThenInternalVariantName = "PipeJunctionInternalVariant",
			ThenRotateDirection = Grid.Direction.Top
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "PipeRightInternalVariant",
			IfInputs = new Grid.Direction[1],
			ThenInternalVariantName = "PipeJunctionInternalVariant",
			ThenRotateDirection = Grid.Direction.Bottom
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "PipeRightInternalVariant",
			IfInputs = new Grid.Direction[1] { Grid.Direction.Top },
			ThenInternalVariantName = "PipeJunctionInternalVariant",
			ThenRotateDirection = Grid.Direction.Left
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "PipeLeftInternalVariant",
			IfInputs = new Grid.Direction[1],
			ThenInternalVariantName = "PipeJunctionInternalVariant",
			ThenRotateDirection = Grid.Direction.Top
		},
		new PathBuildingAutoReplacement
		{
			IfInternalVariantName = "PipeLeftInternalVariant",
			IfInputs = new Grid.Direction[1] { Grid.Direction.Bottom },
			ThenInternalVariantName = "PipeJunctionInternalVariant",
			ThenRotateDirection = Grid.Direction.Left
		}
	};
}
