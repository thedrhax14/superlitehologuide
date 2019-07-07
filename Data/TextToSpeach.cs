using System.Collections;
using System.Collections.Generic;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Utilities;
using IBM.Watson.TextToSpeech;
using IBM.Watson.TextToSpeech.V1;
using IBM.Watson.TextToSpeech.V1.Model;
using UnityEngine;
using System.IO;

public class TextToSpeach : MonoBehaviour {
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

	public string InputText = "It finally works. I can't believe in this.";
	public AudioSource audioSource;
	public bool ReadyToSend;
	TextToSpeechService service;
	string sessionId;
	string audioPath;
	private string allisionVoice = "en-US_AllisonVoice";
	private string synthesizeMimeType = "audio/wav";

	public UIButtonLocker SelectableUIElement;
	public ParticleSystem LoadingParticles;

	void Start () {
		SelectableUIElement.ChangeLock (1);
		audioPath = Application.persistentDataPath + "/voice_response.wav";
		StartCoroutine (Setup ());
	}

	public IEnumerator Setup () {
		Debug.Log ("[" + name + "] Setting up...");
		Debug.Log ("[" + name + "] Setting up credentials");
		TokenOptions tokenOptions = new TokenOptions {
			IamApiKey = iamApikey
		};
		Credentials credentials = new Credentials (tokenOptions, serviceUrl);
		while (!credentials.HasIamTokenData ())
			yield return new WaitForEndOfFrame ();
		service = new TextToSpeechService (credentials) {
			DisableSslVerification = true
		};
		ReadyToSend = true;
		Debug.Log ("[" + name + "] Ready");
		SelectableUIElement.ChangeLock (-1);
	}

	public void Synthesize () {
		ReadyToSend = false;
		SelectableUIElement.ChangeLock (1);
		StartCoroutine (RunSynthesize ());
	}

	public IEnumerator RunSynthesize () {
		Debug.Log ("[" + name + "] Attempting to Synthesize...");
		byte [] synthesizeResponse = null;
		AudioClip clip = null;
		service.Synthesize (
		    callback: (DetailedResponse<byte []> response, IBMError error) => {
			    synthesizeResponse = response.Result;
			    File.WriteAllBytes (audioPath, synthesizeResponse);
			    clip = WaveFile.ParseWAV ("voice_response", synthesizeResponse);
			    LoadingParticles.Stop ();
			    audioSource.PlayOneShot (clip);

		    },
		    text: InputText,
		    voice: allisionVoice,
		    accept: synthesizeMimeType
		);
		while (synthesizeResponse == null)
			yield return null;
		yield return new WaitForSeconds (clip.length);
		ReadyToSend = true;
		SelectableUIElement.ChangeLock (-1);

	}
}
