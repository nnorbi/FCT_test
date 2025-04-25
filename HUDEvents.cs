using System;
using UnityEngine;
using UnityEngine.Events;

public class HUDEvents
{
	[NonSerialized]
	public UnityEvent ShowPauseMenu = new UnityEvent();

	[NonSerialized]
	public UnityEvent ShowPauseMenuSettings = new UnityEvent();

	[NonSerialized]
	public UnityEvent ShowWiki = new UnityEvent();

	[NonSerialized]
	public UnityEvent ShowBlueprintLibrary = new UnityEvent();

	[NonSerialized]
	public UnityEvent<IBlueprint> RequestAddBlueprintToLibrary = new UnityEvent<IBlueprint>();

	[NonSerialized]
	public UnityEvent ShowResearch = new UnityEvent();

	[NonSerialized]
	public UnityEvent<IResearchableHandle> ShowResearchAndHighlight = new UnityEvent<IResearchableHandle>();

	[NonSerialized]
	public UnityEvent<ShapeDefinition> ShowShapeViewer = new UnityEvent<ShapeDefinition>();

	[NonSerialized]
	public UnityEvent<GameObject, Action<HUDDialog>> ShowDialog = new UnityEvent<GameObject, Action<HUDDialog>>();

	[NonSerialized]
	public UnityEvent<HUDNotifications.Notification> ShowNotification = new UnityEvent<HUDNotifications.Notification>();

	[NonSerialized]
	public UnityEvent ShowLoadingOverlay = new UnityEvent();

	[NonSerialized]
	public UnityEvent HUDInitialized = new UnityEvent();

	[NonSerialized]
	public UnityEvent<IResearchableHandle> ResearchCompletedByPlayer = new UnityEvent<IResearchableHandle>();

	[NonSerialized]
	public UnityEvent<HUDMainButtonConfig> RegisterMainButton = new UnityEvent<HUDMainButtonConfig>();

	[NonSerialized]
	public UnityEvent RequestBuildingMassSelectDeleteSelection = new UnityEvent();

	[NonSerialized]
	public UnityEvent RequestIslandMassSelectDeleteSelection = new UnityEvent();

	[NonSerialized]
	public UnityEvent ClearPlayerSelection = new UnityEvent();

	[NonSerialized]
	public UnityEvent StartBuildingBlueprintPlacementFromPlayerSelection = new UnityEvent();

	[NonSerialized]
	public UnityEvent StartIslandBlueprintPlacementFromPlayerSelection = new UnityEvent();
}
