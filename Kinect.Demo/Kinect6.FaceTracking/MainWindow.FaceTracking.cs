using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using Utility;

namespace Kinect6.FaceTracking
{
	public partial class MainWindow
	{
		FaceTracker _faceTracker;
		byte[] _colorPixelData;
		short[] _depthPixelData;
		Skeleton[] _skeletonData;
		private Dictionary<FeaturePoint, Ellipse> _faceEllipses;
		private Dictionary<JointType, Ellipse> _bodyEllipses;

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
			_kinectSensor.ColorStream.Enable();
			_kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution80x60Fps30);
			_kinectSensor.SkeletonStream.Enable();
			_kinectSensor.SkeletonStream.EnableTrackingInNearRange = true;
			//_kinectSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
			_kinectSensor.Start();
			_faceTracker = new FaceTracker(_kinectSensor);
			_faceEllipses = new Dictionary<FeaturePoint, Ellipse>();
            _bodyEllipses = new Dictionary<JointType, Ellipse>();

			Message = "Kinect connected";
		}

		void KinectSensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
		{

			using (var colorFrame = e.OpenColorImageFrame())
			{
				if (colorFrame == null)
					return;
				if (_colorPixelData == null)
					_colorPixelData = new byte[colorFrame.PixelDataLength];
				colorFrame.CopyPixelDataTo(_colorPixelData);

				//ShowColorImage(colorFrame);
			}

			using (var depthFrame = e.OpenDepthImageFrame())
			{
				if (depthFrame == null)
					return;
				if (_depthPixelData == null)
					_depthPixelData = new short[depthFrame.PixelDataLength];
				depthFrame.CopyPixelDataTo(_depthPixelData);
			}

			using (var skeletonFrame = e.OpenSkeletonFrame())
			{
				if (skeletonFrame == null)
					return;

				if (_skeletonData == null)
					_skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];

				skeletonFrame.CopySkeletonDataTo(_skeletonData);
			}

			var trackedSkeleton = _skeletonData.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);

			if (trackedSkeleton == null)
				return;

			DrawJoints(trackedSkeleton);
			var faceTrackFrame = _faceTracker.Track(_kinectSensor.ColorStream.Format, _colorPixelData, _kinectSensor.DepthStream.Format,
													_depthPixelData, trackedSkeleton);
			UpdateFace(faceTrackFrame);
		}

		private void ShowColorImage(ColorImageFrame colorFrame)
		{
			if (ImageSource == null)
				ImageSource = new WriteableBitmap(colorFrame.Width, colorFrame.Height, 96, 96,
															 PixelFormats.Bgr32, null);

			var stride = colorFrame.Width * PixelFormats.Bgr32.BitsPerPixel / 8;
			ImageSource.WritePixels(new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height), _colorPixelData, stride, 0);
		}

		private void UpdateFace(FaceTrackFrame face)
		{
			if (!face.TrackSuccessful)
				return;
			var faceShape = face.GetProjected3DShape();
			foreach (var featureName in Enum.GetNames(typeof(FeaturePoint)))
			{
				var featurePoint = (FeaturePoint)Enum.Parse(typeof(FeaturePoint), featureName);
				if (faceShape[featurePoint] == PointF.Empty)
					continue;

				if (!_faceEllipses.ContainsKey(featurePoint))
				{
					_faceEllipses[featurePoint] = new Ellipse { Width = 5, Height = 5, Fill = Brushes.DarkBlue };
					FaceCanvas.Children.Add(_faceEllipses[featurePoint]);
				}

				Canvas.SetLeft(_faceEllipses[featurePoint], faceShape[featurePoint].X - _faceEllipses[featurePoint].Width / 2);
				Canvas.SetTop(_faceEllipses[featurePoint], faceShape[featurePoint].Y - _faceEllipses[featurePoint].Height / 2);

			}
		}

		private void DrawJoints(Skeleton skeleton)
		{
			foreach (var name in Enum.GetNames(typeof(JointType)))
			{
				var jointType = (JointType)Enum.Parse(typeof(JointType), name);
				var coordinateMapper = new CoordinateMapper(_kinectSensor);
				var joint = skeleton.Joints[jointType];

				var skeletonPoint = joint.Position;
				if (joint.TrackingState == JointTrackingState.NotTracked)
					continue;

				var colorPoint = coordinateMapper.MapSkeletonPointToColorPoint(skeletonPoint, ColorImageFormat.RgbResolution640x480Fps30);
				if (!_bodyEllipses.ContainsKey(jointType))
				{
					_bodyEllipses[jointType] = new Ellipse { Width = 20, Height = 20, Fill = Brushes.SandyBrown };
					FaceCanvas.Children.Add(_bodyEllipses[jointType]);
				}
				Canvas.SetLeft(_bodyEllipses[jointType], colorPoint.X - _bodyEllipses[jointType].Width / 2);
				Canvas.SetTop(_bodyEllipses[jointType], colorPoint.Y - _bodyEllipses[jointType].Height / 2);
			}
		}
	}
}