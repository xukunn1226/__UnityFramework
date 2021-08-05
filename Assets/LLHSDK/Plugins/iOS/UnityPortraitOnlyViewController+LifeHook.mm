//
//  UnityPortraitOnlyViewController+LifeHook.mm
//  Unity-iPhone
//
//  Created by arthur on 2020/9/9.
//

#import "UnityPortraitOnlyViewController+LifeHook.h"
#import <LilithChatWgame/LilithChat.h>

@implementation UnityPortraitOnlyViewController (LifeHook)

- (void)viewDidLoad {
    [super viewDidLoad];
    [LilithChatSDK viewDidLoad];
}

- (void)viewWillAppear:(BOOL)animated {
    [super viewWillAppear:animated];
    [LilithChatSDK viewWillAppear:animated];
}

- (void)viewDidAppear:(BOOL)animated {
    [super viewDidAppear:animated];
    [LilithChatSDK viewDidAppear:animated];
}

- (void)viewWillDisappear:(BOOL)animated {
    [super viewWillDisappear:animated];
    [LilithChatSDK viewWillDisappear:animated];
}

- (void)viewDidDisappear:(BOOL)animated {
    [super viewDidDisappear:animated];
    [LilithChatSDK viewDidDisappear:animated];
}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    [LilithChatSDK didReceiveMemoryWarning];
}

- (void)viewLayoutMarginsDidChange {
    [super viewLayoutMarginsDidChange];
    [LilithChatSDK viewLayoutMarginsDidChange];
}

- (void)viewSafeAreaInsetsDidChange {
    [super viewSafeAreaInsetsDidChange];
    [LilithChatSDK viewSafeAreaInsetsDidChange];
}

- (BOOL)shouldAutorotate {
    return [LilithChatSDK shouldAutorotate];
}

- (UIInterfaceOrientationMask)supportedInterfaceOrientations {
    return [LilithChatSDK supportedInterfaceOrientations];
}

@end
