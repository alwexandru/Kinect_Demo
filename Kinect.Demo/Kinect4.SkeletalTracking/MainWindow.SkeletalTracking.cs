using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Linq;

namespace Kinect4.SkeletalTracking
{
	public partial class MainWindow
	{
		private void Initialize()
		{
			if (_kinectSensor == null)
				return;
			_ellipses = new Dictionary<JointType, Ellipse>();
			_kinectSensor.AllFramesReady += KinectSensorAllFramesReady;
			_kinectSensor.SkeletonStream.Enable();
			_kinectSensor.Start();
			Message = "Kinect connected";
		}

		private Skeleton[] _skeletons;
		private Dictionary<JointType,Ellipse> _ellipses;

		void KinectSensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
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
				var coordinateMapper = new CoordinateMapper(_kinectSensor);
				var skeletonPoint = skeleton.Joints[jointType].Position;
				var colorPoint = coordinateMapper.MapSkeletonPointToColorPoint(skeletonPoint, ColorImageFormat.RgbResolution640x480Fps30);
				if(!_ellipses.ContainsKey(jointType))
				{
					_ellipses[jointType] = new Ellipse{Width = 20, Height = 20, Fill = Brushes.SandyBrown};
					SkeletonCanvas.Children.Add(_ellipses[jointType]);
				}
				Canvas.SetLeft(_ellipses[jointType], colorPoint.X - _ellipses[jointType].Width/2);
				Canvas.SetTop(_ellipses[jointType], colorPoint.Y - _ellipses[jointType].Height/2);
			}
		}
	}
}