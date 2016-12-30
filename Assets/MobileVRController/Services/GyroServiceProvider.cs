using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class GyroServiceProvider : IServiceProvider {

	public const string ServiceName="Gyro";
	Quaternion _GyroData = new Quaternion ();
	Quaternion _CalibGyro = new Quaternion ();
	List<byte> _data=new List<byte>();


	public Quaternion Value {
		get {
			return _GyroData;
		}
	}

	public GyroServiceProvider (ServiceManager m):base(m)
	{
		Input.gyro.enabled = true;
		Calibrate ();
	}

	public void Calibrate()
	{
		_CalibGyro = Quaternion.AngleAxis(-Input.gyro.attitude.eulerAngles.z,Vector3.forward);
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
		if (!_enabled || _mngr.IsReceiver)
			return;
		_GyroData= _CalibGyro*Input.gyro.attitude;

		_data.Clear ();

		_data.AddRange (BitConverter.GetBytes (_GyroData.x));
		_data.AddRange (BitConverter.GetBytes (_GyroData.y));
		_data.AddRange (BitConverter.GetBytes (_GyroData.z));
		_data.AddRange (BitConverter.GetBytes (_GyroData.w));

		//if (OnValueChanged != null)
		//	OnValueChanged (this);
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
