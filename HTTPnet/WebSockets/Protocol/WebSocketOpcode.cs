namespace HTTPnet.WebSockets.Protocol
{
    public enum WebSocketOpcode
    {
        Continuation = 0,
        Text = 1,
        Binary = 2,
        // 0x3-0x7 have no meaning,
        ConnectionClose = 8,
        Ping = 9,
        Pong = 0xA
    }
}
