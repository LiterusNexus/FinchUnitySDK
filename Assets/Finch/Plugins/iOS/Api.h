#pragma once
#include "Definition.h"

#if defined(_WIN32)
    #if defined(FINCH_C_LIB_EXPORT)
        #define FINCH_API __declspec(dllexport)
    #elif defined(FINCH_C_LIB_IMPORT)
        #define FINCH_API __declspec(dllimport)
    #else
        #define FINCH_API
    #endif
#else
    #if defined(FINCH_C_LIB_EXPORT)
        #define FINCH_API  __attribute__ ((visibility ("default") ))
    #elif defined(FINCH_C_LIB_IMPORT)
        #define FINCH_API
    #else
        #define FINCH_API
    #endif
#endif

#ifdef __cplusplus
extern "C"
{
#endif

    /*
     * Core
     */
    /// Get the version of FinchCore.
    FINCH_API uint32_t FinchGetCoreVersion(char* f, uint32_t length);

    /// Init FinchCore.
    FINCH_API FinchInitError FinchInit(FinchControllerType controller, FinchPlatform platform);

    /// Exit FinchCore.
    FINCH_API void FinchExit();

    /// Returns the type of controller that used in the FinchCore.
    FINCH_API FinchControllerType FinchGetControllerType();

    /*
     * Data Getters
     */
    /// Returns true, if bone data is available.
    FINCH_API FinchBool FinchIsBoneAvailable(FinchBone bone);

    /// Returns bone rotation.
    FINCH_API FinchQuaternion FinchGetBoneRotation(FinchBone bone, FinchBool fPose);

    /// Returns controller rotation.
    FINCH_API FinchQuaternion FinchGetControllerRotation(FinchChirality chirality, FinchBool fPose);

    /// Returns bone position relatively midpoint of left and right shoulders.
    FINCH_API FinchVector3 FinchGetBonePosition(FinchBone bone);

    /// Returns controller position relatively midpoint of left and right shoulders.
    FINCH_API FinchVector3 FinchGetControllerPosition(FinchChirality chirality);

    /// Returns bone acceleration in local coordinate system.
    FINCH_API FinchVector3 FinchGetBoneLocalAcceleration(FinchBone bone);

    /// Returns bone angular velocity in local coordinate system.
    FINCH_API FinchVector3 FinchGetBoneLocalAngularVelocity(FinchBone bone);

    /// Returns bone acceleration in global coordinate system.
    FINCH_API FinchVector3 FinchGetBoneGlobalAcceleration(FinchBone bone);

    /// Returns bone angular velocity in global coordinate system.
    FINCH_API FinchVector3 FinchGetBoneGlobalAngularVelocity(FinchBone bone);

    /// Returns coordinates of the touch.
    FINCH_API FinchVector2 FinchGetTouchAxes(FinchChirality chirality);

    /// Returns the events flag of controller's element.
    FINCH_API FinchEventsFlag FinchGetEvents(FinchNodeType node, FinchEventType type);

    /// Returns the value of the index trigger.
    FINCH_API float FinchGetIndexTrigger(FinchChirality chirality);

    /*
     * Settings
     */
    /// Sets user defined coordinate system.
    FINCH_API void FinchSetCs(FinchVector3 x, FinchVector3 y, FinchVector3 z);

    /// Sets coordinate system to default coordinate system.
    FINCH_API void FinchSetDefaultCs();

    /// Sets root coordinate system offset.
    FINCH_API void FinchSetRootOffset(FinchVector3 v);

    /// Sets forward direction of user by selected direction of hand.
    FINCH_API void FinchRecenter(FinchChirality chirality, FinchRecenterMode mode);

    /// Sets forward direction of user by selected direction.
    FINCH_API void FinchRecenterByDirection(FinchChirality chirality, FinchQuaternion targetOrientation);

    /// Sets forward direction of user by selected direction of hands.
    FINCH_API void FinchRecenterByControllersPositions(FinchVector3 leftControllerPositionRelativelyToHmd, FinchVector3 rightControllerPositionRelativelyToHmd);

    /// Calculates axis calibration matrices by current pose.
    FINCH_API void FinchCalibration(FinchChirality chirality, FinchRecenterMode mode);

    /// Resets calibrate the selected chirality.
    FINCH_API void FinchResetCalibration(FinchChirality chirality);

    ///Returns node current orientation without calibration adjust.
    FINCH_API FinchQuaternion FinchGetNodeRawRotation(FinchNodeType node);

    ///Returns node current calibration adjust.
    FINCH_API FinchQuaternion FinchGetCalibrationAdjust(FinchNodeType node, FinchBool isPre, FinchBool useDefaultCS);

    ///Sets node calibration quaternion
    FINCH_API void FinchSetCalibrationAdjust(FinchNodeType node, FinchQuaternion calibrationQ, FinchBool isPre, FinchBool useDefaultCS);

    ///Returns true, if upper arm is reverted.
    FINCH_API FinchBool FinchIsUpperArmReverted(FinchChirality chirality);

    ///Revert upper arm orientation to another.
    FINCH_API void FinchRevertUpperArm(FinchChirality chirality);

    /// Swap the selected node calibrations.
    FINCH_API void FinchSwapCalibrations(FinchBool swapHands, FinchBool swapUpperArms);

    FINCH_API void FinchMorphCalibrationsToOuter();

    /// Full axis calibration.
    FINCH_API void FinchAxisCalibration(FinchChirality chirality, FinchAxisCalibrationStep step);

    /// Returns the bone length skeletal model.
    FINCH_API float FinchGetBoneLength(FinchBone bone);

    /// Sets the value of the selected bone length.
    FINCH_API void FinchSetBoneLength(FinchBone bone, float length);

    /// Returns the value of the distance between midpoint of eyes and the body plane.
    FINCH_API float FinchGetEyesForwardDistance();

    /// Sets the value of the distance between midpoint of eyes and the body plane.
    FINCH_API void FinchSetEyesForwardDistance(float distance);

    /// Returns the controller width.
    FINCH_API float FinchGetControllerWidth();

    /// Sets the controller width.
    FINCH_API void FinchSetControllerWidth(float width);

    /// Returns the neck lean angle in radians.
    FINCH_API float FinchGetNeckLeanAngle();

    /// Sets the neck lean angle in radians.
    FINCH_API void FinchSetNeckLeanAngle(float angle);

    /// Returns the body rotation mode used in the Finch Core.
    FINCH_API FinchBodyRotationMode FinchGetBodyRotationMode();

    /// Sets the value of the body rotation mode.
    FINCH_API void FinchSetBodyRotationMode(FinchBodyRotationMode mode);

    FINCH_API uint32_t FinchGetCalibrations(char* f, uint32_t length);

    FINCH_API void FinchSetCalibrations(const char* f);

    FINCH_API uint32_t FinchGetMathSettings(char* f, uint32_t length);

    FINCH_API void FinchSetMathSettings(const char* f);

    /*
     * Update
     */
    /// Update Finch Core Data.
    FINCH_API FinchUpdateError FinchUpdate();

    /// Update Finch Core Data with a rotation of the HMD.
    FINCH_API FinchUpdateError FinchHmdRotationUpdate(FinchQuaternion qhmd);

    /// Update Finch Core Data with a transform of the HMD.
    FINCH_API FinchUpdateError FinchHmdTransformUpdate(FinchQuaternion qhmd, FinchVector3 phmd);

    /// Extern Update Finch Core Data.
    FINCH_API FinchUpdateError FinchExternUpdate(uint8_t* rightHand, uint8_t* leftHand, uint8_t* rightUpperArm, uint8_t* leftUpperArm);

    /// Extern Update Finch Core Data with a rotation of the HMD.
    FINCH_API FinchUpdateError FinchExternHmdRotationUpdate(uint8_t* rightHand, uint8_t* leftHand, uint8_t* rightUpperArm, uint8_t* leftUpperArm, FinchQuaternion qhmd);

    /// Extern Update Finch Core Data with a transform of the HMD.
    FINCH_API FinchUpdateError FinchExternHmdTransformUpdate(uint8_t* rightHand, uint8_t* leftHand, uint8_t* rightUpperArm, uint8_t* leftUpperArm, FinchQuaternion qhmd, FinchVector3 phmd);

    /*
     * Node
     */
    FINCH_API FinchBool FinchStartScan(FinchScannerType scanner, uint32_t timeMs, int8_t rssiThreshold, FinchBool autoConnect);

    FINCH_API FinchScanResultFlag FinchStopScan(FinchBool autoConnect);

    FINCH_API FinchIOError FinchConnectNode(FinchNodeType node);

    FINCH_API void FinchDisconnectNode(FinchNodeType node, FinchBool unpair);

    /// Returns a flags of nodes state.
    FINCH_API FinchNodesStateFlag FinchGetNodesState();

    /// Returns is of node connected.
    FINCH_API FinchBool FinchIsNodeConnected(FinchNodeType node);

    // Returns is of node data correctly.
    FINCH_API FinchBool FinchIsNodeDataCorrectly(FinchNodeType node);

    /// Get the device name node.
    FINCH_API uint32_t FinchGetNodeName(FinchNodeType node, char* f, uint32_t length);

    /// Get the controller address selected node.
    FINCH_API uint32_t FinchGetNodeAddress(FinchNodeType node, char* f, uint32_t length);

    FINCH_API uint32_t FinchGetNodeManufacturerName(FinchNodeType node, char* f, uint32_t length);

    FINCH_API uint32_t FinchGetNodeModelNumber(FinchNodeType node, char* f, uint32_t length);

    FINCH_API uint32_t FinchGetNodeSerialNumber(FinchNodeType node, char* f, uint32_t length);

    FINCH_API uint32_t FinchGetNodeHardwareRevision(FinchNodeType node, char* f, uint32_t length);

    FINCH_API uint32_t FinchGetNodeFirmwareRevision(FinchNodeType node, char* f, uint32_t length);

    FINCH_API uint32_t FinchGetNodeSoftwareRevision(FinchNodeType node, char* f, uint32_t length);

    FINCH_API uint8_t FinchGetNodeVendorIDSource(FinchNodeType node);

    FINCH_API uint16_t FinchGetNodeVendorID(FinchNodeType node);

    FINCH_API uint16_t FinchGetNodeProductID(FinchNodeType node);

    FINCH_API uint16_t FinchGetNodeProductVersion(FinchNodeType node);

    /// Returns battery charge in percentages.
    FINCH_API uint8_t FinchGetNodeCharge(FinchNodeType node);

    FINCH_API uint64_t FinchGetNodeTime(FinchNodeType node);

    /// Returns the size of raw data selected node. Get the raw data selected node.
    FINCH_API uint32_t FinchGetNodeRawData(FinchNodeType node, uint8_t* data, uint32_t length);

    /// Swap the selected nodes.
    FINCH_API void FinchSwapNodes(FinchNodeType first, FinchNodeType second);

    FINCH_API FinchBool FinchSuspendNode(FinchNodeType node);

    FINCH_API FinchBool FinchResumeNode(FinchNodeType node);

    /// Writes data to a node in synchronously mode.
    FINCH_API FinchIOError FinchSendDataToNode(FinchNodeType node, uint8_t* data, uint32_t length);

#ifdef __cplusplus
}
#endif
