using System;
using System.Collections.Generic;
using System.Media;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using Microsoft.Kinect;

namespace Kinect9.LightSaber
{
	public partial class MainWindow
	{
		private const int SabrePositionCount = 20;

		private void Initialize()
		{
			if (_kinectSensor == null)
				return;
			_kinectSensor.AllFramesReady += KinectSensorAllFramesReady;
			_kinectSensor.ColorStream.Enable();
			_kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters
				                                    {
				Correction = 0.5f,
				JitterRadius = 0.05f,
				MaxDeviationRadius = 0.05f,
				Prediction = 0.5f,
				Smoothing = 0.5f
			});
			_previousSabrePositionX = new List<double>(SabrePositionCount);
			_kinectSensor.Start();
			Message = "Kinect connected";
		}


		public ColorImagePoint RightWrist { get; set; }
		public ColorImagePoint RightElbow { get; set; }
		public ColorImagePoint RightHand { get; set; }

		private Skeleton[] _skeletons;
		private List<double> _previousSabrePositionX;

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
			var jointElbow = skeleton.Joints[JointType.ElbowRight];
			var jointHand = skeleton.Joints[JointType.HandRight];

			if ((jointWrist.TrackingState == JointTrackingState.NotTracked) ||
				(jointElbow.TrackingState == JointTrackingState.NotTracked) ||
				(jointHand.TrackingState == JointTrackingState.NotTracked))
				return;

			var mapper = new CoordinateMapper(_kinectSensor);

			RightWrist = mapper.MapSkeletonPointToColorPoint(jointWrist.Position, ColorImageFormat.RgbResolution640x480Fps30); 
			RightElbow = mapper.MapSkeletonPointToColorPoint(jointElbow.Position, ColorImageFormat.RgbResolution640x480Fps30);
			RightHand = mapper.MapSkeletonPointToColorPoint(jointHand.Position, ColorImageFormat.RgbResolution640x480Fps30);

			double handAngleInDegrees;
			if (RightElbow.X == RightWrist.X)
				handAngleInDegrees = 0;
			else
			{
				var handAngleInRadian = Math.Atan((double)(RightElbow.Y - RightWrist.Y)/(RightWrist.X - RightElbow.X));
				handAngleInDegrees = handAngleInRadian*180/Math.PI;
			}

			//Message = string.Format("{0}, {1}, {2}",RightElbow.Y, RightWrist.Y, handAngleInDegrees.ToString());

			const int magicFudgeNumber = 90;
			var rotationAngleOffsetInDegrees = handAngleInDegrees - magicFudgeNumber;
			var rotationAngleOffsetInRadians = rotationAngleOffsetInDegrees * Math.PI / 180;
			Sabre.X1 = ((double)RightWrist.X + RightHand.X) / 2;
			Sabre.Y1 = ((double)RightWrist.Y + RightHand.Y) / 2;

			const int sabreLength = 250;
			Sabre.X2 = Sabre.X1 - sabreLength * Math.Cos(rotationAngleOffsetInRadians);
			Sabre.Y2 = Sabre.Y1 + sabreLength * Math.Sin(rotationAngleOffsetInRadians);

			if (_previousSabrePositionX.Count >= SabrePositionCount)
				_previousSabrePositionX.RemoveAt(0);

			PlaySabreSoundOnWave();
			_previousSabrePositionX.Add(Sabre.X2);
		}

		private void PlaySabreSoundOnWave()
		{
			if (!_previousSabrePositionX.Any()) return;

			const int minimumDistanceForSoundEffect = 100;
			if (Sabre.X2 < _previousSabrePositionX.Last())
			{
				if (Sabre.X2 < _previousSabrePositionX.Min() - minimumDistanceForSoundEffect)
					PlaySabreSound();
			}
			else
			{
				if (Sabre.X2 > _previousSabrePositionX.Max() + minimumDistanceForSoundEffect)
					PlaySabreSound();
			}
		}

		private void PlaySabreSound()
		{
			var soundPlayer = new SoundPlayer("lightsabre.wav");
			soundPlayer.Play();
			_previousSabrePositionX.Clear();
		}
	}
}