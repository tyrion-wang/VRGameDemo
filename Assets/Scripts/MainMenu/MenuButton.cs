using UnityEngine;
using System.Collections;

public class MenuButton : MonoBehaviour {

	public string label = "";
	// Use this for initialization
	void Start () {
		transform.FindChild("ButtonAnimation").FindChild("Label").GetComponent<UILabel>().text = label;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
