using Microsoft.Kinect;
using Utility;

namespace Kinect5.AudioFundamentals
{
	public partial class MainWindow
	{
		private double _beamAngle;
		private KinectAudioSource _kinectAudioSource;

		private void Initialize()
		{
			if (_kinectSensor == null)
				return;

			_kinectSensor.Start();
			_kinectAudioSource = _kinectSensor.AudioSource;
			_kinectAudioSource.Start();
			_kinectAudioSource.BeamAngleChanged += BeamAngleChanged;
			_kinectAudioSource.SoundSourceAngleChanged += SoundSourceAngleChanged;
			Message = "Kinect connected";
		}

		void SoundSourceAngleChanged(object sender, SoundSourceAngleChangedEventArgs e)
		{
			BeamAngle = (_kinectAudioSource.SoundSourceAngle - KinectAudioSource.MinSoundSourceAngle)/
			            (KinectAudioSource.MaxSoundSourceAngle - KinectAudioSource.MinSoundSourceAngle);
		}

		public double BeamAngle
		{
			get { return _beamAngle; }
			set
			{
				if (value.Equals(_beamAngle)) return;
				_beamAngle = value;
				PropertyChanged.Raise(() => BeamAngle);
			}
		}

		void BeamAngleChanged(object sender, BeamAngleChangedEventArgs e)
		{
			BeamAngle = (_kinectAudioSource.BeamAngle - KinectAudioSource.MinBeamAngle)/
			            (KinectAudioSource.MaxBeamAngle - KinectAudioSource.MinBeamAngle);

		}
	}
}