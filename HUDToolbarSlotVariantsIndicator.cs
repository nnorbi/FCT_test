using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using UnityEngine;
using UnityEngine.UI;

public class HUDToolbarSlotVariantsIndicator : HUDComponent
{
	[SerializeField]
	private RectTransform UIItemsParent;

	[SerializeField]
	private Sprite UISpriteNormal;

	[SerializeField]
	private Sprite UISpriteLocked;

	private readonly List<Image> Images = new List<Image>();

	private MetaBuilding _building;

	private ResearchManager ResearchManager;

	public MetaBuilding Building
	{
		get
		{
			return _building;
		}
		set
		{
			if (!(value == _building))
			{
				_building = value;
				UpdateImages();
				UpdateState();
			}
		}
	}

	private List<MetaBuildingVariant> Variants
	{
		get
		{
			if (Building == null)
			{
				return new List<MetaBuildingVariant>();
			}
			return Building.Variants.Where((MetaBuildingVariant v) => v.PlayerBuildable && v.ShowInToolbar).ToList();
		}
	}

	[Construct]
	private void Construct(ResearchManager researchManager)
	{
		ResearchManager = researchManager;
		ResearchManager.Progress.OnChanged.AddListener(UpdateState);
	}

	protected override void OnDispose()
	{
		ResearchManager.Progress.OnChanged.RemoveListener(UpdateState);
		foreach (Image image in Images)
		{
			Object.Destroy(image.gameObject);
		}
		Images.Clear();
	}

	private void UpdateImages()
	{
		List<MetaBuildingVariant> variants = Variants;
		while (Images.Count < variants.Count)
		{
			GameObject obj = new GameObject(string.Empty, typeof(Image));
			RectTransform rect = obj.GetComponent<RectTransform>();
			rect.SetParent(UIItemsParent, worldPositionStays: false);
			rect.localPosition = new Vector3(0f, 0f, 0f);
			rect.localScale = new Vector3(1f, 1f, 1f);
			rect.sizeDelta = new Vector2(8f, 8f);
			Image image = obj.GetComponent<Image>();
			image.material = Globals.Resources.DefaultUISpriteMaterial;
			Images.Add(image);
		}
		while (Images.Count > variants.Count)
		{
			List<Image> images = Images;
			Object.Destroy(images[images.Count - 1].gameObject);
			Images.RemoveAt(Images.Count - 1);
		}
	}

	private void UpdateState()
	{
		List<MetaBuildingVariant> variants = Variants;
		for (int i = 0; i < variants.Count; i++)
		{
			MetaBuildingVariant variant = variants[i];
			Image image = Images[i];
			bool unlocked = ResearchManager.Progress.IsUnlocked(variant);
			image.sprite = (unlocked ? UISpriteNormal : UISpriteLocked);
		}
	}
}
