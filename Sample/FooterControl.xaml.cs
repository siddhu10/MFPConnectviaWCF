using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Sample
{
    public sealed partial class FooterControl : UserControl
    {
        public FooterControl()
        {
            this.InitializeComponent();
        }

        public string MFPStatus
        {
            get { return (string)GetValue(MFPStatusProperty); }
            set { SetValue(MFPIPProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MFPStatusProperty =
            DependencyProperty.Register("MFPStatus", typeof(string), typeof(FooterControl), null);



        public string MFPIP
        {
            get { return (string)GetValue(MFPIPProperty); }
            set { SetValue(MFPIPProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MFPIPProperty =
            DependencyProperty.Register("MFPIP", typeof(string), typeof(FooterControl), null);


    }
}
