using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class GyroServiceProvider : IServiceProvider {


	Quaternion _GyroData = new Quaternion ();
	Quaternion _CalibGyro = new Quaternion ();
	List<byte> _data=new List<byte>();


	public Quaternion Value {
		get {
			return _GyroData;
		}
	}

	public GyroServiceProvider()
	{
		//Calibrate ();
	}

	void Calibrate()
	{
		_CalibGyro = Quaternion.Inverse(Input.gyro.attitude);
	}

	public override string GetName()
	{
		return "Gyro";
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
		_GyroData= Input.gyro.attitude;

		_data.Clear ();

		_data.AddRange (BitConverter.GetBytes (_GyroData.x));
		_data.AddRange (BitConverter.GetBytes (_GyroData.y));
		_data.AddRange (BitConverter.GetBytes (_GyroData.z));
		_data.AddRange (BitConverter.GetBytes (_GyroData.w));

		if (OnValueChanged != null)
			OnValueChanged (this);
	}



	public override void ProcessData(byte[] data)
	{
		_GyroData.x=BitConverter.ToSingle (data, 0);
		_GyroData.y=BitConverter.ToSingle (data, 4);
		_GyroData.z=BitConverter.ToSingle (data, 8);
		_GyroData.w=BitConverter.ToSingle (data, 12);
		if (OnValueChanged != null)
			OnValueChanged (this);
	}

	public override string GetDebugString()
	{
		return _GyroData.eulerAngles.ToString ();
	}
}
