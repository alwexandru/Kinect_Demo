using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using Microsoft.Kinect;

namespace Kinect9.LightSaber
{
	public partial class MainWindow
	{
		private void Initialize()
		{
			if (_kinectSensor == null)
				return;
			_kinectSensor.AllFramesReady += KinectSensorAllFramesReady;
			_kinectSensor.ColorStream.Enable();
			_kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters()
			{
				Correction = 0.5f,
				JitterRadius = 0.05f,
				MaxDeviationRadius = 0.05f,
				Prediction = 0.5f,
				Smoothing = 0.5f
			});
			_kinectSensor.Start();
			Message = "Kinect connected";
		}

		private Skeleton[] _skeletons;

		public ColorImagePoint RightWrist { get; set; }
		public ColorImagePoint RightHand { get; set; }

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
				ImageSource = BitmapSource.Create(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null, pixelData, stride);
			}

			using (var frame = e.OpenSkeletonFrame())
			{
				if (frame == null)
					return;

				if (_skeletons == null)
					_skeletons = new Skeleton[frame.SkeletonArrayLength];

				frame.CopySkeletonDataTo(_skeletons);
			}

			var trackedSkeleton = _skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);

			if (trackedSkeleton == null)
				return;

			DrawSaber(trackedSkeleton);
		}

		private void DrawSaber(Skeleton skeleton)
		{
			var jointWrist = skeleton.Joints[JointType.WristRight];
			if (jointWrist.TrackingState != JointTrackingState.Tracked)
				return;
			var jointHand = skeleton.Joints[JointType.HandRight];
			if (jointHand.TrackingState != JointTrackingState.Tracked)
				return;
			var mapper = new CoordinateMapper(_kinectSensor);

			RightWrist = mapper.MapSkeletonPointToColorPoint(jointWrist.Position, ColorImageFormat.RgbResolution640x480Fps30); ;
			RightHand = mapper.MapSkeletonPointToColorPoint(jointHand.Position, ColorImageFormat.RgbResolution640x480Fps30); ;

			if (RightHand.Y == RightWrist.Y)
				return;
			var handAngleInRadian = Math.Atan((RightWrist.X - RightHand.X) / (RightWrist.Y - RightHand.Y));
			var handAngleInDegrees = handAngleInRadian * 180 / Math.PI;
			Message = handAngleInDegrees.ToString();
			handAngleInDegrees = Math.Max(-90, handAngleInDegrees);
			handAngleInDegrees = Math.Min(0, handAngleInDegrees);
			if (handAngleInDegrees == 0 || handAngleInDegrees == -90)
				handAngleInDegrees = -45;
			const int magicFudgeNumber = -135;
			var rotationAngleOffsetInDegrees = magicFudgeNumber + handAngleInDegrees;
			var rotationAngleOffsetInRadians = rotationAngleOffsetInDegrees * Math.PI / 180;
			Sabre.X1 = (RightWrist.X + RightHand.X) / 2;
			Sabre.Y1 = (RightWrist.Y + RightHand.Y) / 2;

			const int sabreLength = 200;
			Sabre.X2 = Sabre.X1 + sabreLength * Math.Sin(rotationAngleOffsetInRadians);
			Sabre.Y2 = Sabre.Y1 + sabreLength * Math.Cos(rotationAngleOffsetInRadians);
		}
	}
}