using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using Kinect.Toolbox;
using Microsoft.Kinect;

namespace Kinect9.LightSaber
{
	public partial class MainWindow
	{
		private void Initialize()
		{
			if (_kinectSensor == null)
				return;
			_ellipses = new Dictionary<JointType, Ellipse>();
			_kinectSensor.AllFramesReady += KinectSensorAllFramesReady;
			_kinectSensor.ColorStream.Enable();
			_kinectSensor.SkeletonStream.Enable();
			_kinectSensor.Start();
			Message = "Kinect connected";
		}

		private Skeleton[] _skeletons;
		private Dictionary<JointType, Ellipse> _ellipses;

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

			DrawJoints(trackedSkeleton);
		}

		private void DrawJoints(Skeleton skeleton)
		{
			foreach (var name in Enum.GetNames(typeof(JointType)))
			{
				var jointType = (JointType)Enum.Parse(typeof(JointType), name);
				var joint = skeleton.Joints[jointType];

				var skeletonPoint = joint.Position;
				if (joint.TrackingState == JointTrackingState.NotTracked)
					continue;

				var mapper = new CoordinateMapper(_kinectSensor);
				switch (jointType)
				{
					case JointType.WristRight:
						RightWrist = mapper.MapSkeletonPointToColorPoint(skeletonPoint, ColorImageFormat.RgbResolution640x480Fps30);
						break;
					case JointType.HandRight:
						RightHand = mapper.MapSkeletonPointToColorPoint(skeletonPoint, ColorImageFormat.RgbResolution640x480Fps30);
						break;
				}

				//var colorPoint = coordinateMapper.MapSkeletonPointToColorPoint(skeletonPoint, ColorImageFormat.RgbResolution640x480Fps30);
				//if (!_ellipses.ContainsKey(jointType))
				//{
				//	_ellipses[jointType] = new Ellipse { Width = 20, Height = 20, Fill = Brushes.SandyBrown };
				//	SkeletonCanvas.Children.Add(_ellipses[jointType]);
				//}
				//Canvas.SetLeft(_ellipses[jointType], colorPoint.X - _ellipses[jointType].Width / 2);
				//Canvas.SetTop(_ellipses[jointType], colorPoint.Y - _ellipses[jointType].Height / 2);
			}

			if (RightHand.Y == RightWrist.Y)
				return;
			var lineAngleInRadian = Math.Atan((RightWrist.X - RightHand.X) / (RightWrist.Y - RightHand.Y));
			var lineAngleInDegrees = lineAngleInRadian * 180 / Math.PI;
			Message = lineAngleInDegrees.ToString();
			lineAngleInDegrees = Math.Max(-90, lineAngleInDegrees);
			lineAngleInDegrees = Math.Min(0, lineAngleInDegrees);
			var rotationAngleOffsetInDegrees = -135 + lineAngleInDegrees;
			var rotationAngleOffsetInRadians = rotationAngleOffsetInDegrees * Math.PI / 180;
			line.X1 = (RightWrist.X + RightHand.X) / 2;
			line.Y1 = (RightWrist.Y + RightHand.Y) / 2;

			line.X2 = line.X1 + 100 * Math.Sin(rotationAngleOffsetInRadians);
			line.Y2 = line.Y1 + 100 * Math.Cos(rotationAngleOffsetInRadians);
		}
	}
}