using System.Windows;
using Microsoft.Kinect;

namespace Kinect1.HelloWorld_2
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var oldSensor = (KinectSensor)e.OldValue;

			StopKinect(oldSensor);

			var newSensor = (KinectSensor)e.NewValue;

			if (newSensor == null)
				return;

			try
			{
                newSensor.ColorStream.Enable();
                newSensor.SkeletonStream.Enable();
                newSensor.DepthStream.Enable();
				newSensor.Start();
			}
			catch (System.IO.IOException)
			{
				sensorChooser.AppConflictOccurred();
			}
		}

		private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			StopKinect(sensorChooser.Kinect);
		}

		private void StopKinect(KinectSensor sensor)
		{
			if (sensor == null || !sensor.IsRunning) 
				return;
			sensor.Stop();

			if (sensor.AudioSource != null)
				sensor.AudioSource.Stop();
		}
	}
}