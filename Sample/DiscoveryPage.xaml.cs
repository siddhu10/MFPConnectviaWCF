using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System.Net;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DiscoveryPage : Page, INotifyPropertyChanged
    {
        //ListView Information as a collection
        public ObservableCollection<PrinterInfo> PrinterInformation;
        //Printer info List received from snmp Discover
        public static IList<String> list = new List<String>();
        public event PropertyChangedEventHandler PropertyChanged;

        //OID List
        public const string OID_NAME = "1.3.6.1.2.1.43.5.1.1.16.1";
        public const string OID_MODEL = "1.3.6.1.2.1.1.5.0";
        public const string OID_MAC_ADDRESS = "1.3.6.1.2.1.2.2.1.6.2";
        public const string OID_SERIAL_NO = "1.3.6.1.2.1.43.5.1.1.17.1";

        Dictionary<string, string> dOIDMap = null;
        Dictionary<string, string> dOIDRes = null;

        public DiscoveryPage()
        {
            this.InitializeComponent();
            dOIDRes = new Dictionary<string, string>();
            dOIDMap = new Dictionary<string, string>();

            dOIDMap.Add(OID_NAME, "Name");
            dOIDMap.Add(OID_MODEL, "Model");
            dOIDMap.Add(OID_MAC_ADDRESS, "Physical Address");
            dOIDMap.Add(OID_SERIAL_NO, "Serial No");
        }

        private bool _bIsActive = false;
        public bool IsProgressBarActive
        {
            get => _bIsActive;
            set
            {
                _bIsActive = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsProgressBarActive"));
                }
            }
        }

        private Visibility _bIsVisible = Visibility.Collapsed;
        public Visibility ShowProgressBar
        {
            get => _bIsVisible;
            set
            {
                _bIsVisible = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ShowProgressBar"));
                }
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {            
            PrinterInformation = new ObservableCollection<PrinterInfo>();
            discPageTitle.Text = Helper.GetResourceString("IDS_DISCOVERMFP");
        }

        public void GetPrinterDetails()
        {
            List<Variable> lstVars = new List<Variable>();
            foreach (string OID in dOIDMap.Keys)
            {
                Variable vTemp = new Variable(new ObjectIdentifier(OID));
                lstVars.Add(vTemp);
            }
            dOIDRes.Clear();
            PrinterInformation.Clear();

            foreach (string x in list)
            {
                try
                {
                    dOIDRes.Clear();
                    var result = Messenger.Get(VersionCode.V1, new IPEndPoint(IPAddress.Parse(x), 161), new OctetString("private"), lstVars, 6000);

                    foreach (var i in result)
                    {
                        string strTemp = CheckForSpecialParams(i);
                        dOIDRes.Add(i.Id.ToString(), strTemp);
                    }

                    PrinterInformation.Add(new PrinterInfo { Name = dOIDRes[OID_NAME], IpAddress = x, Model = dOIDRes[OID_MODEL], MacAddress = dOIDRes[OID_MAC_ADDRESS], SerialNumber = dOIDRes[OID_SERIAL_NO] });
                    var c = 0;
                    c = c * PrinterInformation.Count;
                }
                catch (Lextm.SharpSnmpLib.Messaging.TimeoutException tx)
                {
                    //TBD
                }
            }
        }

        private string CheckForSpecialParams(Variable vParam)
        {
            string strID = vParam.Id.ToString();
            string strValue = vParam.Data.ToString();

            try
            {
                switch (strID)
                {
                    case OID_MAC_ADDRESS:
                        string stTemp = null;
                        char[] cArr = strValue.ToCharArray();
                        foreach (char c in cArr)
                        {
                            stTemp += Convert.ToInt32(c).ToString("X2") + ":";
                        }
                        strValue = stTemp.TrimEnd(':');
                        break;
                }

                if ((string.IsNullOrEmpty(strValue) || (string.Equals(strValue, "NoSuchObject", StringComparison.OrdinalIgnoreCase) == true)))
                {
                    strValue = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SNMPManager :: CheckForSpecialParams() :: Exception Handled: " + ex);
            }
            return strValue;
        }

        public static async Task DiscoverAsync()
        {
            Discoverer discoverer = new Discoverer();
            discoverer.AgentFound += AddDiscoveredMFP;
            await discoverer.DiscoverAsync(VersionCode.V1, new IPEndPoint(IPAddress.Broadcast, 161), new OctetString("public"), 6000);
        }

        static void AddDiscoveredMFP(object sender, AgentFoundEventArgs e)
        {
            String IPDisc = e.Agent.ToString();
            int i = IPDisc.IndexOf(':');
            if (list.Count < 15)
            {
                list.Add(IPDisc.Substring(0, i));
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string s = Helper.GetResourceString("IDS_STARTDISCOVERY");
            if (StartButton.Content.ToString() == Helper.GetResourceString("IDS_STARTDISCOVERY"))
            {
                ShowProgressBar = Visibility.Visible;
                IsProgressBarActive = true;
                discover_status.Text = Helper.GetResourceString("IDS_WAIT_MSG");
                PrinterInformation.Clear();
                list.Clear();
                await Task.Delay(100);
                DiscoverAsync().Wait(1000);
                GetPrinterDetails();
                StartButton.Content = Helper.GetResourceString("IDS_CLEAR");
                discover_status.Text = "";
                ShowProgressBar = Visibility.Collapsed;
                IsProgressBarActive = false;
            }
            else if (StartButton.Content.ToString() == Helper.GetResourceString("IDS_CLEAR"))
            {
                PrinterInformation.Clear();
                list.Clear();
                StartButton.Content = Helper.GetResourceString("IDS_STARTDISCOVERY");
            }
        }
    }
}
