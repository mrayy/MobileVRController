using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AccelServiceProvider : IServiceProvider {


	public const string ServiceName="Accel";
	Vector3 _AccelData = new Vector3 ();
	List<byte> _data=new List<byte>();


	public Vector3 Value {
		get {
			return _AccelData;
		}
	}
	public AccelServiceProvider()
	{
	}


	public override string GetName()
	{
		return ServiceName;
	}


	public override bool IsReliable(){
		return false;
	}


	public override byte[] GetData(){
		return _data.ToArray ();
	}

	public override void Update()
	{
		if (!_enabled)
			return;
		_AccelData= Input.acceleration;

		_data.Clear ();
		_data.AddRange (BitConverter.GetBytes (_AccelData.x));
		_data.AddRange (BitConverter.GetBytes (_AccelData.y));
		_data.AddRange (BitConverter.GetBytes (_AccelData.z));

		if (OnValueChanged != null)
			OnValueChanged (this);
	}



	public override void ProcessData(byte[] data)
	{
		_AccelData.x=BitConverter.ToSingle (data, 0);
		_AccelData.y=BitConverter.ToSingle (data, 4);
		_AccelData.z=BitConverter.ToSingle (data, 8);
		if (OnValueChanged != null)
			OnValueChanged (this);
	}

	public override string GetDebugString()
	{
		return _AccelData.ToString ();
	}
}
