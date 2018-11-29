using UnityEngine;
using Finch;

public class SmoothMovingTest : MonoBehaviour
{
    [Header("Buffer")]
    public int PositionBuffer = 9;
    public int TimeBuffer = 15;

    private float SpeedStartLerp = 0.5f;
    private float SpeedEndLerp = 1f;

    public FinchChirality Chirality = FinchChirality.Right;

    private Vector3[] positionBuffer;

    private Vector3[] positionBufferForTime;
    private float[] timeBuffer;

    void Start()
    {
        positionBuffer = new Vector3[PositionBuffer];

        positionBufferForTime = new Vector3[TimeBuffer];
        timeBuffer = new float[TimeBuffer];
    }

    void Update()
    {
        UpdateBuffers();

        FinchController controller = FinchController.GetController(Chirality);
        transform.position = Vector3.Lerp(GetMiddlePosition(), controller.Position, GetLerpPath());
        transform.rotation = controller.Rotation; //Quaternion.Lerp(GetMiddleRotation(), controller.Rotation, GetLerpPath());


        //log
        if (Chirality == FinchChirality.Left)
        {
            Chooser.Path = PositionBuffer + " " + GetLerpPath().ToString("f2");
        }

        if (FinchController.GetPressDown(FinchChirality.Any, FinchControllerElement.Trigger))
        {
            PositionBuffer++;
            PositionBuffer = Mathf.Clamp(PositionBuffer, 2, 10);
            positionBuffer = new Vector3[PositionBuffer];
        }

        if (FinchController.GetPressDown(FinchChirality.Any, FinchControllerElement.GripButton))
        {
            PositionBuffer--;
            PositionBuffer = Mathf.Clamp(PositionBuffer, 2, 10);
            positionBuffer = new Vector3[PositionBuffer];
        }
    }

    private float GetLerpPath()
    {
        float delta = SpeedEndLerp - SpeedStartLerp;
        return Mathf.Clamp(GetSpeed() - SpeedStartLerp, 0, delta) / delta;
    }

    private float GetSpeed()
    {
        float speed = 0;
        for (int i = 1; i < TimeBuffer; i++)
        {
            float ds = (positionBufferForTime[i] - positionBufferForTime[i - 1]).magnitude;
            float dt = (timeBuffer[i] - timeBuffer[i - 1]);

            if (dt == 0)
            {
                speed = 0;
                break;
            }

            speed += Mathf.Abs(ds / dt);
        }

        speed /= TimeBuffer;

        return speed;
    }

    private void UpdateBuffers()
    {
        for (int i = PositionBuffer - 1; i > 0; i--)
        {
            positionBuffer[i] = positionBuffer[i - 1];
        }

        positionBuffer[0] = FinchInput.GetPosition(Chirality);

        for (int i = TimeBuffer - 1; i > 0; i--)
        {
            positionBufferForTime[i] = positionBufferForTime[i - 1];
            timeBuffer[i] = timeBuffer[i - 1];
        }

        positionBufferForTime[0] = FinchInput.GetPosition(Chirality);
        timeBuffer[0] = Time.time;
    }

    private Vector3 GetMiddlePosition()
    {
        Vector3 middlePos = Vector3.zero;

        foreach (var i in positionBuffer)
        {
            middlePos += i;
        }

        return middlePos / positionBuffer.Length;
    }
}
