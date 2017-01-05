using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class MenuList : MonoBehaviour {

	ButtonInfo buttonInfo = new ButtonInfo();
	void Start () {
		buttonInfo.set("TeleporterTest", "TeleporterTest", "TeleporterTest");
		buttonInfo.set("VehicleScene", "VehicleScene", "VehicleScene");
		buttonInfo.set("Quit", "QUIT", "QUIT");

		List<string> btns = buttonInfo.getButtonList();
		int index = 0;
		foreach(string btnName in btns)
		{
			GameObject button = (GameObject)Instantiate(Resources.Load("Prefabs/MenuButton"));   
			button.transform.parent = transform; 
			button.transform.position = transform.position;
			button.transform.rotation = transform.rotation;
			button.transform.localScale = transform.localScale;
			button.transform.localPosition = new Vector3(0,-100 * index,0);
			button.name = btnName;
			button.GetComponent<MenuButton>().label = buttonInfo.getLabelText(btnName);
			UIEventListener.Get(button).onClick = OnClick;

			index++;
		} 
	}

	public void OnClick(GameObject button)
	{
		GlobalVaule.loadLevelName = buttonInfo.getLoadSceneName(button.name);
		Application.LoadLevel("LoadingScene");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private class ButtonInfo{
		public string objName;
		public string label;
		public string loadSceneName;
		List<string> buttonList = new List<string>();
		Dictionary<string, string> labelTextList = new Dictionary<string, string>();
		Dictionary<string, string> loadSceneNameList = new Dictionary<string, string>();
		public void set(string objName, string labelText, string loadSceneName){
			buttonList.Add(objName);
			labelTextList.Add(objName, labelText);
			loadSceneNameList.Add(objName, loadSceneName);
		}

		public string getLabelText(string objName){
			string label = "";
			labelTextList.TryGetValue(objName,out label);
			return label;
		}

		public string getLoadSceneName(string objName){
			string sceneName = "";
			loadSceneNameList.TryGetValue(objName,out sceneName);
			return sceneName;
		}

		public List<string> getButtonList(){
			return buttonList;
		}
	}

}
