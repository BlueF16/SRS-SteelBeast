﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Ciribob.FS3D.SimpleRadio.Standalone.Client.Annotations;
using Ciribob.FS3D.SimpleRadio.Standalone.Client.Settings;
using Ciribob.FS3D.SimpleRadio.Standalone.Client.Utils;
using Ciribob.SRS.Common.Helpers;
using Ciribob.SRS.Common.Network.Client;
using Ciribob.SRS.Common.Network.Models;
using Ciribob.SRS.Common.Network.Models.EventMessages;
using Ciribob.SRS.Common.Network.Singletons;
using Newtonsoft.Json;
using NLog;

namespace Ciribob.FS3D.SimpleRadio.Standalone.Client.Singletons.Models
{

    public class PlayerUnitState : PropertyChangedBase
    {
        private  static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string HANDHELD_RADIO_JSON = "handheld-radio.json";
        private static readonly string MULTI_RADIO_JSON = "multi-radio.json";
        public static readonly string TYPE_AIRCRAFT = "AIRCRAFT";
        public static readonly string TYPE_GROUND = "GROUND";

        //HOTAS or IN COCKPIT controls
        public enum RadioSwitchControls
        {
            HOTAS = 0,
            IN_COCKPIT = 1
        }

        public enum SimultaneousTransmissionControl
        {
            ENABLED_INTERNAL_SRS_CONTROLS = 1
        }

        [JsonIgnore] public static readonly int NUMBER_OF_RADIOS = 11;

        public static readonly uint
            UnitIdOffset = 100000000; // this is where non aircraft "Unit" Ids start from for satcom intercom

        public RadioSwitchControls control = RadioSwitchControls.HOTAS;


        public bool InAircraft { get; set; }

        public int Coalition { get; set; }

        public LatLngPosition LatLng { get; set; } = new();

        public Transponder Transponder { get; set; } = new();

        public string UnitType
        {
            get => _unitType;
            set
            {
                _unitType = value;

                EventBus.Instance.PublishOnBackgroundThreadAsync(new UnitUpdateMessage() { FullUpdate = false, UnitUpdate = ClientStateSingleton.Instance.PlayerUnitState.PlayerUnitStateBase });
            }
        }

        public string Name
        {
            get => GlobalSettingsStore.Instance.GetClientSetting(GlobalSettingsKeys.LastUsedName).RawValue;
            set
            {
                GlobalSettingsStore.Instance.SetClientSetting(GlobalSettingsKeys.LastUsedName, value);
                EventBus.Instance.PublishOnBackgroundThreadAsync(new UnitUpdateMessage() { FullUpdate = false, UnitUpdate = ClientStateSingleton.Instance.PlayerUnitState.PlayerUnitStateBase });
            }
        }

        public uint UnitId
        {
            get => GlobalSettingsStore.Instance.GetUnitId();
            set
            {
                _unitId = value;

                GlobalSettingsStore.Instance.SetUnitID(value);
                EventBus.Instance.PublishOnBackgroundThreadAsync(new UnitUpdateMessage() { FullUpdate = false, UnitUpdate = ClientStateSingleton.Instance.PlayerUnitState.PlayerUnitStateBase });
            }
        }


        public bool
            simultaneousTransmission; // Global toggle enabling simultaneous transmission on multiple radios, activated via the AWACS panel

        public SimultaneousTransmissionControl simultaneousTransmissionControl =
            SimultaneousTransmissionControl.ENABLED_INTERNAL_SRS_CONTROLS;

        private string _unitType = "";
        private uint _unitId;

        public PlayerUnitState()
        {
            //initialise with 11 things
            //10 radios +1 intercom (the first one is intercom)
            //try the handheld radios first
            Radios = new ObservableCollection<Radio>(Radio.LoadRadioConfig(HANDHELD_RADIO_JSON));
            SelectedRadio = 1;
        }

        public void LoadMultiRadio()
        {
            Radios = new ObservableCollection<Radio>(Radio.LoadRadioConfig(MULTI_RADIO_JSON));
            SelectedRadio = 1;
            UnitType = TYPE_AIRCRAFT;
            IntercomHotMic = true;
            EventBus.Instance.PublishOnBackgroundThreadAsync(new UnitUpdateMessage() { FullUpdate = true, UnitUpdate = ClientStateSingleton.Instance.PlayerUnitState.PlayerUnitStateBase });
        }

        public void LoadHandHeldRadio()
        {
            Radios = new ObservableCollection<Radio>(Radio.LoadRadioConfig(HANDHELD_RADIO_JSON));
            SelectedRadio = 1;
            UnitType = TYPE_GROUND;
            IntercomHotMic = false;
            EventBus.Instance.PublishOnBackgroundThreadAsync(new UnitUpdateMessage() { FullUpdate = true, UnitUpdate = ClientStateSingleton.Instance.PlayerUnitState.PlayerUnitStateBase });
        }

        public int Seat { get; set; }

        public short SelectedRadio { get; set; } 

        public bool IntercomHotMic { get; set; } = true;

        public ObservableCollection<Radio> Radios { get; private set; } = new();

        public List<RadioBase> BaseRadios
        {
            get
            {
                List<RadioBase> radios = new List<RadioBase>();
                foreach (var radio in Radios)
                {
                    radios.Add(radio.RadioBase);
                    
                }

                return radios;
            }
        }

        public PlayerUnitStateBase PlayerUnitStateBase
        {
            get
            {
                return new PlayerUnitStateBase()
                {
                    Coalition = Coalition,
                    Name = Name,
                    LatLng = LatLng,
                    Radios = BaseRadios,
                    Transponder = Transponder,
                    UnitId = UnitId,
                    UnitType = UnitType
                };
            }
        }
        //
        // public void Reset()
        // {
        //     Name = "";
        //     LatLng = new LatLngPosition();
        //     SelectedRadio = 0;
        //     UnitType = "";
        //     simultaneousTransmission = false;
        //     simultaneousTransmissionControl = SimultaneousTransmissionControl.ENABLED_INTERNAL_SRS_CONTROLS;
        //     LastUpdate = 0;
        //
        //     Radios.Clear();
        //     for (var i = 0; i < NUMBER_OF_RADIOS; i++) Radios.Add(new Radio());
        // }


    }
}