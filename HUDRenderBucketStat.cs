using Core.Dependency;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDRenderBucketStat : HUDComponent
{
	[Space(10f)]
	[Header("Misc")]
	[SerializeField]
	private HUDIconButton UIToggleButton;

	[SerializeField]
	private TMP_Text UITitleText;

	[Space(10f)]
	[Header("Instanced draw calls")]
	[SerializeField]
	private TMP_Text UIStatInstancedDrawCalls;

	[SerializeField]
	private Image UIStatInstancedDrawCallsImage;

	[Space(10f)]
	[Header("Instanced instances")]
	[SerializeField]
	private TMP_Text UIStatInstancedObjectsCount;

	[SerializeField]
	private Image UIStatInstancedObjectsCountImage;

	[Space(10f)]
	[Header("Normal draw calls")]
	[SerializeField]
	private TMP_Text UIStatNormalDrawCalls;

	[SerializeField]
	private Image UIStatNormalDrawCallsImage;

	[Space(10f)]
	[Header("Normal Tris")]
	[SerializeField]
	private TMP_Text UIStatNormalTriangles;

	[SerializeField]
	private Image UIStatNormalTrianglesImage;

	[Space(10f)]
	[Header("Shadow Tris")]
	[SerializeField]
	private TMP_Text UIStatShadowTriangles;

	[SerializeField]
	private Image UIStatShadowTrianglesImage;

	private RenderCategoryBucket _Bucket;

	public RenderCategoryBucket Bucket
	{
		set
		{
			if (_Bucket != value)
			{
				_Bucket = value;
				UpdateTitle();
			}
		}
	}

	[Construct]
	public void Construct()
	{
		AddChildView(UIToggleButton);
		UIToggleButton.Clicked.AddListener(ToggleCategory);
	}

	protected override void OnDispose()
	{
		UIToggleButton.Clicked.RemoveListener(ToggleCategory);
	}

	private void ToggleCategory()
	{
		_Bucket.RenderingEnabled = !_Bucket.RenderingEnabled;
		UIToggleButton.Active = !_Bucket.RenderingEnabled;
	}

	private void UpdateTitle()
	{
		UITitleText.text = _Bucket.Category.ToString();
	}

	private void ApplyStat(TMP_Text text, Image image, int value, int warningThreshold, int errorThreshold)
	{
		if (value == 0)
		{
			Color color = (image.color = new Color(1f, 1f, 1f, 0.01f));
			text.color = color;
			text.text = "-";
			return;
		}
		text.text = StringFormatting.FormatIntegerMax4Digits(value);
		if (value < 5)
		{
			Color color = (image.color = new Color(1f, 1f, 1f, 0.1f));
			text.color = color;
		}
		else if (value < warningThreshold)
		{
			Color color = (image.color = new Color(1f, 1f, 1f, 0.4f));
			text.color = color;
		}
		else if (value < errorThreshold)
		{
			Color color = (image.color = new Color(1f, 0.7f, 0.3f, 1f));
			text.color = color;
		}
		else
		{
			Color color = (image.color = new Color(1f, 0f, 0.1f, 1f));
			text.color = color;
		}
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
		if (_Bucket != null)
		{
			ApplyStat(UIStatInstancedDrawCalls, UIStatInstancedDrawCallsImage, _Bucket.InstancedDrawCalls, 50, 200);
			ApplyStat(UIStatInstancedObjectsCount, UIStatInstancedObjectsCountImage, _Bucket.InstancedObjectsCount, 200, 1000);
			ApplyStat(UIStatNormalDrawCalls, UIStatNormalDrawCallsImage, _Bucket.RegularDrawCalls, 10, 50);
			ApplyStat(UIStatNormalTriangles, UIStatNormalTrianglesImage, _Bucket.TrianglesRenderedNoShadow, 200000, 1000000);
			ApplyStat(UIStatShadowTriangles, UIStatShadowTrianglesImage, _Bucket.TrianglesRenderedShadow, 100000, 200000);
			_Bucket.Reset();
		}
	}
}
