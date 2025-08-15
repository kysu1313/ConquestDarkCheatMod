using UnityEngine;
using System;
using MelonLoader;

namespace ConquestDarkCheatMods
{
    public class GodModeUI
    {
        private readonly MelonLogger.Instance _logger;

        public Action onApplyClicked;
        public Action onResetClicked;
        public Action onToggleGodModeClicked;

        public bool LiveApply { get; private set; }

        // Window geometry
        private Rect _win = new Rect(40, 40, 420, 540);
        private const float HeaderH = 24f;
        private const float Pad = 8f;
        private const float LineH = 22f;
        private const float LabelW = 150f;
        private const float ValW = 90f;
        private const float BtnW = 28f;
        private const float Gap = 6f;
        private const float RowGap = 6f;
        private const float BtnH = 28f;
        private const float MinW = 420f;
        private const float MinH = 540f;


        // Dragging
        private bool _dragging;
        private Vector2 _dragOffset;

        // Manual scroll
        private float _scrollY;

        // Minimal keypad popup
        private bool _popupOpen;
        private Rect _popupRect = new Rect(0, 0, 300, 340);
        private string _popupTitle;
        private string _popupBuffer = "";
        private bool _popupIsFloat;
        private Action<float> _popupApplyFloat;
        private Action<int> _popupApplyInt;
        
        // Track the active button between down/up
        private bool _btnArmed;
        private Rect _btnArmedScreenRect;
        private Rect _btnArmedRect;
        private int  _btnArmedId = -1;

        public GodModeUI(MelonLogger.Instance logger)
        {
            _logger = logger;
        }

        public void SyncStringsFromValues(ModSettings m)
        {
            /* no-op; steppers show live values */
        }

        public void DrawWindow(ModSettings m, bool canApply, bool godMode)
        {
            // Ensure min size
            if (_win.width < MinW) _win.width = MinW;
            if (_win.height < MinH) _win.height = MinH;

            GUI.depth = -1000;
            GUI.skin.label.richText = true;

            // Window bg + title
            GUI.Box(_win, "ConquestDark — God Mode (Esc to toggle)", GUI.skin.window);
            HandleDrag(new Rect(_win.x, _win.y, _win.width, HeaderH));

            // Content rect
            var content = new Rect(_win.x + Pad, _win.y + HeaderH + Pad, _win.width - (Pad * 2),
                _win.height - (HeaderH + Pad * 2));
            
            float areaH = content.height - (BtnH + 12f);
            var areaRect = new Rect(content.x, content.y, content.width, areaH);

            // Compute total content height roughly (rows)
            int lines = 1 + 6 + 1 + 6; // tip + main + header + extras
            float viewH = lines * (LineH + RowGap) + 20f;
            float maxScroll = Mathf.Max(0f, viewH - areaH);

            // Mouse wheel scroll when cursor over area
            var e = Event.current;
            if (e != null && e.type == EventType.ScrollWheel && areaRect.Contains(e.mousePosition))
            {
                _scrollY += e.delta.y * 12f;
                e.Use();
            }

            if (_scrollY < 0f) _scrollY = 0f;
            if (_scrollY > maxScroll) _scrollY = maxScroll;

            GUI.BeginGroup(areaRect);
            GUI.BeginGroup(new Rect(0, -_scrollY, areaRect.width - 16f, viewH));

            float y = 0f;

            // the inner group's screen-space origin
            var groupOrigin = new Vector2(areaRect.x, areaRect.y - _scrollY);

            GUI.Label(new Rect(0, y, areaRect.width - 16f, LineH),
                "Tip: toggle 'Live Apply' to push changes continuously. Otherwise click 'Apply to Player'.");
            y += LineH + RowGap;

            // Main fields (int/float steppers + Set… keypad)
            float a = RowIntStepper   (y, "Target Health",       m.TargetHealth,       v => m.TargetHealth = v,      50, 500, 0, int.MaxValue, groupOrigin);
            float b = RowFloatStepper (a, "Attack Speed",        m.AttackSpeedBoost,   v => m.AttackSpeedBoost = v,  10f, 50f, 0f,  99999f, groupOrigin);
            float c = RowFloatStepper (b, "Block Chance",        m.BlockChance,        v => m.BlockChance = v,       10f,   50f, 0f, 1000f, groupOrigin);
            float d = RowFloatStepper (c, "Rare Find",           m.RareFind,           v => m.RareFind = v,          10f,   50f, 0f, 1000f, groupOrigin);
            float f = RowFloatStepper (d, "Ability Cooldown",    m.AutoAttackCoolDown, v => m.AutoAttackCoolDown = v,10f, 50f, 0f, 999f, groupOrigin);
            float g = RowFloatStepper (f, "Base Movement Speed", m.BaseMovementSpeed,  v => m.BaseMovementSpeed = v, 10f,   50f, 0f, 9999f, groupOrigin);


            // Extras header
            GUI.Label(new Rect(0, g, areaRect.width - 16f, LineH), "<b>Extras</b>");
            g += LineH + RowGap;

            // Extra fields
            float h = RowFloatStepper(g, "Crit Chance", m.CritChance, v => m.CritChance = v, 0.01f, 0.1f, 0f, 1f, groupOrigin);
            float i = RowFloatStepper(h, "Crit Damage", m.CritDamage, v => m.CritDamage = v, 0.5f, 5f, 0f, 999f, groupOrigin);
            float j = RowIntStepper(i, "Projectile Amount", m.ProjAmount, v => m.ProjAmount = v, 1, 10, 0, 9999, groupOrigin);
            float k = RowIntStepper(j, "Pierce Amount", m.PierceAmount, v => m.PierceAmount = v, 1, 10, 0, 9999, groupOrigin);
            float l = RowIntStepper(k, "Target Amount", m.TargetAmount, v => m.TargetAmount = v, 1, 10, 0, 9999, groupOrigin);
            float n = RowIntStepper(l, "Chain Targets", m.ChainTargets, v => m.ChainTargets = v, 1, 10, 0, 9999, groupOrigin);

            GUI.EndGroup();

            // Vertical scrollbar
            if (maxScroll > 0f)
            {
                float thumbSize = Mathf.Max(30f, areaH * (areaH / (viewH + 0.0001f)));
                _scrollY = GUI.VerticalScrollbar(new Rect(areaRect.width - 14f, 0f, 14f, areaRect.height),
                    _scrollY, thumbSize, 0f, maxScroll);
            }

            GUI.EndGroup();

            // Bottom buttons
            float barY = content.y + content.height - (BtnH + 4f);
            float x = content.x;
            float btnW = 140f;

            bool newLive = GUI.Toggle(new Rect(x, barY, 110f, BtnH), LiveApply, " Live Apply");
            if (newLive != LiveApply) LiveApply = newLive;
            x += 120f;

            GUI.enabled = canApply;
            if (GUI.Button(new Rect(x, barY, btnW, BtnH), "Apply to Player"))
                onApplyClicked?.Invoke();
            x += btnW + Gap;
            GUI.enabled = true;

            if (GUI.Button(new Rect(x, barY, btnW, BtnH), "Reset to Defaults"))
                onResetClicked?.Invoke();
            x += btnW + Gap;

            string gmLabel = godMode ? "God Mode: ON" : "God Mode: OFF";
            if (GUI.Button(new Rect(content.x + content.width - 140f, barY, 140f, BtnH), gmLabel))
                onToggleGodModeClicked?.Invoke();

            if (_popupOpen)
                DrawNumericPopup();
        }

        public void ClampToScreen()
        {
            float maxX = Screen.width - _win.width;
            float maxY = Screen.height - _win.height;
            if (_win.x < 0) _win.x = 0;
            if (_win.y < 0) _win.y = 0;
            if (_win.x > maxX) _win.x = maxX;
            if (_win.y > maxY) _win.y = maxY;
        }

        private bool Btn(Rect localRect, string text, Vector2 originScreen, int id)
        {
            GUI.Box(localRect, text, GUI.skin.button);

            var e = Event.current;
            if (e == null) return false;

            // convert to screen-space
            Vector2 mouseScreen = originScreen + e.mousePosition;
            Rect screenRect = new Rect(
                originScreen.x + localRect.x,
                originScreen.y + localRect.y,
                localRect.width,
                localRect.height
            );

            // click down
            if (e.type == EventType.MouseDown && e.button == 0 && screenRect.Contains(mouseScreen))
            {
                _btnArmed = true;
                _btnArmedId = id;
                _btnArmedScreenRect = screenRect;
                e.Use();
                return false;
            }

            // release click
            if (e.type == EventType.MouseUp && e.button == 0 && _btnArmed && _btnArmedId == id)
            {
                bool clicked = _btnArmedScreenRect.Contains(mouseScreen);
                _btnArmed = false;
                _btnArmedId = -1;
                if (clicked) e.Use();
                return clicked;
            }

            // reset armed btn
            if (e.rawType == EventType.MouseUp && _btnArmed && _btnArmedId == id)
            {
                _btnArmed = false;
                _btnArmedId = -1;
            }

            return false;
        }

        // ------------- Rows -------------
        private float RowFloatStepper(float y, string label, float value,
            System.Action<float> set, float step, float bigStep,
            float min, float max, Vector2 originScreen)
        {
            GUI.Label(new Rect(0, y, LabelW, LineH), label);

            int idBase = label.GetHashCode();

            float x = LabelW + Gap;
            float applied = 0f;

            if (Btn(new Rect(x, y, BtnW, LineH), "-", originScreen, idBase * 2))
            {
                applied -= (Event.current.shift || Event.current.control) ? bigStep : step;
                _logger.Msg($"{label} - {applied}");
            }
            x += BtnW + Gap;

            var valRect = new Rect(x, y, ValW, LineH);
            GUI.Box(valRect, GUIContent.none, GUI.skin.textField);
            string valStr = value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            GUI.Label(valRect, valStr);
            x += ValW + Gap;

            if (Btn(new Rect(x, y, BtnW, LineH), "+", originScreen, idBase * 2 + 1))
            {
                applied += (Event.current.shift || Event.current.control) ? bigStep : step;
                _logger.Msg($"{label} + {applied}");
            }
            x += BtnW + Gap;

            var e = Event.current;
            if (e != null && e.type == EventType.ScrollWheel && valRect.Contains(e.mousePosition))
            {
                float s = (e.shift || e.control) ? bigStep : step;
                applied += (-e.delta.y) * s;
                e.Use();
            }

            if (applied != 0f)
            {
                value = Mathf.Clamp(value + applied, min, max);
                set(value);
                valStr = value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
                GUI.Label(valRect, valStr);
            }

            return y + LineH + RowGap;
        }

        private float RowIntStepper(float y, string label, int value,
            System.Action<int> set, int step, int bigStep,
            int min, int max, Vector2 originScreen)
        {
            GUI.Label(new Rect(0, y, LabelW, LineH), label);

            int idBase = label.GetHashCode();

            float x = LabelW + Gap;
            int delta = 0;

            if (Btn(new Rect(x, y, BtnW, LineH), "-", originScreen, idBase * 2))
            {
                delta -= (Event.current.shift || Event.current.control) ? bigStep : step;
                _logger.Msg($"{label} - {delta}");
            }
            x += BtnW + Gap;

            var valRect = new Rect(x, y, ValW, LineH);
            GUI.Box(valRect, GUIContent.none, GUI.skin.textField);
            GUI.Label(valRect, value.ToString());
            x += ValW + Gap;

            if (Btn(new Rect(x, y, BtnW, LineH), "+", originScreen, idBase * 2 + 1))
            {
                delta += (Event.current.shift || Event.current.control) ? bigStep : step;
                _logger.Msg($"{label} + {delta}");
            }
            x += BtnW + Gap;

            var e = Event.current;
            if (e != null && e.type == EventType.ScrollWheel && valRect.Contains(e.mousePosition))
            {
                int s = (e.shift || e.control) ? bigStep : step;
                delta += (int)(-e.delta.y) * s;
                e.Use();
            }

            if (delta != 0)
            {
                value = Mathf.Clamp(value + delta, min, max);
                set(value);
                GUI.Label(valRect, value.ToString()); // refresh
            }

            return y + LineH + RowGap;
        }

        // ------------- Popup keypad -------------

        private void OpenNumericPopup(string title, string seed, bool isFloat, Action<float> applyFloat)
        {
            _popupOpen = true;
            _popupTitle = title;
            _popupBuffer = seed ?? "";
            _popupIsFloat = isFloat;
            _popupApplyFloat = applyFloat;
            _popupApplyInt = null;

            CenterPopup();
        }

        private void OpenNumericPopup(string title, string seed, bool isFloat, Action<int> applyInt)
        {
            _popupOpen = true;
            _popupTitle = title;
            _popupBuffer = seed ?? "";
            _popupIsFloat = isFloat;
            _popupApplyFloat = null;
            _popupApplyInt = applyInt;

            CenterPopup();
        }

        private void CenterPopup()
        {
            _popupRect.x = (_win.x + _win.width / 2f) - (_popupRect.width / 2f);
            _popupRect.y = (_win.y + _win.height / 2f) - (_popupRect.height / 2f);
        }

        private void DrawNumericPopup()
        {
            GUI.Box(_popupRect, _popupTitle, GUI.skin.window);

            float x = _popupRect.x + 12f;
            float y = _popupRect.y + 32f;
            float w = _popupRect.width - 24f;

            // Display buffer
            GUI.Box(new Rect(x, y, w, 28f), _popupBuffer);
            y += 34f;

            // Key rows
            float bw = (w - 2 * 6f) / 3f;
            float bh = 36f;
            float gx = x;
            float gy = y;

            // Row helper
            void Key(string k)
            {
                if (GUI.Button(new Rect(gx, gy, bw, bh), k))
                {
                    if (k == "CLR") _popupBuffer = "";
                    else if (k == "DEL" && _popupBuffer.Length > 0)
                        _popupBuffer = _popupBuffer.Substring(0, _popupBuffer.Length - 1);
                    else if (k == "." && !_popupIsFloat)
                    {
                        /* ignore for now */
                    }
                    else if (k == "-")
                    {
                        if (_popupBuffer.StartsWith("-")) _popupBuffer = _popupBuffer.Substring(1);
                        else _popupBuffer = "-" + _popupBuffer;
                    }
                    else _popupBuffer += k;
                }

                gx += bw + 6f;
            }

            /* Key pad setup */
            // 7 8 9
            gx = x;
            Key("7");
            Key("8");
            Key("9");
            gy += bh + 6f;
            // 4 5 6
            gx = x;
            Key("4");
            Key("5");
            Key("6");
            gy += bh + 6f;
            // 1 2 3
            gx = x;
            Key("1");
            Key("2");
            Key("3");
            gy += bh + 6f;
            // +/- 0 .
            gx = x;
            Key("-");
            Key("0");
            Key(".");
            gy += bh + 6f;
            // CLR  DEL
            gx = x;
            Key("CLR");
            Key("DEL");
            gy += bh + 10f;

            // Action buttons
            float aw = (w - 6f) / 2f;
            if (GUI.Button(new Rect(x, gy, aw, 32f), "Cancel")) _popupOpen = false;

            bool ok = GUI.Button(new Rect(x + aw + 6f, gy, aw, 32f), "OK");
            if (ok)
            {
                if (_popupIsFloat)
                {
                    if (float.TryParse(_popupBuffer, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out var fv))
                        _popupApplyFloat?.Invoke(fv);
                }
                else
                {
                    if (int.TryParse(_popupBuffer, out var iv))
                        _popupApplyInt?.Invoke(iv);
                }

                _popupOpen = false;
            }
        }

        // ------------- Misc -------------

        private void HandleDrag(Rect headerRect)
        {
            var e = Event.current;
            if (e == null) return;

            if (e.type == EventType.MouseDown && headerRect.Contains(e.mousePosition))
            {
                _dragging = true;
                _dragOffset = e.mousePosition - new Vector2(_win.x, _win.y);
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _dragging)
            {
                var pos = e.mousePosition - _dragOffset;
                _win.x = pos.x;
                _win.y = pos.y;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && _dragging)
            {
                _dragging = false;
                e.Use();
            }
        }

        private static float Clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }
    }
}