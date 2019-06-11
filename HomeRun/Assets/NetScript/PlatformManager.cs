namespace HomeRun.Net
{
    using UnityEngine;
    using Oculus.Platform;
    using Oculus.Platform.Models;
    using HomeRun.Game;

    public class PlatformManager : MonoBehaviour
    {
        // Special Jobs for Platform Manager!
        [SerializeField] private Transform m_remoteHead = null;
        [SerializeField] private Transform m_remoteBat = null;
        [SerializeField] private Transform m_remoteGlove = null;

        [SerializeField] private Transform m_localHead = null;
        [SerializeField] private Transform m_localBat = null;
        [SerializeField] private Transform m_localGlove = null;
        [SerializeField] private Transform m_localRHand = null;
        [SerializeField] private Transform m_localRRender = null;

        public BallSelector LocalBallSelector
        {
            get { return m_localGlove.GetComponentInChildren<BallSelector>(); }
        }
        public BallSelector RemoteBallSelector
        {
            get { return m_remoteGlove.GetComponentInChildren<BallSelector>(); }
        }

        private static PlatformManager s_instance;

        public static PlatformManager Instance
        {
            get { return s_instance; }
        }
        private MatchmakingManager m_matchmaking;
        private P2PManager m_p2p;
        public void P2PThrowBall(int id, Vector3 pos, Vector3 vel, Vector3 strikePos) {
            m_p2p.SendBallThrow(id, pos, vel, strikePos);
        }

        public void P2PHitBall(int id, Vector3 pos, Vector3 vel) {
            m_p2p.SendBallHit(id, pos, vel);
        }
        
        private State m_currentState;
        // GameObject that represents the Head of the remote Avatar

        // my Application-scoped Oculus ID
        private ulong m_myID;

        // my Oculus user name
        private string m_myOculusID;

        void Update()
        {
            m_p2p.UpdateNetwork();
        }

        #region Initialization and Shutdown

        public void SetTransformActiveFromType(PlayerType type)
        {
            switch (type)
            {
                case PlayerType.Batter:
                    m_localBat.gameObject.SetActive(true);
                    m_remoteBat.gameObject.SetActive(false);
                    m_localGlove.gameObject.SetActive(false);
                    m_remoteGlove.gameObject.SetActive(true);
                    m_localRRender.gameObject.SetActive(false);
                    break;

                case PlayerType.Pitcher:
                    m_localBat.gameObject.SetActive(false);
                    m_remoteBat.gameObject.SetActive(true);
                    m_localGlove.gameObject.SetActive(true);
                    m_remoteGlove.gameObject.SetActive(false);
                    m_localRRender.gameObject.SetActive(true);
                    break;

                case PlayerType.None:
                    m_localBat.gameObject.SetActive(true);
                    m_remoteBat.gameObject.SetActive(true);
                    m_localGlove.gameObject.SetActive(false);
                    m_remoteGlove.gameObject.SetActive(false);
                    m_localRRender.gameObject.SetActive(true);
                    break;
            }

        }


        void Awake()
        {
            // make sure only one instance of this manager ever exists
            if (s_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            GlobalSettings.UseNetwork = true;

            s_instance = this;
            //DontDestroyOnLoad(gameObject);

            Core.Initialize();
            m_matchmaking = new MatchmakingManager();
            m_p2p = new P2PManager(m_remoteHead, m_remoteBat, m_remoteGlove, m_localHead, m_localBat, m_localGlove, m_localRHand);
        }


        void Start()
        {
            // First thing we should do is perform an entitlement check to make sure
            // we successfully connected to the Oculus Platform Service.
            Entitlements.IsUserEntitledToApplication().OnComplete(IsEntitledCallback);
        }

        void IsEntitledCallback(Message msg)
        {
            // if (msg.IsError)
            // {
            //     TerminateWithError(msg);
            //     return;
            // }

            // Next get the identity of the user that launched the Application.
            Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
        }

        void GetLoggedInUserCallback(Message<User> msg)
        {
            if (msg.IsError)
            {
                TerminateWithError(msg);
                return;
            }

            m_myID = msg.Data.ID;
            m_myOculusID = msg.Data.OculusID;

            TransitionToState(State.WAITING_TO_PRACTICE_OR_MATCHMAKE);
        }

        // In this example, for most errors, we terminate the Application.  A full App would do
        // something more graceful.
        public static void TerminateWithError(Message msg)
        {
            Debug.Log("Error: " + msg.GetError().Message);
            //UnityEngine.Application.Quit();
        }

        public void QuitButtonPressed()
        {
            UnityEngine.Application.Quit();
        }

        void OnApplicationQuit()
        {
            // be a good matchmaking citizen and leave any queue immediately
            Matchmaking.LeaveQueue();
        }

        #endregion

        #region Properties

        public static MatchmakingManager Matchmaking
        {
            get { return s_instance.m_matchmaking; }
        }

        public static P2PManager P2P
        {
            get { return s_instance.m_p2p; }
        }

        public static State CurrentState
        {
            get { return s_instance.m_currentState; }
        }

        public static ulong MyID
        {
            get
            {
                if (s_instance != null)
                {
                    return s_instance.m_myID;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static string MyOculusID
        {
            get
            {
                if (s_instance != null && s_instance.m_myOculusID != null)
                {
                    return s_instance.m_myOculusID;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        #endregion

        #region State Management

        public enum State
        {
            // loading platform library, checking application entitlement,
            // getting the local user info
            INITIALIZING,

            // waiting on the user to join a matchmaking queue or play a practice game
            WAITING_TO_PRACTICE_OR_MATCHMAKE,

            // waiting for the match to start or viewing results
            MATCH_TRANSITION,

            // actively playing a practice match
            PLAYING_A_LOCAL_MATCH,

            // actively playing an online match
            PLAYING_A_NETWORKED_MATCH,
        };

        public static void TransitionToState(State newState)
        {
            if (s_instance && s_instance.m_currentState != newState)
            {
                s_instance.m_currentState = newState;
            }
        }

        #endregion
    }
}
