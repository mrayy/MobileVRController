using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;

public class ServiceManager : MonoBehaviour {


	Dictionary<string,IServiceProvider> _Services=new Dictionary<string,IServiceProvider>();

	UdpClient _udpClient;
	TcpClient _tcpClient;

	TcpListener _tcpServer;

	Thread _clientThrad;

	public int UDPPort = 7070;
	public int TCPPort= 7000;

	bool _isDone=false;

	// Use this for initialization
	void Start () {
		_udpClient = new UdpClient (UDPPort);
		_tcpClient = new TcpClient ();

		_tcpServer = new TcpListener (TCPPort);


		_clientThrad = new Thread(new ThreadStart(TcpThreadHandler));
		_clientThrad.Start();

	}

	void OnDestroy()
	{
		_isDone = true;
		_tcpClient.Close ();
		_udpClient.Close ();
		_clientThrad.Join ();
	}

	public void TcpThreadHandler()
	{
		while(!_isDone)
		{
			TcpClient client= _tcpServer.AcceptTcpClient ();
			if (client != null) {
			}
		}
	}

	
	// Update is called once per frame
	void Update () {
		foreach (var s in _Services.Values) {
			s.Update ();
		}
	}
}
