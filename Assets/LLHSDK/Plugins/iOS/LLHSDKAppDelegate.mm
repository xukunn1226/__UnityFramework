//
//  LLHSDKAppDelegate.m
//  Unity-iPhone
//
//  Created by duanefaith on 2017/12/18.
//

#import "LLHSDKAppDelegate.h"
#import <LLH/LLHSDK.h>
#import <LilithChatWgame/LilithChat.h>

@implementation LLHSDKAppDelegate

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
    [self startLLHSDK];
    [[LLHSDK sharedInstance] app:application didFinishLaunchingWithOptions:launchOptions];
    return YES;
}


- (void)applicationDidBecomeActive:(UIApplication *)application
{
    [[LLHSDK sharedInstance] appDidBecomeActive:application];
}

- (BOOL)application:(UIApplication *)application handleOpenURL:(NSURL *)url
{
    return [[LLHSDK sharedInstance] app:application handleOpenURL:url];
}

- (BOOL)application:(UIApplication *)application
openURL:(NSURL *)url
sourceApplication:(NSString *)sourceApplication
annotation:(id)annotation {

    return [[LLHSDK sharedInstance] app:application
            openURL:url
            sourceApplication:sourceApplication
            annotation:annotation];
}

- (BOOL)application:(UIApplication *)application
continueUserActivity:(NSUserActivity *)userActivity
restorationHandler:(void (^)(NSArray *_Nullable))restorationHandler
{

    return [[LLHSDK sharedInstance] application:application
            continueUserActivity:userActivity
            restorationHandler:restorationHandler];
}

- (void)application:(UIApplication *)application
 didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken 
 {
    [[LLHSDK sharedInstance] application:application 
    didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
}

- (void)startUnity:(UIApplication *)application {
    [self initChatSDK];
}

- (void)startLLHSDK {
    LLHSDK *llhSDK = [LLHSDK sharedInstance];
    [llhSDK setDebug:YES];
    llhSDK.loginUIStyle = GameCenterStyle;
    [llhSDK initLLHSDK];
    [llhSDK setShouldSaveExtToLocal:YES];
}

- (void)initChatSDK {
    LilithChat::CheckNetwork();
    LilithChat::PreInit((__bridge void *)UnityGetGLViewController());
    [LilithChatSDK chatInit:UnityGetGLViewController()];
}

@end