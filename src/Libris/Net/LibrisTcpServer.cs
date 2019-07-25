﻿using Libris.EventArgs;
using Libris.Models;
using Libris.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Libris.Net
{
    internal class LibrisTcpServer
    {
        public delegate Task PacketReceivedHandler(PacketReceivedEventArgs packetReceived);
        public event PacketReceivedHandler PacketReceived;

        private readonly TcpListener _tcpListener;
        private readonly LibrisMinecraftServer _minecraftServer;

        public LibrisTcpServer(LibrisMinecraftServer server)
        {
            _tcpListener = new TcpListener(IPAddress.Any, 25565);
            _tcpListener.Start();
            _minecraftServer = server;
        }

        public async Task StartListeningAsync()
        {
            while (true)
            {
                var client = await _tcpListener.AcceptTcpClientAsync();
                byte[] buffer = new byte[1024];
                var stream = client.GetStream();
                stream.Read(buffer, 0, buffer.Length);

                var packetLength = Converters.ReadVariableInteger(buffer, out byte[] remainder);
                var packetId = Converters.ReadVariableInteger(remainder, out byte[] r0);

                PacketReceived?.Invoke(new PacketReceivedEventArgs(client, packetId, r0));

                switch (packetId)
                {
                    case 0:
                        var protocolVersion = Converters.ReadVariableInteger(r0, out byte[] serverAddr);
                        var serverAddress = Converters.ReadUtf8String(serverAddr, out byte[] serverP);
                        var serverPort = Converters.ReadUnsignedShort(serverP, out byte[] nextStateR);
                        var nextState = Converters.ReadVariableInteger(nextStateR, out byte[] empty);
                        Console.WriteLine($"Protocol version: {protocolVersion} | Server address: {serverAddress} | Port: {serverPort} | Next state: {nextState}");
                        if (nextState == 1)
                        {
                            var d1 = JsonConvert.SerializeObject(
                                new ServerListPingResponse(new ServerListPingResponseVersion(LibrisMinecraftServer.ServerVersion, LibrisMinecraftServer.ProtocolVersion),
                                new ServerListPingResponsePlayerList(_minecraftServer.MaximumPlayers, 0, new List<int> { }),
                                new ServerListPingResponseDescription(_minecraftServer.Description))
                                );
                            Console.WriteLine(d1);
                            new ClientboundPacket(0, d1).WriteToStream(stream);
                            client.Close();
                        }
                        break;
                    default:
                        Console.WriteLine("Received unknown packet with ID " + packetId);
                        break;
                }
            }
        }
    }
}