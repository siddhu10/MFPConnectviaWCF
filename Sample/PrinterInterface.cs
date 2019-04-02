using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Enumeration.Pnp;

namespace Sample
{
    class PrinterInterface
    {
        const string PRINTERS_CLASS_GUID_IDENTIFIER = "{0ecef634-6ef0-472a-8085-5ad023ecbccd}";
        const string INTERFACE_CLASS_GUID = "System.Devices.InterfaceClassGuid";
        const string PROPERTY_CONTAINER_ID = "System.Devices.ContainerId";
        const string PROPERTY_MODEL_NAME = "System.Devices.ModelName";
        const string PROPERTY_PRINTER_PORTNAME = "System.DeviceInterface.PrinterPortName";

        string _strIPAddress = string.Empty;
        string _strDriverName = string.Empty;

        public string DriverIP
        {
            get => _strIPAddress;
        }

        public string DriverName
        {
            get => _strDriverName;
        }

        public async Task<string> IsSupportedPrinter()
        {
            string strError = string.Empty;

            bool bIsSupportedPrin = await IsDefaultSupportedPrinter();
            if (bIsSupportedPrin)
            {
                await PopulateDriverIP();
                if (string.IsNullOrEmpty(_strIPAddress))
                {
                    strError = Helper.GetResourceString("ID_STARTUP_ERR_PORT_MSG");
                }
            }
            return strError;
        }

        async Task<bool> IsDefaultSupportedPrinter()
        {
            bool bSupported = false;
            
            string aqsFilter = INTERFACE_CLASS_GUID + ":=\"" + PRINTERS_CLASS_GUID_IDENTIFIER + "\"";

            string[] propertiesToRetrieve = new string[] { PROPERTY_CONTAINER_ID };

            DeviceInformationCollection deviceInfoCollection = await DeviceInformation.FindAllAsync(aqsFilter, propertiesToRetrieve);

            foreach (DeviceInformation deviceInfo in deviceInfoCollection)
            {
                if (deviceInfo.IsDefault == true)
                {
                    string[] containerPropertiesToGet = new string[] { PROPERTY_MODEL_NAME };
                    string containerIdwithBraces = "{" + deviceInfo.Properties[PROPERTY_CONTAINER_ID] + "}";

                    PnpObject containerInfo = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, containerIdwithBraces, containerPropertiesToGet);
                    string strModel = (string)containerInfo.Properties[PROPERTY_MODEL_NAME];

                    if (!string.IsNullOrEmpty(strModel))
                    {
                        bSupported = await IsOEMSupportedPrinter(strModel);
                        _strDriverName = bSupported ? strModel : string.Empty;
                    }
                    break;
                }
            }
            return bSupported;
        }

        async Task<bool> IsOEMSupportedPrinter(string strModel)
        {
            bool bSupported = false;
            string strOrgName = Helper.GetResourceString("ID_ORG_NAME");
            //if (strModel.Contains(strOrgName, StringComparison.OrdinalIgnoreCase))
            {
                bSupported = true;
            }
            return bSupported;
        }

        async Task PopulateDriverIP()
        {
            string aqsFilter = INTERFACE_CLASS_GUID + ":=\"" + PRINTERS_CLASS_GUID_IDENTIFIER + "\"";

            string[] propertiesToRetrieve = new string[] { PROPERTY_PRINTER_PORTNAME };

            DeviceInformationCollection deviceInfoCollection = await DeviceInformation.FindAllAsync(aqsFilter, propertiesToRetrieve);

            foreach (DeviceInformation deviceInfo in deviceInfoCollection)
            {
                if (deviceInfo.IsDefault == true)
                {
                    string strPortName = (string)deviceInfo.Properties[PROPERTY_PRINTER_PORTNAME];

                    if (!string.IsNullOrEmpty(strPortName))
                    {
                        strPortName = await ParsePortName(strPortName);
                        if (!string.IsNullOrEmpty(strPortName))
                        {
                            _strIPAddress = strPortName;
                        }
                    }
                    break;
                }
            }
        }

        async Task<string> ParsePortName(string strPort)
        {
            string strIP = string.Empty;
            try
            {
                IPAddress ipAddr = IPAddress.Parse(strPort);
                strIP = ipAddr.ToString();
            }
            catch (FormatException ex)
            {
                if (strPort.Contains("_", StringComparison.OrdinalIgnoreCase) == true)
                {
                    string[] temp = strPort.Split('_');
                    foreach (string strTemp in temp)
                    {
                        if (true == ValidateIP(strTemp))
                        {
                            strIP = strTemp;
                            break;
                        }
                    }
                }
            }
            return strIP;
        }

        bool ValidateIP(string strIPAddress)
        {
            bool bValid = true;
            Regex regExpr = new Regex("\\b(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\\." +
                                        "(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\\." +
                                        "(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\\." +
                                        "(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\\b");
            bValid = regExpr.IsMatch(strIPAddress);
            return bValid;
        }
    }
}
