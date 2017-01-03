//
//  PVRManager.h
//  IPVRSDK
//
//  Created by Peiwen.Liu on 16/6/22.
//  Copyright © 2016年 PivoVR. All rights reserved.
//

#import "PVREye.h"
#import "PVRSDKEnum.h"
#import "PVRBLEModel.h"
#import <GLKit/GLKit.h>
#import "PVRSingleton.h"
#import "PVRGLNativeRender.h"

@class UnityAppController;
@class PVRSDKSettingManager;

@interface PVRSDKManager : NSObject

singleton_interface(PVRSDKManager)

@property (nonatomic, assign) PVRLensType lensType;

@property (nonatomic, assign, getter=isNative) BOOL native;

@property (nonatomic, assign, getter = isChromaticAberration) BOOL chromaticAberration;

@property (nonatomic, strong) PVRGLNativeRender *nativeRender;

@property (nonatomic, strong) PVRSDKSettingManager *settingManager;

@property (nonatomic, strong) PVRBLEModel *bleModel;

@property (nonatomic, weak) UnityAppController *AppController;

@property (nonatomic, assign) PVRSDKFromType sdkFromType;

- (void)setupRender;

- (void)updateRender;

- (void)renderNative:(GLKMatrix4)headview;

- (void)renderBase:(GLKMatrix4)headview;

- (void)shutdownRender;

- (PVREye *)eyeWithType:(PVREyeType)eyetype;

- (float)eyeFov;

- (float)sepration;

- (void)startTracking:(UIInterfaceOrientation)orientation;

- (void)resetTracking;

- (void)stopTracking;

- (void)updateDeviceOrientation:(UIInterfaceOrientation)orientation;

- (GLKMatrix4)lastHeadView;

- (NSString *)phoneType;

- (BOOL)openBLECenter;

- (BOOL)scanBLEDevice;

- (BOOL)connectBLEDevice:(NSString *)mac;

- (BOOL)getSoftWare;

- (BOOL)sendMCU;

- (BOOL)upgradeOTABinData:(NSData *)data num:(long)num;

- (BOOL)stopBLECenter;
@end
