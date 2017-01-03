//
//  PVRBLEModel.h
//  PicoVRSDK
//
//  Created by Periwen on 2016/10/9.
//  Copyright © 2016年 PivoVR. All rights reserved.
//

#import "PVRSDKEnum.h"
#import <Foundation/Foundation.h>
#import <CoreBluetooth/CoreBluetooth.h>

@interface PVRBLEModel : NSObject

/** psener是否靠近 */
@property (nonatomic, assign) BOOL isPsensorNear;
/** 蓝牙连接状态 */
@property (nonatomic, assign) PVRBLEState bleState;
/** 当前软件版本 */
@property (nonatomic, strong) NSString *versionCode;
/** 蓝牙mac地址 */
@property (nonatomic, strong) NSString *bleMacAdress;
/** OTA升级状态 */
@property (nonatomic, assign) PVROTAUpgradeState otaUpgradeState;
/** 蓝牙开关状态 */
@property (nonatomic, assign) CBCentralManagerState centralManagerState;

@end
