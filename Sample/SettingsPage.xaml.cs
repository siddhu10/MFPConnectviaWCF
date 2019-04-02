using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        ApplicationDataContainer localSettings;

        public SettingsPage()
        {
            this.InitializeComponent();
            localSettings = ApplicationData.Current.LocalSettings;
            refreshSlider.ValueChanged += RefreshSlider_ValueChanged;
        }

        async Task<StartupTask> GetStartupState()
        {
            StartupTask curState = await StartupTask.GetAsync("MyStartupTask");
            return curState;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            StartupTask startupState = await GetStartupState();

            switch (startupState.State)
            {
                case StartupTaskState.Disabled:
                    startupButton.IsChecked = false;
                    break;

                case StartupTaskState.Enabled:
                    startupButton.IsChecked = true;
                    break;
            }

            if (localSettings.Values.ContainsKey("StartPageIndex"))
            {
                comboStartup.SelectedIndex = Convert.ToInt32(localSettings.Values["StartPageIndex"]);
            }

            foreach (KeyValuePair<string, object> property in localSettings.Values)
            {
                switch (property.Key)
                {
                    case "DrawEmptyNtfctn":
                        drawEmptyOption.IsChecked = (bool)localSettings.Values[property.Key];
                        break;

                    case "CoverOpenNtfctn":
                        coverOpenOption.IsChecked = (bool)localSettings.Values[property.Key];
                        break;

                    case "JobSuspendNtfctn":
                        jobSuspendOption.IsChecked = (bool)localSettings.Values[property.Key];
                        break;

                    case "NewWasteTonerNtfctn":
                        newWasteTonerOption.IsChecked = (bool)localSettings.Values[property.Key];
                        break;
                }
            }

            if (localSettings.Values.ContainsKey("NtfctnRefreshTimer"))
            {
                refreshSlider.Value = Convert.ToDouble(localSettings.Values["NtfctnRefreshTimer"]);
                if (refreshSlider.Value == 60)
                {
                    refTimerValue.Text = Helper.GetResourceString("ID_MINUTE");
                }
                else
                {
                    refTimerValue.Text = refreshSlider.Value + Helper.GetResourceString("ID_SECONDS");
                }
            }
        }

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            StartupTask startupState = await GetStartupState();

            switch (startupState.State)
            {
                case StartupTaskState.Disabled:
                    await startupState.RequestEnableAsync();
                    StartupTask newState = await GetStartupState();

                    if (newState.State == StartupTaskState.Enabled)
                    {
                        startupButton.IsChecked = true;
                    }
                    else
                    {
                        startupButton.IsChecked = false;
                    }
                    break;

                case StartupTaskState.DisabledByUser:
                    startupButton.IsChecked = false;
                    string strMsg = Helper.GetResourceString("ID_STARTUP_DISABLED_USER_MSG");
                    ContentDialog dialog = Helper.GetDialog();
                    dialog.Content = strMsg;
                    await dialog.ShowAsync();
                    break;

                case StartupTaskState.Enabled:
                    startupState.Disable();
                    startupButton.IsChecked = false;
                    break;
            }
        }

        private void ComboStartup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int iStartPage = comboStartup.SelectedIndex;
            localSettings.Values["StartPageIndex"] = iStartPage;
        }

        private void CheckBox_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            CheckBox chkBoxSrc = sender as CheckBox;

            switch(chkBoxSrc.Name)
            {
                case "drawEmptyOption":
                    localSettings.Values["DrawEmptyNtfctn"] = drawEmptyOption.IsChecked;
                    break;

                case "coverOpenOption":
                    localSettings.Values["CoverOpenNtfctn"] = coverOpenOption.IsChecked;
                    break;

                case "jobSuspendOption":
                    localSettings.Values["JobSuspendNtfctn"] = jobSuspendOption.IsChecked;
                    break;

                case "newWasteTonerOption":
                    localSettings.Values["NewWasteTonerNtfctn"] = newWasteTonerOption.IsChecked;
                    break;
            }
        }

        private void CheckBox_Unchecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            CheckBox chkBoxSrc = sender as CheckBox;

            switch (chkBoxSrc.Name)
            {
                case "drawEmptyOption":
                    localSettings.Values["DrawEmptyNtfctn"] = drawEmptyOption.IsChecked;
                    break;

                case "coverOpenOption":
                    localSettings.Values["CoverOpenNtfctn"] = coverOpenOption.IsChecked;
                    break;

                case "jobSuspendOption":
                    localSettings.Values["JobSuspendNtfctn"] = jobSuspendOption.IsChecked;
                    break;

                case "newWasteTonerOption":
                    localSettings.Values["NewWasteTonerNtfctn"] = newWasteTonerOption.IsChecked;
                    break;
            }
        }

        private void RefreshSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            localSettings.Values["NtfctnRefreshTimer"] = refreshSlider.Value;
            if (refreshSlider.Value == 60)
            {
                refTimerValue.Text = Helper.GetResourceString("ID_MINUTE");
            }
            else
            {
                refTimerValue.Text = refreshSlider.Value + Helper.GetResourceString("ID_SECONDS");
            }
        }
    }
}
