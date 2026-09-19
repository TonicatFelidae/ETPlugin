using UnityEngine;
#if ENABLE_FACEBOOK
using Facebook.Unity;
#endif

namespace ET.Monetization
{
    // Meta app-events + install attribution (FPSA-130). Same shape as
    // AdjustManager: the class always compiles so call sites need no #if guard,
    // and a no-op fallback covers builds without ENABLE_FACEBOOK.
    //
    // Note there is no token parameter here, unlike Adjust. The Facebook SDK
    // reads the app id / client token from FacebookSettings.asset at build time
    // and from the AndroidManifest meta-data at runtime — both must carry the
    // 依頼シート values, and a mismatch fails silently by sending events to the
    // wrong app rather than erroring.
    public class FacebookManager
    {
#if ENABLE_FACEBOOK
        public void Init()
        {
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
                return;
            }

            FB.Init(() =>
            {
                FB.ActivateApp();
                Debug.Log("Facebook SDK Ready");
            });
        }

        // ActivateApp has to fire on every foreground, not just first launch —
        // it is the session signal Meta counts for attribution, so skipping the
        // resume case silently under-reports every returning player.
        public void OnApplicationPause(bool paused)
        {
            if (!paused && FB.IsInitialized)
            {
                FB.ActivateApp();
            }
        }
#else
        public void Init() { }

        public void OnApplicationPause(bool paused) { }
#endif
    }
}
