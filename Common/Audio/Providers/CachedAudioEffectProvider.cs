﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ciribob.FS3D.SimpleRadio.Standalone.Client.Settings;
using Ciribob.FS3D.SimpleRadio.Standalone.Common.Audio.Models;
using Ciribob.FS3D.SimpleRadio.Standalone.Common.Settings;

namespace Ciribob.FS3D.SimpleRadio.Standalone.Common.Audio.Providers;

public class CachedAudioEffectProvider
{
    private static CachedAudioEffectProvider _instance;

    private readonly string sourceFolder;

    private CachedAudioEffectProvider()
    {
        sourceFolder = AppDomain.CurrentDomain.BaseDirectory + "\\AudioEffects\\";

        //init lists
        RadioTransmissionStart = new List<CachedAudioEffect>();
        RadioTransmissionEnd = new List<CachedAudioEffect>();

        IntercomTransmissionStart = new List<CachedAudioEffect>();
        IntercomTransmissionEnd = new List<CachedAudioEffect>();

        LoadRadioStartAndEndEffects();
        LoadIntercomStartAndEndEffects();

        NATOTone = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.NATO_TONE);
        ;

        FMNoise = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.FM_NOISE);
        VHFNoise = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.VHF_NOISE);
        UHFNoise = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.UHF_NOISE);
        HFNoise = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.HF_NOISE);

        //sort out volume (if needed)
        // CreateAudioEffectDouble(HAVEQUICKTone);
        // CreateAudioEffectDouble(NATOTone);
        // CreateAudioEffectDouble(FMNoise);
        // CreateAudioEffectDouble(UHFNoise);
        // CreateAudioEffectDouble(VHFNoise);
        // CreateAudioEffectDouble(HFNoise);

        AMCollision = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.AM_COLLISION);

        GroundNoise = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.GROUND_NOISE);

        AircraftNoise = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.AIRCRAFT_NOISE);
        // CreateAudioEffectDouble(AMCollision);
    }

    public List<CachedAudioEffect> RadioTransmissionStart { get; }
    public List<CachedAudioEffect> RadioTransmissionEnd { get; }

    public List<CachedAudioEffect> IntercomTransmissionStart { get; }
    public List<CachedAudioEffect> IntercomTransmissionEnd { get; }

    public static CachedAudioEffectProvider Instance
    {
        get
        {
            if (_instance == null) _instance = new CachedAudioEffectProvider();
            //stops cyclic init
            return _instance;
        }
    }

    public CachedAudioEffect SelectedRadioTransmissionStartEffect
    {
        get
        {
            var selectedTone = GlobalSettingsStore.Instance.ProfileSettingsStore
                .GetClientSettingString(ProfileSettingsKeys.RadioTransmissionStartSelection).ToLowerInvariant();

            foreach (var startEffect in RadioTransmissionStart)
                if (startEffect.FileName.ToLowerInvariant().Equals(selectedTone))
                    return startEffect;

            return RadioTransmissionStart[0];
        }
    }

    public CachedAudioEffect SelectedRadioTransmissionEndEffect
    {
        get
        {
            var selectedTone = GlobalSettingsStore.Instance.ProfileSettingsStore
                .GetClientSettingString(ProfileSettingsKeys.RadioTransmissionEndSelection).ToLowerInvariant();

            foreach (var endEffect in RadioTransmissionEnd)
                if (endEffect.FileName.ToLowerInvariant().Equals(selectedTone))
                    return endEffect;

            return RadioTransmissionEnd[0];
        }
    }

    public CachedAudioEffect SelectedIntercomTransmissionStartEffect
    {
        get
        {
            var selectedTone = GlobalSettingsStore.Instance.ProfileSettingsStore
                .GetClientSettingString(ProfileSettingsKeys.IntercomTransmissionStartSelection).ToLowerInvariant();

            foreach (var startEffect in IntercomTransmissionStart)
                if (startEffect.FileName.ToLowerInvariant().Equals(selectedTone))
                    return startEffect;

            return IntercomTransmissionStart[0];
        }
    }

    public CachedAudioEffect SelectedIntercomTransmissionEndEffect
    {
        get
        {
            var selectedTone = GlobalSettingsStore.Instance.ProfileSettingsStore
                .GetClientSettingString(ProfileSettingsKeys.IntercomTransmissionEndSelection).ToLowerInvariant();

            foreach (var endEffect in IntercomTransmissionEnd)
                if (endEffect.FileName.ToLowerInvariant().Equals(selectedTone))
                    return endEffect;

            return IntercomTransmissionEnd[0];
        }
    }

    public CachedAudioEffect NATOTone { get; }


    public CachedAudioEffect FMNoise { get; }
    public CachedAudioEffect UHFNoise { get; }
    public CachedAudioEffect VHFNoise { get; }
    public CachedAudioEffect HFNoise { get; }

    public CachedAudioEffect AMCollision { get; set; }

    public CachedAudioEffect GroundNoise { get; }

    public CachedAudioEffect AircraftNoise { get; }

    private void LoadRadioStartAndEndEffects()
    {
        if (Directory.Exists(sourceFolder))
        {
            var audioEffectsList = Directory.EnumerateFiles(sourceFolder);

            //might need to split the path - we'll see
            foreach (var effectPath in audioEffectsList)
            {
                var effect = effectPath.Split(Path.DirectorySeparatorChar).Last();

                if (effect.ToLowerInvariant().StartsWith(CachedAudioEffect.AudioEffectTypes.RADIO_TRANS_START
                        .ToString().ToLowerInvariant()))
                {
                    var audioEffect = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.RADIO_TRANS_START,
                        effect, effectPath);

                    if (audioEffect.AudioEffectFloat != null) RadioTransmissionStart.Add(audioEffect);
                }
                else if (effect.ToLowerInvariant().StartsWith(CachedAudioEffect.AudioEffectTypes.RADIO_TRANS_END
                             .ToString().ToLowerInvariant()))
                {
                    var audioEffect = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.RADIO_TRANS_END,
                        effect, effectPath);

                    if (audioEffect.AudioEffectFloat != null) RadioTransmissionEnd.Add(audioEffect);
                }
            }

            //IF the audio folder is missing - to avoid a crash, init with a blank one
            if (RadioTransmissionStart.Count == 0)
                RadioTransmissionStart.Add(
                    new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.RADIO_TRANS_START));

            if (RadioTransmissionEnd.Count == 0)
                RadioTransmissionEnd.Add(new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.RADIO_TRANS_END));
        }
        else
        {
            //IF the audio folder is missing - to avoid a crash, init with a blank one
            if (RadioTransmissionStart.Count == 0)
                RadioTransmissionStart.Add(
                    new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.RADIO_TRANS_START));

            if (RadioTransmissionEnd.Count == 0)
                RadioTransmissionEnd.Add(new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.RADIO_TRANS_END));
        }
    }

    private void LoadIntercomStartAndEndEffects()
    {
        if (Directory.Exists(sourceFolder))
        {
            var audioEffectsList = Directory.EnumerateFiles(sourceFolder);

            //might need to split the path - we'll see
            foreach (var effectPath in audioEffectsList)
            {
                var effect = effectPath.Split(Path.DirectorySeparatorChar).Last();

                if (effect.ToLowerInvariant().StartsWith(CachedAudioEffect.AudioEffectTypes.INTERCOM_TRANS_START
                        .ToString().ToLowerInvariant()))
                {
                    var audioEffect = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.INTERCOM_TRANS_START,
                        effect, effectPath);

                    if (audioEffect.AudioEffectFloat != null) IntercomTransmissionStart.Add(audioEffect);
                }
                else if (effect.ToLowerInvariant().StartsWith(CachedAudioEffect.AudioEffectTypes.INTERCOM_TRANS_END
                             .ToString().ToLowerInvariant()))
                {
                    var audioEffect = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.INTERCOM_TRANS_END,
                        effect, effectPath);

                    if (audioEffect.AudioEffectFloat != null) IntercomTransmissionEnd.Add(audioEffect);
                }
            }

            //IF the audio folder is missing - to avoid a crash, init with a blank one
            if (IntercomTransmissionStart.Count == 0)
                IntercomTransmissionStart.Add(
                    new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.INTERCOM_TRANS_START));

            if (IntercomTransmissionEnd.Count == 0)
                IntercomTransmissionEnd.Add(
                    new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.INTERCOM_TRANS_END));
        }
        else
        {
            //IF the audio folder is missing - to avoid a crash, init with a blank one
            if (IntercomTransmissionStart.Count == 0)
                IntercomTransmissionStart.Add(
                    new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.INTERCOM_TRANS_START));

            if (IntercomTransmissionEnd.Count == 0)
                IntercomTransmissionEnd.Add(
                    new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.INTERCOM_TRANS_END));
        }
    }
}