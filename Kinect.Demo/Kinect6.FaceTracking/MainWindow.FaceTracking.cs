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
		private Dictionary<FeaturePoint, Ellipse> _ellipses;

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
			_ellipses = new Dictionary<FeaturePoint, Ellipse>();

			Message = "Kinect connected";
		}

		void KinectSensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
		{

			using (var colorFrame = e.OpenColorImageFrame())
			{
				if(colorFrame==null)
					return;
				if(_colorPixelData==null)
					_colorPixelData = new byte[colorFrame.PixelDataLength];
				colorFrame.CopyPixelDataTo(_colorPixelData);

				ShowColorImage(colorFrame);
			}

			using (var depthFrame = e.OpenDepthImageFrame())
			{
				if(depthFrame ==null)
					return;
				if(_depthPixelData==null)
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

			var faceTrackFrame = _faceTracker.Track(_kinectSensor.ColorStream.Format, _colorPixelData, _kinectSensor.DepthStream.Format,
			                              _depthPixelData, trackedSkeleton);
			UpdateFace(faceTrackFrame);
		}

		private void ShowColorImage(ColorImageFrame colorFrame)
		{
			if (ImageSource == null)
				ImageSource = new WriteableBitmap(colorFrame.Width, colorFrame.Height, 96, 96,
				                                  PixelFormats.Bgr32, null);

			var stride = colorFrame.Width*PixelFormats.Bgr32.BitsPerPixel/8;
			ImageSource.WritePixels(new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height), _colorPixelData, stride, 0);
		}

		private void UpdateFace(FaceTrackFrame face)
		{
			if(!face.TrackSuccessful)
				return;
			var faceShape = face.GetProjected3DShape();
			foreach (var featureName in Enum.GetNames(typeof(FeaturePoint)))
			{
				var featurePoint = (FeaturePoint)Enum.Parse(typeof(FeaturePoint),featureName);
				if(faceShape[featurePoint]==PointF.Empty)
					continue;

				if(!_ellipses.ContainsKey(featurePoint))
				{
					_ellipses[featurePoint] = new Ellipse{Width = 5, Height = 5, Fill = Brushes.DarkBlue};
					FaceCanvas.Children.Add(_ellipses[featurePoint]);
				}

				Canvas.SetLeft(_ellipses[featurePoint],faceShape[featurePoint].X - _ellipses[featurePoint].Width/2);
				Canvas.SetTop(_ellipses[featurePoint],faceShape[featurePoint].Y - _ellipses[featurePoint].Height/2);
				
			}
		}
	}
}