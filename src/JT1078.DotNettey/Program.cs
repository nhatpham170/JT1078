using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using IChannel = DotNetty.Transport.Channels.IChannel;

namespace JT1078.DotNettey
{
    class Program
    {
        static void Main(string[] args)
        {
            JT1078Server jT1078Server = new JT1078Server(2202);
            _ = jT1078Server.StartAsync();
            Console.ReadLine();
        }
    }

    /// <summary>
    /// JT1078 RTP Header
    /// </summary>
    public class JT1078RtpHeader
    {
        /// <summary>
        /// RTP protocol version, 2 bits, current version is 2
        /// </summary>
        public byte Version { get; set; } = 2;
        /// <summary>
        /// Padding flag, 1 bit, if P=1, padding bytes are added at the end of packet
        /// </summary>
        public bool Padding { get; set; }
        /// <summary>
        /// Extension flag, 1 bit, if X=1, RTP header is followed by extension header
        /// For JT1078, X is fixed to 1
        /// </summary>
        public bool Extension { get; set; } = true;
        /// <summary>
        /// CSRC counter, 4 bits, indicates number of CSRC identifiers
        /// For JT1078, CC is fixed to 0
        /// </summary>
        public byte CC { get; set; } = 0;
        /// <summary>
        /// Marker bit, 1 bit, different meanings for different payloads, for video, marks end of frame
        /// </summary>
        public bool Marker { get; set; }
        /// <summary>
        /// Payload type, 7 bits, used to indicate payload type in RTP packet
        /// </summary>
        public byte PayloadType { get; set; }
        /// <summary>
        /// Sequence number, 16 bits, used to identify sequence number of sender's RTP packet
        /// </summary>
        public ushort SequenceNumber { get; set; }
        /// <summary>
        /// Timestamp, 32 bits, reflects sampling time of first octet in RTP packet
        /// </summary>
        public uint Timestamp { get; set; }
        /// <summary>
        /// Synchronization Source identifier, 32 bits, used to identify synchronization source
        /// </summary>
        public uint SSRC { get; set; }

        /// <summary>
        /// Indicates if JT1078 extension header exists
        /// </summary>
        public bool HasExtensionHeader => Extension;

        public static JT1078RtpHeader Decode(IByteBuffer buffer)
        {
            var header = new JT1078RtpHeader();
            var firstByte = buffer.ReadByte();

            // Parse first byte
            header.Version = (byte)((firstByte >> 6) & 0x03);
            header.Padding = ((firstByte >> 5) & 0x01) == 1;
            header.Extension = ((firstByte >> 4) & 0x01) == 1;
            header.CC = (byte)(firstByte & 0x0F);

            // Parse second byte
            var secondByte = buffer.ReadByte();
            header.Marker = ((secondByte >> 7) & 0x01) == 1;
            header.PayloadType = (byte)(secondByte & 0x7F);

            // Parse sequence number (16 bits)
            header.SequenceNumber = buffer.ReadUnsignedShort();

            // Parse timestamp (32 bits)
            header.Timestamp = buffer.ReadUnsignedInt();

            // Parse SSRC (32 bits)
            header.SSRC = buffer.ReadUnsignedInt();

            return header;
        }

        public void Encode(IByteBuffer buffer)
        {
            // Write first byte
            byte firstByte = (byte)(
                ((Version & 0x03) << 6) |
                ((Padding ? 1 : 0) << 5) |
                ((Extension ? 1 : 0) << 4) |
                (CC & 0x0F)
            );
            buffer.WriteByte(firstByte);

            // Write second byte
            byte secondByte = (byte)(
                ((Marker ? 1 : 0) << 7) |
                (PayloadType & 0x7F)
            );
            buffer.WriteByte(secondByte);

            // Write sequence number (16 bits)
            buffer.WriteShort(SequenceNumber);

            // Write timestamp (32 bits)
            buffer.WriteInt((int)Timestamp);

            // Write SSRC (32 bits)
            buffer.WriteInt((int)SSRC);
        }
    }

    /// <summary>
    /// JT1078 Extension Header
    /// </summary>
    public class JT1078ExtensionHeader
    {
        /// <summary>
        /// SIM card number (BCD encoded), 6 bytes
        /// </summary>
        public byte[] SimNumber { get; set; } = new byte[6];

        /// <summary>
        /// Logical channel number, 1 byte
        /// </summary>
        public byte LogicalChannelNumber { get; set; }

        /// <summary>
        /// Data type, 4 bits
        /// 0: Video I frame
        /// 1: Video P frame
        /// 2: Video B frame
        /// 3: Audio frame
        /// 4: Transparent data
        /// </summary>
        public byte DataType { get; set; }

        /// <summary>
        /// Subpackage processing mark, 4 bits
        /// 0: Atomic packet, cannot be split
        /// 1: Subpackage processing, first packet
        /// 2: Subpackage processing, last packet
        /// 3: Subpackage processing, middle packet
        /// </summary>
        public byte SubpackageType { get; set; }

        /// <summary>
        /// Time base, 4 bytes
        /// </summary>
        public uint LastIFrameInterval { get; set; }

        /// <summary>
        /// Absolute timestamp of last key frame, 8 bytes, unsigned integer
        /// </summary>
        public ulong LastIFrameTimestamp { get; set; }

        public static JT1078ExtensionHeader Decode(IByteBuffer buffer)
        {
            var header = new JT1078ExtensionHeader();

            // Read SIM card number (6 bytes)
            header.SimNumber = new byte[6];
            buffer.ReadBytes(header.SimNumber);

            // Read logical channel number (1 byte)
            header.LogicalChannelNumber = buffer.ReadByte();

            // Read data type and subpackage mark (1 byte)
            byte typeByte = buffer.ReadByte();
            header.DataType = (byte)((typeByte >> 4) & 0x0F);
            header.SubpackageType = (byte)(typeByte & 0x0F);

            // Read time base (4 bytes)
            header.LastIFrameInterval = buffer.ReadUnsignedInt();

            // Read absolute timestamp of last key frame (8 bytes)
            header.LastIFrameTimestamp = (ulong)buffer.ReadLong();

            return header;
        }

        public void Encode(IByteBuffer buffer)
        {
            // Write SIM card number (6 bytes)
            buffer.WriteBytes(SimNumber);

            // Write logical channel number (1 byte)
            buffer.WriteByte(LogicalChannelNumber);

            // Write data type and subpackage mark (1 byte)
            byte typeByte = (byte)(
                ((DataType & 0x0F) << 4) |
                (SubpackageType & 0x0F)
            );
            buffer.WriteByte(typeByte);

            // Write time base (4 bytes)
            buffer.WriteInt((int)LastIFrameInterval);

            // Write absolute timestamp of last key frame (8 bytes)
            buffer.WriteLong((long)LastIFrameTimestamp);
        }
    }

    /// <summary>
    /// JT1078 Protocol Encoder
    /// </summary>
    public class JT1078Encoder : MessageToByteEncoder<JT1078Packet>
    {
        protected override void Encode(IChannelHandlerContext context, JT1078Packet message, IByteBuffer output)
        {
            // Encode RTP header
            message.RtpHeader.Encode(output);

            // Encode JT1078 extension header
            if (message.RtpHeader.HasExtensionHeader && message.ExtHeader != null)
            {
                message.ExtHeader.Encode(output);
            }

            // Write payload data
            if (message.Payload != null && message.Payload.Length > 0)
            {
                output.WriteBytes(message.Payload);
            }
        }
    }

    /// <summary>
    /// JT1078 Protocol Decoder
    /// Uses state machine to solve TCP sticky packet and half-packet problems
    /// </summary>
    public class JT1078Decoder : ByteToMessageDecoder
    {
        private const int JT1078_RTP_HEADER_SIZE = 12;
        private const int JT1078_EXT_HEADER_SIZE = 20;

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            // Check if there are enough bytes to read (at least RTP header size)
            if (input.ReadableBytes < JT1078_RTP_HEADER_SIZE)
            {
                return;
            }

            // Mark reading position for possible reset
            input.MarkReaderIndex();

            try
            {
                // Decode RTP header
                JT1078RtpHeader rtpHeader = JT1078RtpHeader.Decode(input);

                // If there is an extension header, check if there are enough bytes
                if (rtpHeader.HasExtensionHeader)
                {
                    if (input.ReadableBytes < JT1078_EXT_HEADER_SIZE)
                    {
                        // If not enough bytes, reset reading position and wait for more data
                        input.ResetReaderIndex();
                        return;
                    }

                    // Decode extension header
                    JT1078ExtensionHeader extHeader = JT1078ExtensionHeader.Decode(input);

                    // Create packet
                    int payloadLength = input.ReadableBytes;
                    byte[] payload = new byte[payloadLength];
                    input.ReadBytes(payload);

                    // Create complete JT1078 packet and output
                    var packet = new JT1078Packet
                    {
                        RtpHeader = rtpHeader,
                        ExtHeader = extHeader,
                        Payload = payload
                    };

                    output.Add(packet);
                }
                else
                {
                    // No extension header, read payload directly
                    int payloadLength = input.ReadableBytes;
                    byte[] payload = new byte[payloadLength];
                    input.ReadBytes(payload);

                    var packet = new JT1078Packet
                    {
                        RtpHeader = rtpHeader,
                        Payload = payload
                    };

                    output.Add(packet);
                }
            }
            catch (Exception ex)
            {
                // If parsing error occurs, log and skip current byte
                Console.WriteLine($"Error decoding JT1078 packet: {ex.Message}");
                input.ResetReaderIndex();
                input.SkipBytes(1); // Skip one byte, try to resynchronize
            }
        }
    }

    /// <summary>
    /// JT1078 Packet
    /// </summary>
    public class JT1078Packet
    {
        /// <summary>
        /// RTP header
        /// </summary>
        public JT1078RtpHeader RtpHeader { get; set; }

        /// <summary>
        /// JT1078 extension header
        /// </summary>
        public JT1078ExtensionHeader ExtHeader { get; set; }

        /// <summary>
        /// Payload data
        /// </summary>
        public byte[] Payload { get; set; }
    }

    /// <summary>
    /// JT1078 Session Manager
    /// </summary>
    public class JT1078SessionManager
    {
        private readonly ConcurrentDictionary<string, JT1078Session> _sessions = new ConcurrentDictionary<string, JT1078Session>();

        /// <summary>
        /// Get or create session
        /// </summary>
        public JT1078Session GetOrCreateSession(string simNumber, byte channelNumber)
        {
            string sessionKey = $"{simNumber}_{channelNumber}";
            return _sessions.GetOrAdd(sessionKey, key => new JT1078Session
            {
                SimNumber = simNumber,
                ChannelNumber = channelNumber,
                CreationTime = DateTime.Now
            });
        }

        /// <summary>
        /// Get session by SSRC
        /// </summary>
        public JT1078Session GetSessionBySSRC(uint ssrc)
        {
            return _sessions.Values.FirstOrDefault(s => s.SSRC == ssrc);
        }

        /// <summary>
        /// Remove session
        /// </summary>
        public bool RemoveSession(string simNumber, byte channelNumber)
        {
            string sessionKey = $"{simNumber}_{channelNumber}";
            return _sessions.TryRemove(sessionKey, out _);
        }
    }

    /// <summary>
    /// JT1078 Session
    /// </summary>
    public class JT1078Session
    {
        /// <summary>
        /// SIM card number
        /// </summary>
        public string SimNumber { get; set; }

        /// <summary>
        /// Channel number
        /// </summary>
        public byte ChannelNumber { get; set; }

        /// <summary>
        /// SSRC (Synchronization Source Identifier)
        /// </summary>
        public uint SSRC { get; set; }

        /// <summary>
        /// Creation time
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Last active time
        /// </summary>
        public DateTime LastActiveTime { get; set; }

        /// <summary>
        /// Whether key frame has been received
        /// </summary>
        public bool HasKeyFrame { get; set; }

        /// <summary>
        /// Whether audio is available
        /// </summary>
        public bool HasAudio { get; set; }
    }

    /// <summary>
    /// JT1078 Server
    /// </summary>
    public class JT1078Server
    {
        private readonly int _port;
        private IChannel _serverChannel;
        private readonly JT1078SessionManager _sessionManager;
        private readonly IEventLoopGroup _bossGroup;
        private readonly IEventLoopGroup _workerGroup;

        // Packet handler delegate
        public delegate void PacketHandlerDelegate(JT1078Packet packet, IChannelHandlerContext context);

        // Packet handling event
        public event PacketHandlerDelegate OnPacketReceived;

        public JT1078Server(int port)
        {
            _port = port;
            _sessionManager = new JT1078SessionManager();
            _bossGroup = new MultithreadEventLoopGroup(1);
            _workerGroup = new MultithreadEventLoopGroup();
        }

        public async Task StartAsync()
        {
            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap
                    .Group(_bossGroup, _workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 1024)
                    .Option(ChannelOption.TcpNodelay, true) // Disable Nagle algorithm to reduce latency
                    .Option(ChannelOption.SoReuseaddr, true)
                    .Option(ChannelOption.SoKeepalive, true)
                    .Option(ChannelOption.SoRcvbuf, 1024 * 1024) // Set receive buffer size
                    .Option(ChannelOption.SoSndbuf, 1024 * 1024) // Set send buffer size
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default) // Use pooled ByteBuf allocator
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        // Set up pipeline
                        IChannelPipeline pipeline = channel.Pipeline;

                        // Add JT1078 decoder
                        pipeline.AddLast("jt1078Decoder", new JT1078Decoder());

                        // Add JT1078 encoder
                        pipeline.AddLast("jt1078Encoder", new JT1078Encoder());

                        // Add business handler
                        pipeline.AddLast("jt1078Handler", new JT1078ServerHandler(this, _sessionManager));
                    }));

                // Bind port and start server
                _serverChannel = await bootstrap.BindAsync(IPAddress.Any, _port);

                Console.WriteLine($"JT1078 server started, listening on port: {_port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting JT1078 server: {ex}");
                await ShutdownAsync();
            }
        }

        public async Task ShutdownAsync()
        {
            try
            {
                // Close server channel
                if (_serverChannel != null)
                {
                    await _serverChannel.CloseAsync();
                }

                // Gracefully shut down event loop groups
                var bossTask = _bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
                var workerTask = _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));

                await Task.WhenAll(bossTask, workerTask);

                Console.WriteLine("JT1078 server closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error shutting down JT1078 server: {ex}");
            }
        }

        // Trigger packet received event
        internal void TriggerPacketReceived(JT1078Packet packet, IChannelHandlerContext context)
        {
            OnPacketReceived?.Invoke(packet, context);
        }
    }

    /// <summary>
    /// JT1078 Server Handler
    /// </summary>
    public class JT1078ServerHandler : SimpleChannelInboundHandler<JT1078Packet>
    {
        private readonly JT1078Server _server;
        private readonly JT1078SessionManager _sessionManager;

        public JT1078ServerHandler(JT1078Server server, JT1078SessionManager sessionManager)
        {
            _server = server;
            _sessionManager = sessionManager;
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, JT1078Packet msg)
        {
            try
            {
                // Update or create session
                if (msg.ExtHeader != null)
                {
                    // Convert BCD encoded SIM card number to string
                    string simNumber = ConvertBcdToString(msg.ExtHeader.SimNumber);

                    // Get or create session
                    var session = _sessionManager.GetOrCreateSession(simNumber, msg.ExtHeader.LogicalChannelNumber);

                    // Update session information
                    session.SSRC = msg.RtpHeader.SSRC;
                    session.LastActiveTime = DateTime.Now;

                    // Check data type, update session status
                    if (msg.ExtHeader.DataType == 0) // I frame
                    {
                        session.HasKeyFrame = true;
                    }
                    else if (msg.ExtHeader.DataType == 3) // Audio frame
                    {
                        session.HasAudio = true;
                    }
                }

                // Trigger packet received event
                _server.TriggerPacketReceived(msg, ctx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing JT1078 packet: {ex}");
            }
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            Console.WriteLine($"Client connected: {context.Channel.RemoteAddress}");
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Console.WriteLine($"Client disconnected: {context.Channel.RemoteAddress}");
            base.ChannelInactive(context);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine($"Channel exception: {exception}");
            context.CloseAsync();
        }

        /// <summary>
        /// Convert BCD encoded SIM card number to string
        /// </summary>
        private string ConvertBcdToString(byte[] bcd)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bcd)
            {
                sb.Append((b >> 4) & 0x0F);
                sb.Append(b & 0x0F);
            }
            return sb.ToString().TrimStart('0');
        }
    }

    /// <summary>
    /// JT1078 Packet Buffer
    /// Used to process subpackaged JT1078 data
    /// </summary>
    public class JT1078PacketBuffer
    {
        private readonly ConcurrentDictionary<string, List<JT1078Packet>> _buffers = new ConcurrentDictionary<string, List<JT1078Packet>>();

        /// <summary>
        /// Add packet and try to reassemble
        /// </summary>
        /// <returns>If packet is complete, returns complete packet, otherwise returns null</returns>
        public JT1078Packet AddPacketAndTryReassemble(JT1078Packet packet)
        {
            if (packet.ExtHeader == null)
            {
                return packet; // If no extension header, return original packet
            }

            // Generate buffer key based on SIM card number and channel number
            string simNumber = ConvertBcdToString(packet.ExtHeader.SimNumber);
            string bufferKey = $"{simNumber}_{packet.ExtHeader.LogicalChannelNumber}_{packet.RtpHeader.SequenceNumber}";

            // If it's an atomic packet, return directly
            if (packet.ExtHeader.SubpackageType == 0)
            {
                return packet;
            }

            // If it's the first packet of a subpackage
            if (packet.ExtHeader.SubpackageType == 1)
            {
                List<JT1078Packet> packetList = new List<JT1078Packet> { packet };
                _buffers[bufferKey] = packetList;
                return null;
            }

            // If it's the middle or last packet of a subpackage
            if (_buffers.TryGetValue(bufferKey, out var packets))
            {
                packets.Add(packet);

                // If it's the last packet, try to reassemble
                if (packet.ExtHeader.SubpackageType == 2)
                {
                    // Sort by sequence number
                    packets.Sort((a, b) => a.RtpHeader.SequenceNumber.CompareTo(b.RtpHeader.SequenceNumber));

                    // Merge payloads
                    int totalLength = packets.Sum(p => p.Payload.Length);
                    byte[] mergedPayload = new byte[totalLength];
                    int offset = 0;

                    foreach (var p in packets)
                    {
                        Array.Copy(p.Payload, 0, mergedPayload, offset, p.Payload.Length);
                        offset += p.Payload.Length;
                    }

                    // Create merged packet
                    JT1078Packet mergedPacket = new JT1078Packet
                    {
                        RtpHeader = packet.RtpHeader,
                        ExtHeader = packet.ExtHeader,
                        Payload = mergedPayload
                    };

                    // Remove buffer
                    _buffers.TryRemove(bufferKey, out _);

                    return mergedPacket;
                }
            }

            return null;
        }

        /// <summary>
        /// Convert BCD encoded SIM card number to string
        /// </summary>
        private string ConvertBcdToString(byte[] bcd)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bcd)
            {
                sb.Append((b >> 4) & 0x0F);
                sb.Append(b & 0x0F);
            }
            return sb.ToString().TrimStart('0');
        }

        /// <summary>
        /// Clean up expired packet buffers
        /// </summary>
        public void CleanupExpiredBuffers(TimeSpan timeout)
        {
            // Here you can implement a timed cleanup mechanism to clean up buffers that haven't received complete packets for a long time
            // This method can be called periodically by a timer
        }
    }

    /// <summary>
    /// JT1078 Client Example
    /// </summary>
    public class JT1078Client
    {
        private readonly string _host;
        private readonly int _port;
        private DotNetty.Transport.Channels.IChannel _clientChannel;
        private readonly IEventLoopGroup _group;

        public JT1078Client(string host, int port)
        {
            _host = host;
            _port = port;
            _group = new MultithreadEventLoopGroup();
        }

        public async Task ConnectAsync()
        {
            try
            {
                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(_group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Option(ChannelOption.SoKeepalive, true)
                    .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(5))
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        // Add JT1078 encoder
                        pipeline.AddLast("jt1078Encoder", new JT1078Encoder());

                        // Add JT1078 decoder
                        pipeline.AddLast("jt1078Decoder", new JT1078Decoder());

                        // Add client handler
                        pipeline.AddLast("clientHandler", new JT1078ClientHandler());
                    }));

                // Connect to server
                _clientChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(_host), _port));

                Console.WriteLine($"Connected to JT1078 server: {_host}:{_port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to JT1078 server: {ex}");
                await ShutdownAsync();
            }
        }

        /// <summary>
        /// Send JT1078 packet
        /// </summary>
        public async Task SendPacketAsync(JT1078Packet packet)
        {
            if (_clientChannel != null && _clientChannel.Active)
            {
                await _clientChannel.WriteAndFlushAsync(packet);
            }
            else
            {
                throw new InvalidOperationException("Client not connected or connection closed");
            }
        }

        public async Task ShutdownAsync()
        {
            try
            {
                if (_clientChannel != null)
                {
                    await _clientChannel.CloseAsync();
                }

                await _group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));

                Console.WriteLine("JT1078 client closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error shutting down JT1078 client: {ex}");
            }
        }
    }

    /// <summary>
    /// JT1078 Client Handler
    /// </summary>
    public class JT1078ClientHandler : SimpleChannelInboundHandler<JT1078Packet>
    {
        // Packet received event
        public event Action<JT1078Packet> OnPacketReceived;

        protected override void ChannelRead0(IChannelHandlerContext ctx, JT1078Packet msg)
        {
            // Trigger packet received event
            OnPacketReceived?.Invoke(msg);
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            Console.WriteLine("Connected to server");
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Console.WriteLine("Disconnected from server");
            base.ChannelInactive(context);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine($"Client exception: {exception}");
            context.CloseAsync();
        }

    }

}
