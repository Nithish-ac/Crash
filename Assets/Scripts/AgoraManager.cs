using Agora.Rtc;
using Agora_RTC_Plugin.API_Example;
using ExitGames.Client.Photon.StructWrapping;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using static UnityEditor.Experimental.GraphView.GraphView;

public class AgoraManager : MonoBehaviour
{
    public static AgoraManager Instance { get; private set; }

    [SerializeField] private string appID;
    [SerializeField] private GameObject canvas;
    [SerializeField] private string tokenBase = "https://agoraapi.vercel.app/token";

    private IRtcEngine RtcEngine;

    private string token = "";
    private string channelName = "Sample";
    private PlayerController mainPlayerInfo;
    private PlayerController clientPlayerInfo;

    public CONNECTION_STATE_TYPE connectionState = CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED;
    public Dictionary<string, List<uint>> usersJoinedInAChannel;
    private DSU dsu;
    private Dictionary<int, List<int>> neighbors = new Dictionary<int, List<int>>();
    private Dictionary<string, HashSet<PlayerController>> channels = new Dictionary<string, HashSet<PlayerController>>();



    private void Awake()
    {
        Instance = this;
        usersJoinedInAChannel = new Dictionary<string, List<uint>>();
        dsu = new DSU();

    }

    private void Start()
    {
        InitRtcEngine();
        SetBasicConfiguration();
    }

    #region Configuration Functions

    private void InitRtcEngine()
    {
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngineContext context = new RtcEngineContext();
        context.appId = appID;
        context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
        context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
        context.areaCode = AREA_CODE.AREA_CODE_GLOB;

        RtcEngine.Initialize(context);
        RtcEngine.InitEventHandler(handler);
    }

    private void SetBasicConfiguration()
    {
        RtcEngine.EnableAudio();
        RtcEngine.EnableVideo();

        //Setting up Video Configuration
        VideoEncoderConfiguration config = new VideoEncoderConfiguration();
        config.dimensions = new VideoDimensions(640, 360);
        config.frameRate = 15;
        config.bitrate = 0;
        RtcEngine.SetVideoEncoderConfiguration(config);

        RtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
    }

    #endregion

    #region Channel Join/Leave Handler Functions

    /// <summary>
    /// The function that is to be called whenever a collision happens. Determines the channel to be joined for the main player based on some conditions.
    /// </summary>
    /// <param name="player1Info"></param>
    /// <param name="player2Info"></param>
    public void JoinChannel(PlayerController player1Info, PlayerController player2Info)
    {
        if (!neighbors.ContainsKey(player1Info.GetPlayerId())) neighbors[player1Info.GetPlayerId()] = new List<int>();
        if (!neighbors.ContainsKey(player2Info.GetPlayerId())) neighbors[player2Info.GetPlayerId()] = new List<int>();

        neighbors[player1Info.GetPlayerId()].Add(player2Info.GetPlayerId());
        neighbors[player2Info.GetPlayerId()].Add(player1Info.GetPlayerId());

        dsu.Add(player1Info);
        dsu.Add(player2Info);
        dsu.Union(player1Info, player2Info);

        string channel1 = channels.ContainsKey(player1Info.GetChannelName()) ? player1Info.GetChannelName() : null;
        string channel2 = channels.ContainsKey(player2Info.GetChannelName()) ? player2Info.GetChannelName() : null;

        Debug.Log("channel names " + channel1 + "and" + channel2);
        if (channel1.IsNullOrEmpty() && channel2.IsNullOrEmpty())
        {
            Debug.Log(" both dont have channels ");
            channelName = GenerateChannelName();
            channels. = channelName;
            channels[player2Info.gameObject] = channelName;

        }
        else if (!channel1.IsNullOrEmpty() && channel2.IsNullOrEmpty())
        {
            channels[player2Info.gameObject] = channel1;
            channelName = channel1;
        }
        else if (channel1.IsNullOrEmpty() && !channel2.IsNullOrEmpty())
        {
            channels[player1Info.gameObject] = channel2;
            channelName = channel2;
        }
        else
        {
            Debug.Log("Checking groups");
            var group1 = new HashSet<PlayerController>(dsu.GetComponents(dsu.Find(player1Info)));
            var group2 = new HashSet<PlayerController>(dsu.GetComponents(dsu.Find(player2Info)));
            if (group1.Count >= group2.Count)
            {
                foreach (var member in group2)
                {
                    channels[member] = channel1;
                }
            }
            else
            {
                foreach (var member in group1)
                {
                    channels[member] = channel2;
                }
            }
        }
        JoinChannel();
    }

    /// <summary>
    /// Responsible for joining the user to a channel and making a video view of the user 
    /// </summary>
    private void JoinChannel()
    {
        //If a token is not yet generated, we first generate one and then join the channel
        try
        {
            if (token.Length == 0)
            {
                StartCoroutine(HelperClass.FetchToken(tokenBase, channelName, 0, UpdateToken));
                Debug.Log("Token fetching initiated");
                return;
            }
            
            RtcEngine.JoinChannel(token, channelName, "", 0);
            UpdateUsersInAChannelTable(channelName, 0);
            RtcEngine.StartPreview();
            MakeVideoView(0);

            Debug.Log($"Joined channel: {channelName} with token: {token}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error joining channel: {ex.Message}");
        }
    }

    /// <summary>
    /// Responsible for leaving a channel for the user and destroying video views
    /// </summary>
    public void LeaveChannel()
    {
        UpdatePropertiesForPlayer(mainPlayerInfo, "", "");

        RtcEngine.StopPreview();
        DestroyVideoView(0);
        RtcEngine.LeaveChannel();
    }

    /// <summary>
    /// Responsible for removing the sole user left in a channel
    /// </summary>
    public void LeaveChannelIfNoOtherUsersPresent()
    {
        string channel = mainPlayerInfo.GetChannelName();
        if (usersJoinedInAChannel[channel].Count != 1) return;

        RemoveAllTheUsersFromChannel(channel);
        LeaveChannel();
    }

    #endregion

    #region Helper Functions

    /// <summary>
    /// Responsible for destroying the video view of a user
    /// </summary>
    /// <param name="uid">User Id of the user whose video view is to be destroyed</param>
    private void DestroyVideoView(uint uid)
    {
        GameObject videoView = GameObject.Find(uid.ToString());
        if (videoView != null)
        {
            Destroy(videoView);
        }
    }

    /// <summary>
    /// Responsible for updating the users in a channel dictionary
    /// </summary>
    /// <param name="channel">
    /// Name of the channel that the user is joining
    /// </param>
    /// <param name="uid">
    /// User Id of the user
    /// </param>
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

    /// <summary>
    /// Generate a channel name at the runtime
    /// </summary>
    /// <returns></returns>
    private string GenerateChannelName()
    {
        return GetRandomChannelName(10);
    }

    /// <summary>
    /// Generate a random channel name of a specified length
    /// </summary>
    /// <param name="length">
    /// Required length for the channel name
    /// </param>
    /// <returns>
    /// Returns a randomly generated channel name
    /// </returns>
    private string GetRandomChannelName(int length)
    {
        string characters = "abcdefghijklmnopqrstuvwxyzABCDDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        string randomChannelName = "";

        for (int i = 0; i < length; i++)
        {
            randomChannelName += characters[UnityEngine.Random.Range(0, characters.Length)];
        }
        Debug.Log("RandomChannel name ="+randomChannelName);
        return randomChannelName;
    }

    /// <summary>
    /// Callback function for updating the token, whenever it is generated from the server
    /// </summary>
    /// <param name="newToken"></param>
    private void UpdateToken(string newToken)
    {
        token = newToken;

        mainPlayerInfo.SetToken(token);
        clientPlayerInfo.SetToken(token);
        if (connectionState == CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED || connectionState == CONNECTION_STATE_TYPE.CONNECTION_STATE_FAILED)
        {
            JoinChannel();
        }
    }

    /// <summary>
    /// Responsible for removing all the users from a channel
    /// </summary>
    /// <param name="userChannel">
    /// Name of the channel
    /// </param>
    private void RemoveAllTheUsersFromChannel(string userChannel)
    {
        uint uid = usersJoinedInAChannel[userChannel][0];
        usersJoinedInAChannel.Remove(userChannel);
        DestroyVideoView(uid);
    }

    /// <summary>
    /// Responsible for updating channel name and token of a player with the given values
    /// </summary>
    /// <param name="player">The player whose valeus are to be updated</param>
    /// <param name="channelName">The name of the new channel</param>
    /// <param name="token">The new token</param>
    private void UpdatePropertiesForPlayer(PlayerController player, string channelName, string token)
    {
        player.SetChannelName(channelName);
        player.SetToken(token);

        if (player == mainPlayerInfo)
        {
            this.channelName = channelName;
            this.token = token;
        }
    }

    #endregion

    #region Video View Rendering Logic
    private void MakeVideoView(uint uid, string channelId = "")
    {
        GameObject videoView = GameObject.Find(uid.ToString());
        if (videoView != null)
        {
            //Video view for this user id already exists
            return;
        }

        // create a video surface game object and assign it to the user
        VideoSurface videoSurface = MakeImageSurface(uid.ToString());
        if (videoSurface == null) return;

        // configure videoSurface
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
            if (transform)
            {
                //If render in RawImage. just set rawImage size.
                transform.sizeDelta = new Vector2(width / 2, height / 2);
                transform.localScale = Vector3.one;
            }
            else
            {
                //If render in MeshRenderer, just set localSize with MeshRenderer
                float scale = (float)height / (float)width;
                videoSurface.transform.localScale = new Vector3(-1, 1, scale);
            }
            Debug.LogError("OnTextureSizeModify: " + width + "  " + height);
        };

        videoSurface.SetEnable(true);
    }

    private VideoSurface MakeImageSurface(string goName)
    {
        GameObject gameObject = new GameObject();

        if (gameObject == null)
        {
            return null;
        }

        gameObject.name = goName;
        // to be renderered onto
        gameObject.AddComponent<RawImage>();
        // make the object draggable
        gameObject.AddComponent<UIElementDrag>();
        if (canvas != null)
        {
            //Add the video view as a child of the canvas
            gameObject.transform.parent = canvas.transform;
        }
        else
        {
            Debug.LogError("Canvas is null video view");
        }

        // set up transform
        gameObject.transform.Rotate(0f, 0.0f, 180.0f);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localScale = new Vector3(2f, 3f, 1f);

        // configure videoSurface
        VideoSurface videoSurface = gameObject.AddComponent<VideoSurface>();
        return videoSurface;
    }

    #endregion

    #region User Events
    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private AgoraManager agoraManager;
        internal UserEventHandler(AgoraManager agoraManager)
        {
            this.agoraManager = agoraManager;
        }

        /// <summary>
        /// Responsible for deleting all the views that are present on a user's screen, when the user leaves a channel
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="stats"></param>
        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            if (!agoraManager.usersJoinedInAChannel.ContainsKey(connection.channelId)) return;

            foreach (uint uid in agoraManager.usersJoinedInAChannel[connection.channelId])
            {
                agoraManager.DestroyVideoView(uid);
            }
        }

        /// <summary>
        /// Responsible for adding the newly joined user to the channel's uid pool
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="uid"></param>
        /// <param name="elapsed"></param>
        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            agoraManager.MakeVideoView(uid, connection.channelId);

            agoraManager.UpdateUsersInAChannelTable(connection.channelId, uid);
        }

        /// <summary>
        /// Responsible for removing a remote user's video view, if the user leaves the channel
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="uid"></param>
        /// <param name="reason"></param>
        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            agoraManager.DestroyVideoView(uid);

            string userChannel = connection.channelId;

            if (agoraManager.usersJoinedInAChannel.ContainsKey(userChannel))
            {
                agoraManager.usersJoinedInAChannel[userChannel].Remove(uid);
            }
        }

        public override void OnConnectionStateChanged(RtcConnection connection, CONNECTION_STATE_TYPE state, CONNECTION_CHANGED_REASON_TYPE reason)
        {
            agoraManager.connectionState = state;
        }
    }
    #endregion
}
