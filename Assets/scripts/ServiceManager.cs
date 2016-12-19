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

	Thread _serverThrad;

	public bool IsReceiver=false;

	public int LocalTCPPort= 7000;
	public int LocalUDPPort= 7001;

	public int RemoteTCPPort= 7005;
	public int RemoteUDPPort = 7070;

	bool _isDone=false;

	MemoryStream _ReliableDataMem = new MemoryStream ();
	MemoryStream _UnReliableDataMem = new MemoryStream ();

	bool _ReliableDataDirty=false;
	bool _UnReliableDataDirty=false;

	BinaryWriter _ReliableDataWriter ;
	BinaryWriter _UnReliableDataWriter ;

	public enum EMessageType
	{
		ControlMessage,
		ServiceMessage,
		DataMessage
	}

	public enum EControlMessage
	{
		EnableService,
		DisableService
	}

	// Use this for initialization
	void Start () {
		_udpClient = new UdpClient ();
		_tcpClient = new TcpClient ();

		_tcpServer = new TcpListener (LocalTCPPort);


		_serverThrad = new Thread(new ThreadStart(TcpServerThreadHandler));
		_serverThrad.Start();


		_Services.Add(new GyroServiceProvider ());
		_Services.Add(new AccelServiceProvider ());
		_Services.Add(new SwipeServiceProvider ());
		_Services.Add(new TouchServiceProvider ());


		_ReliableDataWriter = new BinaryWriter (_ReliableDataMem);
		_UnReliableDataWriter = new BinaryWriter (_UnReliableDataMem);

	}

	void OnDestroy()
	{
		_isDone = true;

		if (_currentClient != null)
			_currentClient.Close ();
		_tcpClient.Close ();
		_udpClient.Close ();
		_serverThrad.Join ();
	}

	void _NewClientConnected()
	{
		_tcpClient.Close ();
		_udpClient.Close ();

		IPEndPoint addr=((IPEndPoint)_currentClient.Client.RemoteEndPoint);
		_tcpClient.Connect (addr.Address, RemoteTCPPort);
		_udpClient.Connect(addr.Address, RemoteUDPPort);


	}

	void _ProcessControlMessage(BinaryReader rdr)
	{
	}

	void _ProcessServiceMessage(BinaryReader rdr)
	{
		string serviceName=rdr.ReadString ().ToLower();

	}

	public void TcpServerThreadHandler()
	{
		Byte[] bytes = new Byte[256];
		String data = null;
		BinaryReader rdr;
		MemoryStream ms;

		while(!_isDone)
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

			if (_currentClient == null)
				continue;

			var stream=_currentClient.GetStream();
			while (_currentClient!=null && _currentClient.Connected) {
				//process client
				int len=stream.Read (bytes, 0, bytes.Length);
				if (len == 0)
					continue;
				ms = new MemoryStream (bytes, 0, len, false);
				rdr=new BinaryReader(ms);
				//parse message name
				EMessageType msg=(EMessageType) rdr.ReadInt32();
				switch (msg) {
				case EMessageType.ControlMessage:
					_ProcessControlMessage (rdr);
					break;
				case EMessageType.ServiceMessage:
					_ProcessServiceMessage (rdr);
					break;
				}
			}
		}
	}

	void _AddReliableData(string service,byte[] data)
	{
		_ReliableDataDirty = true;
	}

	void _AddUnReliableData(string service,byte[] data)
	{
		_UnReliableDataDirty = true;
	}
	// Update is called once per frame
	void Update () {

		_ReliableDataDirty = false;
		_UnReliableDataDirty = false;

		_ReliableDataMem.Seek (0,SeekOrigin.Begin);
		_ReliableDataMem.SetLength (0);
		_UnReliableDataMem.Seek (0,SeekOrigin.Begin);
		_UnReliableDataMem.SetLength (0);

		_ReliableDataWriter.Write ((int)EMessageType.DataMessage);
		_UnReliableDataWriter.Write ((int)EMessageType.DataMessage);
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

		if (_ReliableDataDirty && _tcpClient.Connected)
			_tcpClient.GetStream ().Write (_ReliableDataMem.GetBuffer (), 0, (int) _ReliableDataMem.Length);
		if (_UnReliableDataDirty && _udpClient.Client.Connected)
			_udpClient.Send (_UnReliableDataMem.GetBuffer (), (int)_UnReliableDataMem.Length);

	}


	void OnGUI()
	{
		string text = "";
		foreach (var s in _Services) {
			text += s.GetName () + ": ";
			text+=s.GetDebugString ();
			text += "\n";
		}

		GUI.Label (new Rect (20, 20, 500, 500), text);
	}
}
