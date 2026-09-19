using UnityEngine;
using System.Runtime.InteropServices;

namespace AudienceNetwork
{
    /// <summary>
    /// Meta Audience Network's iOS advertiser-tracking flag.
    ///
    /// The native half of this - _FBAdSettingsBridgeSetAdvertiserTrackingEnabled -
    /// ships with Meta's own Audience Network *Unity* SDK, which this project does
    /// not have. Note that installing the iOS adapter
    /// (com.applovin.mediation.adapters.facebook.ios) does NOT provide it: that pod
    /// brings the native FBAudienceNetwork framework, whose interface is the
    /// Objective-C class FBAdSettings, not this C bridge symbol. Calling it
    /// unconditionally still fails the Xcode link with "Undefined symbol:
    /// _FBAdSettingsBridgeSetAdvertiserTrackingEnabled", so it stays behind the
    /// guard - do not "fix" this by deleting the #if.
    ///
    /// Define ENABLE_AUDIENCE_NETWORK in the iPhone scripting defines only once
    /// something in the project actually exports that symbol, either Meta's Unity
    /// SDK or a hand-written .mm calling [FBAdSettings setAdvertiserTrackingEnabled:].
    /// Until then this is a deliberate no-op, and MAX is left to pass the ATT state
    /// to Meta on its own.
    /// </summary>
    public static class AdSettings
    {
#if UNITY_IOS && !UNITY_EDITOR && ENABLE_AUDIENCE_NETWORK
        [DllImport("__Internal")]
        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);
#endif

        public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
        {
#if UNITY_IOS && !UNITY_EDITOR && ENABLE_AUDIENCE_NETWORK
            FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
#endif
        }
    }
}
