using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Utility;

namespace Kinect2.ColorStream
{
	public partial class MainWindow
	{
		private WriteableBitmap _imageSource;

		public WriteableBitmap ImageSource
		{
			get { return _imageSource; }
			set
			{
				if (value.Equals(_imageSource)) return;
				_imageSource = value;
				PropertyChanged.Raise(() => ImageSource);
			}
		}

		private void Initialize()
		{
			if (_kinectSensor == null)
				return;
			_kinectSensor.AllFramesReady += KinectSensorAllFramesReady;
			_kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
			_kinectSensor.Start();
			Message = "Kinect connected";
		}

		void KinectSensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			using (var frame = e.OpenColorImageFrame())
			{
				if (frame == null)
					return;

				var pixelData = new byte[frame.PixelDataLength];
				frame.CopyPixelDataTo(pixelData);
				if (ImageSource == null)
					ImageSource = new WriteableBitmap(frame.Width, frame.Height, 96, 96,
							PixelFormats.Bgr32, null);

				var stride = frame.Width * PixelFormats.Bgr32.BitsPerPixel / 8;
				//ImageSource = BitmapSource.Create(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null, pixelData, stride);
				ImageSource.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), pixelData, stride, 0);
			}
		}
	}
}