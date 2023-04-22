﻿using System;
using Ciribob.FS3D.SimpleRadio.Standalone.Audio;
using Ciribob.FS3D.SimpleRadio.Standalone.Common.Audio.Models;
using Ciribob.FS3D.SimpleRadio.Standalone.Common.Audio.Opus.Core;
using Ciribob.FS3D.SimpleRadio.Standalone.Common.Network.Client;
using NLog;

namespace Ciribob.FS3D.SimpleRadio.Standalone.Common.Audio.Providers;

public abstract class AudioProvider
{
    public static readonly int SILENCE_PAD = 200;

    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected OpusDecoder _decoder;

    protected bool passThrough;

    protected AudioProvider(bool passThrough)
    {
        this.passThrough = passThrough;

        _decoder = OpusDecoder.Create(Constants.OUTPUT_SAMPLE_RATE, 1);
        _decoder.ForwardErrorCorrection = false;
        _decoder.MaxDataBytes = Constants.OUTPUT_SAMPLE_RATE * 4;
    }

    public long LastUpdate { get; set; }

    //is it a new transmission?
    protected bool LikelyNewTransmission()
    {
        if (passThrough) return false;

        //400 ms since last update
        var now = DateTime.Now.Ticks;
        if (now - LastUpdate > 4000000) //400 ms since last update
            return true;

        return false;
    }

    public abstract JitterBufferAudio AddClientAudioSamples(ClientAudio audio);


    //destructor to clear up opus
    ~AudioProvider()
    {
        _decoder?.Dispose();
        _decoder = null;
    }
}