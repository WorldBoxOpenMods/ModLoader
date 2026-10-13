using HarmonyLib;
using NeoModLoader.constants;
using NeoModLoader.services;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector2 = UnityEngine.Vector2;
using Vector = System.Numerics.Vector2;
namespace NeoModLoader.AndroidCompatibilityModule.PCInputSystem;

static class PCInputPatches
{
    [HarmonyPatch(typeof(Input), nameof(Input.GetKey), new[] { typeof(KeyCode) })]
    [HarmonyPrefix]
    public static bool GetButton(KeyCode key, ref bool __result)
    {
        if (!PCInputSystem.ContainsInput(key))
        {
            return true;
        }

        __result = PCInputSystem.GetState(key) == KeyState.Hold;
        return false;
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDown), new[] { typeof(KeyCode) })]
    [HarmonyPrefix]
    public static bool GetButtonDown(KeyCode key, ref bool __result)
    {
        if (!PCInputSystem.ContainsInput(key))
        {
            return true;
        }

        __result = PCInputSystem.GetState(key) == KeyState.Pressed;
        return false;
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyUp), new[] { typeof(KeyCode) })]
    [HarmonyPrefix]
    public static bool GetButtonUp(KeyCode key, ref bool __result)
    {
        if (!PCInputSystem.ContainsInput(key))
        {
            return true;
        }

        __result = PCInputSystem.GetState(key) == KeyState.LetGo;
        return false;
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKey), new[] { typeof(string) })]
    [HarmonyPrefix]
    public static bool GetButton2(string name, ref bool __result)
    {
        if (!PCInputSystem.ContainsInput(name))
        {
            return true;
        }

        __result = PCInputSystem.GetState(name) == KeyState.Hold;
        return false;
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDown), new[] { typeof(string) })]
    [HarmonyPrefix]
    public static bool GetButtonDown2(string name, ref bool __result)
    {
        if (!PCInputSystem.ContainsInput(name))
        {
            return true;
        }

        __result = PCInputSystem.GetState(name) == KeyState.Pressed;
        return false;
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyUp), new[] { typeof(string) })]
    [HarmonyPrefix]
    public static bool GetButtonUp2(string name, ref bool __result)
    {
        if (!PCInputSystem.ContainsInput(name))
        {
            return true;
        }

        __result = PCInputSystem.GetState(name) == KeyState.LetGo;
        return false;
    }
    /* MOUSE UNUSUBLE DUE TO ICALLS
    [HarmonyPatch(typeof(MoveCamera), nameof(MoveCamera.updateMobileCamera))]
    [HarmonyPrefix]
    public static bool CanMoveCamera()
    {
        return PCInputSystem.CurrentMode == PCInputSystem.Mode.None;
    }
    [HarmonyPatch(typeof(Input), "get_touchCount")]
    [HarmonyPostfix] //prevent anything using touches during mouse mode
    public static void GetTouchCount(ref int __result)
    {
        if (!PCInputSystem.MouseEnabled) return;
        __result = 0;
    }
    [HarmonyPatch(typeof(Input), "get_mousePosition")]
    [HarmonyPostfix]
    public static void GetMousePosition(ref Vector3 __result)
    {
        if (PCInputSystem.MouseEnabled)
        {
            __result = PCInputSystem.MouseSystem.MousePosition;
        }
    }
    [HarmonyPatch(typeof(Input), "get_touchSupported")]
    [HarmonyPostfix]
    public static void TouchPresent(ref bool __result)
    {
        if (PCInputSystem.MouseEnabled)
        {
            __result = false;
        }
    }
    [HarmonyPatch(typeof(Input), "get_mousePresent")]
    [HarmonyPostfix]
    public static void MousePresent(ref bool __result)
    {
        if (PCInputSystem.MouseEnabled)
        {
            __result = true;
        }
    }
    [HarmonyPatch(typeof(Input), "get_mouseScrollDelta")]
    [HarmonyPostfix]
    public static void MousePresent(ref Vector2 __result)
    {
        if (PCInputSystem.MouseEnabled)
        {
            __result.y = PCInputSystem.MouseScrollWheel;
        }
    }
    [HarmonyPatch(typeof(Input), "get_anyKey")]
    [HarmonyPostfix]
    public static void AnyKey(ref bool __result)
    {
        if (PCInputSystem.AnyKey)
        {
            __result = true;
        }
    }
    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButton))]
    [HarmonyPrefix]
    public static bool GetMouse(int button, ref bool __result)
    {
        if (PCInputSystem.MouseEnabled)
        {
            __result = PCInputSystem.MouseSystem.GetState(button) == KeyState.Hold;
            return false;
        }
        return true;
    }
    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonDown))]
    [HarmonyPrefix]
    public static bool GetMouseDown(int button, ref bool __result)
    {
        if (PCInputSystem.MouseEnabled)
        {
            __result = PCInputSystem.MouseSystem.GetState(button) == KeyState.Pressed;
            return false;
        }
        return true;
    }
    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonUp))]
    [HarmonyPrefix]
    public static bool GetMouseUp(int button, ref bool __result)
    {
        if (PCInputSystem.MouseEnabled)
        {
            __result = PCInputSystem.MouseSystem.GetState(button) == KeyState.LetGo;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetAxis))]
    [HarmonyPostfix]
    public static void GetAxis(string axisName, ref float __result)
    {
        if (axisName == "Mouse ScrollWheel" && PCInputSystem.MouseEnabled)
        {
            __result = PCInputSystem.MouseScrollWheel;
        }
    }*/
}

public enum KeyState
{
    None,
    Hold,
    LetGo,
    Pressed
}
public class PCInput
{
    public Rect ButtonRect;
    public string Name;

    private bool isHeld;
    private bool pressedThisFrame;
    private bool releasedThisFrame;
    
    public void Press()
    {
        if (!isHeld)
            pressedThisFrame = true;

        isHeld = true;
    }
    public void Release()
    {
        if (isHeld)
            releasedThisFrame = true;

        isHeld = false;
    }

    public KeyState State
    {
        get
        {
            if (pressedThisFrame)
                return KeyState.Pressed;
            if (releasedThisFrame)
                return KeyState.LetGo;
            if (isHeld)
                return KeyState.Hold;
            return KeyState.None;
        }
    }

    public void EndFrame()
    {
        pressedThisFrame = false;
        releasedThisFrame = false;
    }

    public void SetPos(Vector2 pos)
    {
        ButtonRect = new Rect(pos.x, pos.y, ButtonRect.width, ButtonRect.height);
    }

    public void SetSize(Vector2 size)
    {
        ButtonRect = new Rect(ButtonRect.x, ButtonRect.y, size.x, size.y);
    }
}

public class PCInputConfig
{
    public Dictionary<KeyCode, PCInput> Inputs = new();
}
public class PCButtonSettings
{
    public class PCButton
    {
        public KeyCode Code;
        public string Name;
        public Vector Position;
        public Vector Size;
        public PCInput FromButton()
        {
            PCInput input = new PCInput();
            input.Name = Name;
            input.ButtonRect = new Rect(Position.X, Position.Y, Size.X, Size.Y);
            return input;
        }
        public static PCButton ToButton(PCInput input, KeyCode code)
        {
            PCButton pcButton = new PCButton();
            pcButton.Name = input.Name;
            pcButton.Size =  new Vector(input.ButtonRect.width, input.ButtonRect.height);
            pcButton.Position = new Vector(input.ButtonRect.x, input.ButtonRect.y);
            pcButton.Code = code;
            return pcButton;
        }
    }
    public List<PCButton> Buttons = new List<PCButton>();
    public static PCButtonSettings LoadFromPath(string Path)
    {
        try
        {
            string settings = File.ReadAllText(Path);
            return JsonConvert.DeserializeObject<PCButtonSettings>(settings);
        }
        catch
        {
            return new PCButtonSettings();
        }
    }
    public void SaveToPath(string Path)
    {
        try
        {
            string settings = JsonConvert.SerializeObject(this);
            File.WriteAllText(Path, settings);
        }
        catch (Exception e)
        {
            LogService.LogError($"Failed to write PC Input Settings due to {e}");
        }
    }

    public PCInputConfig FromSettings()
    {
        var Config = new PCInputConfig();
        foreach (var button in Buttons)
        {
            Config.Inputs.Add(button.Code, button.FromButton());
        }
        return Config;
    }

    public static PCButtonSettings ToSettings(PCInputConfig config)
    {
        PCButtonSettings settings = new PCButtonSettings();
        foreach (var pair in config.Inputs)
        {
            settings.Buttons.Add(PCButton.ToButton(pair.Value, pair.Key));
        }
        return settings;
    }
}
public static class Helper
{
    public static KeyCode GetKeyFromString(string name)
    {
        if (Enum.TryParse<KeyCode>(name, true, out var key))
            return key;
        return KeyCode.None;
    }
    public static Vector2 GetButtonSize(string Name)
    {
        return PCInputSystem.BoxStyle.CalcSize(new GUIContent(Name));
    }

    public static Vector2 ToGUI(this Vector2 vector)
    {
        return new Vector2(vector.x, Screen.height-vector.y);
    }

    public static string IsEnabled(this bool b)
    {
        return b ? "Enabled" : "Disabled";
    }

    public static bool IsPCInputSystem(this GameObject obj)
    {
        return obj.Equals(PCInputSystem.Instance.gameObject);
    }
}

public class PCInputSystem : MonoBehaviour
{
    public static class MouseSystem
    {
        public class MouseButton
        {
            public int FingerID = -1;
            public KeyState State = KeyState.None;
            public bool UpdatedThisFrame = false;
            public Vector2 Position;
        }

        internal static bool MultiClicking
        {
            get;
            set
            {
                field = value;
                if (field)
                {
                    WorldTip.instance.showToolbarText("use first finger to left click, second to double click and third to middle click");
                }
            }
        }

        internal static bool ClickingEnabled = true;
        private static MouseButton Left = new();
        private static MouseButton Right = new();
        private static MouseButton Middle = new();

        private static MouseButton[] Buttons = [Left, Right, Middle];

        public static Vector2 MousePosition { get; private set; }

        public static KeyState GetState(int index) => Buttons[index].State;

        public static Rect GetMouseRect(Vector2 Size)
        {
            var Pos = MousePosition.ToGUI();
            return new Rect(Pos.x - Size.x / 2, Pos.y - Size.y / 2, Size.x, Size.y);
        }
        public static void Reset()
        {
            foreach (var b in Buttons)
            {
                b.FingerID = -1;
                b.State = KeyState.None;
                b.UpdatedThisFrame = false;
                b.Position = Vector2.zero;
            }
            MousePosition = Vector2.zero;
        }
        public static void Update()
        {
            var touches = Input.touches;
            foreach (var b in Buttons)
                b.UpdatedThisFrame = false;
            
            foreach (var touch in touches)
            {
                if (IsWithinAnything(touch.position.ToGUI(), out bool inwindow))
                {
                    if (!inwindow)
                    {
                        ResetScrollWheel();
                    }
                    continue;
                }
                var button = GetButton(touch.fingerId);
                if (button == null) continue;
                ResetScrollWheel();
                button.UpdatedThisFrame = true;
                if (ClickingEnabled)
                {
                    button.State = touch.phase switch
                    {
                        TouchPhase.Began => KeyState.Pressed,
                        TouchPhase.Moved or TouchPhase.Stationary => KeyState.Hold,
                        TouchPhase.Ended or TouchPhase.Canceled => KeyState.LetGo,
                        _ => button.State
                    };
                }
                button.Position = touch.position;
            }
            foreach (var b in Buttons)
            {
                if (b.FingerID != -1 && !b.UpdatedThisFrame)
                {
                    if (b.State is KeyState.Hold or KeyState.Pressed)
                        b.State = KeyState.LetGo;
                }
            }
            UpdateMousePosition();
        }
        private static void UpdateMousePosition()
        {
            foreach (var Button in Buttons)
            {
                if (!Button.UpdatedThisFrame) continue;
                MousePosition = Button.Position;
                return;
            }
        }
        public static void LateUpdate()
        {
            foreach (var b in Buttons)
            {
                if (b.State == KeyState.LetGo)
                {
                    b.State = KeyState.None;
                    b.FingerID = -1;
                }
                else if (b.State == KeyState.Pressed)
                {
                    b.State = KeyState.Hold;
                }
            }
        }
        private static bool Check(MouseButton button, int fingerId)
        {
            if (button.FingerID == fingerId)
            {
                return true;
            }
            if (button.FingerID != -1) return false;
            button.FingerID = fingerId;
            return true;
        }
        private static MouseButton GetButton(int fingerId)
        {
            if (!MultiClicking) return Check(SelectedButton, fingerId) ? SelectedButton : null;
            foreach (var but in Buttons)
            {
                if (Check(but, fingerId))
                {
                    return but;
                }
            }
            return null;
        }

        public static string GetButtonName(int ID) => ID switch
        {
            0 => "Left Button",
            1 => "Right Button",
            2 => "Middle Button",
            _ => throw new ArgumentOutOfRangeException()
        };
        public static MouseButton SelectedButton => Buttons[selectedbutton];
        internal static int selectedbutton = 0;
    }
    public static KeyState GetState(KeyCode Code)
    {
        return Config.Inputs[Code].State;
    }
    public static KeyState GetState(string name)
    {
        return Config.Inputs[Helper.GetKeyFromString(name)].State;
    }

    public static bool ContainsInput(KeyCode Code)
    {
        return Config.Inputs.ContainsKey(Code);
    }
    public static bool ContainsInput(string name)
    {
        return Config.Inputs.ContainsKey(Helper.GetKeyFromString(name));
    }

    public static KeyCode GetKey(PCInput input)
    {
        return Config.Inputs.FirstOrDefault(x => x.Value == input).Key;
    }
    public static PCInputSystem Instance { get; private set; }

    public enum Mode
    {
        None,
        Editing,
        Mouse
    }

    public static Mode CurrentMode
    {
        get;
        private set
        {
            switch (value)
            {
                case Mode.None:
                    StopEditing();
                    MouseSystem.Reset();
                    break;
                case Mode.Editing:
                    BeginEditing();
                    break;
                case Mode.Mouse: 
                    WorldTip.instance.showToolbarText("you are now simulating a mouse on your touch screen");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
            field = value;
        }
    }

    public static bool MouseEnabled => CurrentMode == Mode.Mouse;

    #region  GUI
    private static Rect MainButton;
    private static Rect MouseButton;
    private static Rect MainWindow;
    private static GUI.WindowFunction MainWindowFunction;
    private static GUI.WindowFunction MouseWindowFunction;
    private static PCInputConfig Config;
    #endregion
    internal static void Init()
    {
        if (!Others.IsMobile)
        {
            return;
        }
        Harmony.CreateAndPatchAll(typeof(PCInputPatches), Others.harmony_id);
        Config = PCButtonSettings.LoadFromPath(Paths.PCInputConfigPath).FromSettings();
        Instance = new GameObject("PCInputSystem").AddComponent<PCInputSystem>();
        Object.DontDestroyOnLoad(Instance.gameObject);
        InitGUI();
    }

    static void InitGUI()
    {
        MainWindowFunction = ManagePCInputs;
        MouseWindowFunction = ManageMouse;
        MainButton = new Rect(0, 0,60, 30);
        MainWindow = new Rect(0, 0, Screen.width/2.5f, Screen.height/4.5f);
        MouseButton = new Rect(60, 0,85, 30);
    }

    public static void SaveConfig(string Path)
    {
        PCButtonSettings.ToSettings(Config).SaveToPath(Path);
    }

    static void BeginEditing()
    {
        global::Config.paused = true;
    }

    static void StopEditing()
    {
        global::Config.paused = false;
        global::Config.ui_main_hidden = false;
        SelectedInput = null;
    }

    void DrawButtons()
    {
        foreach (var pair in Config.Inputs)
        {
            var button = pair.Value;
            var state = button.State;
            Rect rect = button.ButtonRect;
            bool pressed = state == KeyState.Hold || state == KeyState.Pressed;
            if (pressed || SelectedInput == button)
            {
                rect.x -= 5;
                rect.y -= 5;
                rect.width += 5;
                rect.height += 5;
            }
            GUI.Box(rect, button.Name);
        }
    }
    internal static GUIStyle BoxStyle;
    private void OnGUI()
    {
        BoxStyle ??= GUI.skin.box;
        DrawButtons();
        if (CurrentMode == Mode.Editing)
        {
            global::Config.ui_main_hidden = true;
            GUI.Window(67, MainWindow, MainWindowFunction, "PCInputManager");
            MoveInputs();
            return;
        }
        CheckInputs();
        if (CurrentMode == Mode.Mouse)
        {
            GUI.Window(69, MainWindow, MouseWindowFunction, "Mouse Manager");
            GUI.Box(MouseSystem.GetMouseRect(Helper.GetButtonSize("Cursor")), "Cursor");
        }
        else
        {
            if (GUI.Button(MainButton, "PCInput"))
            {
                CurrentMode = Mode.Editing;
            }
            /*if (GUI.Button(MouseButton, "MouseMode"))
            {
                CurrentMode = Mode.Mouse;
            }*/
        }
    }

    private static PCInput SelectedInput;
    void CheckInputs()
    {
        foreach (var pair in Config.Inputs)
        {
            var button = pair.Value;
            bool inside = false;
            foreach (var Touch in Input.touches)
            {
                var touchPos = Touch.position.ToGUI();
               
                if (button.ButtonRect.Contains(touchPos))
                {
                    inside = true;
                    break;
                }
            }
            if (inside)
                button.Press();
            else
                button.Release();
        }
    }

    void Update()
    {
        if (MouseEnabled)
        {
            MouseSystem.Update();
        }
    }
    void LateUpdate()
    {
        foreach (var input in Config.Inputs.Values)
        {
            input.EndFrame();
        }
        if (MouseEnabled)
        {
            MouseSystem.LateUpdate();
        }
    }

    static bool keySelectorOpen = false;
    private static Vector2 scrollPos;

    private static KeyCode pendingKey = KeyCode.None;

    private static void DrawKeySelector()
    {
        if (GUILayout.Button($"Selected Key: {pendingKey}"))
        {
            keySelectorOpen = !keySelectorOpen;
        }

        if (!keySelectorOpen)
            return;

        GUILayout.Label("Choose Key:");
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (GUILayout.Button(key.ToString()))
            {
                pendingKey = key;
                keySelectorOpen = false;
                break;
            }
        }

        GUILayout.EndScrollView();
    }
    public enum TouchState
    {
        NoTouch,
        InWindow,
        Touch,
        MissTouch
    }
    public static TouchState CheckTouch(out Vector2 pos, out PCInput selected)
    {
        selected = null;
        pos = default;
        if (Input.touchCount == 0) return TouchState.NoTouch;
        var touch = Input.GetTouch(0);
        pos = touch.position.ToGUI();
        if (MainWindow.Contains(pos))
        {
            return TouchState.InWindow;
        }
        foreach (var pair in Config.Inputs)
        {
            var button = pair.Value;

            if (button.ButtonRect.Contains(pos))
            {
                selected = button;
                return TouchState.Touch;
            }
        }
        return TouchState.MissTouch;
    }

    public static bool AnyKey
    {
        get
        {
            foreach (var button in Config.Inputs)
            {
                if (button.Value.State != KeyState.None)
                {
                    return true;
                }
            }
            return false;
        }
    }

    private TouchState PrevState;
    void MoveInputs()
    {
        var state = CheckTouch(out var pos, out var selected);
        if (state != TouchState.Touch)
        {
            if (state == TouchState.MissTouch)
            {
                if (PrevState == TouchState.NoTouch)
                {
                    SelectedInput = null;
                }
            }
            else
            {
                PrevState = state;
                return;
            }
        }
        else if(SelectedInput == null)
        {
            SelectedInput = selected;
            currentY = SelectedInput.ButtonRect.size.y;
            currentX =  SelectedInput.ButtonRect.size.x;
        }
        else if (PrevState == TouchState.NoTouch && selected != SelectedInput)
        {
            SelectedInput = null;
        }
        PrevState = state;
        SelectedInput?.SetPos(pos - SelectedInput.ButtonRect.size/2);
    }
    public static Rect GetNextButtonRect(Vector2 Size)
    {
        for (int x = 0; x < Screen.width/Size.x; x ++)
        {
            for (int y = 0; y < Screen.height / Size.y; y++)
            {
                Rect rect = new Rect(x * Size.x, y * Size.y, Size.x, Size.y);
                if (!DoesRectIntersectAnything(rect))
                {
                    return rect;
                }
            }
        }
        return default;
    }
    public static PCInput CreateNewButton(string Name, KeyCode Code, Rect rect = default)
    {
        if (ContainsInput(Code))
        {
            return Config.Inputs[Code];
        }
        if (rect == default)
        {
            rect = GetNextButtonRect(Helper.GetButtonSize(Name)*2);
        }
        PCInput input = new PCInput { Name = Name, ButtonRect = rect };
        Config.Inputs.Add(Code, input);
        return input;
    }

    public static void DeleteButton(KeyCode code)
    {
        Config.Inputs.Remove(code);
    }
    public static bool DoesRectIntersectAnything(Rect rect)
    {
        if (MainWindow.Overlaps(rect))
        {
            return true;
        }
        foreach (var button in Config.Inputs.Values)
        {
            if (button.ButtonRect.Overlaps(rect))
            {
                return true;
            }
        }
        return false;
    }
    public static bool IsWithinAnything(Vector2 pos, out bool InWindow)
    {
        if (MainWindow.Contains(pos))
        {
            InWindow = true;
            return true;
        }

        InWindow = false;
        foreach (var button in Config.Inputs.Values)
        {
            if (button.ButtonRect.Contains(pos))
            {
                return true;
            }
        }
        return false;
    }

    private static float currentX = 50;
    private static float currentY = 50;
    static void ManagePCInputs(int windowid)
    {
        if (GUILayout.Button("Stop Editing"))
        {
            CurrentMode = Mode.None;
        }
        if (GUILayout.Button("Save Settings"))
        {
            SaveConfig(Paths.PCInputConfigPath);
        }
        DrawKeySelector();
        GUILayout.Label("Button Size X");
        currentX = GUILayout.HorizontalSlider(
            currentX,
            0,
            300,
            GUI.skin.horizontalSlider,
            GUI.skin.horizontalSliderThumb,
            new[] { GUILayout.Width(200), GUILayout.Height(20) }
        );
        GUILayout.Label("Button Size Y");
        currentY = GUILayout.HorizontalSlider(
            currentY,
            0,
            300,
            GUI.skin.horizontalSlider,
            GUI.skin.horizontalSliderThumb,
            new[] { GUILayout.Width(200), GUILayout.Height(20) }
        );
        if (SelectedInput != null)
        {
            if (GUILayout.Button("Delete Button"))
            {
                DeleteButton(GetKey(SelectedInput));
                SelectedInput = null;
                return;
            }
            SelectedInput.SetSize(new Vector2(currentX, currentY));
            SelectedInput.Name = GUILayout.TextField(SelectedInput.Name);
            if (pendingKey == KeyCode.None) return;
            var old = GetKey(SelectedInput);
            if (old == pendingKey || old == default) return;
            Config.Inputs.Remove(old);
            Config.Inputs[pendingKey] = SelectedInput;
            pendingKey = KeyCode.None;
        }
        else
        {
           string Name = GUILayout.TextField("New Button Name");
           if (GUILayout.Button("Create New Button") && pendingKey != KeyCode.None)
           {
               SelectedInput = CreateNewButton(Name, pendingKey, GetNextButtonRect(new Vector2(currentX, currentY)));
               pendingKey = KeyCode.None;
           }
        }
    }

    internal static float MouseScrollWheel = 0;
    static void ManageMouse(int windowid)
    {
        if (GUILayout.Button("Disable Mouse"))
        {
            CurrentMode = Mode.None;
        }
        if (GUILayout.Button($"Multi Clicking : {MouseSystem.MultiClicking.IsEnabled()}"))
        {
            MouseSystem.MultiClicking = !MouseSystem.MultiClicking;
        }
        if (!MouseSystem.MultiClicking)
        {
            GUILayout.Label($"Current Button: {MouseSystem.GetButtonName(MouseSystem.selectedbutton)}");
            for (int i = 0; i < 3; i++)
            {
                if (!GUILayout.Button(MouseSystem.GetButtonName(i))) continue;
                ResetScrollWheel();
                MouseSystem.selectedbutton = i;
            }
        }
        GUILayout.Label("Mouse ScrollWheel");
        MouseScrollWheel = GUILayout.HorizontalSlider(
            MouseScrollWheel,
            -1,
            1,
            GUI.skin.horizontalSlider,
            GUI.skin.horizontalSliderThumb, new [] {GUILayout.Width(200), GUILayout.Height(20)});
        if (Event.current.type == EventType.MouseUp)
        {
           ResetScrollWheel();
        }
        if (GUILayout.Button($"Clicking : {MouseSystem.ClickingEnabled.IsEnabled()}"))
        {
            MouseSystem.ClickingEnabled = !MouseSystem.ClickingEnabled;
        }
    }
    static void ResetScrollWheel()
    {
        MouseScrollWheel = 0;
    }
}