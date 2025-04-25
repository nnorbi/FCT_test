using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
	private void Start()
	{
		Invoke("StartGame", 1f);
	}

	private void StartGame()
	{
		DOTween.KillAll();
		AsyncOperation operation = SceneManager.LoadSceneAsync("Ingame", LoadSceneMode.Single);
	}
}
