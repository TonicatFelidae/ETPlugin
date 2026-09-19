using System;
using System.Threading.Tasks;
#if ENABLE_ADJUST
using AdjustSdk;
#endif
using UnityEngine;

namespace ET.Monetization
{
    // Adjust install/event attribution (FPSA-125). Same shape as FacebookManager:
    // the class always compiles so call sites need no #if guard, and a no-op
    // fallback (Init does nothing, GetAdidAsync returns null) covers builds
    // without ENABLE_ADJUST. This replaces the old duplicate stub that used to
    // live in the Game assembly and shadowed this type whenever ENABLE_ADJUST
    // was on, which is why attribution was silently lost.
    public static class AdjustManager
    {
        private static string _cachedAdid;
#if ENABLE_ADJUST
        // FPSA-125: forced to Sandbox for now (editor AND device builds) so
        // attribution/QA verification reports to the Adjust Sandbox dashboard
        // instead of polluting Production. Switch back to Production (or gate on
        // !UNITY_EDITOR) before the store release.
        private const AdjustEnvironment environment = AdjustEnvironment.Sandbox;
#endif

        public static void Init(string adjustAppToken)
        {
#if ENABLE_ADJUST
            if (string.IsNullOrEmpty(adjustAppToken))
            {
                Debug.LogWarning("[AdjustManager] App token is empty; skipping Adjust init.");
                return;
            }

            AdjustConfig config = new AdjustConfig(adjustAppToken, environment);
            Adjust.InitSdk(config);
            Debug.Log("Adjust Ready");

            // Pre-fetch ADID in the background so saves don't block
            _ = GetAdidAsync();
#endif
        }

        /// <summary>
        /// Retrieves the Adjust ADID with a timeout. Returns the cached value if
        /// available, and null when ENABLE_ADJUST is not defined.
        /// </summary>
        public static async Task<string> GetAdidAsync(int timeoutMs = 2000)
        {
#if ENABLE_ADJUST
            if (_cachedAdid != null)
                return _cachedAdid;

            try
            {
                var tcs = new TaskCompletionSource<string>();
                Adjust.GetAdid(adid => tcs.TrySetResult(adid));
                if (await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs)) == tcs.Task)
                {
                    var adid = tcs.Task.Result;
                    // Filter out all-zeros (Android Limit Ad Tracking / iOS ATT denied)
                    if (!string.IsNullOrEmpty(adid) && adid != "00000000-0000-0000-0000-000000000000")
                    {
                        _cachedAdid = adid;
                        return _cachedAdid;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AdjustManager] Failed to get ADID: {ex.Message}");
            }
#else
            await Task.CompletedTask;
#endif
            return null;
        }
    }
}
