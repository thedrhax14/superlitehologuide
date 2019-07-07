using System.Collections;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Utilities;
using IBM.Watson.Assistant.V2;
using IBM.Watson.Assistant.V2.Model;
using UnityEngine;

public class ChatBotManager : MonoBehaviour {
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
	private AssistantService service;
	private string sessionId;

	public UnityEngine.UI.Text debugText;
	public TextToSpeach textToSpeach;
	public UIButtonLocker SelectableUIElement;

	void Start () {
		SelectableUIElement.ChangeLock (1);
		Runnable.Run (CreateService ());
	}

	IEnumerator CreateService () {
		Debug.Log ("[ChatBotManager] Setting up service...");
		if (string.IsNullOrEmpty (iamApikey)) {
			throw new IBMException ("Plesae provide IAM ApiKey for the service.");
		}
		TokenOptions tokenOptions = new TokenOptions {
			IamApiKey = iamApikey
		};
		Credentials credentials = new Credentials (tokenOptions, serviceUrl);
		while (!credentials.HasIamTokenData ())
			yield return null;
		service = new AssistantService (versionDate, credentials) {
			DisableSslVerification = true
		};
		service.CreateSession (OnCreateSession, assistantId);
	}

	public void PostAMessage (string msg) {
		ReadyToSend = false;
		SelectableUIElement.ChangeLock (1);
		Runnable.Run (RunMessage (msg));
	}

	IEnumerator RunMessage (string msg) {
		Debug.Log ("[" + name + "] RunMessage msg = " + msg);
		var input = new MessageInput {
			Text = msg,
			Options = new MessageInputOptions {
				ReturnContext = true
			}
		};
		service.Message (OnMessageRecieved, assistantId, sessionId, input);
		while (!ReadyToSend)
			yield return null;
	}

	void OnMessageRecieved (DetailedResponse<MessageResponse> response, IBMError error) {
		Debug.Log ("[" + name + "] OnMessageRecieved response = " + response.Result.Output.Generic [0].Text);
		ReadyToSend = true;
		debugText.text = response.Result.Output.Generic [0].Text;
		textToSpeach.InputText = debugText.text;
		textToSpeach.Synthesize ();
		SelectableUIElement.ChangeLock (-1);
	}

	void OnCreateSession (DetailedResponse<SessionResponse> response, IBMError error) {
		if (response != null) {
			if (response.Result != null) {

			} else {
				Debug.Log ("[" + name + "] response.Result = NULL");
				return;
			}
		} else {
			Debug.Log ("[" + name + "] response = NULL");
			return;
		}
		Debug.Log ("[" + name + "] OnCreateSession response = " + response.Result.SessionId);
		sessionId = response.Result.SessionId;
		ReadyToSend = true;
		SelectableUIElement.ChangeLock (-1);
		Debug.Log ("[ChatBotManager] Ready");
	}
}