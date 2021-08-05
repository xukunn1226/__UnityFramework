//
//  UnityAppController+LifeHook.m
//  Unity-iPhone
//
//  Created by duanefaith on 2017/12/18.
//

#import "UnityAppController+LifeHook.h"
#import "LLHSDKAppDelegate.h"
#import <objc/runtime.h>

static LLHSDKAppDelegate *delegate;

@implementation UnityAppController (LifeHook)

+ (void)load
{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^
    {
        delegate = [[LLHSDKAppDelegate alloc] init];
        
#ifndef REPLACE_HOOK_METHOD
#define REPLACE_HOOK_METHOD(METHOD_NAME) [self hookMethod:@selector(METHOD_NAME) withMethod:@selector(hook_##METHOD_NAME)]
#endif
        REPLACE_HOOK_METHOD(application:didFinishLaunchingWithOptions:);
        REPLACE_HOOK_METHOD(applicationDidBecomeActive:);
        REPLACE_HOOK_METHOD(application:handleOpenURL:);
        REPLACE_HOOK_METHOD(application:continueUserActivity:restorationHandler:);
        REPLACE_HOOK_METHOD(application:openURL:sourceApplication:annotation:);
        REPLACE_HOOK_METHOD(application:openURL:options:);
        REPLACE_HOOK_METHOD(startUnity:);
    });
}

+ (void)hookMethod:(SEL)originalSelector withMethod:(SEL)swizzledSelector
{
    Method originalMethod = class_getInstanceMethod(self.class, originalSelector);
    Method swizzledMethod = class_getInstanceMethod(self.class, swizzledSelector);

    BOOL success = class_addMethod(self.class, originalSelector, method_getImplementation(swizzledMethod), method_getTypeEncoding(swizzledMethod));
    if (success)
    {
        class_replaceMethod(self.class, swizzledSelector, method_getImplementation(originalMethod), method_getTypeEncoding(originalMethod));
    }
    else
    {
        method_exchangeImplementations(originalMethod, swizzledMethod);
    }
}

- (BOOL)hook_application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    BOOL ret = [self hook_application:application didFinishLaunchingWithOptions:launchOptions];
    BOOL delegateRet = YES;
    if ([delegate respondsToSelector:@selector(application:didFinishLaunchingWithOptions:)])
    {
        delegateRet = [delegate application:application didFinishLaunchingWithOptions:launchOptions];
    }
    return ret | delegateRet;
}

- (void)hook_applicationDidBecomeActive:(UIApplication *)application
{
    [self hook_applicationDidBecomeActive:application];
    if ([delegate respondsToSelector:@selector(applicationDidBecomeActive:)])
    {
        [delegate applicationDidBecomeActive:application];
    }
}

- (BOOL)hook_application:(UIApplication *)application handleOpenURL:(NSURL *)url
{
    BOOL ret = [self hook_application:application handleOpenURL:url];
    BOOL delegateRet = YES;
    if ([delegate respondsToSelector:@selector(application:handleOpenURL:)])
    {
        delegateRet = [delegate application:application handleOpenURL:url];
    }
    return ret | delegateRet;
}

- (BOOL)hook_application:(UIApplication *)application
            openURL:(NSURL *)url
  sourceApplication:(NSString *)sourceApplication
         annotation:(id)annotation
{
    BOOL ret = [self hook_application:application
                              openURL:url
                    sourceApplication:sourceApplication
                           annotation:annotation];
    BOOL delegateRet = YES;
    if ([delegate respondsToSelector:@selector(application:openURL:sourceApplication:annotation:)])
    {
        delegateRet = [delegate application:application
                                    openURL:url
                          sourceApplication:sourceApplication
                                 annotation:annotation];
    }
    return ret | delegateRet;
}

- (BOOL)hook_application:(UIApplication *)application
continueUserActivity:(NSUserActivity *)userActivity
 restorationHandler:(void (^)(NSArray *_Nullable))restorationHandler
{
    BOOL ret = [self hook_application:application
                 continueUserActivity:userActivity
                   restorationHandler: restorationHandler];
    BOOL delegateRet = YES;
    if ([delegate respondsToSelector: @selector(application:continueUserActivity:restorationHandler:)])
    {
        delegateRet = [delegate application:application continueUserActivity:userActivity restorationHandler:restorationHandler];
    }
    return ret | delegateRet;
}

- (void)hook_startUnity:(UIApplication*)application {
    [self hook_startUnity:application];
    if ([delegate respondsToSelector:@selector(startUnity:)])
    {
        [delegate startUnity:application];
    }
}

- (BOOL)hook_application:(UIApplication *)application
openURL:(NSURL *)url
options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options
{
    BOOL ret = [self hook_application:application
                              openURL:url
                              options: options];
    BOOL delegateRet = YES;
    if ([delegate respondsToSelector: @selector(application:openURL:options:)])
    {
        delegateRet = [delegate application:application openURL:url options:options];
    }
    return ret | delegateRet;
}


@end
