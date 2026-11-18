using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace QuickJoin
{
    /// <summary>
    /// Shows the current session of the <see cref="QuickJoinButton"/>'s session type:
    /// name, player count, own player name, join code, player list and status. Also handles Copy and Leave.
    /// </summary>
    public class SessionInfoPanel : MonoBehaviour
    {
        [SerializeField] QuickJoinButton quickJoinButton;

        [Header("Texts")]
        [SerializeField] TMP_Text sessionNameText;
        [SerializeField] TMP_Text playerCountText;
        [SerializeField] TMP_Text playerNameText;
        [SerializeField] TMP_Text sessionCodeText;
        [SerializeField] TMP_Text statusText;

        [Header("Player List")]
        [SerializeField] Transform playerListContent;
        [Tooltip("Inactive label that is copied once per player.")]
        [SerializeField] TMP_Text playerEntryTemplate;

        [Header("Buttons")]
        [SerializeField] Button copyButton;
        [SerializeField] Button leaveButton;

        SessionObserver m_Observer;
        ISession m_Session;
        readonly List<GameObject> m_PlayerEntries = new();

        void Awake()
        {
            copyButton.onClick.AddListener(CopySessionCode);
            leaveButton.onClick.AddListener(LeaveSession);
        }

        void OnEnable()
        {
            m_Observer = new SessionObserver(quickJoinButton.SessionType);
            m_Observer.AddingSessionStarted += OnAddingSessionStarted;
            m_Observer.AddingSessionFailed += OnAddingSessionFailed;
            m_Observer.SessionAdded += OnSessionAdded;
            if (m_Observer.Session != null)
                OnSessionAdded(m_Observer.Session);
            else
                Refresh();
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

        void OnAddingSessionStarted(AddingSessionOptions options)
        {
            statusText.text = "Awaiting Session information...";
        }

        void OnAddingSessionFailed(AddingSessionOptions options, SessionException exception)
        {
            statusText.text = "Session operation failed: " + exception.Message;
        }

        void OnSessionAdded(ISession session)
        {
            m_Session = session;
            m_Session.Changed += Refresh;
            m_Session.PlayerPropertiesChanged += Refresh;
            m_Session.PlayerJoined += OnPlayersChanged;
            m_Session.PlayerHasLeft += OnPlayersChanged;
            m_Session.RemovedFromSession += OnSessionRemoved;
            m_Session.Deleted += OnSessionRemoved;
            Refresh();
        }

        void OnPlayersChanged(string playerId) => Refresh();

        void OnSessionRemoved()
        {
            CleanupSession();
            Refresh();
        }

        void CleanupSession()
        {
            m_Session.Changed -= Refresh;
            m_Session.PlayerPropertiesChanged -= Refresh;
            m_Session.PlayerJoined -= OnPlayersChanged;
            m_Session.PlayerHasLeft -= OnPlayersChanged;
            m_Session.RemovedFromSession -= OnSessionRemoved;
            m_Session.Deleted -= OnSessionRemoved;
            m_Session = null;
        }

        /// <summary>
        /// Redraws everything from the current session (or the empty state when there is none).
        /// </summary>
        void Refresh()
        {
            var inSession = m_Session != null;
            var hasCode = inSession && !string.IsNullOrEmpty(m_Session.Code);

            sessionNameText.text = inSession ? m_Session.Name : "No Session joined";
            playerCountText.text = inSession ? $"{m_Session.Players.Count} / {m_Session.MaxPlayers} Players" : "- / - Players";
            playerNameText.text = inSession ? m_Session.CurrentPlayer.GetPlayerName() : "No Player Joined";
            sessionCodeText.text = hasCode ? m_Session.Code : "No Session Code";
            statusText.text = inSession ? "In session: " + m_Session.Name : "Not in a session";

            copyButton.interactable = hasCode;
            leaveButton.interactable = inSession;

            foreach (var entry in m_PlayerEntries)
                Destroy(entry);
            m_PlayerEntries.Clear();

            if (inSession && m_Session.Players.Count > 0)
            {
                foreach (var player in m_Session.Players)
                    AddPlayerEntry(player.GetPlayerName());
            }
            else
            {
                AddPlayerEntry("No player has joined");
            }
        }

        void AddPlayerEntry(string text)
        {
            var entry = Instantiate(playerEntryTemplate, playerListContent);
            entry.text = text;
            entry.gameObject.SetActive(true);
            m_PlayerEntries.Add(entry.gameObject);
        }

        void CopySessionCode()
        {
            if (!string.IsNullOrEmpty(m_Session?.Code))
                GUIUtility.systemCopyBuffer = m_Session.Code;
        }

        async void LeaveSession()
        {
            if (m_Session == null)
                return;

            try
            {
                await m_Session.LeaveAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
