using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceRecorder : MonoBehaviour {

	AudioClip clip;
	//List<float> tempRecording = new List<float> ();
	bool isRecording;
	public SpeachToText speachToText;
	//public AudioSource source;
	public static int RecordNumber;

	//void Start () {
	//	InvokeRepeating ("ResizeRecording", 1, 1);
	//}

	public void SetRecording (bool state) {
		isRecording = state;
		if (state) {
			clip = Microphone.Start (null, true, 8, SpeachToText._recordingHZ);
		} else {
			Microphone.End (null);
			SavWav.Save ("voice_query" + RecordNumber, clip);
			//source.PlayOneShot (clip);
			speachToText.Recognise ();
			RecordNumber++;
			//tempRecording.Clear ();
		}
	}

	//void ResizeRecording () {
	//	if (isRecording) {
	//		int length = 44100;
	//		float [] clipData = new float [length];
	//		clip.GetData (clipData, 0);
	//		tempRecording.AddRange (clipData);
	//	}
	//}
}
