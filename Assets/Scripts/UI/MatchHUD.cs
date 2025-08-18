using TMPro;
using UnityEngine;
using Unity.Netcode;

namespace MemeArena.UI
{
    /// <summary>
    /// Simple HUD that shows match time and team scores. Attach to a screen-space UI
    /// and assign the text references. It will auto-bind to MatchManager.Instance.
    /// </summary>
    public class MatchHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text team0Text;
        [SerializeField] private TMP_Text team1Text;

        private MemeArena.Game.MatchManager mm;

        void OnEnable()
        {
            mm = MemeArena.Game.MatchManager.Instance;
            if (mm != null)
            {
                mm.Team0Score.OnValueChanged += OnTeam0Changed;
                mm.Team1Score.OnValueChanged += OnTeam1Changed;
            }
            RefreshAll();
        }

        void OnDisable()
        {
            if (mm != null)
            {
                mm.Team0Score.OnValueChanged -= OnTeam0Changed;
                mm.Team1Score.OnValueChanged -= OnTeam1Changed;
            }
        }

        void Update()
        {
            // We don't have a public timer NetworkVariable yet; show a local countdown estimate only on server.
            if (mm != null && timeText != null)
            {
                string t = "";
                if (mm.IsServer)
                {
                    // We can’t access remaining directly; expose later if needed.
                    // For now, just show "LIVE" to indicate ongoing match.
                    t = "LIVE";
                }
                else
                {
                    t = "";
                }
                timeText.text = t;
            }
        }

        void OnTeam0Changed(int prev, int cur) { if (team0Text) team0Text.text = cur.ToString(); }
        void OnTeam1Changed(int prev, int cur) { if (team1Text) team1Text.text = cur.ToString(); }

        void RefreshAll()
        {
            if (mm == null) return;
            OnTeam0Changed(0, mm.Team0Score.Value);
            OnTeam1Changed(0, mm.Team1Score.Value);
        }
    }
}
