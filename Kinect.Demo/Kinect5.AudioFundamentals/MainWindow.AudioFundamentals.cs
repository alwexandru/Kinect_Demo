using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Kinect;
using Microsoft.Speech.Recognition;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Kinect5.AudioFundamentals
{
	public partial class MainWindow
	{
		private KinectAudioSource _kinectAudioSource;
		private SpeechRecognitionEngine _speechRecognizer;

		private void Initialize()
		{
			if (_kinectSensor == null)
				return;
			_speechRecognizer = CreateSpeechRecognizer();
			_speechRecognizer.SetInputToDefaultAudioDevice();
			_speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
			_kinectSensor.AllFramesReady += KinectSensorAllFramesReady;
			_kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
			_kinectSensor.Start();
			_kinectAudioSource = _kinectSensor.AudioSource;
			_kinectAudioSource.Start();
			Message = "Kinect connected";
		}

		private SpeechRecognitionEngine CreateSpeechRecognizer()
		{
			var recognizerInfo = GetKinectRecognizer();

			SpeechRecognitionEngine speechRecognitionEngine;
			try
			{
				speechRecognitionEngine = new SpeechRecognitionEngine(recognizerInfo.Id);
			}
			catch
			{
				MessageBox.Show(
					 @"There was a problem initializing Speech Recognition.
Ensure you have the Microsoft Speech SDK installed and configured.",
					 "Failed to load Speech SDK");
				Close();
				return null;
			}

			var grammar = new Choices();
			grammar.Add("red");
			grammar.Add("green");
			grammar.Add("blue");
			grammar.Add("Camera on");
			grammar.Add("Camera off");
			grammar.Add("Stop");

			var gb = new GrammarBuilder { Culture = recognizerInfo.Culture };
			gb.Append(grammar);

			var g = new Grammar(gb);

			speechRecognitionEngine.LoadGrammar(g);
			speechRecognitionEngine.SpeechRecognized += SreSpeechRecognized;

			return speechRecognitionEngine;
		}

		private void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
		{
			if (e.Result.Confidence < 0.4)
				return;

			switch (e.Result.Text.ToUpperInvariant())
			{
				case "RED":
					MessageTextBlock.Foreground = Brushes.Red;
					break;
				case "GREEN":
					MessageTextBlock.Foreground = Brushes.Green;
					break;
				case "BLUE":
					MessageTextBlock.Foreground = Brushes.Blue;
					break;
				case "CAMERA ON":
					CameraStream.Visibility = Visibility.Visible;
					break;
				case "CAMERA OFF":
					CameraStream.Visibility = Visibility.Hidden;
					break;
				case "STOP":
					Message = "HAMMER TIME!!";
					break;
			}
		}

		private static RecognizerInfo GetKinectRecognizer()
		{
			Func<RecognizerInfo, bool> matchingFunc = r =>
			{
				string value;
				r.AdditionalInfo.TryGetValue("Kinect", out value);
				return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
			};
			return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
		}
	}
}