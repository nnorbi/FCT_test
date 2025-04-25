using System;
using Core.Dependency;
using Core.Disposing;
using Unity.Core.View;
using UnityEngine;

public class HUD : IDisposable
{
	private readonly PrefabViewInstanceConstructor ViewConstructor;

	private readonly DependencyContainer DependencyContainer;

	private readonly Transform Root;

	private readonly DisposableList<HUDPart> Parts = new DisposableList<HUDPart>();

	[NonSerialized]
	public HUDEvents Events = new HUDEvents();

	public HUDDialogStack DialogStack { get; }

	public HUD(IDependencyResolver dependencyResolver, Transform root)
	{
		DialogStack = new HUDDialogStack(Events.ShowDialog);
		DependencyContainer = dependencyResolver.CreateChildContainer();
		DependencyContainer.Bind<HUDEvents>().To(Events);
		DependencyContainer.Bind<IHUDDialogStack>().To(DialogStack);
		ViewConstructor = DependencyContainer.Create<PrefabViewInstanceConstructor>();
		Root = root;
	}

	public void Dispose()
	{
		foreach (HUDPart hudPart in Parts)
		{
			ViewConstructor.ReleaseView(hudPart);
		}
		Parts.Clear();
		ViewConstructor.Dispose();
		DependencyContainer.Dispose();
		DialogStack.Dispose();
	}

	public void Initialize(IHUDConfiguration configuration)
	{
		int index = -1;
		foreach (PrefabViewReference<HUDPart> partReference in configuration.Parts)
		{
			index++;
			HUDPart part = ViewConstructor.RequestView(partReference).PlaceAt(Root);
			if (!part.ShouldInitialize)
			{
				ViewConstructor.ReleaseView(part);
				continue;
			}
			Parts.Add(part);
			if (!part.gameObject.TryGetComponent<Canvas>(out var canvas))
			{
				throw new Exception("Invalid hud part.");
			}
			canvas.pixelPerfect = false;
			canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.TexCoord3;
			Transform t = part.transform;
			t.SetAsFirstSibling();
			t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, 10 + index * 200);
		}
		Events.HUDInitialized.Invoke();
	}

	public void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		foreach (HUDPart part in Parts)
		{
			part.OnGameUpdate(context, drawOptions);
		}
	}
}
