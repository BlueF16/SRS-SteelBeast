﻿using System;
using System.Net;
using System.Threading;
using Ciribob.SRS.Common;
using Ciribob.SRS.Common.DCSState;
using Ciribob.SRS.Common.Network;
using Ciribob.FS3D.SimpleRadio.Standalone.ExternalAudioClient.Audio;
using Ciribob.FS3D.SimpleRadio.Standalone.ExternalAudioClient.Models;
using Ciribob.FS3D.SimpleRadio.Standalone.ExternalAudioClient.Network;
using Ciribob.SRS.Common.Network.Models;
using Ciribob.SRS.Common.Network.Singletons;
using Ciribob.SRS.Common.PlayerState;
using NLog;
using Timer = Cabhishek.Timers.Timer;

namespace Ciribob.FS3D.SimpleRadio.Standalone.ExternalAudioClient.Client
{
    internal class ExternalAudioClient
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private double[] freq;
        private Modulation[] modulation;
        private byte[] modulationBytes;

        private readonly string Guid = ShortGuid.NewGuid();

        private CancellationTokenSource finished = new CancellationTokenSource();
        private PlayerUnitState gameState;
        private UdpVoiceHandler udpVoiceHandler;
        private Program.Options opts;

        public ExternalAudioClient(double[] freq, Modulation[] modulation, Program.Options opts)
        {
            this.freq = freq;
            this.modulation = modulation;
            this.opts = opts;
            this.modulationBytes = new byte[modulation.Length];
            for (int i = 0; i < modulationBytes.Length; i++)
            {
                modulationBytes[i] = (byte)modulation[i];
            }
        }

        public void Start()
        {

            MessageHubSingleton.Instance.Subscribe<ReadyMessage>(ReadyToSend);
            MessageHubSingleton.Instance.Subscribe<DisconnectedMessage>(Disconnected);

            gameState = new PlayerUnitState();
            gameState.Radios[1].Modulation = modulation[0];
            gameState.Radios[1].Frequency = freq[0]; // get into Hz
            gameState.Radios[1].Name = opts.Name;

            Logger.Info($"Starting with params:");
            for (int i = 0; i < freq.Length; i++)
            {
                Logger.Info($"Frequency: {freq[i]} Hz - {modulation[i]} ");
            }

            LatLngPosition position = new LatLngPosition()
            {
                alt = opts.Altitude,
                lat = opts.Latitude,
                lng = opts.Longitude
            };

            var srsClientSyncHandler = new SRSClientSyncHandler(Guid, gameState,opts.Name, opts.Coalition,position);

            srsClientSyncHandler.TryConnect(new IPEndPoint(IPAddress.Loopback, opts.Port));

            //wait for it to end
            finished.Token.WaitHandle.WaitOne();
            Logger.Info("Finished - Closing");

            udpVoiceHandler?.RequestStop();
            srsClientSyncHandler?.Disconnect();

            MessageHubSingleton.Instance.ClearSubscriptions();
        }

        private void ReadyToSend(ReadyMessage ready)
        {
            if (udpVoiceHandler == null)
            {
                Logger.Info($"Connecting UDP VoIP");
                udpVoiceHandler = new UdpVoiceHandler(Guid, IPAddress.Loopback, opts.Port, gameState);
                udpVoiceHandler.Start();
                new Thread(SendAudio).Start();
            }
        }

        private void Disconnected(DisconnectedMessage disconnected)
        {
            finished.Cancel();
        }

        private void SendAudio()
        {
            Logger.Info("Sending Audio... Please Wait");
            AudioGenerator audioGenerator = new AudioGenerator(opts);
            var opusBytes = audioGenerator.GetOpusBytes();
            int count = 0;

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            //get all the audio as Opus frames of 40 ms
            //send on 40 ms timer 

            //when empty - disconnect
            //user timer for accurate sending
            var _timer = new Timer(() =>
            {

                if (!finished.IsCancellationRequested)
                {
                    if (count < opusBytes.Count)
                    {
                        udpVoiceHandler.Send(opusBytes[count], opusBytes[count].Length, freq, modulationBytes);
                        count++;

                        if (count % 50 == 0)
                        {
                            Logger.Info($"Playing audio - sent {count * 40}ms - {((float)count / (float)opusBytes.Count) * 100.0:F0}% ");
                        }
                    }
                    else
                    {
                        tokenSource.Cancel();
                    }
                }
                else
                {
                    Logger.Error("Client Disconnected");
                    tokenSource.Cancel();
                    return;
                }

            }, TimeSpan.FromMilliseconds(40));
            _timer.Start();

            //wait for cancel
            tokenSource.Token.WaitHandle.WaitOne();
            _timer.Stop();

            Logger.Info("Finished Sending Audio");
            finished.Cancel();
        }
    }
}