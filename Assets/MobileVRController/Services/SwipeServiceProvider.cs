using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SwipeServiceProvider : IServiceProvider {

	public const string ServiceName="Swipe";

	public enum ESwipeType
	{
		None,
		Left,
		Right,
		Top,
		Bottom,
	}

	ESwipeType _swipe=ESwipeType.None;
	List<byte> _data=new List<byte>();

	float threshold=50;


	public ESwipeType Value {
		get {
			return _swipe;
		}
	}

	public SwipeServiceProvider()
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


	ESwipeType DetectSwipe()
	{

		if (Input.touches.Length != 1)
			return ESwipeType.None;

		if (Input.touches [0].phase != TouchPhase.Moved)
			return ESwipeType.None;

		Vector2 dir=Input.touches [0].deltaPosition / Input.touches [0].deltaTime;

		if(dir.x>threshold)
			return ESwipeType.Right;

		if(dir.x<-threshold)
			return ESwipeType.Left;

		if(dir.y>threshold)
			return ESwipeType.Top;

		if(dir.y<-threshold)
			return ESwipeType.Bottom;

		return ESwipeType.None;
	}

	public override void Update()
	{
		if (!_enabled)
			return;
			
		ESwipeType s= DetectSwipe ();
		if (s == _swipe)
			return;//no change

		_swipe = s;

		_data.Clear ();
		_data.AddRange (BitConverter.GetBytes ((int)_swipe));

		if (OnValueChanged != null)
			OnValueChanged (this);
	}



	public override void ProcessData(byte[] data)
	{
		_swipe=(ESwipeType) BitConverter.ToInt32 (data, 0);
		if (OnValueChanged != null)
			OnValueChanged (this);
		
	}

	public override string GetDebugString()
	{
		return _swipe.ToString ();
	}
}
