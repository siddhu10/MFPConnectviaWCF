using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ApplicationTrigger _appTrigger = null;
        const string BACKGROUND_TASK_NAME = "Stage2PushNotifications";

        public MainPage()
        {
            this.InitializeComponent();
            navView.DisplayModeChanged += NavView_DisplayModeChanged;
            _appTrigger = new ApplicationTrigger();
        }

        private void NavView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            if (args.DisplayMode == NavigationViewDisplayMode.Compact || args.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                linkName.Margin = new Thickness(12, 0, 0, 0);
                linkName.Content = "TA";
            }
            else
            {
                linkName.Margin = new Thickness(48, 0, 0, 0);
                linkName.Content = Helper.GetResourceString("ID_TOPACCESS");
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                NavigationViewItem item = args.SelectedItem as NavigationViewItem;

                switch (item.Tag)
                {
                    case "basicInfo":
                        contentFrame.Navigate(typeof(HomePage));
                        break;

                    case "discInfo":
                        contentFrame.Navigate(typeof(DiscoveryPage));
                        break;
                }
            }
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("StartPageIndex"))
            {
                int iLaunchIndex = Convert.ToInt32(localSettings.Values["StartPageIndex"]);
                switch (iLaunchIndex)
                {
                    case 0:
                        contentFrame.Navigate(typeof(HomePage));
                        break;

                    case 1:
                        contentFrame.Navigate(typeof(DiscoveryPage));
                        break;

                    case 2:
                        contentFrame.Navigate(typeof(SettingsPage));
                        break;
                }
            }
            else
            {
                foreach (NavigationViewItemBase item in navView.MenuItems)
                {
                    if (item is NavigationViewItem && item.Tag.ToString() == "basicInfo")
                    {
                        navView.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void LinkName_Click(object sender, RoutedEventArgs e)
        {
            string strURI = "http://" + Helper.MFPIP + "/";
            var URI = new Uri(strURI);
            Launcher.LaunchUriAsync(URI);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            string strPage = contentFrame.Content.ToString();

            switch (strPage)
            {
                case "Sample.HomePage":
                    contentFrame.Navigate(typeof(HomePage));
                    break;

                case "Sample.DiscoveryPage":
                    contentFrame.Navigate(typeof(DiscoveryPage));
                    break;

                case "Sample.SettingsPage":
                    contentFrame.Navigate(typeof(SettingsPage));
                    break;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            navView.SelectedItem = navView.SettingsItem;
            contentFrame.Navigate(typeof(SettingsPage));
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                await RegisterBackgroundTask();
                await _appTrigger.RequestAsync();
            }
            catch (Exception ex)
            {

            }
        }

        async Task RegisterBackgroundTask()
        {
            foreach (var backTask in BackgroundTaskRegistration.AllTasks)
            {
                if (backTask.Value.Name == BACKGROUND_TASK_NAME)
                {
                    backTask.Value.Unregister(false);
                }
            }

            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
            taskBuilder.Name = BACKGROUND_TASK_NAME;
            taskBuilder.SetTrigger(_appTrigger);
            taskBuilder.Register();
        }
    }
}
