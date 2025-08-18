using UnityEngine;
using TMPro;

namespace MemeArena.UI
{
    /// <summary>
    /// Lightweight binder that listens for MatchManager client events and toggles UI banners.
    /// Attach to a Canvas object and assign the TMP_Texts. Safe to exist on client-only.
    /// </summary>
    public class MatchEventsUI : MonoBehaviour
    {
        [Header("References")] public TMP_Text overtimeBanner;
        public TMP_Text endBanner;

        [Header("Behavior")] public float overtimeBannerSeconds = 3f;

        float _overtimeTimer;

        void OnEnable()
        {
            MemeArena.Game.MatchManager.OnOvertimeStarted += HandleOvertime;
            MemeArena.Game.MatchManager.OnMatchEnded += HandleMatchEnded;
            SetVisible(overtimeBanner, false);
            SetVisible(endBanner, false);
        }

        void OnDisable()
        {
            MemeArena.Game.MatchManager.OnOvertimeStarted -= HandleOvertime;
            MemeArena.Game.MatchManager.OnMatchEnded -= HandleMatchEnded;
        }

        void Update()
        {
            if (_overtimeTimer > 0f)
            {
                _overtimeTimer -= Time.deltaTime;
                if (_overtimeTimer <= 0f)
                {
                    SetVisible(overtimeBanner, false);
                }
            }
        }

        void HandleOvertime()
        {
            if (overtimeBanner)
            {
                overtimeBanner.text = "OVERTIME!";
                SetVisible(overtimeBanner, true);
                _overtimeTimer = overtimeBannerSeconds;
            }
        }

        void HandleMatchEnded(int winningTeam, int team0, int team1)
        {
            if (endBanner)
            {
                endBanner.text = $"Team {winningTeam} wins! {team0} - {team1}";
                SetVisible(endBanner, true);
            }
        }

        static void SetVisible(TMP_Text t, bool v)
        {
            if (!t) return;
            var go = t.gameObject;
            if (go.activeSelf != v) go.SetActive(v);
        }
    }
}
