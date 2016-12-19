using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TouchServiceProvider : IServiceProvider {


	public const string ServiceName="Touch";

	List<Vector3> _touchPos=new List<Vector3>();
	List<byte> _data=new List<byte>();


	public List<Vector3> Value {
		get {
			return _touchPos;
		}
	}

	public TouchServiceProvider()
	{
	}


	public override string GetName()
	{
		return ServiceName;
	}
	public override bool IsReliable(){
		return true;
	}


	public override byte[] GetData(){
		return _data.ToArray ();
	}


	public override void Update()
	{
		if (!_enabled)
			return;

		_data.Clear ();
		_touchPos.Clear ();
		_data.AddRange(BitConverter.GetBytes ((int)Input.touches.Length));
		Vector2 screenInv = new Vector2 (1.0f / Screen.width, 1.0f / Screen.height);

		foreach (var t in Input.touches) {
			Vector3 p = new Vector3 (t.position.x*screenInv.x, t.position.y*screenInv.y, t.pressure/t.maximumPossiblePressure);
			_touchPos.Add (p);
			_data.AddRange (BitConverter.GetBytes (p.x));
			_data.AddRange (BitConverter.GetBytes (p.y));
			_data.AddRange (BitConverter.GetBytes (p.z));
		}
		if (OnValueChanged != null)
			OnValueChanged (this);
	}


	public override void ProcessData(byte[] data)
	{
		int idx = 0;
		int count=BitConverter.ToInt32 (data, idx);
		idx += sizeof(int);
		_touchPos.Clear ();
		Vector3 p = new Vector3 ();
		for (int i = 0; i < count; ++i) {	
			p.x = BitConverter.ToSingle (data, idx+0);
			p.y = BitConverter.ToSingle (data, idx+4);
			p.z = BitConverter.ToSingle (data, idx+8);
			_touchPos.Add (p);
			idx += 3 * sizeof(float);
		}
		if (OnValueChanged != null)
			OnValueChanged (this);
	}

	public override string GetDebugString()
	{
		return _touchPos.Count.ToString();
	}
}
