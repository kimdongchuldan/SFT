using System;
using System.Collections.Generic;
using Foundation;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputManager
{
    public class Input
    {
        protected FSM<StateType> fsm = new();

        protected enum StateType
        {
            Standby,
            Reserved,
        }

        public virtual Vector3 ScreenPoint { get; }
        public virtual Vector3 WorldPoint { 
            get
            {
                var mc = Camera.main;
                if (mc == null)
                {
                    return new Vector3();
                }
                
                return mc.ScreenToWorldPoint(ScreenPoint);
            } 
        }

        public virtual bool Pressed { get; }
        public virtual bool Reserved { 
            get
            {
                if (fsm.Current() == (int)StateType.Standby)
                    return false;
                return true;
            }
            
            set
            {
                if (value)
                {   
                    fsm.Transit(StateType.Reserved, false);
                }
                else
                {
                    fsm.Transit(StateType.Standby, false);
                }
            }
        }

        internal void Start()
        {
            {
                FSM<StateType>.Handler b = delegate ()
                {
                    log.Log($"{GetType().Name} 대기");
                };

                FSM<StateType>.Handler u = delegate ()
                {
                };

                FSM<StateType>.Handler e = delegate ()
                {
                };

                fsm.Add(StateType.Standby, b, u, e);
            }

            {
                FSM<StateType>.Handler b = delegate ()
                {
                    log.Log($"{GetType().Name} 입력중");
                };

                FSM<StateType>.Handler u = delegate ()
                {
                    if (false == Pressed)
                    {
                        fsm.Transit(StateType.Standby);
                    }
                };

                FSM<StateType>.Handler e = delegate ()
                {
                };

                fsm.Add(StateType.Reserved, b, u, null);
            }

            fsm.Transit(StateType.Standby);
        }

        public virtual bool HasIntersect(Collider2D col)
        {
            return false;
        }

        public virtual void Update()
        {
            fsm.Update();
        }
    }

    public class TouchInput : Input
    {
        public UnityEngine.InputSystem.Controls.TouchControl Touch { get; internal set; }

        public override Vector3 ScreenPoint 
        {
            get
            {
                return Touch.position.value;
            }
        }
        
        public override bool Pressed 
        { 
            get
            {
                switch (Touch.phase.ReadValue())
                {
                    case UnityEngine.InputSystem.TouchPhase.Began:
                    case UnityEngine.InputSystem.TouchPhase.Moved:
                    case UnityEngine.InputSystem.TouchPhase.Stationary:
                        return true;
                }

                return false;
            }
        }

        public override bool HasIntersect(Collider2D col)
        {
            var mc = Camera.main;
            if (mc == null)
            {
                return false;
            }

            var sp = Touch.position.ReadValue();
            var screenPos = new Vector3(sp.x, sp.y, mc.nearClipPlane);
            var worldPoint = mc.ScreenToWorldPoint(screenPos);
            //var hit = Physics2D.Raycast(worldPoint, Vector2.zero);
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit == col)
            {
                return true;
            }
            return false;
        }
    }

    public class MouseInput : Input
    {
        public override Vector3 ScreenPoint 
        {
            get
            {
                return Mouse.current.position.ReadValue();
            }
        }
        
        public override bool Pressed 
        { 
            get
            {
                //return Mouse.current.leftButton.wasPressedThisFrame;
                return Mouse.current.leftButton.IsPressed();
            }
        }

        public override bool HasIntersect(Collider2D col)
        {
            var mc = Camera.main;
            if (mc == null)
            {
                return false;
            }

            var sp = Mouse.current.position.ReadValue();
            var screenPos = new Vector3(sp.x, sp.y, mc.nearClipPlane);
            var worldPoint = mc.ScreenToWorldPoint(screenPos);
            //var hit = Physics2D.Raycast(worldPoint, Vector2.zero);
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit == col)
            {
                return true;
            }
            
            return false;
        }
    }

    public static MouseInput MouseDevice = new();
    public static TouchInput[] Touches;
    private static readonly ILogger log = Debug.unityLogger;

    private static void HandleDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Touchscreen &&
            (change == InputDeviceChange.Added ||
            change == InputDeviceChange.Removed ||
            change == InputDeviceChange.ConfigurationChanged))
        {
            log.Log("터치 장치 변경됨");
            InitializeTouches();
        }
    }

    static void InitializeTouches()
    {
        var cur = Touchscreen.current;
        if (null != cur)
        {        
            var ts = cur.touches;
            int count = ts.Count;
            Touches = new TouchInput[count];
            for (int i=0; i<count; i++)
            {
                var t = new TouchInput();
                t.Touch = ts[i];
                Touches[i] = t;
                Touches[i].Start();
            }
        }
        else
        {
            log.Log("터치 장치 없음");
        }
    }

    static bool Initialized;

    public static void Initialize()
    {
        InitializeTouches();
        MouseDevice.Start();

        InputSystem.onDeviceChange -= HandleDeviceChange; // 중복 등록 방지
        InputSystem.onDeviceChange += HandleDeviceChange;

        Initialized = true;
    }

    static Input TestMouse(Collider2D col, bool onlyPressing)
    {
        MouseDevice.Update();

        // 이미 누군가 할당중이면 리턴
        if (MouseDevice.Reserved)
            return null;

        // 눌렀을때만 감지하려면
        if (onlyPressing)
        {
            if (false == MouseDevice.Pressed)
                return null;
        }

        // 충돌중이면
        if (MouseDevice.HasIntersect(col))
        {
            log.Log("마우스 입력 감지");
            MouseDevice.Reserved = true;
            return MouseDevice;
        }

        return null;
    }

    public static void Update()
    {
        if (false == Initialized)
            return;
    }

    public static Input Take(Collider2D col, bool onlyPressing = false)
    {
        if (false == Initialized)
            return null;

        var mc = Camera.main;
        if (mc == null)
        {
            return null;
        }

        // 마우스
        var m = TestMouse(col, onlyPressing);
        if (null != m)
            return m;
        
        // 터치
        if (null != Touches)
        {
            foreach (var t in Touches)
            {
                t.Update();

                if (t.Reserved)
                    continue;

                if (t.Pressed)
                {
                    log.Log("터치 입력 ");
                    if (true == t.HasIntersect(col))
                    {
                        t.Reserved = true;
                        return t;
                    }
                }
            }
        }

        return null;
    }
}
