using Sample.DeviceInfoService;
using Sample.OutputManagementService;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.ServiceModel;
using Microsoft.Toolkit.Uwp.UI.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
            homePanel.Width = Window.Current.Bounds.Width - 160;
            loginPanel.Width = Window.Current.Bounds.Width;
            unameValue.Focus(FocusState.Programmatic);
            Stage2Notifications.HomeInst = this;
        }

        public TextBlock StatusLabel
        {
            get { return statusLabel; }
        }

        public StackPanel StatusBlock
        {
            get { return statusDetail; }
        }

        public Expander StatusExpander
        {
            get { return statusExpander; }
        }

        public TextBlock PrinStatus
        {
            get { return prinStatus; }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if (Stage2Interface.GetInstance().IsStag2Inited)
                {
                    await PopulateMFPInfo();
                }
                else
                {
                    loginPopup.IsOpen = true;
                    ipValue.Text = Helper.MFPIP;
                    statusLabel.Text = string.Empty;
                    statusMsg.Text = string.Empty;
                    progBar.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                HandleError();

                ContentDialog dialog = Helper.GetDialog();
                dialog.Content = ex.Message;
                await dialog.ShowAsync();
            }
        }

        async Task PopulateMFPInfo()
        {
            try
            {
                parentUI.IsEnabled = true;
                statusLabel.Text = Helper.GetResourceString("ID_FOOTER_DEF_STATUS");
                statusMsg.Text = Helper.GetResourceString("ID_STATUS_DEF_MSG");
                progBar.Visibility = Visibility.Visible;
                //await Stage2Interface.GetInstance().InternalLogin();

                if (Stage2Interface.GetInstance().IsStag2Inited)
                {
                    statusLabel.Foreground = new SolidColorBrush(Colors.Green);
                    statusLabel.Text = "Connected";
                    ipLabel.Text = Helper.MFPIP;

                    await Task.Delay(1000);
                    await PopulateDeviceInfo();
                    await Task.Delay(1000);
                    await PopulateDeviceStatus();
                    await Task.Delay(1000);
                    await PopulateHardwareInfo();
                    await Task.Delay(1000);
                    await PopulateDeviceCounters();

                    progBar.Visibility = Visibility.Collapsed;
                    statusMsg.Foreground = new SolidColorBrush(Colors.Green);
                    statusMsg.Text = Helper.GetResourceString("ID_STATUS_SUCCESS_MSG");
                    await Task.Delay(2000);

                    statusMsg.Text = string.Empty;
                    statusMsg.Visibility = Visibility.Collapsed;
                }
                else
                {
                    HandleError();
                }
            }
            catch (Exception ex)
            {
                ContentDialog dialog = Helper.GetDialog();
                dialog.Content = ex.Message;

                if (ex is TimeoutException)
                {
                    dialog.PrimaryButtonText = Helper.GetResourceString("ID_RETRY_MSG");
                    dialog.PrimaryButtonClick += Dialog_PrimaryButtonClick;
                    dialog.CloseButtonClick += Dialog_CloseButtonClick;
                    dialog.Content = Helper.GetResourceString("ID_TIMEOUT_MSG");
                    await dialog.ShowAsync();
                }
                else
                {
                    await dialog.ShowAsync();
                    HandleError();
                }
            }
        }

        private void Dialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            HandleError();
        }

        private void Dialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            PopulateMFPInfo();
        }

        async Task PopulateDeviceInfo()
        {
            try
            {
                statusMsg.Foreground = new SolidColorBrush(Colors.Green);
                statusMsg.Text = Helper.GetResourceString("ID_STATUS_BASIC_MSG");
                OutputManagementService.getBasicDeviceInfoResponse1 devInfo = await Stage2Interface.GetInstance().GetBasicDeviceInfoAsync();

                if (devInfo != null)
                {
                    mfpName.Text = devInfo.getBasicDeviceInfoResponse.BasicInfo.Name;
                    mfpManf.Text = devInfo.getBasicDeviceInfoResponse.BasicInfo.Manufacturer;
                    mfpSerial.Text = devInfo.getBasicDeviceInfoResponse.BasicInfo.MFPSerialNumber;
                    mfpMAC.Text = devInfo.getBasicDeviceInfoResponse.BasicInfo.MACAddress[0];
                    mfpModel.Text = devInfo.getBasicDeviceInfoResponse.BasicInfo.CopierModel;
                    mfpLoc.Text = devInfo.getBasicDeviceInfoResponse.BasicInfo.PhysicalLocation;
                    mfpShipType.Text = devInfo.getBasicDeviceInfoResponse.BasicInfo.ShipmentType;
                }
            }
            catch (FaultException<OutputManagementService.eBridgeServiceExceptionType> ex)
            {
                //statusMsg.Foreground = new SolidColorBrush(Colors.Red);
                //statusMsg.Text = ex.Detail.Description;

                if (ex is FaultException<OutputManagementService.eBridgeServiceExceptionType>)
                {
                    FaultException<OutputManagementService.eBridgeServiceExceptionType> fx = ex as FaultException<OutputManagementService.eBridgeServiceExceptionType>;
                    if (fx.Detail.FaultType == "ERR_EBS_INVALID_TOKEN")
                    {
                        statusMsg.Foreground = new SolidColorBrush(Colors.Blue);
                        statusMsg.Text = Helper.GetResourceString("ID_REFRESH");
                        await Stage2Interface.GetInstance().InternalLogin();
                        PopulateDeviceInfo();
                    }
                }
            }
            catch (TimeoutException)
            {
                statusMsg.Foreground = new SolidColorBrush(Colors.Red);
                statusMsg.Text = Helper.GetResourceString("ID_STATUS_BASIC_TIMEOUT_MSG");
            }
        }

        async Task PopulateDeviceStatus()
        {
            try
            {
                statusMsg.Foreground = new SolidColorBrush(Colors.Green);
                statusMsg.Text = Helper.GetResourceString("ID_STATUS_DEVSTATUS_MSG");
                getDeviceStatusResponse1 devStatus = await Stage2Interface.GetInstance().GetDeviceStatusAsync();

                if (devStatus != null)
                {
                    statusLabel.Text = devStatus.getDeviceStatusResponse.Status.DeviceStatus.ToString();
                    prinStatus.Text = devStatus.getDeviceStatusResponse.Status.PrinterStatus.ToString();
                    statusDetailType[] statusTypes = devStatus.getDeviceStatusResponse.Status.Detail;

                    statusDetail.Children.Clear();
                    foreach (statusDetailType type in statusTypes)
                    {
                        TextBlock txtBlk = new TextBlock();

                        string strTitle = string.Empty;
                        string strContent = string.Empty;
                        Helper.GetDisplayStrings(type.CodeName, out strTitle, out strContent);
                        txtBlk.Text = strTitle;

                        if (string.IsNullOrEmpty(strTitle))
                        {
                            txtBlk.Text = type.CodeName;
                        }
                        txtBlk.FontSize = 20;
                        statusDetail.Children.Add(txtBlk);
                    }

                    statusExpander.IsExpanded = true;
                }
            }
            catch (FaultException<OutputManagementService.eBridgeServiceExceptionType> ex)
            {
                //statusMsg.Foreground = new SolidColorBrush(Colors.Red);
                //statusMsg.Text = ex.Detail.Description;

                if (ex is FaultException<OutputManagementService.eBridgeServiceExceptionType>)
                {
                    FaultException<OutputManagementService.eBridgeServiceExceptionType> fx = ex as FaultException<OutputManagementService.eBridgeServiceExceptionType>;
                    if (fx.Detail.FaultType == "ERR_EBS_INVALID_TOKEN")
                    {
                        statusMsg.Foreground = new SolidColorBrush(Colors.Blue);
                        statusMsg.Text = Helper.GetResourceString("ID_REFRESH");
                        await Stage2Interface.GetInstance().InternalLogin();
                        PopulateDeviceStatus();
                    }
                }
            }
            catch (TimeoutException)
            {
                statusMsg.Foreground = new SolidColorBrush(Colors.Red);
                statusMsg.Text = Helper.GetResourceString("ID_STATUS_DEVSTATUS_TIMEOUT_MSG");
            }
        }

        async Task PopulateHardwareInfo()
        {
            try
            {
                statusMsg.Foreground = new SolidColorBrush(Colors.Green);
                statusMsg.Text = Helper.GetResourceString("ID_STATUS_HWINFO_MSG");
                getHardwareInfoResponse1 devHWResp = await Stage2Interface.GetInstance().GetHardwareInfoAsync();

                if (devHWResp != null)
                {
                    DeviceInfoService.finisherType finInfo = devHWResp.getHardwareInfoResponse.HardwareInfo.Finisher;
                    finishStatus.Text = finInfo.Status;
                    stapleStatus.Text = finInfo.StapleUnit.Status;
                    saddleStatus.Text = finInfo.SaddleStitchUnit.Status;
                    holeStatus.Text = finInfo.HolePunch.Status;

                    faxStatus.Text = devHWResp.getHardwareInfoResponse.HardwareInfo.Fax.Status;
                    adfStatus.Text = devHWResp.getHardwareInfoResponse.HardwareInfo.ADF.Status;
                    hddStatus.Text = devHWResp.getHardwareInfoResponse.HardwareInfo.HDD.Status;

                    DeviceInfoService.tonerUnitType tonerInfo = devHWResp.getHardwareInfoResponse.HardwareInfo.Toner;
                    tonerStatus.Text = tonerInfo.Status;
                    yTonerStatus.Text = tonerInfo.Y.State;
                    mTonerStatus.Text = tonerInfo.M.State;
                    cTonerStatus.Text = tonerInfo.C.State;
                    kTonerStatus.Text = tonerInfo.K.State;

                    DeviceInfoService.memoryType memInfo = devHWResp.getHardwareInfoResponse.HardwareInfo.Memory;
                    memStatus.Text = memInfo.Status;
                    memSize.Text = memInfo.MainMemory.Size + " MB";

                    DeviceInfoService.cassetteUnitType cassetteInfo = devHWResp.getHardwareInfoResponse.HardwareInfo.Cassette;
                    cas1Status.Text = cassetteInfo.Cassette1.Status;
                    cas1PaperSize.Text = cassetteInfo.Cassette1.PaperSize;
                    cas1RemPaper.Text = cassetteInfo.Cassette1.RemainingPaper;
                    cas2Status.Text = cassetteInfo.Cassette2.Status;
                    cas2PaperSize.Text = cassetteInfo.Cassette2.PaperSize;
                    cas2RemPaper.Text = cassetteInfo.Cassette2.RemainingPaper;

                    hWInfoExpander.IsExpanded = true;
                }
            }
            catch (FaultException<DeviceInfoService.eBridgeServiceExceptionType> ex)
            {
                //statusMsg.Foreground = new SolidColorBrush(Colors.Red);
                //statusMsg.Text = ex.Detail.Description;

                if (ex is FaultException<DeviceInfoService.eBridgeServiceExceptionType>)
                {
                    FaultException<DeviceInfoService.eBridgeServiceExceptionType> fx = ex as FaultException<DeviceInfoService.eBridgeServiceExceptionType>;
                    if (fx.Detail.FaultType == "ERR_EBS_INVALID_TOKEN")
                    {
                        statusMsg.Foreground = new SolidColorBrush(Colors.Blue);
                        statusMsg.Text = Helper.GetResourceString("ID_REFRESH");
                        await Stage2Interface.GetInstance().InternalLogin();
                        PopulateHardwareInfo();
                    }
                }
            }
            catch (TimeoutException)
            {
                statusMsg.Foreground = new SolidColorBrush(Colors.Red);
                statusMsg.Text = Helper.GetResourceString("ID_STATUS_HWINFO_TIMEOUT_MSG");
            }
        }

        async Task PopulateDeviceCounters()
        {
            try
            {
                statusMsg.Foreground = new SolidColorBrush(Colors.Green);
                statusMsg.Text = Helper.GetResourceString("ID_STATUS_COUNT_MSG");
                getDeviceTotalResponse1 devTotals = await Stage2Interface.GetInstance().GetDeviceTotalAsync();

                if (devTotals != null)
                {
                    deviceTotalType devTotalType = devTotals.getDeviceTotalResponse.Total as deviceTotalType;

                    printCounter.Text = devTotalType.JobCounter.Printer.Total.Total.TotalCount.ToString();
                    scanCounter.Text = devTotalType.JobCounter.Scanner.Total.Total.TotalCount.ToString();
                    faxCounter.Text = devTotalType.JobCounter.Fax.Total.TotalCount.ToString();

                    counterExpander.IsExpanded = true;
                }
            }
            catch (FaultException<OutputManagementService.eBridgeServiceExceptionType> ex)
            {
                //statusMsg.Foreground = new SolidColorBrush(Colors.Red);
                //statusMsg.Text = ex.Detail.Description;

                if (ex is FaultException<OutputManagementService.eBridgeServiceExceptionType>)
                {
                    FaultException<OutputManagementService.eBridgeServiceExceptionType> fx = ex as FaultException<OutputManagementService.eBridgeServiceExceptionType>;
                    if (fx.Detail.FaultType == "ERR_EBS_INVALID_TOKEN")
                    {
                        statusMsg.Foreground = new SolidColorBrush(Colors.Blue);
                        statusMsg.Text = Helper.GetResourceString("ID_REFRESH");
                        await Stage2Interface.GetInstance().InternalLogin();
                        PopulateDeviceCounters();
                    }
                }
            }
            catch (TimeoutException)
            {
                statusMsg.Foreground = new SolidColorBrush(Colors.Red);
                statusMsg.Text = Helper.GetResourceString("ID_STATUS_COUNT_TIMEOUT_MSG");
            }
        }

        void HandleError()
        {
            progBar.Visibility = Visibility.Collapsed;
            statusMsg.Foreground = new SolidColorBrush(Colors.Red);
            statusMsg.Text = Helper.GetResourceString("ID_STATUS_FAILURE_MSG");
            statusLabel.Foreground = new SolidColorBrush(Colors.Red);
            statusLabel.Text = Helper.GetResourceString("ID_FOOTER_FAIL_STATUS");
        }

        async Task HandleLogin()
        {
            try
            {
                string strUname = unameValue.Text;
                string strPwd = passValue.Password;

                if (!(string.IsNullOrEmpty(strUname)) && !(string.IsNullOrEmpty(strPwd)))
                {
                    progRing.IsActive = true;
                    progRing.Visibility = Visibility.Visible;
                    statusText.Foreground = new SolidColorBrush(Colors.Green);
                    statusText.Text = Helper.GetResourceString("ID_LOGIN_PROGRESS_MSG");

                    await Stage2Interface.GetInstance().InternalLogin(strUname, strPwd);

                    if (Stage2Interface.GetInstance().IsStag2Inited)
                    {
                        loginPopup.IsOpen = false;
                        loginUI.IsEnabled = false;
                        parentUI.IsEnabled = true;
                        PopulateMFPInfo();
                    }
                    else
                    {
                        string strMsg = Helper.GetResourceString("ID_LOGIN_FAILURE_MSG");
                        HandleFailure(strMsg);
                    }
                }
                else
                {
                    statusText.Text = Helper.GetResourceString("ID_LOGIN_EMPTY_MSG");
                    statusText.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            catch (CommunicationException cx)
            {
                string strMsg = Helper.GetResourceString("ID_LOGIN_EXCPTN_MSG");
                HandleFailure(strMsg);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HandleLogin();
        }

        void HandleFailure(string strMsg)
        {
            progRing.IsActive = false;
            progRing.Visibility = Visibility.Collapsed;
            statusText.Foreground = new SolidColorBrush(Colors.Red);
            statusText.Text = strMsg;
        }

        private void PassValue_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            HandleLogin();
        }
    }
}
