using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Building Variant", menuName = "Metadata/Building/2 - Building Variant", order = 2)]
public class MetaBuildingVariant : ScriptableObject, IResearchUnlock, IEquatable<MetaBuildingVariant>
{
	[HideInInspector]
	public MetaBuilding Building;

	[SerializeField]
	[FormerlySerializedAs("Icon")]
	private Sprite _Icon;

	public bool Removable = true;

	public bool Selectable = true;

	public bool PlayerBuildable = true;

	public bool AllowPlaceOnNonFilledTiles = false;

	public bool AutoConnectBelts = true;

	public MetaBuildingVariant PipetteOverride = null;

	public bool IsBeltTransportBuilding = false;

	public bool AllowPlaceOnNotch = false;

	public bool AllowNonForcingReplacementByOtherBuildings = true;

	public bool ShouldSkipReplacementIOChecks = false;

	public bool ShowInToolbar = true;

	public EditorClassID<BuildingPlacementBehaviour> PlacementBehaviour = new EditorClassID<BuildingPlacementBehaviour>("RegularBuildingPlacementBehaviour");

	[Space(20f)]
	public EditorClassIDSingleton<IBuildingPlacementIndicator>[] PlacementIndicators = new EditorClassIDSingleton<IBuildingPlacementIndicator>[0];

	[Space(20f)]
	public EditorClassIDSingleton<IPlacementRequirement>[] PlacementRequirements = new EditorClassIDSingleton<IPlacementRequirement>[0];

	public MetaBuildingInternalVariant[] InternalVariants;

	public string Description => ("building-variant." + base.name + ".description").tr();

	public Sprite Icon => _Icon;

	public string Title => ("building-variant." + base.name + ".title").tr();

	private void OnValidate()
	{
		if (InternalVariants.Length == 0)
		{
			throw new Exception("Variant " + base.name + " has no internal variants!");
		}
		PlacementBehaviour.Validate();
		EditorClassIDSingleton<IPlacementRequirement>[] placementRequirements = PlacementRequirements;
		foreach (EditorClassIDSingleton<IPlacementRequirement> requirement in placementRequirements)
		{
			requirement.Validate();
		}
	}

	public bool Equals(MetaBuildingVariant other)
	{
		return other == this;
	}

	public void Init(MetaBuilding building)
	{
		Building = building;
		MetaBuildingInternalVariant[] internalVariants = InternalVariants;
		foreach (MetaBuildingInternalVariant internalVariant in internalVariants)
		{
			internalVariant.Init(this);
		}
	}
}
