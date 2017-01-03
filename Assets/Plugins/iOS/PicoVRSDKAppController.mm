// Copyright 2015 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


#import <PicoVRSDK/PicoVRSDK.h>
#import <PicoVRSDK/PVRSDKUnityPlugin.h>
#import "PicoVRSDKAppController.h"



@interface PicoVRSDKAppController()

- (void)shouldAttachRenderDelegate;

@end

@implementation PicoVRSDKAppController

- (void)shouldAttachRenderDelegate{
    [PVRSDKManager shared].lensType = PVR_LENS_PICO_1S;
    UnityRegisterRenderingPlugin(&UnitySetGraphicsDevice, &UnityRenderEvent);
}

@end
IMPL_APP_CONTROLLER_SUBCLASS(PicoVRSDKAppController)
