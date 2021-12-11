using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using Google.Cloud.Speech.V1;
using Google.Protobuf.Collections;
using NAudio.Mixer;
using WinRecognize;

namespace EyeGaze.SpeechToText
{
    class GoogleCloudSpeachToText : InterfaceSpeechToText
    {
        //private List<string> recordingDevices = new List<string>();
        //private AudioRecorder audioRecorder = new AudioRecorder();
        //private BufferedWaveProvider waveBuffer;
        //private WaveInEvent waveIn = new NAudio.Wave.WaveInEvent();
        //
        //public void connect(string key, string keyInfo)
        //{
        //
        //    //Mixer
        //    //Hook Up Audio Mic for sound peak detection
        //    audioRecorder.SampleAggregator.MaximumCalculated += OnRecorderMaximumCalculated;
        //
        //    for (int n = 0; n < WaveIn.DeviceCount; n++)
        //    {
        //        recordingDevices.Add(WaveIn.GetCapabilities(n).ProductName);
        //    }
        //
        //    //Set up Google specific code
        //    //oneShotConfig = new RecognitionConfig();
        //    //oneShotConfig.Encoding = RecognitionConfig.Types.AudioEncoding.Linear16;
        //    //oneShotConfig.SampleRateHertz = 16000;
        //    //oneShotConfig.LanguageCode = "en";
        //
        //
        //
        //    //Set up NAudio waveIn object and events
        //    waveIn.DeviceNumber = 0;
        //    waveIn.WaveFormat = new NAudio.Wave.WaveFormat(16000, 1);
        //    //Need to catch this event to fill our audio beffer up
        //    waveIn.DataAvailable += WaveIn_DataAvailable;
        //    //the actuall wave buffer we will be sending to googles for voice to text conversion
        //    waveBuffer = new BufferedWaveProvider(waveIn.WaveFormat);
        //    waveBuffer.DiscardOnBufferOverflow = true;
        //
        //    //We are using a timer object to fire a one second record interval
        //    //this gets enabled and disabled based on when we get a peak detection from NAudio
        //    timer1.Enabled = false;
        //    //One second record window
        //    timer1.Interval = 1000;
        //    //Hook up to timer tick event
        //    timer1.Tick += Timer1_Tick;
        //}
        //
        //public void disconnect()
        //{
        //    throw new NotImplementedException();
        //}
        //
        //public string listen()
        //{
        //    throw new NotImplementedException();
        //}
        //
        //
        //void OnRecorderMaximumCalculated(object sender, MaxSampleEventArgs e)
        //{
        //    float peak = Math.Max(e.MaxSample, Math.Abs(e.MinSample));
        //
        //    // multiply by 100 because the Progress bar's default maximum value is 100
        //    peak *= 100;
        //
        //    //Console.WriteLine("Recording Level " + peak);
        //    if (peak > 5)
        //    {
        //        //Timer should not be enabled, meaning, we are not already recording
        //        if (timer1.Enabled == false)
        //        {
        //            timer1.Enabled = true;
        //            waveIn.StartRecording();
        //        }
        //
        //    }
        //
        //}
        //private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        //{
        //    waveBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
        //
        //}
        public void connect(string key, string keyInfo)
        {
            throw new NotImplementedException();
        }

        public void disconnect()
        {
            throw new NotImplementedException();
        }

        public string listen()
        {
            throw new NotImplementedException();
        }
    }
}
