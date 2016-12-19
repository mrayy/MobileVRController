using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationReceiver : MonoBehaviour {
	public ServiceManager Service;
	Quaternion rotation;
	public Vector3[] points;

	public GameObject TouchObject;

	List<GameObject> _touchObjects=new List<GameObject>();

	// Use this for initialization
	void Start () {
		Service.OnValueChanged += OnValueChanged;

		for (int i = 0; i < 5; ++i) {
			GameObject o = GameObject.Instantiate (TouchObject);
			o.transform.parent = transform;
			o.transform.localPosition = Vector3.zero;
			o.SetActive (false);
			_touchObjects.Add (o);
		}

	}

	void OnValueChanged(ServiceManager m,IServiceProvider s)
	{
		if (s.GetName () == GyroServiceProvider.ServiceName) {
			Vector3 e = (s as GyroServiceProvider).Value.eulerAngles;
			Vector3 r = e;
			r.x = -r.x;
			r.y = -e.z;
			r.z = -e.y;

			rotation = Quaternion.Euler (r);
		} else if (s.GetName ()==TouchServiceProvider.ServiceName) {
			points = (s as TouchServiceProvider).Value.ToArray();
		}
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation = rotation;


		for (int i = 0; i < points.Length;++i) {
			_touchObjects [i].SetActive (true);
			_touchObjects [i].transform.localPosition = new Vector3 (points [i].x-0.5f, 0.55f,points [i].y-0.5f);
			_touchObjects [i].transform.localScale = new Vector3(points [i].z,points [i].z,points [i].z);
		}
		for (int i = points.Length; i < _touchObjects.Count; ++i) {
			_touchObjects [i].SetActive (false);
		}
	}
}
