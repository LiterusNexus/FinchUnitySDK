#pragma once
#include <cstdint>
#include <cstddef>

typedef uint8_t FinchBool;
static constexpr FinchBool FinchBoolTrue = static_cast<FinchBool>(1);
static constexpr FinchBool FinchBoolFalse = static_cast<FinchBool>(0);

typedef uint8_t FinchNodesStateFlag;

typedef uint16_t FinchEventsFlag;

typedef uint8_t FinchScanResultFlag;

struct FinchVector2
{
    float x;
    float y;
};

struct FinchVector3
{
    float x;
    float y;
    float z;
};

struct FinchQuaternion
{
    float x;
    float y;
    float z;
    float w;
};

/// Describes init error code.
enum class FinchInitError : uint8_t
{
    None,
    AlreadyInitialized,
    NotInitialized,
    IllegalArgument,
    RuntimeError,

    Last,
    Unknown = Last
};

/// Describes update error code.
enum class FinchUpdateError : uint8_t
{
    None,
    NotInitialized,
    IllegalArgument,
    RuntimeError,

    Last,
    Unknown = Last
};

/// Describes IO error code.
enum class FinchIOError : uint8_t
{
    None,
    NotInitialized,
    IllegalArgument,
    RuntimeError,

    Last,
    Unknown = Last
};

/// Describes device position.
enum class FinchNodeType : uint8_t
{
    RightHand,
    LeftHand,
    RightUpperArm,
    LeftUpperArm,

    Last,
    Unknown = Last
};

/// Describes chirality of controller.
enum class FinchChirality : uint8_t
{
    Right,
    Left,
    Both = 255,

    Last = 2,
    Unknown = Last
};

/// Describes type of controller.
enum class FinchControllerType : uint8_t
{
    Hand,
    Shift,
    WaveVRDash,
    Dash,
    UniversalController,

    Last,
    Unknown = Last
};

/// Describes flag of node state.
enum class FinchNodesStateType : uint8_t
{
    Connected,
    Correctly,

    Last,
    Unknown = Last
};

/// Describes elements of a controller.
enum class FinchControllerElement : uint8_t
{
    ButtonZero = 7,
    ButtonOne = 0,
    ButtonTwo = 1,
    ButtonThree = 2,
    ButtonFour = 3,
    ButtonThumb = 9,
    Touch = 15,
    IndexTrigger = 4,
    ButtonGrip = 5,
    LeftProximity = 6,
    RightProximity = 8,
    LedChirality = 10,
    IsLedOn = 12,
    IsTouchpadNotAvailable = 11,

    Last = 16,
    Unknown = Last
};

/// Describes flag of event state.
enum class FinchEventType : uint8_t
{
    Begin,
    Process,
    End,

    Last,
    Unknown = Last
};

/// Describes recenter mode.
enum class FinchRecenterMode : uint8_t
{
    Forward,
    HmdRotation,

    Last,
    Unknown = Last
};

/// Describes body rotation mode.
enum class FinchBodyRotationMode : uint8_t
{
    None,
    ShoulderRotation,
    HandRotation,
    HandMotion,
    HmdRotation,
    ShoulderRotationWithReachout,
    FullBodyRotation,

    Last,
    Unknown = Last
};

/// Describes step of axis calibration.
enum class FinchAxisCalibrationStep : uint8_t
{
    One,
    Two,
    Three,
    Four,

    Last,
    Unknown = Last
};

/// Describes the bone of the skeleton for animation.
enum class FinchBone : uint8_t
{
    Hips,
    LeftUpperLeg,
    RightUpperLeg,
    LeftLowerLeg,
    RightLowerLeg,
    LeftFoot,
    RightFoot,
    Spine,
    Chest,
    Neck,
    Head,
    LeftShoulder,
    RightShoulder,
    LeftUpperArm,
    RightUpperArm,
    LeftLowerArm,
    RightLowerArm,
    LeftHand,
    RightHand,
    LeftToes,
    RightToes,
    LeftEye,
    RightEye,
    Jaw,
    LeftThumbProximal,
    LeftThumbIntermediate,
    LeftThumbDistal,
    LeftIndexProximal,
    LeftIndexIntermediate,
    LeftIndexDistal,
    LeftMiddleProximal,
    LeftMiddleIntermediate,
    LeftMiddleDistal,
    LeftRingProximal,
    LeftRingIntermediate,
    LeftRingDistal,
    LeftLittleProximal,
    LeftLittleIntermediate,
    LeftLittleDistal,
    RightThumbProximal,
    RightThumbIntermediate,
    RightThumbDistal,
    RightIndexProximal,
    RightIndexIntermediate,
    RightIndexDistal,
    RightMiddleProximal,
    RightMiddleIntermediate,
    RightMiddleDistal,
    RightRingProximal,
    RightRingIntermediate,
    RightRingDistal,
    RightLittleProximal,
    RightLittleIntermediate,
    RightLittleDistal,

    LeftHandCenter,
    LeftThumbTip,
    LeftIndexTip,
    LeftMiddleTip,
    LeftRingTip,
    LeftLittleTip,

    RightHandCenter,
    RightThumbTip,
    RightIndexTip,
    RightMiddleTip,
    RightRingTip,
    RightLittleTip,

    LeftClavicleBase,
    RightClavicleBase,
    LeftClavicleOffset,
    RightClavicleOffset,

    Last,
    Unknown = Last
};

enum class FinchPlatform : uint8_t
{
    External,
    Internal,
    Unity3D,
    UnrealEngine4,
    WaveVR,

    Last,
    Unknown = Last
};

enum class FinchScannerType : uint8_t
{
    None,
    Bonded,
    BA, // Bonded and Advertising, with Bonded priority.
    AB, // Advertising and Bonded, with Advertising priority.

    Last,
    Unknown = Last
};

enum class FinchUpdateType : uint8_t
{
    Internal,
    HmdRotation,
    HmdTransform,

    Last,
    Unknown = Last
};
