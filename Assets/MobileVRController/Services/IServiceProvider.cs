using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IServiceProvider
{

	protected bool _enabled=true;
	protected ServiceManager _mngr;

	public delegate void OnValueChangedDeleg(IServiceProvider s);
	public OnValueChangedDeleg OnValueChanged;

	public IServiceProvider(ServiceManager m)
	{
		_mngr = m;
	}

	public virtual void SetEnabled(bool e)
	{
		_enabled = e;
	}
	public virtual bool IsEnabled()
	{
		return _enabled;
	}

	public abstract string GetName();

	//Is TCP required for reliable data send
	public abstract bool IsReliable();

	public abstract byte[] GetData();

	public abstract void Update();

	public abstract void ProcessData(byte[] data);

	public abstract string GetDebugString();
}
