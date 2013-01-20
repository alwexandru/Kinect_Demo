using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Microsoft.Kinect;
using Utility;

namespace Kinect1.HelloWorld
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		public string Message
		{
			get { return _message; }
			set
			{
				if (value.Equals(_message)) return;
				_message = value;
				PropertyChanged.Raise(() => Message);
			}
		}

		private KinectSensor _kinectSensor;
		private string _message;

		private void MainWindowLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				KinectSensor.KinectSensors.StatusChanged += KinectSensorsStatusChanged;

				_kinectSensor = KinectSensor.KinectSensors.FirstOrDefault(sensor => sensor.Status == KinectStatus.Connected);
				if (_kinectSensor == null)
					Message = "No Kinect found on startup";
				else
					Initialize();
			}
			catch (Exception ex)
			{
				Message = ex.Message;
			}
		}

		void KinectSensorsStatusChanged(object sender, StatusChangedEventArgs e)
		{
			switch (e.Status)
			{
				case KinectStatus.Disconnected:
					if (_kinectSensor == e.Sensor)
					{
						Clean();
						Message = "Kinect disconnected";
					}
					break;
				case KinectStatus.Connected:
					_kinectSensor = e.Sensor;
					Initialize();
					break;
				case KinectStatus.NotPowered:
					Message = "Kinect is not powered";
					break;
				case KinectStatus.NotReady:
					Message = "Kinect is not ready";
					break;
				case KinectStatus.Initializing:
					Message = "Initializing";
					break;
				default:
					Message = string.Concat("Status: ", e.Status);
					break;
			}
		}

		private void Initialize()
		{
			if (_kinectSensor == null)
				return;
			_kinectSensor.Start();
			Message = "Kinect connected";
		}

		private void Clean()
		{
			if (_kinectSensor == null)
				return;
			_kinectSensor.Stop();
			_kinectSensor.Dispose();
			_kinectSensor = null;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
