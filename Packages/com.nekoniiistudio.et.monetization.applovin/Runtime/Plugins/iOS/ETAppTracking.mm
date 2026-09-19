// App Tracking Transparency bridge.
//
// The C# side (ET.Monetization.AppTracking) drives this: it decides *when* to ask
// — the request must happen while the app is genuinely active, otherwise iOS
// resolves it to "denied" without ever drawing the dialog — and then polls
// ETAttGetStatus() until the user answers.
//
// AppTrackingTransparency.framework is weak-linked by
// Assets/Editor/IosAppTrackingPostProcess.cs.

#import <Foundation/Foundation.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>

extern "C" {

// Mirrors ATTrackingManagerAuthorizationStatus:
// 0 notDetermined, 1 restricted, 2 denied, 3 authorized.
int ETAttGetStatus()
{
    if (@available(iOS 14, *))
    {
        return (int)ATTrackingManager.trackingAuthorizationStatus;
    }

    // Pre-iOS 14 has no ATT; the IDFA is available unless the user turned on
    // Limit Ad Tracking, which the SDKs detect on their own. Report authorized.
    return 3;
}

void ETAttRequest()
{
    if (@available(iOS 14, *))
    {
        [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:
            ^(ATTrackingManagerAuthorizationStatus status) {
                // Deliberately empty. Handing the result back through a function
                // pointer would land on iOS's completion queue rather than
                // Unity's main thread; the C# side reads it via ETAttGetStatus()
                // instead, which is safe to call from anywhere.
            }];
    }
}

}
