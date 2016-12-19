using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationReceiver : MonoBehaviour {
	public ServiceManager Service;
	Quaternion rotation;
	// Use this for initialization
	void Start () {
		Service.OnValueChanged += OnValueChanged;
	}

	void OnValueChanged(ServiceManager m,IServiceProvider s)
	{
		if (s.GetName () == "Gyro")
			rotation= (s as GyroServiceProvider).Value;
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation = rotation;
	}
}
