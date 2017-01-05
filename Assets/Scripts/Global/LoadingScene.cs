using UnityEngine;
using System.Collections;

public class LoadingScene : MonoBehaviour {

	public GameObject ProgressBar;
	private AsyncOperation async;

	void Start () {
		if(GlobalVaule.loadLevelName == null){
			GlobalVaule.loadLevelName = "MainMenu";
		} else if(GlobalVaule.loadLevelName == "QUIT"){
			Application.Quit();
		}
		AsyncOperation async = Application.LoadLevelAsync(GlobalVaule.loadLevelName);
		StartCoroutine(LoadSceneProgress_1(async));
	}
	
	void Update () {
		// ProgressBar.GetComponent<UISlider>().value = async.progress;
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
		int displayProgress = 0;
		int toProgress = 0;
		
		async.allowSceneActivation = false;
		while(async.progress < 0.9f) {
			toProgress = (int)async.progress * 100;
			while(displayProgress < toProgress) {
				++displayProgress;
				SetLoadingPercentage(displayProgress);
				yield return new WaitForEndOfFrame();
			}
		}

		toProgress = 100;
		while(displayProgress < toProgress){
			++displayProgress;
			SetLoadingPercentage(displayProgress);
			yield return new WaitForEndOfFrame();
		}
		async.allowSceneActivation = true;
	}

	public void SetLoadingPercentage(float displayProgress){
		ProgressBar.GetComponent<UISlider>().value = displayProgress/100;
	}
}
