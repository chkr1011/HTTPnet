namespace HTTPnet.WebSockets.Protocol
{
    public sealed class WebSocketFrame
    {
        public bool Fin { get; set; } = true;
        public WebSocketOpcode Opcode { get; set; } = WebSocketOpcode.Binary;
        public uint MaskingKey { get; set; }
        public byte[] Payload { get; set; } = new byte[0];
    }
}
