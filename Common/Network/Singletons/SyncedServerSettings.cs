﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Ciribob.SRS.Common.Setting;
using NLog;

namespace Ciribob.SRS.Common.Network.Singletons
{
    public class SyncedServerSettings
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static SyncedServerSettings instance;
        private static readonly object _lock = new object();
        private static readonly Dictionary<string, string> defaults = DefaultServerSettings.Defaults;

        private readonly ConcurrentDictionary<string, string> _settings;

        public List<double> GlobalFrequencies { get; set; } = new List<double>();

        // Node Limit of 0 means no retransmission
        public int RetransmitNodeLimit { get; set; } = 0;

        public SyncedServerSettings()
        {
            _settings = new ConcurrentDictionary<string, string>();
        }

        public static SyncedServerSettings Instance
        {
            get
            {
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = new SyncedServerSettings();
                    }
                }
                return instance;
            }
        }

        public string GetSetting(ServerSettingsKeys key)
        {
            string setting = key.ToString();

            return _settings.GetOrAdd(setting, defaults.ContainsKey(setting) ? defaults[setting] : "");
        }

        public bool GetSettingAsBool(ServerSettingsKeys key)
        {
            return Convert.ToBoolean(GetSetting(key));
        }

        public void Decode(Dictionary<string, string> encoded)
        {
            foreach (KeyValuePair<string, string> kvp in encoded)
            {
                _settings.AddOrUpdate(kvp.Key, kvp.Value, (key, oldVal) => kvp.Value);
                
                if(kvp.Key.Equals(ServerSettingsKeys.RETRANSMISSION_NODE_LIMIT.ToString()))
                {
                    if (!int.TryParse(kvp.Value, out var nodeLimit))
                    {
                        nodeLimit = 0;
                    }
                    else
                    {
                        RetransmitNodeLimit = nodeLimit;
                    }


                }
            }

            MessageHubSingleton.Instance.Publish(new ServerSettingsUpdatedMessage(_settings));
        }
    }
}
