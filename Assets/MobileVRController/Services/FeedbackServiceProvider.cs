using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FeedbackServiceProvider : IServiceProvider {


	public const string ServiceName="Feedback";

	List<byte> _data=new List<byte>();
	public float _force=0;

	float _lastForace=0;


	public float Value {
		get {
			return _force;
		}
	}
	public FeedbackServiceProvider (ServiceManager m):base(m)
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

		if (!_mngr.IsReceiver) {
			if (_force > 0) {
				Handheld.Vibrate ();
				_force = 0;
			}
			return;
		}

		_data.Clear ();
		if (_force == _lastForace)
			return;
		_lastForace = _force;
		_data.AddRange (BitConverter.GetBytes (_force));
		_force = 0;
	}



	public override void ProcessData(byte[] data)
	{
		_force=BitConverter.ToSingle (data, 0);

		if (_force > 0) {
			Debug.Log ("Vibrate");
		}

		if (OnValueChanged != null)
			OnValueChanged (this);
	}

	public override string GetDebugString()
	{
		return _force.ToString ();
	}
}
