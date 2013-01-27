using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;
using Kinect.Toolbox;
using Microsoft.Kinect;

namespace Kinect7.Gestures
{
	public partial class MainWindow
	{
		private void Initialize()
		{
			if (_kinectSensor == null)
				return;

			_gestureDetector = new SwipeGestureDetector {DisplayCanvas = SkeletonCanvas, DisplayColor = Colors.Crimson};
			_gestureDetector.OnGestureDetected += OnGestureDetected;
			_postureDetector = new AlgorithmicPostureDetector();
			_postureDetector.PostureDetected += OnPostureDetected;

			_ellipses = new Dictionary<JointType, Ellipse>();
			_kinectSensor.AllFramesReady += KinectSensorAllFramesReady;
			_kinectSensor.SkeletonStream.Enable();
			_kinectSensor.Start();
			Message = "Kinect connected";
		}


		private Skeleton[] _skeletons;
		private Dictionary<JointType, Ellipse> _ellipses;
		private SwipeGestureDetector _gestureDetector;
		private AlgorithmicPostureDetector _postureDetector;

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

			_postureDetector.TrackPostures(trackedSkeleton);
			ProcessJoints(trackedSkeleton);
		}

		private void ProcessJoints(Skeleton skeleton)
		{
			foreach (var name in Enum.GetNames(typeof(JointType)))
			{
				var jointType = (JointType)Enum.Parse(typeof(JointType), name);

				var coordinateMapper = new CoordinateMapper(_kinectSensor);
				var joint = skeleton.Joints[jointType];

				var skeletonPoint = joint.Position;
				if (joint.TrackingState == JointTrackingState.NotTracked)
					continue;

				if (jointType == JointType.HandRight)
					_gestureDetector.Add(joint.Position, _kinectSensor);

				var colorPoint = coordinateMapper.MapSkeletonPointToColorPoint(skeletonPoint, ColorImageFormat.RgbResolution640x480Fps30);
				if (!_ellipses.ContainsKey(jointType))
				{
					_ellipses[jointType] = new Ellipse { Width = 20, Height = 20, Fill = Brushes.SandyBrown };
					SkeletonCanvas.Children.Add(_ellipses[jointType]);
				}
				Canvas.SetLeft(_ellipses[jointType], colorPoint.X - _ellipses[jointType].Width / 2);
				Canvas.SetTop(_ellipses[jointType], colorPoint.Y - _ellipses[jointType].Height / 2);
			}
		}

		void OnGestureDetected(string gesture)
		{
			Message = string.Format("Gesture Detected: {0}", gesture);
		}

		void OnPostureDetected(string gesture)
		{
			Message = string.Format("Posture Detected: {0}", gesture);
		}
	}
}