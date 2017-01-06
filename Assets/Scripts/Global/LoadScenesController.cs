using UnityEngine;
using System.Collections;

public class LoadScenesController {
	private static Stack sceneStack = new Stack();
	public static string loadLevelName = null;
	public static void go(string nextLevelName){
		if(nextLevelName == "QUIT"){
			Application.Quit();
			return;
		}
		loadLevelName = nextLevelName;
		sceneStack.Push(Application.loadedLevelName);
		Application.LoadLevel("LoadingScene");
	}

	public static void back(){
		if(sceneStack.Count == 0){
			Application.Quit();
			return;
		}
		loadLevelName = (string)sceneStack.Pop();
		Application.LoadLevel("LoadingScene");
	}

	public static string getLevel(){
		if(loadLevelName == null){
			loadLevelName = "MainMenu";
		}
		return loadLevelName;
	}
}
