using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IServiceProvider {

	string GetName();

	//Is TCP required for reliable data send
	bool IsReliable();


	void SetEnabled(bool e);
	bool IsEnabled();


	byte[] GetData();

	void Update();

	void ProcessData(byte[] data);

}
