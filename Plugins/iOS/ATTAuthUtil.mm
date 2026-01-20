#import <Foundation/Foundation.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <AdSupport/AdSupport.h>
#import "UnityInterface.h"
#import <AVFoundation/AVFoundation.h>
extern "C" {
     void _RequestTrackingAuthorizationWithCompletionHandler() {
         if (@available(iOS 14, *)) {
             [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
                 NSString *stringInt = [NSString stringWithFormat:@"%lu",(unsigned long)status];
                 const char* charStatus = [stringInt UTF8String];
                 UnitySendMessage("LevelsMap", "GetAuthorizationStatus", charStatus);
             }];
         } else {
             // iOS14以下版本依然使用老方法
             // 判断在设置-隐私里用户是否打开了广告跟踪
             if ([[ASIdentifierManager sharedManager] isAdvertisingTrackingEnabled]) {
                 UnitySendMessage("LevelsMap", "GetAuthorizationStatus", "3");
             } else {
                 UnitySendMessage("LevelsMap", "GetAuthorizationStatus", "2");
             }
         }
     }
    
     int _GetAppTrackingAuthorizationStatus() {
         if (@available(iOS 14, *)) {
             return (int)[ATTrackingManager trackingAuthorizationStatus];
         } else {
             if ([[ASIdentifierManager sharedManager] isAdvertisingTrackingEnabled]){
                 return 3;
             }
             return 2;
         }
     }

    

    void RequestIDFA()
    {

        if (@available(iOS 14, *)) {
                // iOS14及以上版本需要先请求权限
                [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
                    // 获取到权限后，依然使用老方法获取idfa
                    if (status == ATTrackingManagerAuthorizationStatusAuthorized) {
                        NSString *idfa = [[ASIdentifierManager sharedManager].advertisingIdentifier UUIDString];
                        //NSLog(@"--------%@",idfa);
                    } else {
                        //添加弹出提示框
    //                    UIAlertView*AlertView=[[UIAlertView alloc]initWithTitle:@"Permission Request" message:
    //                                           @"Please allow App to request tracking in Settings-Privacy-Tracking. "
    //                                                                   delegate:self cancelButtonTitle:@"OK" otherButtonTitles:nil];
    //                    [AlertView show];//展示出来
                       // [VCpresentViewController:alert animated:YES completion:nil];
                        //NSLog(@"请在设置-隐私-跟踪中允许App请求跟踪");
                    }
                }];
            } else {
                // iOS14以下版本依然使用老方法
                // 判断在设置-隐私里用户是否打开了广告跟踪
                if ([[ASIdentifierManager sharedManager] isAdvertisingTrackingEnabled]) {
                    NSString *idfa = [[ASIdentifierManager sharedManager].advertisingIdentifier UUIDString];
                    NSLog(@"----%@",idfa);
                } else {
                    //添加弹出提示框
    //                    UIAlertView*AlertView=[[UIAlertView alloc]initWithTitle:@"Permission Request" message:
    //                                           @"Please allow App to request tracking in Settings-Privacy-Tracking. "
    //                                                                   delegate:self cancelButtonTitle:@"OK" otherButtonTitles:nil];
    //                    [AlertView show];//展示出来
                }
            }
    }


    void SetIOSSound(){
     NSError *sessionError = nil;
        [[AVAudioSession sharedInstance]setCategory:AVAudioSessionCategorySoloAmbient error:&sessionError];
        [[AVAudioSession sharedInstance] setActive:YES error:&sessionError];
    }
}
