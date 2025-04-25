using UnityEngine;

public class DestroyOutsideOfEditor : MonoBehaviour
{
	private void Start()
	{
		if (!Application.isEditor)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
