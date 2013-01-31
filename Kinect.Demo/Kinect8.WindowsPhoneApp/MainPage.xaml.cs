using System.Windows;
using Coding4Fun.Kinect.KinectService.Common;
using Coding4Fun.Kinect.KinectService.PhoneClient;
using Microsoft.Phone.Controls;

namespace Kinect8.WindowsPhoneApp
{
	public partial class MainPage : PhoneApplicationPage
	{
		private readonly ColorClient _colorClient;

		public MainPage()
		{
			InitializeComponent();

			_colorClient = new ColorClient();
			_colorClient.ColorFrameReady += ClientColorFrameReady;
		}

		private void StartClick(object sender, RoutedEventArgs e)
		{
			if (!_colorClient.IsConnected)
				_colorClient.Connect(ServerIp.Text, 4530);
			else
				_colorClient.Disconnect();
		}

		void ClientColorFrameReady(object sender, ColorFrameReadyEventArgs e)
		{
			if (e.ColorFrame.BitmapImage != null)
				Color.Source = e.ColorFrame.BitmapImage;
		}
	}
}