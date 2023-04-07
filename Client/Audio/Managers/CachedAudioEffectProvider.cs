﻿// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using Ciribob.FS3D.SimpleRadio.Standalone.Client.Audio.Models;
// using Ciribob.FS3D.SimpleRadio.Standalone.Client.Settings;
// using Ciribob.SRS.Common.Helpers;
//
// namespace Ciribob.FS3D.SimpleRadio.Standalone.Client.Audio.Managers
// {
//     internal class CachedAudioEffectProvider
//     {
//         private static CachedAudioEffectProvider _instance;
//
//         private readonly string sourceFolder;
//
//         private CachedAudioEffectProvider()
//         {
//             sourceFolder = AppDomain.CurrentDomain.BaseDirectory + "\\AudioEffects\\";
//
//             //init lists
//             RadioTransmissionStart = new List<CachedAudioEffect>();
//             RadioTransmissionEnd = new List<CachedAudioEffect>();
//
//             LoadRadioStartAndEndEffects();
//
//             KY58EncryptionTransmitTone = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.KY_58_TX);
//             KY58EncryptionEndTone = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.KY_58_RX);
//
//             NATOTone = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.NATO_TONE);
//
//             MIDSTransmitTone = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.MIDS_TX);
//             MIDSEndTone = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.MIDS_TX_END);
//
//             HAVEQUICKTone = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.HAVEQUICK_TONE);
//
//             FMNoise = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.FM_NOISE);
//             VHFNoise = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.VHF_NOISE);
//             UHFNoise = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.UHF_NOISE);
//             HFNoise = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.HF_NOISE);
//
//             AircraftNoise = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.AIRCRAFT_NOISE);
//             GroundNoise = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.GROUND_NOISE);
//
//             //sort out volume (if needed)
//             CreateAudioEffectDouble(HAVEQUICKTone);
//             CreateAudioEffectDouble(NATOTone);
//             CreateAudioEffectDouble(FMNoise);
//             CreateAudioEffectDouble(UHFNoise);
//             CreateAudioEffectDouble(VHFNoise);
//             CreateAudioEffectDouble(HFNoise);
//             CreateAudioEffectDouble(AircraftNoise);
//             CreateAudioEffectDouble(GroundNoise);
//         }
//
//       
//
//         public List<CachedAudioEffect> RadioTransmissionStart { get; }
//         public List<CachedAudioEffect> RadioTransmissionEnd { get; }
//
//         public static CachedAudioEffectProvider Instance
//         {
//             get
//             {
//                 if (_instance == null)
//                     _instance = new CachedAudioEffectProvider();
//
//                 //stops cyclic init
//                 return _instance;
//             }
//         }
//
//         public CachedAudioEffect SelectedRadioTransmissionStartEffect
//         {
//             get
//             {
//                 var selectedTone = GlobalSettingsStore.Instance.ProfileSettingsStore
//                     .GetClientSettingString(ProfileSettingsKeys.RadioTransmissionStartSelection).ToLowerInvariant();
//
//                 foreach (var startEffect in RadioTransmissionStart)
//                     if (startEffect.FileName.ToLowerInvariant().Equals(selectedTone))
//                         return startEffect;
//
//                 return RadioTransmissionStart[0];
//             }
//         }
//
//         public CachedAudioEffect SelectedRadioTransmissionEndEffect
//         {
//             get
//             {
//                 var selectedTone = GlobalSettingsStore.Instance.ProfileSettingsStore
//                     .GetClientSettingString(ProfileSettingsKeys.RadioTransmissionEndSelection).ToLowerInvariant();
//
//                 foreach (var endEffect in RadioTransmissionEnd)
//                     if (endEffect.FileName.ToLowerInvariant().Equals(selectedTone))
//                         return endEffect;
//
//                 return RadioTransmissionEnd[0];
//             }
//         }
//
//         public CachedAudioEffect KY58EncryptionTransmitTone { get; }
//         public CachedAudioEffect KY58EncryptionEndTone { get; }
//         public CachedAudioEffect NATOTone { get; }
//         public CachedAudioEffect MIDSTransmitTone { get; }
//         public CachedAudioEffect MIDSEndTone { get; }
//
//         public CachedAudioEffect HAVEQUICKTone { get; }
//
//         public CachedAudioEffect FMNoise { get; }
//         public CachedAudioEffect UHFNoise { get; }
//         public CachedAudioEffect VHFNoise { get; }
//         public CachedAudioEffect HFNoise { get; }
//         public CachedAudioEffect GroundNoise { get; }
//         public CachedAudioEffect AircraftNoise { get; }
//
//         private void CreateAudioEffectDouble(CachedAudioEffect effect)
//         {
//             if (effect.Loaded)
//             {
//                 var effectShort = ConversionHelpers.ByteArrayToShortArray(effect.AudioEffectBytes);
//                 var effectDouble = new double[effectShort.Length];
//
//                 for (var i = 0; i < effectShort.Length; i++) effectDouble[i] = effectShort[i] / 32768f;
//                 effect.AudioEffectDouble = effectDouble;
//             }
//         }
//
//         private void LoadRadioStartAndEndEffects()
//         {
//             if (Directory.Exists(sourceFolder))
//             {
//                 var audioEffectsList = Directory.EnumerateFiles(sourceFolder);
//
//                 //might need to split the path - we'll see
//                 foreach (var effectPath in audioEffectsList)
//                 {
//                     var effect = effectPath.Split(Path.DirectorySeparatorChar).Last();
//
//                     if (effect.ToLowerInvariant().StartsWith(CachedAudioEffect.AudioEffectTypes.RADIO_TRANS_START
//                         .ToString().ToLowerInvariant()))
//                     {
//                         var audioEffect = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.RADIO_TRANS_START,
//                             effect, effectPath);
//
//                         if (audioEffect.AudioEffectBytes != null) RadioTransmissionStart.Add(audioEffect);
//                     }
//                     else if (effect.ToLowerInvariant().StartsWith(CachedAudioEffect.AudioEffectTypes.RADIO_TRANS_END
//                         .ToString().ToLowerInvariant()))
//                     {
//                         var audioEffect = new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.RADIO_TRANS_END,
//                             effect,
//                             effectPath);
//
//                         if (audioEffect.AudioEffectBytes != null) RadioTransmissionEnd.Add(audioEffect);
//                     }
//                 }
//             }
//
//             //IF the audio folder is missing - to avoid a crash, init with a blank one
//             if (RadioTransmissionStart.Count == 0)
//                 RadioTransmissionStart.Add(new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.RADIO_TRANS_START));
//             if (RadioTransmissionEnd.Count == 0)
//                 RadioTransmissionEnd.Add(new CachedAudioEffect(CachedAudioEffect.AudioEffectTypes.RADIO_TRANS_END));
//         }
//     }
// }