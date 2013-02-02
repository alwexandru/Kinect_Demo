using System;
using System.Collections.Generic;
using System.Media;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Windows.Shapes;
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
			_previousSabre1PositionX = new List<double>();
			_previousSabre2PositionX = new List<double>();
			_kinectSensor.Start();
			Message = "Kinect connected";
		}

		private Skeleton[] _skeletons;
		private List<double> _previousSabre1PositionX, _previousSabre2PositionX;

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

			var trackedSkeleton = _skeletons.Where(s => s.TrackingState == SkeletonTrackingState.Tracked).ToList();

			if (!trackedSkeleton.Any())
				return;

			DrawSaber(trackedSkeleton[0], Sabre1, FightingHand.Right);
			if (trackedSkeleton.Count > 1)
				DrawSaber(trackedSkeleton[1], Sabre2, FightingHand.Left);
		}

		private void DrawSaber(Skeleton skeleton, Line sabre, FightingHand fightingHand)
		{
			Joint jointWrist, jointHand, jointElbow;

			switch (fightingHand)
			{
				case FightingHand.Left:
					jointWrist = skeleton.Joints[JointType.WristLeft];
					jointElbow = skeleton.Joints[JointType.ElbowLeft];
					jointHand = skeleton.Joints[JointType.HandLeft];
					break;
				case FightingHand.Right:
					jointWrist = skeleton.Joints[JointType.WristRight];
					jointElbow = skeleton.Joints[JointType.ElbowRight];
					jointHand = skeleton.Joints[JointType.HandRight];
					break;
				default:
					throw new ArgumentOutOfRangeException("fightingHand");
			}

			if ((jointWrist.TrackingState == JointTrackingState.NotTracked) ||
				(jointElbow.TrackingState == JointTrackingState.NotTracked) ||
				(jointHand.TrackingState == JointTrackingState.NotTracked))
				return;

			var mapper = new CoordinateMapper(_kinectSensor);

			var wrist = mapper.MapSkeletonPointToColorPoint(jointWrist.Position, ColorImageFormat.RgbResolution640x480Fps30);
			var elbow = mapper.MapSkeletonPointToColorPoint(jointElbow.Position, ColorImageFormat.RgbResolution640x480Fps30);
			var hand = mapper.MapSkeletonPointToColorPoint(jointHand.Position, ColorImageFormat.RgbResolution640x480Fps30);

			double handAngleInDegrees;
			if (elbow.X == wrist.X)
				handAngleInDegrees = 0;
			else
			{
				var handAngleInRadian = Math.Atan((double)(elbow.Y - wrist.Y) / (wrist.X - elbow.X));
				handAngleInDegrees = handAngleInRadian * 180 / Math.PI;
			}

			if (( fightingHand==FightingHand.Right && (wrist.X < elbow.X ))
			    || (fightingHand==FightingHand.Left && (wrist.X<elbow.X || wrist.Y>elbow.Y)))
				handAngleInDegrees = 180 + handAngleInDegrees;
			//Message = string.Format("{0}, {1}, {2}", elbow.Y, wrist.Y, handAngleInDegrees.ToString());	

			const int magicFudgeNumber = 45;
			double rotationAngleOffsetInDegrees;
			switch (fightingHand)
			{
				case FightingHand.Left:
					rotationAngleOffsetInDegrees = handAngleInDegrees - magicFudgeNumber;
					break;
				case FightingHand.Right:
					rotationAngleOffsetInDegrees = handAngleInDegrees + magicFudgeNumber;
					break;
				default:
					throw new ArgumentOutOfRangeException("fightingHand");
			}
			var rotationAngleOffsetInRadians = rotationAngleOffsetInDegrees * Math.PI / 180;
			sabre.X1 = ((double)wrist.X + hand.X) / 2;
			sabre.Y1 = ((double)wrist.Y + hand.Y) / 2;

			const int sabreLength = 250;
			sabre.X2 = sabre.X1 + sabreLength * Math.Cos(rotationAngleOffsetInRadians);
			sabre.Y2 = sabre.Y1 - sabreLength * Math.Sin(rotationAngleOffsetInRadians);


			PlaySabreSoundOnWave(sabre, fightingHand==FightingHand.Right?_previousSabre1PositionX:_previousSabre2PositionX);
		}

		private void PlaySabreSoundOnWave(Line sabre, List<double> previousPositions)
		{
			if(!previousPositions.Any())
			{
				previousPositions.Add(sabre.X2);
				return;
			}

			if (previousPositions.Count >= SabrePositionCount)
				previousPositions.RemoveAt(0);

			const int minimumDistanceForSoundEffect = 100;
			if (sabre.X2 < previousPositions.Last())
			{
				if (sabre.X2 < (previousPositions.Min() - minimumDistanceForSoundEffect))
					PlaySabreSound(previousPositions);
			}
			else
			{
				if (sabre.X2 >(previousPositions.Max() + minimumDistanceForSoundEffect))
					PlaySabreSound(previousPositions);
			}
			previousPositions.Add(sabre.X2);
		}

		private void PlaySabreSound(List<double> previousPositions)
		{
			var soundPlayer = new SoundPlayer("lightsabre.wav");
			soundPlayer.Play();
			previousPositions.Clear();
		}
	}

	enum FightingHand
	{
		Left,
		Right
	}
}