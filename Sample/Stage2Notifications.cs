using Microsoft.Toolkit.Uwp.Notifications;
using Sample.OutputManagementService;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;

namespace Sample
{
    class Stage2Notifications
    {
        ThreadPoolTimer _periodicTimer = null;
        IBackgroundTaskInstance _taskInstance = null;
        BackgroundTaskDeferral _deferral = null;
        ApplicationDataContainer localSettings;
        volatile bool _cancelRequested = false;
        int iNtfctnTimer = 10;

        string strTitle = string.Empty;
        string strContent = string.Empty;

        public static HomePage HomeInst { get; set; }

        enum eNotification
        {
            Drawer1PaperEmpty,
            Drawer2PaperEmpty,
            Drawer3PaperEmpty,
            Drawer4PaperEmpty,
            ADFCoverOpen,
            ADUMissingError,
            Drawer1Open,
            Drawer2Open,
            Drawer3Open,
            Drawer4Open,
            Completed,
            Suspended,
            NewWasteTonerBox
        }

        Stage2Notifications()
        {
            localSettings = ApplicationData.Current.LocalSettings;
        }

        public static void Start(IBackgroundTaskInstance instance)
        {
            Stage2Notifications backgroundActivity = new Stage2Notifications();
            backgroundActivity.Run(instance);
        }

        public void Run(IBackgroundTaskInstance instance)
        {
            instance.Canceled += Instance_Canceled;
            _deferral = instance.GetDeferral();
            _taskInstance = instance;

            int iTemp = Convert.ToInt32(localSettings.Values["NtfctnRefreshTimer"]);
            if (iTemp > iNtfctnTimer)
            {
                iNtfctnTimer = iTemp;
            }
            _periodicTimer = ThreadPoolTimer.CreatePeriodicTimer(PollAndUpdateData, new TimeSpan(0, 0, iNtfctnTimer));
        }

        private void Instance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _cancelRequested = true;
        }

        async void PollAndUpdateData(ThreadPoolTimer timer)
        {
            try
            {
                if (_cancelRequested)
                {
                    _periodicTimer.Cancel();
                    _deferral.Complete();
                }
                else
                {
                    if (Stage2Interface.GetInstance().IsStag2Inited)
                    {
                        await PostNotifications();
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is FaultException<eBridgeServiceExceptionType>)
                {
                    FaultException<eBridgeServiceExceptionType> fx = ex as FaultException<eBridgeServiceExceptionType>;
                    if (fx.Detail.FaultType == "ERR_EBS_INVALID_TOKEN")
                    {
                        await Stage2Interface.GetInstance().InternalLogin();
                    }
                }
                if (ex is TimeoutException)
                {
                    await Task.Delay(5000);
                    await Stage2Interface.GetInstance().InternalLogin();
                }
            }
        }

        async Task PostNotifications()
        {
            getDeviceStatusResponse1 devStatus = await Stage2Interface.GetInstance().GetDeviceStatusAsync();

            if (devStatus != null)
            {
                statusDetailType[] statusTypes = devStatus.getDeviceStatusResponse.Status.Detail;

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                      () =>
                      {
                          HomeInst.StatusExpander.IsExpanded = false;
                          HomeInst.StatusBlock.Children.Clear();

                          foreach (statusDetailType type in statusTypes)
                          {
                              TextBlock txtBlk = new TextBlock();
                              Helper.GetDisplayStrings(type.CodeName, out strTitle, out strContent);
                              txtBlk.Text = strTitle;
                              if (string.IsNullOrEmpty(strTitle))
                              {
                                  txtBlk.Text = type.CodeName;
                              }
                              txtBlk.FontSize = 20;
                              HomeInst.StatusBlock.Children.Add(txtBlk);
                          }

                          HomeInst.StatusLabel.Text = devStatus.getDeviceStatusResponse.Status.DeviceStatus.ToString();
                          HomeInst.PrinStatus.Text = devStatus.getDeviceStatusResponse.Status.PrinterStatus.ToString();
                          HomeInst.StatusExpander.IsExpanded = true;
                      });

                bool bCoverNtfctnEnable = localSettings.Values.ContainsKey("CoverOpenNtfctn") ? (bool)localSettings.Values["CoverOpenNtfctn"] : false;
                if (bCoverNtfctnEnable)
                {
                    foreach (statusDetailType type in statusTypes)
                    {
                        Helper.GetDisplayStrings(type.CodeName, out strTitle, out strContent);

                        switch (type.CodeName)
                        {
                            case "ADUMissingError":
                                NotificationHandler.PostAToast(strTitle, strContent, eNotification.ADUMissingError.ToString());
                                break;

                            case "ADFCoverOpen":
                                NotificationHandler.PostAToast(strTitle, strContent, eNotification.ADFCoverOpen.ToString());
                                break;

                            case "Drawer1Open":
                                NotificationHandler.PostAToast(strTitle, strContent, eNotification.Drawer1Open.ToString());
                                break;

                            case "Drawer2Open":
                                NotificationHandler.PostAToast(strTitle, strContent, eNotification.Drawer2Open.ToString());
                                break;

                            case "Drawer3Open":
                                NotificationHandler.PostAToast(strTitle, strContent, eNotification.Drawer3Open.ToString());
                                break;

                            case "Drawer4Open":
                                NotificationHandler.PostAToast(strTitle, strContent, eNotification.Drawer4Open.ToString());
                                break;
                        }
                    }
                }

                bool bDrawOpenNtfctnEnable = localSettings.Values.ContainsKey("DrawEmptyNtfctn") ? (bool)localSettings.Values["DrawEmptyNtfctn"] : false;
                if (bDrawOpenNtfctnEnable)
                {
                    foreach (statusDetailType type in statusTypes)
                    {
                        Helper.GetDisplayStrings(type.CodeName, out strTitle, out strContent);

                        switch (type.CodeName)
                        {
                            case "Drawer1PaperEmpty":
                                NotificationHandler.PostAToast(strTitle, strContent, eNotification.Drawer1PaperEmpty.ToString());
                                break;

                            case "Drawer2PaperEmpty":
                                NotificationHandler.PostAToast(strTitle, strContent, eNotification.Drawer2PaperEmpty.ToString());
                                break;

                            case "Drawer3PaperEmpty":
                                NotificationHandler.PostAToast(strTitle, strContent, eNotification.Drawer3PaperEmpty.ToString());
                                break;

                            case "Drawer4PaperEmpty":
                                NotificationHandler.PostAToast(strTitle, strContent, eNotification.Drawer4PaperEmpty.ToString());
                                break;
                        }
                    }
                }

                bool bUsedTonerFullNtfctnEnable = localSettings.Values.ContainsKey("NewWasteTonerNtfctn") ? (bool)localSettings.Values["NewWasteTonerNtfctn"] : false;
                if (bUsedTonerFullNtfctnEnable)
                {
                    bool bExitLoop = false;
                    foreach (statusDetailType type in statusTypes)
                    {
                        Helper.GetDisplayStrings(type.CodeName, out strTitle, out strContent);

                        switch (type.CodeName)
                        {
                            case "WasteTonerBoxNearFull":
                                NotificationHandler.PostAToast(strTitle, strContent, eNotification.NewWasteTonerBox.ToString());
                                bExitLoop = true;
                                break;
                        }

                        if (bExitLoop)
                            break;
                    }
                }
            }

            bool bJobNtfctnEnable = localSettings.Values.ContainsKey("JobSuspendNtfctn") ? (bool)localSettings.Values["JobSuspendNtfctn"] : false;
            if (bJobNtfctnEnable)
            {
                getAllWorkflowResponse1 allWorkflowResponse = await Stage2Interface.GetInstance().GetAllWorkFlows();
                int iAllWorkflows = allWorkflowResponse.getAllWorkflowResponse.WorkflowList.Length;

                foreach (workflowType workItem in allWorkflowResponse.getAllWorkflowResponse.WorkflowList)
                {
                    if (string.Equals(workItem.BasicInfo.Status, eNotification.Suspended.ToString(), StringComparison.CurrentCultureIgnoreCase) == true)
                    {
                        string stTemp = Helper.GetResourceString("ID_NT_TITLE_JOBSUSPEND");
                        strTitle = workItem.Name + stTemp;
                        strContent = ((printJobType)workItem.StepInfo[0].Item).DocumentName;
                        NotificationHandler.PostAToast(strTitle, strContent, eNotification.Completed.ToString());
                    }
                }
            }
        }
    }

    class NotificationHandler
    {
        public static void PostAToast(string strTitle, string strContent, string strTag)
        {
            ToastVisual visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText()
                        {
                            Text = strTitle
                        },

                        new AdaptiveText()
                        {
                            Text = strContent
                        }
                    },

                    AppLogoOverride = new ToastGenericAppLogo()
                    {
                        Source = "ms-appx:///Assets/StoreLogo.scale-200.png",
                        HintCrop = ToastGenericAppLogoCrop.Circle
                    }
                }
            };

            ToastContent content = new ToastContent()
            {
                Visual = visual,
                DisplayTimestamp = new DateTimeOffset(DateTime.Now)
            };

            ToastNotification notification = new ToastNotification(content.GetXml());
            notification.Tag = strTag;
            notification.Group = "DocMon";

            ToastNotificationManager.CreateToastNotifier().Show(notification);
        }
    }
}
