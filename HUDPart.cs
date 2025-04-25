using System;
using Core.Dependency;
using Core.Logging;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
[RequireComponent(typeof(Canvas))]
public abstract class HUDPart : HUDComponent
{
	public const string TOKEN_MAIN_INTERACTION = "HUDPart$main_interaction";

	public const string TOKEN_RIGHT_UPPER_SCREEN_AREA = "HUDPart$right_screen_area";

	public const string TOKEN_TOOLBAR_CONTEXT_ACTIONS = "HUDPart$context_actions";

	public const string TOKEN_RENDER_3D = "HUDPart$render_3d";

	public const string TOKEN_ADVANCE_PLAYTIME = "HUDPart$advance_playtime";

	public const string TOKEN_FULLSCREEN_OVERLAY = "HUDPart$confine_cursor";

	protected HUDEvents Events;

	protected Player Player;

	protected GraphicRaycaster Raycaster { get; private set; }

	public virtual bool ShouldInitialize => true;

	public virtual bool NeedsGraphicsRaycaster => true;

	[Construct]
	public void Construct(HUDEvents events, Player player, Core.Logging.ILogger logger)
	{
		Events = events;
		Player = player;
		GraphicRaycaster raycaster;
		if (NeedsGraphicsRaycaster)
		{
			Raycaster = base.gameObject.AddComponent<GraphicRaycaster>();
			Raycaster.blockingMask = LayerMask.GetMask("UI");
		}
		else if (base.gameObject.TryGetComponent<GraphicRaycaster>(out raycaster))
		{
			logger.Warning?.Log(GetType().Name + " has a GraphicRaycaster but NeedsGraphicsRaycaster is false. Removing it ..");
			UnityEngine.Object.Destroy(raycaster);
		}
	}

	public virtual void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		DoUpdate(context);
	}
}
