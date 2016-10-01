﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using NetTools;
using Fonlow.Diagnostics;

namespace Fonlow.TraceHub
{
    public sealed class HubSettings
    {
        private static readonly Lazy<HubSettings> lazy =
            new Lazy<HubSettings>(() => new HubSettings());

        public static HubSettings Instance { get { return lazy.Value; } }

        private HubSettings()
        {
            var rangesText= ConfigurationManager.AppSettings["loggingHub_AllowedIpAddresses"];
            AllowedIpAddresses = IPAddressRangesHelper.ParseIPAddressRanges(rangesText);

            int bufferSize = 2000;
            int.TryParse(ConfigurationManager.AppSettings["loggingHub_ClientBufferSize"], out bufferSize);
            if (bufferSize>Constants.ClientBufferSizeMax)
            {
                bufferSize = Constants.ClientBufferSizeMax;
            }
            else if (bufferSize< Constants.ClientBufferSizeMin)
            {
                bufferSize = Constants.ClientBufferSizeMin;
            }

            DirectWrite = String.Equals("true", ConfigurationManager.AppSettings["loggingHub_DirectWrite"], StringComparison.CurrentCultureIgnoreCase);

            ClientSettings = new ClientSettings
            {
                AdvancedMode=String.Equals("true", ConfigurationManager.AppSettings["loggingHub_AdvancedMode"], StringComparison.CurrentCultureIgnoreCase),
                BufferSize=bufferSize,
            };

        }

        /// <summary>
        /// A CSV of IP addresses, ranges and subnet. When this is not null or empty, only connections from these IP addresses will be allowed to call Hub server functions
        /// </summary>
        /// <remarks>To restrict client connections like Web browsers, you have better to do the restrictions on the Web server like IIS.</remarks>
        public IPAddressRange[] AllowedIpAddresses { get; private set; }

        public ClientSettings ClientSettings { get; private set; }


        /// <summary>
        /// Direct write uploaded trace to clients without pending in a priority queue for up to 1 second.
        /// </summary>
        public bool DirectWrite { get; private set; }

        public bool ClientCallRestricted
        {
            get
            {
                return AllowedIpAddresses != null;
            }
        }

        public bool AllowedToCallServer(string ipAddress)
        {
            if (AllowedIpAddresses==null)
            {
                return true;
            }

            return AllowedIpAddresses.IsInRanges(ipAddress);
        }

  
    }
}
