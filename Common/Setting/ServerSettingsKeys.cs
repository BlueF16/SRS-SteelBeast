﻿using System.Collections.Generic;

namespace Ciribob.SRS.Common.Setting
{
    public enum ServerSettingsKeys
    {
        SERVER_PORT = 0,
        LOS_ENABLED = 1,
        DISTANCE_ENABLED = 2,
        IRL_RADIO_TX = 3,
        IRL_RADIO_RX_INTERFERENCE = 4,
        IRL_RADIO_STATIC = 5, // Not used
        TEST_FREQUENCIES = 6,
        SHOW_TUNED_COUNT = 7,
        SHOW_TRANSMITTER_NAME = 8,
        RETRANSMISSION_NODE_LIMIT = 9
    }

    public class DefaultServerSettings
    {
        public static readonly Dictionary<string, string> Defaults = new Dictionary<string, string>()
        {
            { ServerSettingsKeys.DISTANCE_ENABLED.ToString(), "false" },
            { ServerSettingsKeys.IRL_RADIO_RX_INTERFERENCE.ToString(), "false" },
            { ServerSettingsKeys.IRL_RADIO_STATIC.ToString(), "false" },
            { ServerSettingsKeys.IRL_RADIO_TX.ToString(), "false" },
            { ServerSettingsKeys.LOS_ENABLED.ToString(), "false" },
            { ServerSettingsKeys.SERVER_PORT.ToString(), "5002" },
            { ServerSettingsKeys.TEST_FREQUENCIES.ToString(), "247.2,120.3" },
            { ServerSettingsKeys.SHOW_TUNED_COUNT.ToString(), "true" },
            { ServerSettingsKeys.SHOW_TRANSMITTER_NAME.ToString(), "false" },
            { ServerSettingsKeys.RETRANSMISSION_NODE_LIMIT.ToString(), "0" }
        };
    }
}