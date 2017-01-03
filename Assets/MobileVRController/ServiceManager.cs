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
	UdpClient _udpSender;
	TcpClient _tcpClient;

	TcpClient _currentClient;

	TcpListener _tcpServer;

	Thread _serverThread;

	Thread _tcpThread;
	Thread _udpThread;

	public bool IsReceiver=false;

	public bool DebugPrint=false;

	public int TCPPort= 7000;
	public int UDPPort= 7001;

	//public int MobileTCPPort= 7005;
	//public int MobileUDPPort = 7070;

	//public string MobileIP="";

	bool _isDone=false;

	MemoryStream _ReliableDataMem;
	MemoryStream _UnReliableDataMem ;

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

		_ReliableDataMem = new MemoryStream ();
		_UnReliableDataMem = new MemoryStream ();

		_ReliableDataWriter = new BinaryWriter (_ReliableDataMem);
		_UnReliableDataWriter = new BinaryWriter (_UnReliableDataMem);

		if (IsReceiver) {
		} else {

		}
		_tcpClient = new TcpClient ();
		_tcpServer = new TcpListener (IPAddress.Any, TCPPort);
		_udpClient = new UdpClient (UDPPort);
		_udpSender = new UdpClient ();

		_tcpServer.Start ();
		_serverThread = new Thread (new ThreadStart (TcpServerThreadHandler));
		_serverThread.Start ();


		_udpThread=new Thread (new ThreadStart (UdpClientThreadHandler));
		_udpThread.Start ();


		_Services.Add(new GyroServiceProvider (this));
		_Services.Add(new AccelServiceProvider (this));
		_Services.Add(new SwipeServiceProvider (this));
		_Services.Add(new TouchServiceProvider (this));
		_Services.Add(new FeedbackServiceProvider (this));


		for (int i = 0; i < _Services.Count; ++i)
			_Services [i].OnValueChanged += _OnServiceValueChanged;

	}

	void OnDestroy()
	{
		_isDone = true;

		if (_currentClient != null)
			_currentClient.Close ();
		if(_tcpClient!=null)
			_tcpClient.Close ();
		_udpClient.Close ();
		_udpSender.Close ();
		_tcpServer.Stop ();

		//_serverThread.Abort ();
		_serverThread.Join ();
		_udpThread.Join ();
	}

	void _CloseConnection()
	{
		//if (IsReceiver) 
		if(_tcpClient!=null)
			_tcpClient.Close ();
		if(_udpSender!=null)
			_udpSender.Close ();

		_tcpClient = null;
		_udpSender = null;

	}

	void _NewClientConnected()
	{
		Debug.Log ("_NewClientConnected() - Client connected!");
		IPEndPoint addr=((IPEndPoint)_currentClient.Client.RemoteEndPoint);
		_CloseConnection ();
		_tcpClient = new TcpClient ();
		//connect back to the other end 
		_tcpClient.Connect (addr.Address, TCPPort);
		_udpSender = new UdpClient ();
		_udpSender.Connect(addr.Address, UDPPort);


	}

	public IServiceProvider GetService(string name)
	{
		name = name.ToLower ();
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
				Debug.LogError ("UdpClientThreadHandler() - "+e.Message);
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
				Debug.Log ("TcpServerThreadHandler() - Waiting for connection.");
  				TcpClient client= _tcpServer.AcceptTcpClient ();
				if (client != null) {
					/*//new client, make sure only one client is connected at a time
					if (_currentClient != null && _currentClient.Connected) {
						//ignore the new client
						client.Close ();
						Debug.Log ("TcpServerThreadHandler() - Ignoring connection.");
						continue;
					} else */
					{
						_currentClient = client;
						_NewClientConnected ();
					}
				}
			}catch(Exception e) {
				Debug.LogError ("TcpServerThreadHandler() - "+e.Message);
				continue;
			}

			if (_currentClient == null)
				continue;

			var stream=_currentClient.GetStream();
			while (_currentClient!=null && _tcpClient!=null && _currentClient.Connected
				&& _tcpClient.Connected) {
				//process client
				try
				{
					int len=stream.Read (bytes, 0, bytes.Length);

					if (len == 0)
						continue;//investigate more why the socket return 0, although the socket is still open
					try{
						_ProcessReceivedData (_currentClient.Client.RemoteEndPoint, bytes, len);
					}catch(Exception)
					{
						continue;
					}
				}catch(SocketException e) {
					Debug.LogError ("TcpServerThreadHandler() - "+e.Message);
					break;
				}catch(Exception e) {
					Debug.LogError ("TcpServerThreadHandler() - "+e.Message);
					break;
				}

			}
			Debug.Log ("TcpServerThreadHandler() - Remote Disconnected.");
			if(_tcpClient!=null)
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
			if (data.Length == 0)
				continue;
			if (s.IsReliable ())
				_AddReliableData (s.GetName (), data);
			else 
				_AddUnReliableData (s.GetName (), data);
		}

		try{
			if (_ReliableDataDirty && _tcpClient.Connected)
				_tcpClient.GetStream ().Write (_ReliableDataMem.GetBuffer (), 0, (int) _ReliableDataMem.Length);
			if (_UnReliableDataDirty && _udpSender.Client.Connected)
				_udpSender.Send (_UnReliableDataMem.GetBuffer (), (int)_UnReliableDataMem.Length);
		}
		catch(SocketException e)
		{
			Debug.LogError ("_ProcessSendData() - Socket Error: "+e.Message);
			_CloseConnection ();
			
		}
		catch(Exception e) {
			Debug.LogError ("_ProcessSendData() - "+e.Message);
		}
	}
	// Update is called once per frame
	void Update () {

		//if(!IsReceiver)
		_ProcessSendData ();

	}


	GUIStyle _style=new GUIStyle();

	void OnGUI()
	{
		if (!DebugPrint)
			return;
		_style.fontSize = 24;
		_style.normal.textColor=Color.white;
//		_style.font.material.color = Color.white;
		string text = "IP Address:"+Network.player.ipAddress + "\n";
		foreach (var s in _Services) {
			text += s.GetName () + ": ";
			text += s.GetDebugString ();
			text += "\n";
		}
		//if (!IsReceiver)
		{
			text += "UDP Size:"+_UnReliableDataMem.Length.ToString()+"\n";
			text += "TCP Size:"+_ReliableDataMem.Length.ToString()+"\n";
		}

		GUI.Label (new Rect (20, 20, 500, 500), text,_style);
	}



	public void CalibrateGyro()
	{
		var s=GetService(GyroServiceProvider.ServiceName) as GyroServiceProvider;
		s.Calibrate ();
	}

	public void ConnectTo(string OtherEnd)
	{
		//if (!IsReceiver) 
		{
			_CloseConnection ();
			if (_tcpClient == null)
				_tcpClient = new TcpClient ();
			IPAddress addr = IPAddress.Parse (OtherEnd);
			_tcpClient.BeginConnect (addr, TCPPort, null, false);
		}
	}
}
