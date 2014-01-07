using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using ServiceStack;
using ServiceStack.Text;

namespace SL5TestApp
{
    public class HelloResponse
    {
        public string Result { get; set; }
    }

    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void btnGoSync_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = new HelloResponse { Result = "Hello, " + txtName.Text };
                var json = response.ToJson();
                lblResults.Content = json;
            }
            catch (Exception ex)
            {
                lblResults.Content = ex.ToString();
            }
        }

        private void btnGoAsync_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Licensing.RegisterLicense("1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBCdXNpbmVzcyxUeXBlOkJ1c2luZXNzLEhhc2g6UHVNTVRPclhvT2ZIbjQ5MG5LZE1mUTd5RUMzQnBucTFEbTE3TDczVEF4QUNMT1FhNXJMOWkzVjFGL2ZkVTE3Q2pDNENqTkQyUktRWmhvUVBhYTBiekJGUUZ3ZE5aZHFDYm9hL3lydGlwUHI5K1JsaTBYbzNsUC85cjVJNHE5QVhldDN6QkE4aTlvdldrdTgyTk1relY2eis2dFFqTThYN2lmc0JveHgycFdjPSxFeHBpcnk6MjAxMy0wMS0wMX0=");
                lblResults.Content = "OK";
            }
            catch (Exception ex)
            {
                lblResults.Content = ex.ToString();
            }
        }
    }
}
