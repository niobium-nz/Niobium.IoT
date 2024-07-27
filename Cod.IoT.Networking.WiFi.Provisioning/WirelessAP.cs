using System;
using System.Net.NetworkInformation;
using AuthenticationType = System.Net.NetworkInformation.AuthenticationType;

namespace Cod.IoT.Networking.WiFi.Provisioning
{
    /// <summary>
    /// Provides methods and properties to manage a wireless access point.
    /// </summary>
    internal static class WirelessAP
    {
        /// <summary>
        /// Disable the Soft AP for next restart.
        /// </summary>
        public static void Disable()
        {
            WirelessAPConfiguration wapconf = Helper.GetWiFiAPConfiguration();
            wapconf.Options = WirelessAPConfiguration.ConfigurationOptions.None;
            wapconf.SaveConfiguration();
        }

        /// <summary>
        /// Set-up the Wireless AP settings, enable and save
        /// </summary>
        /// <returns>True if already set-up</returns>
        public static bool Setup(string ip, string netmask, string ssid, string password)
        {
            NetworkInterface ni = Helper.GetWiFiAPInterface();
            WirelessAPConfiguration wapconf = Helper.GetWiFiAPConfiguration();

            // Check if already Enabled and return true
            if (wapconf.Options == (WirelessAPConfiguration.ConfigurationOptions.Enable |
                                    WirelessAPConfiguration.ConfigurationOptions.AutoStart)
                && ni.IPv4Address == ip)
            {
                return true;
            }

            // Set up IP address for Soft AP
            ni.EnableStaticIPv4(ip, netmask, ip);

            // Set Options for Network Interface
            //
            // Enable    - Enable the Soft AP ( Disable to reduce power )
            // AutoStart - Start Soft AP when system boots.
            // HiddenSSID- Hide the SSID
            //
            wapconf.Options = WirelessAPConfiguration.ConfigurationOptions.AutoStart |
                            WirelessAPConfiguration.ConfigurationOptions.Enable;

            // Set the SSID for Access Point. If not set will use default  "nano_xxxxxx"
            wapconf.Ssid = $"{ssid}-{BitConverter.ToString(ni.PhysicalAddress).Substring(12)}";

            // Maximum number of simultaneous connections, reserves memory for connections
            wapconf.MaxConnections = 3;

            // To set-up Access point with no Authentication
            //wapconf.Authentication = System.Net.NetworkInformation.AuthenticationType.Open;
            //wapconf.Password = string.Empty;

            // To set up Access point with no Authentication. Password minimum 8 chars.
            wapconf.Authentication = AuthenticationType.WPA2;
            wapconf.Password = password;

            // Save the configuration so on restart Access point will be running.
            wapconf.SaveConfiguration();

            return false;
        }
    }
}

