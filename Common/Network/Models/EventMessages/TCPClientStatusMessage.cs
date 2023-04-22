﻿using System.Net;

namespace Ciribob.SRS.Common.Network.Models.EventMessages;

public class TCPClientStatusMessage
{
    public enum ErrorCode
    {
        MISMATCHED_SERVER,
        TIMEOUT,
        INVALID_SERVER,
        USER_DISCONNECTED
    }

    public TCPClientStatusMessage(bool connected)
    {
        Connected = connected;
    }

    public TCPClientStatusMessage(bool connected, IPEndPoint address)
    {
        Connected = connected;
        Address = address;
    }

    public TCPClientStatusMessage(bool connected, ErrorCode error)
    {
        Error = error;
        Connected = connected;
    }

    public ErrorCode Error { get; }
    public IPEndPoint Address { get; }

    public bool Connected { get; }
}