using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Chat;
using Photon.Realtime;
using AuthenticationValues = Photon.Chat.AuthenticationValues;
using Photon.Pun;

public class PhotonChatManager : MonoBehaviour, IChatClientListener {

    public static PhotonChatManager Instance;

    public int HistoryLengthToFetch; // set in inspector. Up to a certain degree, previously sent messages can be fetched for context

	public string UserName { get; set; }

    [SerializeField]
    protected internal ChatAppSettings chatAppSettings;

    public ChatClient chatClient;

    public InputField InputFieldChat;
    public GameObject ChatPanel;
    public UITweener FadeInTween, FadeOutTween;
    public string Channel;
    public bool useSceneChannel = true;

    public Text CurrentChannelText;
    public float TimeToFadeOut = 5f;

    private bool focused = false, inTimeout=false;
    private float fadeTimer = 0f, timeout = 0f;

    // Start is called before the first frame update
    void Start() {
        PhotonChatManager.Instance = this;
        this.chatAppSettings = PhotonNetwork.PhotonServerSettings.AppSettings.GetChatSettings();
    }

    public bool IsChatActive => ChatPanel.activeInHierarchy;

    public void Connect() {
        if (PlayerManager.LocalInstance == null) return;
        this.UserName = PlayerManager.LocalInstance.GetName();
        if (useSceneChannel) this.Channel = SceneManager.GetActiveScene().name;

		this.chatClient = new ChatClient(this);
        this.chatClient.UseBackgroundWorkerForSending = true;
        this.chatClient.AuthValues = new AuthenticationValues(this.UserName);
		this.chatClient.ConnectUsingSettings(this.chatAppSettings);
		Debug.Log("Connecting as: " + this.UserName);
	}

    /// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnDestroy.</summary>
    public void OnDestroy()
    {
        if (this.chatClient != null)
        {
            this.chatClient.Disconnect();
        }
    }

    /// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnApplicationQuit.</summary>
    public void OnApplicationQuit()
	{
		if (this.chatClient != null)
		{
			this.chatClient.Disconnect();
		}
	}

	public void Update() {
		if (this.chatClient != null) {
			this.chatClient.Service(); // make sure to call this regularly! it limits effort internally, so calling often is ok!
		} else {
            Connect();
        }
	}

    public void FixedUpdate() {
        focused = InputFieldChat.isFocused;
        if (ChatPanel.activeInHierarchy && !focused) {
            fadeTimer += Time.deltaTime;
            if (fadeTimer >= TimeToFadeOut) {
                fadeTimer = 0f;
                FadeOutTween.HandleTween();
            }
        } else {
            fadeTimer = 0f;
        }

        if (inTimeout) {
            timeout += Time.deltaTime;
            if (timeout >= 0.15f) {
                inTimeout = false;
                timeout = 0f;
            }
        }
    }


	public void OnEnterSend() {
		if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)) {
		    this.SendChatMessage(this.InputFieldChat.text);
			this.InputFieldChat.text = "";
            this.InputFieldChat.DeactivateInputField();
            inTimeout = true;
		}
	}

	public void OnClickSend() {
		if (this.InputFieldChat != null) {
		    this.SendChatMessage(this.InputFieldChat.text);
			this.InputFieldChat.text = "";
            this.InputFieldChat.DeactivateInputField();
            inTimeout = true;
		}
	}

	private void SendChatMessage(string inputLine) {
		if (string.IsNullOrEmpty(inputLine)) {
			return;
		}
        this.chatClient.PublishMessage(this.Channel, inputLine);
	}

    public void OnConnected() {
		ShowPanel();
		this.chatClient.SetOnlineStatus(ChatUserStatus.Online); // You can set your online state (without a message).
        this.chatClient.Subscribe(Channel, this.HistoryLengthToFetch);
	}

	public void OnDisconnected(){
	    // Do nothing
	}

	public void OnChatStateChange(ChatState state) {
		// Do nothing
	}

	public void OnSubscribed(string[] channels, bool[] results) {
		this.chatClient.PublishMessage(Channel, "says 'hi'."); // you don't HAVE to send a msg on join but you could.
	}

    /// <inheritdoc />
    public void OnSubscribed(string channel, string[] users, Dictionary<object, object> properties)
    {
        Debug.LogFormat("OnSubscribed: {0}, users.Count: {1} Channel-props: {2}.", channel, users.Length, properties.ToStringFull());
    }

    public void OnUnsubscribed(string[] channels) {
		// Do nothing for now
	}

	public void OnGetMessages(string channelName, string[] senders, object[] messages) {
        ShowChannel(Channel);
	}

    public void OnUserSubscribed(string channel, string user)
    {
        Debug.LogFormat("OnUserSubscribed: channel=\"{0}\" userId=\"{1}\"", channel, user);
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        Debug.LogFormat("OnUserUnsubscribed: channel=\"{0}\" userId=\"{1}\"", channel, user);
    }

    public void OnChannelPropertiesChanged(string channel, string userId, Dictionary<object, object> properties)
    {
        Debug.LogFormat("OnChannelPropertiesChanged: {0} by {1}. Props: {2}.", channel, userId, Extensions.ToStringFull(properties));
    }

    public void OnUserPropertiesChanged(string channel, string targetUserId, string senderUserId, Dictionary<object, object> properties)
    {
        Debug.LogFormat("OnUserPropertiesChanged: (channel:{0} user:{1}) by {2}. Props: {3}.", channel, targetUserId, senderUserId, Extensions.ToStringFull(properties));
    }

    public void OnErrorInfo(string channel, string error, object data)
    {
        Debug.LogFormat("OnErrorInfo for channel {0}. Error: {1} Data: {2}", channel, error, data);
    }

    public void OnPrivateMessage(string sender, object message, string channelName) {
		// Do nothing
	}

    public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message) {
		if (level == ExitGames.Client.Photon.DebugLevel.ERROR){
			Debug.LogError(message);
		} else if (level == ExitGames.Client.Photon.DebugLevel.WARNING) {
			Debug.LogWarning(message);
		} else {
			Debug.Log(message);
		}
	}

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) {
        // Do nothing
	}

    public void ShowChannel(string channelName) {
		if (string.IsNullOrEmpty(channelName)) {
			return;
		}

		ChatChannel channel = null;
		bool found = this.chatClient.TryGetChannel(channelName, out channel);
		if (!found) {
			Debug.Log("ShowChannel failed to find channel: " + channelName);
			return;
		}

        if (!ChatPanel.activeInHierarchy) ShowPanel();
		this.CurrentChannelText.text = channel.ToStringMessages();
	}

    public void ShowPanel() {
        ChatPanel.SetActive(true);
        fadeTimer = 0f;
        FadeInTween.HandleTween();
    }

    public void Focus() {
        if (inTimeout) {
            return;
        }
        ShowPanel();
        InputFieldChat.ActivateInputField();
    }

    public void UnFocus() {
        if (focused) {
            this.InputFieldChat.DeactivateInputField();
        } else {
            fadeTimer = 0f;
            FadeOutTween.HandleTween();
        }
    }

}
