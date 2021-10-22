using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NAudio.Wave.MmeInterop;

namespace NAudio.Midi
{
    /// <summary>
    ///     Represents a MIDI out device
    /// </summary>
    public class MidiOut : IDisposable
    {
        private readonly MidiInterop.MidiOutCallback callback;
        private bool disposed;
        private readonly IntPtr hMidiOut = IntPtr.Zero;


        /// <summary>
        ///     Opens a specified MIDI out device
        /// </summary>
        /// <param name="deviceNo">The device number</param>
        public MidiOut(int deviceNo)
        {
            callback = Callback;
            MmException.Try(
                MidiInterop.midiOutOpen(out hMidiOut, (IntPtr)deviceNo, callback, IntPtr.Zero,
                    MidiInterop.CALLBACK_FUNCTION), "midiOutOpen");
        }

        /// <summary>
        ///     Gets the number of MIDI devices available in the system
        /// </summary>
        public static int NumberOfDevices => MidiInterop.midiOutGetNumDevs();

        /// <summary>
        ///     Gets or sets the volume for this MIDI out device
        /// </summary>
        public int Volume
        {
            // TODO: Volume can be accessed by device ID
            get
            {
                var volume = 0;
                MmException.Try(MidiInterop.midiOutGetVolume(hMidiOut, ref volume), "midiOutGetVolume");
                return volume;
            }
            set => MmException.Try(MidiInterop.midiOutSetVolume(hMidiOut, value), "midiOutSetVolume");
        }

        /// <summary>
        ///     Closes this MIDI out device
        /// </summary>
        public void Dispose()
        {
            GC.KeepAlive(callback);
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Gets the MIDI Out device info
        /// </summary>
        public static MidiOutCapabilities DeviceInfo(int midiOutDeviceNumber)
        {
            var caps = new MidiOutCapabilities();
            var structSize = Marshal.SizeOf(caps);
            MmException.Try(MidiInterop.midiOutGetDevCaps((IntPtr)midiOutDeviceNumber, out caps, structSize),
                "midiOutGetDevCaps");
            return caps;
        }

        /// <summary>
        ///     Closes this MIDI out device
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        ///     Resets the MIDI out device
        /// </summary>
        public void Reset()
        {
            MmException.Try(MidiInterop.midiOutReset(hMidiOut), "midiOutReset");
        }

        /// <summary>
        ///     Sends a MIDI out message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="param1">Parameter 1</param>
        /// <param name="param2">Parameter 2</param>
        public void SendDriverMessage(int message, int param1, int param2)
        {
            MmException.Try(MidiInterop.midiOutMessage(hMidiOut, message, (IntPtr)param1, (IntPtr)param2),
                "midiOutMessage");
        }

        /// <summary>
        ///     Sends a MIDI message to the MIDI out device
        /// </summary>
        /// <param name="message">The message to send</param>
        public void Send(int message)
        {
            MmException.Try(MidiInterop.midiOutShortMsg(hMidiOut, message), "midiOutShortMsg");
        }

        /// <summary>
        ///     Closes the MIDI out device
        /// </summary>
        /// <param name="disposing">True if called from Dispose</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
                //if(disposing) Components.Dispose();
                MidiInterop.midiOutClose(hMidiOut);

            disposed = true;
        }

        private void Callback(IntPtr midiInHandle, MidiInterop.MidiOutMessage message, IntPtr userData,
            IntPtr messageParameter1, IntPtr messageParameter2)
        {
        }

        /// <summary>
        ///     Send a long message, for example sysex.
        /// </summary>
        /// <param name="byteBuffer">The bytes to send.</param>
        public void SendBuffer(byte[] byteBuffer)
        {
            var header = new MidiInterop.MIDIHDR();
            header.lpData = Marshal.AllocHGlobal(byteBuffer.Length);
            Marshal.Copy(byteBuffer, 0, header.lpData, byteBuffer.Length);

            header.dwBufferLength = byteBuffer.Length;
            header.dwBytesRecorded = byteBuffer.Length;
            var size = Marshal.SizeOf(header);
            MidiInterop.midiOutPrepareHeader(hMidiOut, ref header, size);
            var errcode = MidiInterop.midiOutLongMsg(hMidiOut, ref header, size);
            if (errcode != MmResult.NoError) MidiInterop.midiOutUnprepareHeader(hMidiOut, ref header, size);

            Marshal.FreeHGlobal(header.lpData);
        }

        /// <summary>
        ///     Cleanup
        /// </summary>
        ~MidiOut()
        {
            Debug.Assert(false);
            Dispose(false);
        }
    }
}