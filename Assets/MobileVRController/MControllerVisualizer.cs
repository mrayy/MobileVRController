using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MControllerVisualizer : MonoBehaviour {
	public ServiceManager Service;
	public Vector3 Rotation;
	Quaternion _rawRotation;
	public Vector3[] Points;
	public SwipeServiceProvider.ESwipeType Swipe;
	public Vector3 Acceleration;

	public GameObject TouchObject;
	public GameObject ScreenSurface;

	LineRenderer AccelerationRenderer;

	List<GameObject> _touchObjects=new List<GameObject>();

	// Use this for initialization
	void Start () {
		Service.OnValueChanged += OnValueChanged;

		for (int i = 0; i < 5; ++i) {
			GameObject o = GameObject.Instantiate (TouchObject);
			o.transform.parent = ScreenSurface.transform;
			o.transform.localPosition = Vector3.zero;
			o.SetActive (false);
			_touchObjects.Add (o);
		}

		AccelerationRenderer = gameObject.AddComponent<LineRenderer> ();
		AccelerationRenderer.useWorldSpace = false;
		AccelerationRenderer.startWidth = 0.02f;
		AccelerationRenderer.endWidth = 0.02f;
		AccelerationRenderer.SetPosition (0, new Vector3 (0, 0, 0));
		AccelerationRenderer.SetPosition(1,new Vector3 (0, 0, 0));
	}

	void OnValueChanged(ServiceManager m,IServiceProvider s)
	{
		if (s.GetName () == GyroServiceProvider.ServiceName) {
			_rawRotation = (s as GyroServiceProvider).Value;
			Vector3 e = _rawRotation.eulerAngles;
			Rotation.x = -e.x;
			Rotation.y = -e.z;
			Rotation.z = -e.y;

			_rawRotation.x = -(s as GyroServiceProvider).Value.x;
			_rawRotation.y = -(s as GyroServiceProvider).Value.z;
			_rawRotation.z = -(s as GyroServiceProvider).Value.y;
		} else if (s.GetName ()==TouchServiceProvider.ServiceName) {
			Points = (s as TouchServiceProvider).Value.ToArray();

		} else if (s.GetName ()==SwipeServiceProvider.ServiceName) {
			Swipe = (s as SwipeServiceProvider).Value;

		} else if (s.GetName ()==AccelServiceProvider.ServiceName) {
			Acceleration.x = (s as AccelServiceProvider).Value.x;
			Acceleration.y = (s as AccelServiceProvider).Value.z;
			Acceleration.z = (s as AccelServiceProvider).Value.y;
		}
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation = _rawRotation;// Quaternion.Euler(Rotation);

		Vector3 pos=transform.position;
		//AccelerationRenderer.SetPosition (0, );
		AccelerationRenderer.SetPosition(1,Acceleration*2);

		for (int i = 0; i < Points.Length;++i) {
			_touchObjects [i].SetActive (true);
			_touchObjects [i].transform.localPosition = new Vector3 (Points [i].x-0.5f, 0.55f,Points [i].y-0.5f);
			_touchObjects [i].transform.localScale = new Vector3(Points [i].z,Points [i].z,Points [i].z);
		}
		for (int i = Points.Length; i < _touchObjects.Count; ++i) {
			_touchObjects [i].SetActive (false);
		}
	}
}
