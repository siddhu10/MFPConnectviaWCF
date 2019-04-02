using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            ipValue.Text = Helper.MFPIP;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string strUname = unameValue.Text;
            string strPwd = passValue.Text;

            if (!(string.IsNullOrEmpty(strUname)) && !(string.IsNullOrEmpty(strPwd)))
            {
                progRing.IsActive = true;
                statusText.Foreground = new SolidColorBrush(Colors.Green);
                statusText.Text = "Logging In...";

                await Stage2Interface.GetInstance().InternalLogin(unameValue.Text, passValue.Text);

                if (Stage2Interface.GetInstance().IsStag2Inited)
                {
                    this.Frame.Navigate(typeof(MainPage));
                }
                else
                {
                    progRing.IsActive = false;
                    progRing.Visibility = Visibility.Collapsed;
                    statusText.Foreground = new SolidColorBrush(Colors.Red);
                    statusText.Text = "Logging Failure!!!";
                }
            }
            else
            {
                statusText.Text = "User Name and/or Password cannot be empty!!!";
                statusText.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
    }
}
