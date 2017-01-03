using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIServerPanel : MonoBehaviour {

	public RectTransform ConfigPanel;
	public Text ConfigButton;
	public Text IPAddress;
	public ServiceManager ServiceMngr;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ToggleConfig()
	{
		ConfigPanel.gameObject.SetActive (!ConfigPanel.gameObject.activeSelf);
		ConfigButton.text = ConfigPanel.gameObject.activeSelf ? "<" : ">";
	}
	public void ConnectTo()
	{
		ServiceMngr.ConnectTo (IPAddress.text);
	}
}
