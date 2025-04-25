using UnityEngine;

public class DontDestroyOnLoadBehaviour : MonoBehaviour
{
	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
