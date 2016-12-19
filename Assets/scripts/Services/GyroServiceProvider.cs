using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GyroServiceProvider : IServiceProvider {


	Quaternion _GyroData = new Quaternion ();
	Quaternion _CalibGyro = new Quaternion ();
	List<byte> _data=new List<byte>();

	bool _enabled=true;

	public GyroServiceProvider()
	{
		Calibrate ();
	}

	void Calibrate()
	{
		_CalibGyro = Quaternion.Inverse(Input.gyro.attitude);
	}

	public string GetName()
	{
		return "Gyro";
	}

	public void SetEnabled(bool e)
	{
		_enabled = e;
	}
	public bool IsEnabled()
	{
		return _enabled;
	}
	public bool IsReliable(){
		return false;
	}


	public byte[] GetData(){
		return _data.ToArray ();
	}

	public void Update()
	{
		if (!_enabled)
			return;
		_GyroData= _CalibGyro*Input.gyro.attitude;

		_data.Clear ();
		_data.AddRange (BitConverter.GetBytes (_GyroData.x));
		_data.AddRange (BitConverter.GetBytes (_GyroData.y));
		_data.AddRange (BitConverter.GetBytes (_GyroData.z));
		_data.AddRange (BitConverter.GetBytes (_GyroData.w));
	}



	public void ProcessData(byte[] data)
	{
		
	}
}
