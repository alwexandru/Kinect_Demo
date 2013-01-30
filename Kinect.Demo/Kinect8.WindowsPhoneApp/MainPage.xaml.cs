using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Coding4Fun.Kinect.KinectService.Common;
using Coding4Fun.Kinect.KinectService.PhoneClient;
using Microsoft.Phone.Controls;

namespace Kinect8.WindowsPhoneApp
{
	public partial class MainPage : PhoneApplicationPage
	{
		private WriteableBitmap _outputBitmap;

		private readonly ColorClient _colorClient;
		private readonly DepthClient _depthClient;
		private readonly SkeletonClient _skeletonClient;

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