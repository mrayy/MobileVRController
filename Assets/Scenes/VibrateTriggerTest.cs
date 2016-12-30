using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibrateTriggerTest : MonoBehaviour {

	ServiceManager manager;
	// Use this for initialization
	void Start () {
		manager = GetComponent<ServiceManager> ();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space))
		{
			var s= manager.GetService (FeedbackServiceProvider.ServiceName) as FeedbackServiceProvider;
			s._force = 1;
		}
	}
}
