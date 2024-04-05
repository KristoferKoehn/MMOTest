using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Managers.SocketServerManager
{

    public partial class SocketServerManager : Node
    {
        int inport = 9002;
        int outport = 9003;


        private SocketServerManager() { }
        private static SocketServerManager instance = null;
        TcpServer TCPin = null;
        StreamPeerTls StreamPeerServertTls = null;
        WebSocketPeer wsp = null;
        StreamPeerTls.Status prevstate = StreamPeerTls.Status.Disconnected;

        X509Certificate cert = null;
        CryptoKey cryptoKey = null;

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
            
            cert = ResourceLoader.Load<X509Certificate>("res://assets/ServerResources/serverCAS.crt");
            cryptoKey = ResourceLoader.Load<CryptoKey>("res://assets/ServerResources/serverKey.key");;



            StreamPeerServertTls = new StreamPeerTls();

            TCPin = new TcpServer();
            TCPin.Listen(9002);
           


            StreamPeerTls tls = new StreamPeerTls();

            if(!TCPin.IsListening())
            {
                GD.Print("TCP server error");
            } else
            {
                GD.Print("TCP server listening...");
            }

        }

        public override void _Process(double delta)
        {
            while (TCPin.IsConnectionAvailable())
            {
                StreamPeerTcp stream = TCPin.TakeConnection();
                GD.Print("web client connected from" + stream.GetConnectedHost());
                stream.Poll();
                StreamPeerServertTls.AcceptStream(stream, TlsOptions.Server(cryptoKey, cert));
                
                GD.Print(stream.GetStatus());
            }

            StreamPeerServertTls.Poll();
            


            StreamPeerTls.Status state = StreamPeerServertTls.GetStatus();

            if (state == StreamPeerTls.Status.Connected)
            {
                if(state != prevstate)
                {
                    GD.Print("Open!");
                }

                while (wsp.GetAvailablePacketCount() > 0)
                {
                    byte[] msg = wsp.GetPacket();
                    GD.Print(msg.ToString());
                }
            }

            if (state == StreamPeerTls.Status.Disconnected && state != prevstate)
            {
                wsp.Close();
                GD.Print("Disconnected");
            }

            if (state == StreamPeerTls.Status.Handshaking && state != prevstate)
            {
                GD.Print("Handshaking...");
            }

            prevstate = state;
        }

    }




}

