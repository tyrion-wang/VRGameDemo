using UnityEngine;
using System.Collections;

public class LoadingScene : MonoBehaviour {

	public float progressInterval = 0.01f;
	public GameObject ProgressBar;
	private AsyncOperation async;

	void Start () {
		AsyncOperation async = Application.LoadLevelAsync(LoadScenesController.getLevel());
		StartCoroutine(LoadSceneProgress_1(async));
	}
	
	void Update () {
		
	}

	IEnumerator LoadSceneProgress_1(AsyncOperation async){
		async.allowSceneActivation = false;
		while(async.progress < 0.9f) {
			SetLoadingPercentage(async.progress);
			yield return new WaitForEndOfFrame();
		}
		SetLoadingPercentage(100);
		yield return new WaitForEndOfFrame();
		async.allowSceneActivation = true; 
	}

	IEnumerator LoadSceneProgress_2(AsyncOperation async){
		float displayProgress = 0;
		float toProgress = 0;
		
		async.allowSceneActivation = false;
		while(async.progress < 0.9f) {
			toProgress = async.progress;
			while(displayProgress < toProgress) {
				displayProgress+=progressInterval;
				SetLoadingPercentage(displayProgress);
				yield return new WaitForEndOfFrame();
			}
		}

		toProgress = 1;
		while(displayProgress < toProgress){
			displayProgress+=progressInterval;
			SetLoadingPercentage(displayProgress);
			yield return new WaitForEndOfFrame();
		}
		async.allowSceneActivation = true;
	}

	public void SetLoadingPercentage(float displayProgress){
		ProgressBar.GetComponent<UISlider>().value = displayProgress;
	}
}
