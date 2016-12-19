using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System;
using System.IO;

public class ServiceManager : MonoBehaviour {


	List<IServiceProvider> _Services=new List<IServiceProvider>();

	UdpClient _udpClient;
	TcpClient _tcpClient;

	TcpClient _currentClient;

	TcpListener _tcpServer;

	Thread _serverThread;

	Thread _tcpThread;
	Thread _udpThread;

	public bool IsReceiver=false;

	public int ReceiverTCPPort= 7000;
	public int ReceiverUDPPort= 7001;

	public int MobileTCPPort= 7005;
	public int MobileUDPPort = 7070;

	public string MobileIP="";

	bool _isDone=false;

	MemoryStream _ReliableDataMem = new MemoryStream ();
	MemoryStream _UnReliableDataMem = new MemoryStream ();

	bool _ReliableDataDirty=false;
	bool _UnReliableDataDirty=false;

	BinaryWriter _ReliableDataWriter ;
	BinaryWriter _UnReliableDataWriter ;

	public delegate void OnValueChangedDeleg(ServiceManager m,IServiceProvider s);
	public OnValueChangedDeleg OnValueChanged;


	public enum EMessageType
	{
		ControlMessage,
		ServiceMessage
	}


	public enum EServiceMessage
	{
		EnableService,
		DisableService,
		ServiceData
	}
	void _OnServiceValueChanged(IServiceProvider service)
	{
		if (OnValueChanged != null)
			OnValueChanged (this,service);
	}

	// Use this for initialization
	void Start () {

		if (IsReceiver) {
			//Client
			_udpClient = new UdpClient (ReceiverUDPPort);
			_tcpClient = new TcpClient ();
			IPAddress addr = IPAddress.Parse (MobileIP);
			_tcpClient.Connect (addr, MobileTCPPort);

			_tcpServer = new TcpListener (IPAddress.Any, ReceiverTCPPort);
			_tcpServer.Start ();

			_tcpThread=new Thread (new ThreadStart (TcpClientThreadHandler));
			_udpThread=new Thread (new ThreadStart (UdpClientThreadHandler));

			_tcpThread.Start ();
			_udpThread.Start ();

		} else {
			//Server (Mobile Side)
			_tcpServer = new TcpListener (IPAddress.Any, MobileTCPPort);
			_serverThread = new Thread (new ThreadStart (TcpServerThreadHandler));

			_udpClient = new UdpClient ();
			_tcpClient = new TcpClient ();

			_ReliableDataWriter = new BinaryWriter (_ReliableDataMem);
			_UnReliableDataWriter = new BinaryWriter (_UnReliableDataMem);

			_tcpServer.Start ();
			_serverThread.Start ();

		}

		_Services.Add(new GyroServiceProvider ());
		_Services.Add(new AccelServiceProvider ());
		_Services.Add(new SwipeServiceProvider ());
		_Services.Add(new TouchServiceProvider ());


		for (int i = 0; i < _Services.Count; ++i)
			_Services [i].OnValueChanged += _OnServiceValueChanged;

	}

	void OnDestroy()
	{
		_isDone = true;

		if (_currentClient != null)
			_currentClient.Close ();
		_tcpClient.Close ();
		_udpClient.Close ();
		_tcpServer.Stop ();

		if (IsReceiver) {
		//	_tcpThread.Abort ();
		//	_udpThread.Abort ();
			_tcpThread.Join ();
			_udpThread.Join ();
			
		} else {
			_serverThread.Abort ();
			_serverThread.Join ();
		}
	}

	void _NewClientConnected()
	{
		Debug.Log ("Client connected!");
		_tcpClient.Close ();
		_udpClient.Close ();

		_tcpClient = new TcpClient ();
		_udpClient = new UdpClient ();

		IPEndPoint addr=((IPEndPoint)_currentClient.Client.RemoteEndPoint);
		_tcpClient.Connect (addr.Address, ReceiverTCPPort);
		_udpClient.Connect(addr.Address, ReceiverUDPPort);


	}

	IServiceProvider GetService(string name)
	{
		foreach(var s in _Services)
		{
			if(s.GetName().ToLower()==name)
				return s;
		}
		return null;
	}

	bool _ProcessControlMessage(BinaryReader rdr)
	{
		return false;
	}

	bool _ProcessServiceMessage(BinaryReader rdr)
	{
		string serviceName=rdr.ReadString ().ToLower();
		EServiceMessage msg=(EServiceMessage) rdr.ReadInt32 ();
		IServiceProvider s = GetService (serviceName);
		if (s == null)
			return false;
		switch (msg) {
		case EServiceMessage.ServiceData:
			{
				int len = rdr.ReadInt32 ();
				byte[] data = rdr.ReadBytes (len);
				s.ProcessData (data);
			}
			break;
		case EServiceMessage.EnableService:
			s.SetEnabled (true);
			break;
		case EServiceMessage.DisableService:
			s.SetEnabled (false);
			break;
		}
		return true;
	}



	void _ProcessReceivedData(EndPoint src, byte[] data,int len)
	{

		BinaryReader rdr;
		MemoryStream ms;

		ms = new MemoryStream (data, 0, len, false);
		rdr=new BinaryReader(ms);
		//parse message name
		EMessageType msg=(EMessageType) rdr.ReadInt32();
		switch (msg) {
		case EMessageType.ControlMessage:
			_ProcessControlMessage (rdr);
			break;
		case EMessageType.ServiceMessage:
			while (ms.Position < ms.Length)
				if (!_ProcessServiceMessage (rdr))
					break;
			break;
		}
	}

	public void TcpClientThreadHandler()
	{
		Byte[] bytes = new Byte[256];
		while (!_isDone) {
			_currentClient=_tcpServer.AcceptTcpClient ();
			while (_currentClient!=null && _currentClient.Connected) {
				try
				{
					int len = _currentClient.GetStream ().Read (bytes, 0, bytes.Length);
					if (len > 0) {
						_ProcessReceivedData (_currentClient.Client.RemoteEndPoint, bytes, len);
					}
				}catch(SocketException e) {
//					Debug.Log (e.Message);
					break;
				}catch(Exception e) {
					//					Debug.Log (e.Message);
					break;
				}
			}
		}
	}

	public void UdpClientThreadHandler()
	{
		IPEndPoint ip=new IPEndPoint(IPAddress.Any, 0);;
		while (!_isDone) {
			try{
				byte[] data= _udpClient.Receive (ref ip);
				if (data != null && data.Length > 0) {
					_ProcessReceivedData (ip, data,data.Length);
				}
			}catch(Exception e) {
			//	Debug.Log (e.Message);
				Thread.Sleep (100);
			}
		}
	}

	public void TcpServerThreadHandler()
	{
		Byte[] bytes = new Byte[256];
		while(!_isDone)
		{
			try
			{
				TcpClient client= _tcpServer.AcceptTcpClient ();
				if (client != null) {
					//new client, make sure only one client is connected at a time
					if (_currentClient != null && _currentClient.Connected) {
						//ignore the new client
						client.Close ();
						continue;
					} else {
						_currentClient = client;
						_NewClientConnected ();
					}
				}
			}catch(Exception e) {
				Debug.Log (e.Message);
				continue;
			}

			if (_currentClient == null)
				continue;

			var stream=_currentClient.GetStream();
			while (_currentClient!=null && _currentClient.Connected
				&& _tcpClient.Connected) {
				//process client
				try
				{
					int len=stream.Read (bytes, 0, bytes.Length);
					if (len == 0)
						break;
					_ProcessReceivedData (_currentClient.Client.RemoteEndPoint, bytes,len);
				}catch(SocketException e) {
				//	Debug.Log (e.Message);
					break;
				}catch(Exception e) {
				//	Debug.Log (e.Message);
					break;
				}

			}
			_tcpClient.Close ();
			_currentClient.Close ();
			_currentClient = null;
		}
	}


	void _WriteData(BinaryWriter w, string service,byte[] data)
	{
		w.Write (service.ToLower());
		w.Write ((int)EServiceMessage.ServiceData);
		w.Write (data.Length);
		w.Write (data);
	}
	void _AddReliableData(string service,byte[] data)
	{
		_ReliableDataDirty = true;
		_WriteData (_ReliableDataWriter, service, data);
	}

	void _AddUnReliableData(string service,byte[] data)
	{
		_UnReliableDataDirty = true;
		_WriteData (_UnReliableDataWriter, service, data);
	}

	void _ProcessSendData()
	{
		if (_tcpClient == null || !_tcpClient.Connected)
			return;
		_ReliableDataDirty = false;
		_UnReliableDataDirty = false;

		_ReliableDataMem.Seek (0,SeekOrigin.Begin);
		_ReliableDataMem.SetLength (0);
		_UnReliableDataMem.Seek (0,SeekOrigin.Begin);
		_UnReliableDataMem.SetLength (0);

		_ReliableDataWriter.Write ((int)EMessageType.ServiceMessage);
		_UnReliableDataWriter.Write ((int)EMessageType.ServiceMessage);
		foreach (var s in _Services) {
			if (!s.IsEnabled ())
				continue;
			s.Update ();
			byte[] data=s.GetData ();
			if (s.IsReliable ())
				_AddReliableData (s.GetName (), data);
			else 
				_AddUnReliableData (s.GetName (), data);
		}

		try{
			if (_ReliableDataDirty && _tcpClient.Connected)
				_tcpClient.GetStream ().Write (_ReliableDataMem.GetBuffer (), 0, (int) _ReliableDataMem.Length);
			if (_UnReliableDataDirty && _udpClient.Client.Connected)
				_udpClient.Send (_UnReliableDataMem.GetBuffer (), (int)_UnReliableDataMem.Length);
		}catch(Exception e) {
			Debug.Log (e.Message);
		}
	}
	// Update is called once per frame
	void Update () {

		if(!IsReceiver)
			_ProcessSendData ();

	}


	void OnGUI()
	{
		string text = "IP Address:"+Network.player.ipAddress + "\n";
		foreach (var s in _Services) {
			text += s.GetName () + ": ";
			text += s.GetDebugString ();
			text += "\n";
		}
		if (!IsReceiver) {
			text += "UDP Size:"+_UnReliableDataMem.Length.ToString()+"\n";
			text += "TCP Size:"+_ReliableDataMem.Length.ToString()+"\n";
		}

		GUI.Label (new Rect (20, 20, 500, 500), text);
	}
}
