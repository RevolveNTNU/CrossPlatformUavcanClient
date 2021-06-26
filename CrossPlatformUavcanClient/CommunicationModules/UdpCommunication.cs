using RevolveUavcan.Communication;
using RevolveUavcan.Uavcan;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CrossPlatformUavcanClient.CommunicationModules
{
    public class UdpCommunication : IUavcanCommunicationModule
    {
        public event EventHandler<UavcanFrame>? UavcanFrameReceived;

        private UdpClient _client;
        private Thread _readThread;
        private IPEndPoint _ipEndPoint;
        private IPAddress _ip;

        public bool IsConnected { get; set; }

        private const int EXTENDED_FRAME_FORMAT_INDEX = 31;

        public UdpCommunication(IPAddress ip, int port)
        {
            if (ip != null)
            {
                InitializeNetwork(ip, port);
            }
        }

        private void InitializeNetwork(IPAddress ip, int port)
        {
            _client = new UdpClient();
            _ipEndPoint = new IPEndPoint(ip, port);
            this._ip = ip;

            try
            {
                _client.EnableBroadcast = true;
                _client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            }
            catch (Exception e)
            {
                return;
            }
            IsConnected = true;
            _readThread = new Thread(ReadThread) { IsBackground = true };
            _readThread.Start();
        }

        private void ReadThread()
        {
            do
            {
                try
                {
                    var bytes = _client.Receive(ref _ipEndPoint);
                    ReadMessages(bytes);
                }
                catch (SocketException e)
                {

                }
            } while (IsConnected);
        }

        private const int UDP_HEADER_SIZE = 13;

        public void ReadMessages(byte[] message)
        {
            uint canId = 0;
            try
            {
                var subMsgStart = 0;
                //{ Datalength, CanId, Data, TailByte}
                while (subMsgStart < message.Length)
                {
                    var headerLength = 1 + 4;
                    byte dataLength = message[subMsgStart];

                    var canIdWithFlagsNetwork = BitConverter.ToUInt32(message, subMsgStart + 1);
                    var canIdWithFlags = canIdWithFlagsNetwork;// IPAddress.NetworkToHostOrder(canIdWithFlagsNetwork);

                    canId = (canIdWithFlags & ~(0b11100000 << 3 * 8));
                    var headerBits = new BitArray(BitConverter.GetBytes(canId));

                    var isExtendedFrameFormat = (canIdWithFlags & (1 << EXTENDED_FRAME_FORMAT_INDEX)) != 0;

                    if (isExtendedFrameFormat)
                    {
                        // This is an extended CAN message => UAVCAN
                        var data = new byte[dataLength];
                        Array.Copy(message, subMsgStart + headerLength, data, 0, dataLength);
                        IsConnected = true;
                        var frame = new UavcanFrame(headerBits, data, DateTimeOffset.Now.ToUnixTimeMilliseconds());
                        UavcanFrameReceived?.Invoke(this, frame);
                    }

                    subMsgStart += headerLength + dataLength;

                    //var timestampNetwork = BitConverter.ToInt64(message, subMsgStart);
                    //var timestamp = IPAddress.NetworkToHostOrder(timestampNetwork);

                    //var canIdWithFlagsNetwork = BitConverter.ToInt32(message, subMsgStart + 8);
                    //var canIdWithFlags = IPAddress.NetworkToHostOrder(canIdWithFlagsNetwork);

                    //var dataLength = message[subMsgStart + 8 + 4];

                    //var isExtendedFrameFormat = (canIdWithFlags & (1 << EXTENDED_FRAME_FORMAT_INDEX)) != 0;

                    //canId = (canIdWithFlags & ~(0b11100000 << 3 * 8));

                    //var headerBits = new BitArray(BitConverter.GetBytes(canId));

                    //if (isExtendedFrameFormat)
                    //{
                    //    // This is an extended CAN message => UAVCAN
                    //    var data = new byte[dataLength];
                    //    Array.Copy(message, subMsgStart + UDP_HEADER_SIZE, data, 0, dataLength);
                    //    IsConnected = true;

                    //    UavcanFrameReceived?.Invoke(this, new UavcanFrame(headerBits, data, timestamp));
                    //}

                    //subMsgStart += UDP_HEADER_SIZE + dataLength;

                }
            }
            catch (Exception e)
            {
                //TODO: Create some logging module to do some logging
            }
        }

        public bool WriteUavcanFrame(UavcanFrame frame)
        {

            var payload = FormatUavcanMessage(frame);

            //UavcanFrameReceived?.Invoke(this, frame);

            try
            {
                var client = new UdpClient();

                client.Send(payload, payload.Length, _ip.ToString(), 1234);
            }
            catch (Exception e) when (e is ObjectDisposedException ||
                                      e is ArgumentNullException ||
                                      e is InvalidOperationException ||
                                      e is SocketException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a byte array of the UavcanFrame in the correct format.
        /// The format is {Datalength, CanId, Data, TailByte}, where DataLength = Data.Length + 1 (for the tailbyte).
        /// </summary>
        /// <param name="uavcanFrame"></param>
        /// <returns>A byte array of the correct format</returns>
        private byte[] FormatUavcanMessage(UavcanFrame uavcanFrame)
        {
            // Calculate message size based on frame's dataLength and tail byte
            var messageDataSize = GetRequiredMessageSize(uavcanFrame.DataLength + 1);

            byte[] dataLength = { Convert.ToByte(messageDataSize) };

            var timestamp = BitConverter.GetBytes((long)0);
            var canIdAsBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(
                (uint)(uavcanFrame.GetCanId() | (1 << EXTENDED_FRAME_FORMAT_INDEX))));

            var data = uavcanFrame.Data;




            var message = new byte[timestamp.Length
                                   + canIdAsBytes.Length
                                   + dataLength.Length
                                   + messageDataSize];

            // Copy timestamp byte to start of message
            Array.Copy(timestamp, 0, message, 0, timestamp.Length);

            // Append canIdAsBytes to message
            Array.Copy(canIdAsBytes, 0, message, timestamp.Length, canIdAsBytes.Length);

            // Append datalength to message
            Array.Copy(dataLength, 0, message, timestamp.Length + canIdAsBytes.Length, dataLength.Length);

            // Append data to message
            Array.Copy(data, 0, message, timestamp.Length + dataLength.Length + canIdAsBytes.Length, data.Length);

            // Append tailbyte to message
            message[^1] = uavcanFrame.GetTailByte();

            return message;
        }

        /// <summary>
        /// Get the required FD message size
        /// </summary>
        /// <param name="dataLength"></param>
        /// <returns></returns>
        public static byte GetRequiredMessageSize(int dataLength)
        {
            // TODO: Rewrite using C# 9 relational mapping.
            // Documentation: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/patterns3#relational-patterns
            if (dataLength <= 8)
            {
                return (byte)dataLength;
            }

            if (dataLength <= 12)
            {
                return 12;
            }

            if (dataLength <= 16)
            {
                return 16;
            }

            if (dataLength <= 20)
            {
                return 20;
            }

            if (dataLength <= 24)
            {
                return 24;
            }

            if (dataLength <= 32)
            {
                return 32;
            }

            if (dataLength <= 48)
            {
                return 48;
            }

            if (dataLength <= 64)
            {
                return 64;
            }

            return 0;
        }
    }
}
