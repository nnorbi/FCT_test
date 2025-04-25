using Core.Dependency;
using TMPro;
using Unity.Core.View;
using UnityEngine;

public class HUDDebugStats : HUDPart, IRunnableView, IView
{
	private const string PLAYER_PREFS_KEY = "debug-stats-visible";

	[SerializeField]
	private TMP_Text UIStatsText;

	public void Run()
	{
	}

	[Construct]
	private void Construct(DebugConsole debugConsole)
	{
		base.gameObject.SetActive(PlayerPrefs.GetInt("debug-stats-visible", 0) != 0);
		debugConsole.Register("debug.toggle-debug-stats", delegate
		{
			base.gameObject.SetActiveSelfExt(!base.gameObject.activeSelf);
			PlayerPrefs.SetInt("debug-stats-visible", base.gameObject.activeSelf ? 1 : 0);
		});
	}

	protected override void OnDispose()
	{
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		string result;
		if (base.gameObject.activeSelf)
		{
			result = "";
			AddSection("Camera");
			AddStat("Zoom", drawOptions.Viewport.Zoom.ToString("N2"));
			AddStat("Position.X", drawOptions.Viewport.Position.x.ToString("N2"));
			AddStat("Position.Y", drawOptions.Viewport.Position.y.ToString("N2"));
			AddStat("Angle", drawOptions.Viewport.Angle.ToString("N2"));
			AddStat("Rotation", drawOptions.Viewport.RotationDegrees.ToString("N2"));
			AddStat("Scope", drawOptions.Viewport.Scope.ToString());
			AddStat("Primary Dir.", drawOptions.Viewport.PrimaryDirection.ToString());
			AddStat("Cursor", drawOptions.Viewport.CursorScreenPosition.ToString());
			AddStat("Layer", drawOptions.Viewport.Layer.ToString());
			AddStat("Height", drawOptions.Viewport.Height.ToString("N2"));
			UIStatsText.text = result;
		}
		void AddSection(string title)
		{
			result = result + "\n<size=22><color=#3399ff><b>" + title.ToUpper() + "</b></color></size>\n";
		}
		void AddStat(string label, string value)
		{
			result = result + "<b>" + label + "</b>: " + value + "\n";
		}
	}
}
