using System.Collections;
using System.Collections.Generic;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.DataTypes;
using IBM.Watson.SpeechToText;
using IBM.Watson.SpeechToText.V1;
using IBM.Watson.SpeechToText.V1.Model;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class SpeachToText : MonoBehaviour {
	#region PLEASE SET THESE VARIABLES IN THE INSPECTOR
	[Space (10)]
	[Tooltip ("The IAM apikey.")]
	public string iamApikey;
	[Tooltip ("The service URL (optional). This defaults to \"https://gateway.watsonplatform.net/assistant/api\"")]
	public string serviceUrl;
	[Tooltip ("The version date with which you would like to use the service in the form YYYY-MM-DD.")]
	public string versionDate;
	[Tooltip ("The assistantId to run the example.")]
	public string assistantId;
	#endregion

	public bool ReadyToSend;
	SpeechToTextService service;
	string sessionId;
	string audioPath;
	string usBroadbandModel = "en-US_BroadbandModel";
	private string acousticResourceUrl = "https://archive.org/download/Greatest_Speeches_of_the_20th_Century/KeynoteAddressforDemocraticConvention_64kb.mp3";
	private byte [] acousticResourceData;

	public UnityEngine.UI.Text debugText;
	public ChatBotManager ChatBot;
	public TextToSpeach textToSpeach;
	public UIButtonLocker SelectableUIElement;
	public ParticleSystem LoadingParticles;

	public string [] DidnunderstandSentences;

	void Start () {
		SelectableUIElement.ChangeLock (1);
		audioPath = Application.persistentDataPath + "/voice_query.wav";
		StartCoroutine (Setup ());
	}

	public IEnumerator Setup () {
		Debug.Log ("[SpeachToText] Setting up credentials");
		TokenOptions tokenOptions = new TokenOptions {
			IamApiKey = iamApikey
		};
		Credentials credentials = new Credentials (tokenOptions, serviceUrl);
		while (!credentials.HasIamTokenData ())
			yield return new WaitForEndOfFrame ();
		service = new SpeechToTextService (credentials) {
			DisableSslVerification = true
		};
		ReadyToSend = true;
		Debug.Log ("[SpeachToText] Ready");
		SelectableUIElement.ChangeLock (-1);
	}

	public static int _recordingHZ = 22050;

	public void Recognise (AudioClip _recording = null) {
		LoadingParticles.Play ();
		ReadyToSend = false;
		SelectableUIElement.ChangeLock (1);
		if (_recording != null) {
			bool bFirstBlock = true;
			int midPoint = _recording.samples / 2;
			float [] samples = new float [midPoint];
			_recording.GetData (samples, bFirstBlock ? 0 : midPoint);

			AudioData record = new AudioData {
				MaxLevel = Mathf.Max (Mathf.Abs (Mathf.Min (samples)), Mathf.Max (samples)),
				Clip = AudioClip.Create ("Recording", midPoint, _recording.channels, _recordingHZ, false)
			};
			record.Clip.SetData (samples, 0);

			service.OnListen (record);
			bFirstBlock = !bFirstBlock;
		} else {
			StartCoroutine (RunRecognize ());
		}
	}

	public IEnumerator RunRecognize () {
		Debug.Log ("[SpeachToText] Attempting to Recognize...");
		audioPath = Application.persistentDataPath + "/voice_query" + VoiceRecorder.RecordNumber + ".wav";
		Debug.Log ("[SpeachToText] audioPath = " + audioPath);
		SpeechRecognitionResults recognizeResponse = null;
		byte [] audioBytes = File.ReadAllBytes (audioPath);
		service.Recognize (
		    callback: (DetailedResponse<SpeechRecognitionResults> response, IBMError error) => {
			    Debug.Log ("[SpeachToText] response.Result = " + response.Response);
			    if (error != null)
				    Debug.Log ("[SpeachToText] error = " + error);
			    recognizeResponse = response.Result;
			    debugText.text = response.Response;
			    try {
				    JSONObject json = new JSONObject (debugText.text);
				    debugText.text = json ["results"] [0] ["alternatives"] [0] ["transcript"].str;
				    ChatBot.PostAMessage (debugText.text);
			    } catch {
				    debugText.text = DidnunderstandSentences [Random.Range (0, DidnunderstandSentences.Length)];
				    textToSpeach.InputText = debugText.text;
				    textToSpeach.Synthesize ();
			    }
		    },
		    audio: audioBytes,
		    model: usBroadbandModel,
		    contentType: "audio/wav"
		);

		while (recognizeResponse == null)
			yield return null;
		ReadyToSend = true;
		SelectableUIElement.ChangeLock (-1);
		Debug.Log ("[SpeachToText] Ready");
	}
}