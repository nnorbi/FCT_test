using System;
using Unity.Core.View;

public interface IHUDSettingsContentGroup : IView, IDisposable
{
	bool TryLeave();
}
