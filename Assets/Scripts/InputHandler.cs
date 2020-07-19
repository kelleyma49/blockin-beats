using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public float deadZoneHorz = 0.1f;
    public float deadZoneVert = 0.5f;
    public float arrowRepeatDelay = 20.0f;
    public float arrowRepeatRate = 4.0f;

    public enum Actions
    {
        None,
        Left,
        Right,
        Down,
        Rotate,
        Pause
    };

    public Actions Action { get; private set; }

    bool JoystickHorzOutsideDeadZone { get; set; }
    bool JoystickVertOutsideDeadZone { get; set; }

    private float _keyRepeatDelayTimer;
    private float _keyRepeatTimer;
    private bool _repeatingKey;
    private bool _allowJoyLeft = true;
    private bool _allowJoyRight = true;
    private bool _allowJoyDown = true;

    // Use this for initialization
    void Start() {
    }

    bool ProcessAxis(string axis,float deadZone,out float axisValue,ref bool outsideDeadZone)
    {
        var checkRepeatKey = axis == "Horizontal";

;       axisValue = Input.GetAxis(axis);
        var trigger = false;

        if (Mathf.Abs(axisValue) < deadZone)
        {
            if (checkRepeatKey)
                _repeatingKey = false;
            outsideDeadZone = false;
        }
        else if (!outsideDeadZone)
        {
            trigger = true;
            outsideDeadZone = true;
            if (checkRepeatKey)
            {
               _repeatingKey = true;
                _keyRepeatDelayTimer = arrowRepeatDelay;
                _keyRepeatTimer = -1.0f;
            }
        }
        else if (_repeatingKey && checkRepeatKey)
        {
            if (_keyRepeatDelayTimer > 0.0f)
            {
                _keyRepeatDelayTimer -= Time.deltaTime;
                if (_keyRepeatDelayTimer < 0.0f)
                {
                    _keyRepeatTimer = 1.0f / arrowRepeatRate;
                }
            }
            else if (_keyRepeatTimer >= 0.0f)
            {
                _keyRepeatTimer -= Time.deltaTime;
                if (_keyRepeatTimer <= 0.0f)
                {
                    _keyRepeatTimer = 1.0f / arrowRepeatRate;
                    trigger = true;
                }
            }
        }

        return trigger;
    }

    // Update is called once per frame
    void Update () {
        Action = Actions.None;

        float horzAxis, vertAxis;
        bool horzOutsideDeadZone = JoystickHorzOutsideDeadZone, vertOutsideDeadZone = JoystickVertOutsideDeadZone;
        var triggerHorz = ProcessAxis("Horizontal", deadZoneHorz, out horzAxis, ref horzOutsideDeadZone);
        JoystickHorzOutsideDeadZone = horzOutsideDeadZone;
        var triggerVert = ProcessAxis("Vertical", deadZoneVert, out vertAxis, ref vertOutsideDeadZone);
        JoystickVertOutsideDeadZone = vertOutsideDeadZone;

        var joyButt1 = Input.GetButtonDown("joystick 1 button 0");
        var joyButt2 = Input.GetButtonDown("joystick 1 button 1");

        if ((horzAxis < 0.0 && triggerHorz && _allowJoyLeft) || Input.GetKeyDown(KeyCode.LeftArrow)) // keyboard left arrow key will trigger this
        {
            Action = Actions.Left;
            Debug.Log("action left");
            _allowJoyLeft = false;
        }
        else if ((horzAxis > 0.0 && triggerHorz && _allowJoyRight) || Input.GetKeyDown(KeyCode.RightArrow)) // // keyboard right arrow key will trigger this
        {
            Action = Actions.Right;
            Debug.Log("action right");
            _allowJoyRight = false;
        }
        else if ((vertAxis < 0.0 && triggerVert && _allowJoyDown) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Action = Actions.Down;
            _allowJoyDown = false;
        }
        else if (joyButt1 || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Action = Actions.Rotate;
        }
        else if (joyButt2 || Input.GetKeyDown(KeyCode.N))
        {
            Action = Actions.Pause;
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow))
            _allowJoyLeft = true;
        if (Input.GetKeyUp(KeyCode.RightArrow))
            _allowJoyRight = true;
        if (Input.GetKeyUp(KeyCode.DownArrow))
            _allowJoyDown = true;
    }
}
