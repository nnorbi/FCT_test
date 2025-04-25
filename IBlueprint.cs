using System;
using System.Collections.Generic;

public interface IBlueprint : IEquatable<IBlueprint>
{
	BlueprintCurrency Cost { get; }

	int BuildingCount { get; }

	bool Mirrorable { get; }

	GameScope Scope { get; }

	IEnumerable<(MetaBuilding, int)> ComputeBuildingsByCountOrdered();

	IBlueprint GenerateRotatedVariant(Grid.Direction rotation);

	IBlueprint GenerateMirroredVariantYAxis();

	IBlueprint GenerateMirroredVariantXAxis();
}
