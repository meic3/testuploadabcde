using System;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace QuickJoin
{
    /// <summary>
    /// Joins a running session of <see cref="SessionType"/>, or creates one if none is found.
    /// The button is disabled while joining and while in a session.
    /// </summary>
    public class QuickJoinButton : MonoBehaviour
    {
        [SerializeField] Button button;

        [Header("Session")]
        [Tooltip("Identifies this kind of session locally. The SessionInfoPanel shows the session of this type.")]
        [SerializeField] string sessionType = "default-session";
        [SerializeField] string sessionName = "default-session-name";
        [SerializeField] int maxPlayers = 4;

        [Header("Quick Join")]
        [Tooltip("Seconds to look for a session before giving up (or creating one).")]
        [SerializeField] float timeout = 5f;
        [Tooltip("Create a new session if none is found within the timeout.")]
        [SerializeField] bool createSessionIfNoneFound = true;

        [Header("Network")]
        [SerializeField] NetworkType networkType = NetworkType.Relay;
        [Tooltip("Only used when Network Type is Direct.")]
        [SerializeField] string ipAddress = "127.0.0.1";
        [Tooltip("Only used when Network Type is Direct.")]
        [SerializeField] ushort port = 7777;

        public string SessionType => sessionType;

        SessionObserver m_Observer;
        ISession m_Session;

        void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        void OnEnable()
        {
            button.interactable = true;
            m_Observer = new SessionObserver(sessionType);
            m_Observer.AddingSessionStarted += OnAddingSessionStarted;
            m_Observer.AddingSessionFailed += OnAddingSessionFailed;
            m_Observer.SessionAdded += OnSessionAdded;
            if (m_Observer.Session != null)
                OnSessionAdded(m_Observer.Session);
        }

        void OnDisable()
        {
            m_Observer.AddingSessionStarted -= OnAddingSessionStarted;
            m_Observer.AddingSessionFailed -= OnAddingSessionFailed;
            m_Observer.SessionAdded -= OnSessionAdded;
            m_Observer.Dispose();
            m_Observer = null;
            if (m_Session != null)
                CleanupSession();
        }

        async void OnClick()
        {
            if (MultiplayerService.Instance == null)
            {
                Debug.LogError("Multiplayer Services are not initialized. Make sure the ServicesInitialization and PlayerAuthentication components on the QuickJoin prefab are enabled.");
                return;
            }

            var quickJoinOptions = new QuickJoinOptions
            {
                Timeout = TimeSpan.FromSeconds(timeout),
                CreateSession = createSessionIfNoneFound
            };

            try
            {
                await MultiplayerService.Instance.MatchmakeSessionAsync(quickJoinOptions, BuildSessionOptions());
            }
            catch (Exception e)
            {
                // The button is re-enabled by OnAddingSessionFailed.
                Debug.LogException(e);
            }
        }

        SessionOptions BuildSessionOptions()
        {
            var options = new SessionOptions
            {
                MaxPlayers = maxPlayers,
                Name = sessionName,
                Type = sessionType,
            }.WithPlayerName();

            switch (networkType)
            {
                case NetworkType.Direct:
                    options.WithDirectNetwork(ipAddress, ipAddress, port);
                    break;
                case NetworkType.Relay:
                    options.WithRelayNetwork();
                    break;
                case NetworkType.DistributedAuthority:
                    options.WithDistributedAuthorityNetwork();
                    break;
            }

            return options;
        }

        void OnAddingSessionStarted(AddingSessionOptions options) => button.interactable = false;

        void OnAddingSessionFailed(AddingSessionOptions options, SessionException exception) => button.interactable = true;

        void OnSessionAdded(ISession session)
        {
            m_Session = session;
            m_Session.RemovedFromSession += OnSessionRemoved;
            m_Session.Deleted += OnSessionRemoved;
            button.interactable = false;
        }

        void OnSessionRemoved()
        {
            CleanupSession();
            button.interactable = true;
        }

        void CleanupSession()
        {
            m_Session.RemovedFromSession -= OnSessionRemoved;
            m_Session.Deleted -= OnSessionRemoved;
            m_Session = null;
        }
    }
}
