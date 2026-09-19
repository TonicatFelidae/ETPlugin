using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ET.Monetization
{
    /// <summary>Mirrors ATTrackingManagerAuthorizationStatus.</summary>
    public enum AppTrackingStatus
    {
        NotDetermined = 0,
        Restricted = 1,
        Denied = 2,
        Authorized = 3,
    }

    /// <summary>
    /// iOS App Tracking Transparency. Ask before initializing any ad SDK: MAX and
    /// every mediated network read the IDFA once, at init, so a prompt raised
    /// afterwards leaves the whole session running on a zeroed IDFA — no
    /// attribution, and a fill rate that looks like an outage.
    ///
    /// No-op on Android and in the Editor, where <see cref="Status"/> reports
    /// Authorized: there is no ATT gate on those platforms, and callers should not
    /// have to special-case them.
    /// </summary>
    public static class AppTracking
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern int ETAttGetStatus();
        [DllImport("__Internal")] private static extern void ETAttRequest();
#endif

        /// <summary>
        /// How long the launch transition is given to finish before the dialog is
        /// raised. Requesting while the app is still coming up gets auto-denied by
        /// iOS with no dialog shown, and that answer is permanent — the user would
        /// have to go into Settings to undo it. Costing a beat of ad availability
        /// is the cheaper side of that trade.
        /// </summary>
        private const float SettleSeconds = 0.5f;

        /// <summary>Safety net only; a user facing the dialog answers in seconds.</summary>
        private const float DefaultTimeoutSeconds = 60f;

        /// <summary>How long to wait for the app to reach the foreground before giving up.</summary>
        private const float FocusTimeoutSeconds = 10f;

        public static AppTrackingStatus Status
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                return (AppTrackingStatus)ETAttGetStatus();
#else
                return AppTrackingStatus.Authorized;
#endif
            }
        }

        /// <summary>True once the user has answered — or when there is nothing to ask.</summary>
        public static bool IsResolved => Status != AppTrackingStatus.NotDetermined;

        /// <summary>
        /// Raises the ATT dialog and does not return until the user answers.
        /// Returns immediately when the status is already resolved, which is every
        /// launch after the first — iOS only ever asks once.
        /// </summary>
        public static IEnumerator RequestRoutine(float timeoutSeconds = DefaultTimeoutSeconds)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (IsResolved)
            {
                Debug.Log($"[ATT] Already resolved: {Status}");
                yield break;
            }

            // iOS silently denies a request raised while the app is not active.
            // Bounded: ad init is downstream of this, and a focus flag that never
            // arrives must not strand the game without ads forever.
            float focusWait = 0f;
            while (!Application.isFocused && focusWait < FocusTimeoutSeconds)
            {
                focusWait += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!Application.isFocused)
            {
                Debug.LogWarning($"[ATT] App never became focused within {FocusTimeoutSeconds:F0}s — "
                                 + "skipping the prompt this launch rather than burning the "
                                 + "one-shot answer on a guaranteed silent denial.");
                yield break;
            }

            yield return new WaitForSecondsRealtime(SettleSeconds);

            Debug.Log("[ATT] Requesting authorization");
            ETAttRequest();

            // The completion handler fires on iOS's own queue, so poll the status
            // rather than marshalling a callback onto the Unity thread.
            float waited = 0f;
            while (!IsResolved && waited < timeoutSeconds)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            if (IsResolved)
                Debug.Log($"[ATT] Resolved: {Status} (after {waited:F1}s)");
            else
                Debug.LogWarning($"[ATT] No answer within {timeoutSeconds:F0}s — continuing without it");
#else
            yield break;
#endif
        }
    }
}
