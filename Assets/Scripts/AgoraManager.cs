using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgoraManager : MonoBehaviour
{
    public static AgoraManager Instance { get; private set; }

    [SerializeField] private string appID;
    [SerializeField] private GameObject canvas;
    [SerializeField] private string tokenBase = "https://agoraapi.vercel.app/token";

    private IRtcEngine RtcEngine;
    private string token = "";
    private string channelName = "Sample";
    private PlayerInfo mainPlayerInfo;

    public CONNECTION_STATE_TYPE connectionState = CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED;
    public Dictionary<string, List<uint>> usersJoinedInAChannel;

    private void Awake()
    {
        Instance = this;
        usersJoinedInAChannel = new Dictionary<string, List<uint>>();
    }

    private void Start()
    {
        InitRtcEngine();
        SetBasicConfiguration();
    }

    private void InitRtcEngine()
    {
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngineContext context = new RtcEngineContext
        {
            appId = appID,
            channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
            audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT,
            areaCode = AREA_CODE.AREA_CODE_GLOB
        };

        RtcEngine.Initialize(context);
        RtcEngine.InitEventHandler(handler);
    }

    private void SetBasicConfiguration()
    {
        RtcEngine.EnableAudio();
        RtcEngine.EnableVideo();

        VideoEncoderConfiguration config = new VideoEncoderConfiguration
        {
            dimensions = new VideoDimensions(640, 360),
            frameRate = 15,
            bitrate = 0
        };
        RtcEngine.SetVideoEncoderConfiguration(config);

        RtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
    }

    public void JoinChannel(PlayerInfo player1Info, PlayerInfo player2Info)
    {
        string player1ChannelName = player1Info.GetChannelName();
        string player2ChannelName = player2Info.GetChannelName();

        mainPlayerInfo = player1Info;

        if (player1ChannelName == "" && player2ChannelName == "")
        {
            string newChannelName = GenerateChannelName();
            channelName = newChannelName;
            mainPlayerInfo.SetChannelName(channelName);
        }
        else if (player1ChannelName != "" && player2ChannelName != "")
        {
            return;
        }
        else if (player2ChannelName != "")
        {
            UpdatePropertiesForPlayer(mainPlayerInfo, player2ChannelName, player2Info.GetToken());
        }

        JoinChannel();
    }

    private void JoinChannel()
    {
        if (token.Length == 0)
        {
            StartCoroutine(HelperClass.FetchToken(tokenBase, channelName, 0, UpdateToken));
            return;
        }

        RtcEngine.JoinChannel(token, channelName, "", 0);
        UpdateUsersInAChannelTable(channelName, 0);
        RtcEngine.StartPreview();
        MakeVideoView(0);
    }

    public void LeaveChannel()
    {
        UpdatePropertiesForPlayer(mainPlayerInfo, "", "");
        RtcEngine.StopPreview();
        DestroyVideoView(0);
        RtcEngine.LeaveChannel();
    }

    public void LeaveChannelIfNoOtherUsersPresent()
    {
        string channel = mainPlayerInfo.GetChannelName();
        if (usersJoinedInAChannel[channel].Count != 1) return;
        RemoveAllTheUsersFromChannel(channel);
        LeaveChannel();
    }

    private void DestroyVideoView(uint uid)
    {
        GameObject videoView = GameObject.Find(uid.ToString());
        if (videoView != null)
        {
            Destroy(videoView);
        }
    }

    private void UpdateUsersInAChannelTable(string channel, uint uid)
    {
        if (usersJoinedInAChannel.ContainsKey(channel))
        {
            usersJoinedInAChannel[channel].Add(uid);
        }
        else
        {
            usersJoinedInAChannel.Add(channel, new List<uint> { uid });
        }
    }

    private string GenerateChannelName()
    {
        return GetRandomChannelName(10);
    }

    private string GetRandomChannelName(int length)
    {
        string characters = "abcdefghijklmnopqrstuvwxyzABCDDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string randomChannelName = "";
        for (int i = 0; i < length; i++)
        {
            randomChannelName += characters[UnityEngine.Random.Range(0, characters.Length)];
        }
        return randomChannelName;
    }

    private void UpdateToken(string newToken)
    {
        token = newToken;
        mainPlayerInfo.SetToken(token);
        if (connectionState == CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED || connectionState == CONNECTION_STATE_TYPE.CONNECTION_STATE_FAILED)
        {
            JoinChannel();
        }
    }

    private void RemoveAllTheUsersFromChannel(string userChannel)
    {
        uint uid = usersJoinedInAChannel[userChannel][0];
        usersJoinedInAChannel.Remove(userChannel);
        DestroyVideoView(uid);
    }

    private void UpdatePropertiesForPlayer(PlayerInfo player, string channelName, string token)
    {
        player.SetChannelName(channelName);
        player.SetToken(token);
        if (player == mainPlayerInfo)
        {
            this.channelName = channelName;
            this.token = token;
        }
    }

    private void MakeVideoView(uint uid, string channelId = "")
    {
        GameObject videoView = GameObject.Find(uid.ToString());
        if (videoView != null) return;

        VideoSurface videoSurface = MakeImageSurface(uid.ToString());
        if (videoSurface == null) return;

        if (uid == 0)
        {
            videoSurface.SetForUser(uid, channelId);
        }
        else
        {
            videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        }

        videoSurface.OnTextureSizeModify += (int width, int height) =>
        {
            RectTransform transform = videoSurface.GetComponent<RectTransform>();
            if (transform != null)
            {
                float scale = Math.Min((float)canvas.GetComponent<RectTransform>().rect.width / (float)width,
                    (float)canvas.GetComponent<RectTransform>().rect.height / (float)height);
                transform.sizeDelta = new Vector2(width * scale, height * scale);
            }
        };

        videoSurface.SetEnable(true);
    }

    private VideoSurface MakeImageSurface(string goName)
    {
        GameObject go = new GameObject();
        if (go == null) return null;

        go.name = goName;
        go.AddComponent<RawImage>();
        go.AddComponent<AspectRatioFitter>();
        go.transform.SetParent(canvas.transform);
        go.transform.Rotate(0f, 0.0f, 180.0f);

        VideoSurface videoSurface = go.AddComponent<VideoSurface>();
        if (videoSurface == null) return null;

        videoSurface.SetEnable(true);
        return videoSurface;
    }

    internal string GetCurrentChannelName()
    {
        return channelName;
    }

}
