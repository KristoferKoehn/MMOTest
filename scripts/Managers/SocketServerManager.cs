using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Managers.SocketServerManager
{

    class PendingPeer
    {
        public ulong ConnectTime;
        public StreamPeerTcp tcp;
        public StreamPeer connection;
        public WebSocketPeer ws;

        PendingPeer(StreamPeerTcp p_tcp)
        {
            tcp = p_tcp;
            connection = p_tcp;
            ConnectTime = Time.GetTicksMsec();
        }
    }



    public partial class SocketServerManager : Node
    {
        int inport = 9002;
        int outport = 9003;


        private SocketServerManager() { }
        private static SocketServerManager instance = null;
        TcpServer TCPin = null;
        WebSocketPeer wsp = null;
        List<StreamPeerTcp> streamPeerTcps = new List<StreamPeerTcp>();


        public static SocketServerManager GetInstance()
        {
            if (instance == null)
            {
                instance = new SocketServerManager();
                instance.Name = "SocketServerManager";
                GameLoop.Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(instance);
            }

            return instance;
        }

        public override void _Ready()
        {
            wsp = new WebSocketPeer();
            TCPin = new TcpServer();
            TCPin.Listen(9002);
        }

        public override void _Process(double delta)
        {
            wsp.Poll();

            if (TCPin.IsConnectionAvailable())
            {
                StreamPeerTcp stream = TCPin.TakeConnection();
                GD.Print("web client connected from" + stream.GetConnectedHost());
                wsp.AcceptStream(stream);
                GD.Print(stream.GetStatus());
            }

            WebSocketPeer.State state = wsp.GetReadyState();

            if (state == WebSocketPeer.State.Open)
            {
                while (wsp.GetAvailablePacketCount() > 0)
                {
                    byte[] msg = wsp.GetPacket();
                    GD.Print(msg.ToString());
                }
            }

            if (state == WebSocketPeer.State.Closed)
            {
                wsp.Close();
            }

            if (state == WebSocketPeer.State.Closing)
            {
                //do nothing?
            }
            if (state == WebSocketPeer.State.Connecting)
            {
                wsp.Poll();
            }
        }

    }




}

