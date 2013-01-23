using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Utility;

namespace Kinect3.DepthStream
{
	public partial class MainWindow
	{
		private ImageSource _imageSource;

		public ImageSource ImageSource
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
			_kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
			//_kinectSensor.SkeletonStream.Enable();
			_kinectSensor.Start();
			Message = "Kinect connected";
		}

		void KinectSensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			using (var frame = e.OpenDepthImageFrame())
			{
				if(frame==null)
					return;

				var pixels = GetColoredBytes(frame);
				if(ImageSource==null)
					ImageSource = new WriteableBitmap(frame.Width, frame.Height, 96, 96,
							PixelFormats.Bgr32, null);

				var stride = frame.Width*PixelFormats.Bgr32.BitsPerPixel/8;
				ImageSource = BitmapSource.Create(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
			}
		}

		byte[] GetColoredBytes(DepthImageFrame frame)
		{
			var depthData = new short[frame.PixelDataLength];
			frame.CopyPixelDataTo(depthData);

			//4 (Red, Green, Blue, empty byte)
			var pixels = new byte[frame.Height * frame.Width * 4];

			const int blueIndex = 0;
			const int greenIndex = 1;
			const int redIndex = 2;

			//pick a RGB color based on distance
			for (int depthIndex = 0, colorIndex = 0;
				 depthIndex < depthData.Length && colorIndex < pixels.Length;
				 depthIndex++, colorIndex += 4)
			{
				var player = depthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;
				var depth = depthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

				if (depth <= 900)
				{
					pixels[colorIndex + blueIndex] = 255;
					pixels[colorIndex + greenIndex] = 0;
					pixels[colorIndex + redIndex] = 0;

				}
				else if (depth > 900 && depth < 2000)
				{
					pixels[colorIndex + blueIndex] = 0;
					pixels[colorIndex + greenIndex] = 255;
					pixels[colorIndex + redIndex] = 0;
				}
				else if (depth > 2000)
				{
					pixels[colorIndex + blueIndex] = 0;
					pixels[colorIndex + greenIndex] = 0;
					pixels[colorIndex + redIndex] = 255;
				}

				if (player > 0)
				{
					pixels[colorIndex + blueIndex] = Colors.Gold.B;
					pixels[colorIndex + greenIndex] = Colors.Gold.G;
					pixels[colorIndex + redIndex] = Colors.Gold.R;
				}
			}
			return pixels;
		}
	}
}