using System.Collections;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Utilities;
using IBM.Watson.Discovery;
using IBM.Watson.Discovery.V1;
using IBM.Watson.Discovery.V1.Model;
using UnityEngine;

public class DiscoverManager : MonoBehaviour {
	#region PLEASE SET THESE VARIABLES IN THE INSPECTOR
	[Space (10)]
	[Tooltip ("The IAM apikey.")]
	[SerializeField]
	private string iamApikey;
	[Tooltip ("The service URL (optional). This defaults to \"https://gateway.watsonplatform.net/discovery/api\"")]
	[SerializeField]
	private string serviceUrl;
	[Tooltip ("The version date with which you would like to use the service in the form YYYY-MM-DD.")]
	[SerializeField]
	private string versionDate;
	#endregion
	public string ServiceState;

	DiscoveryService service;

	void Start () {
		Runnable.Run (CreateService ());
	}

	IEnumerator CreateService () {
		if (string.IsNullOrEmpty (iamApikey))
			throw new IBMException ("Plesae provide IAM ApiKey for the service.");
		TokenOptions tokenOptions = new TokenOptions {
			IamApiKey = iamApikey
		};
		Credentials credentials = new Credentials (tokenOptions, serviceUrl);
		ServiceState = "Waiting for token data";
		while (!credentials.HasIamTokenData ())
			yield return null;
		service = new DiscoveryService (versionDate, credentials);
	}

	public void ListEnvironmentsReciever (ListEnvironmentsResponse response) {

	}
}