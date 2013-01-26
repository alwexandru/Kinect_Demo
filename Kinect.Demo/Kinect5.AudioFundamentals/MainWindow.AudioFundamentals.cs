using Microsoft.Kinect;
using Utility;

namespace Kinect5.AudioFundamentals
{
	public partial class MainWindow
	{
		private double _beamAngle;

		private void Initialize()
		{
			if (_kinectSensor == null)
				return;

			_kinectSensor.AudioSource.BeamAngleChanged += BeamAngleChanged;
			_kinectSensor.AudioSource.SoundSourceAngleChanged += SoundSourceAngleChanged;
			_kinectSensor.Start();
			_kinectSensor.AudioSource.Start();
			Message = "Kinect connected";
		}

		void SoundSourceAngleChanged(object sender, SoundSourceAngleChangedEventArgs e)
		{
			BeamAngle = (_kinectSensor.AudioSource.SoundSourceAngle - KinectAudioSource.MinSoundSourceAngle)/
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
			BeamAngle = (_kinectSensor.AudioSource.BeamAngle - KinectAudioSource.MinBeamAngle)/
			            (KinectAudioSource.MaxBeamAngle - KinectAudioSource.MinBeamAngle);

		}
	}
}