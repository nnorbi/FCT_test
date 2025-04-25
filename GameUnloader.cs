using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUnloader : MonoBehaviour
{
	private void Start()
	{
		Invoke("MoveToMenu", 0.5f);
	}

	private void MoveToMenu()
	{
		DOTween.KillAll();
		AsyncOperation operation = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
	}
}
