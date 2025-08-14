using UnityEngine;
using System;

namespace ConquestDarkNet6Mods
{
    public class GodModeUI
    {
        // Public callbacks wired by driver
        public Action onApplyClicked;
        public Action onResetClicked;
        public Action onToggleGodModeClicked;

        public bool LiveApply { get; private set; }

        // Window geometry
        private Rect _win = new Rect(40, 40, 420, 540);
        private const float HeaderH = 24f;
        private const float Pad     = 8f;
        private const float LineH   = 22f;
        private const float LabelW  = 180f;
        private const float ValW    = 120f;   // value label box
        private const float BtnW    = 38f;    // step buttons
        private const float Gap     = 6f;
        private const float RowGap  = 6f;
        private const float BtnH    = 28f;
        private const float MinW    = 420f;
        private const float MinH    = 540f;

        // Dragging
        private bool _dragging;
        private Vector2 _dragOffset;

        // Manual scroll
        private float _scrollY;

        // --- Minimal keypad popup (all GUI.Button/Label, no TextField) ---
        private bool _popupOpen;
        private Rect _popupRect = new Rect(0, 0, 300, 340);
        private string _popupTitle;
        private string _popupBuffer = "";
        private bool _popupIsFloat;
        private Action<float> _popupApplyFloat;
        private Action<int>   _popupApplyInt;

        // Kept for driver compatibility; no longer used
        public void SyncStringsFromValues(ModSettings m) { /* no-op; steppers show live values */ }

        public void DrawWindow(ModSettings m, bool canApply, bool godMode)
        {
            // Ensure min size
            if (_win.width < MinW) _win.width = MinW;
            if (_win.height < MinH) _win.height = MinH;

            GUI.skin.label.richText = true;

            // Window bg + title
            GUI.Box(_win, "ConquestDark — God Mode (Esc to toggle)", GUI.skin.window);
            HandleDrag(new Rect(_win.x, _win.y, _win.width, HeaderH));

            // Content rect
            var content = new Rect(_win.x + Pad, _win.y + HeaderH + Pad, _win.width - (Pad * 2), _win.height - (HeaderH + Pad * 2));
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

            // Clip group
            GUI.BeginGroup(areaRect);
            // Content group shifted up by scroll
            GUI.BeginGroup(new Rect(0, -_scrollY, areaRect.width - 16f, viewH));

            float y = 0f;

            // Tip
            GUI.Label(new Rect(0, y, areaRect.width - 16f, LineH),
                "Tip: toggle 'Live Apply' to push changes continuously. Otherwise click 'Apply to Player'.");
            y += LineH + RowGap;

            // Main fields (int/float steppers + Set… keypad)
            y = RowIntStepper   (y, "Target Health",       m.TargetHealth,       v => m.TargetHealth = v,      1, 10, 0, int.MaxValue);
            y = RowFloatStepper (y, "Attack Speed",        m.AttackSpeedBoost,   v => m.AttackSpeedBoost = v,  0.1f, 1f, 0f,  99999f);
            y = RowFloatStepper (y, "Block Chance",        m.BlockChance,        v => m.BlockChance = v,       1f,   10f, 0f, 100f);
            y = RowFloatStepper (y, "Rare Find",           m.RareFind,           v => m.RareFind = v,          1f,   10f, 0f, 100f);
            y = RowFloatStepper (y, "Ability Cooldown",    m.AutoAttackCoolDown, v => m.AutoAttackCoolDown = v,0.05f, 0.5f, 0f,  999f);
            y = RowFloatStepper (y, "Base Movement Speed", m.BaseMovementSpeed,  v => m.BaseMovementSpeed = v, 1f,   10f, 0f,  9999f);

            // Extras header
            GUI.Label(new Rect(0, y, areaRect.width - 16f, LineH), "<b>Extras</b>");
            y += LineH + RowGap;

            // Extra fields
            y = RowFloatStepper (y, "Crit Chance",         m.CritChance,         v => m.CritChance = v,       0.01f, 0.1f, 0f,  1f);
            y = RowFloatStepper (y, "Crit Damage",         m.CritDamage,         v => m.CritDamage = v,       0.5f,  5f,   0f,  999f);
            y = RowIntStepper   (y, "Projectile Amount",   m.ProjAmount,         v => m.ProjAmount = v,       1,     10,    0,   9999);
            y = RowIntStepper   (y, "Pierce Amount",       m.PierceAmount,       v => m.PierceAmount = v,     1,     10,    0,   9999);
            y = RowIntStepper   (y, "Target Amount",       m.TargetAmount,       v => m.TargetAmount = v,     1,     10,    0,   9999);
            y = RowIntStepper   (y, "Chain Targets",       m.ChainTargets,       v => m.ChainTargets = v,     1,     10,    0,   9999);

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

            // Popup last so it draws on top
            if (_popupOpen)
                DrawNumericPopup();
        }

        public void ClampToScreen()
        {
            float maxX = Screen.width  - _win.width;
            float maxY = Screen.height - _win.height;
            if (_win.x < 0) _win.x = 0;
            if (_win.y < 0) _win.y = 0;
            if (_win.x > maxX) _win.x = maxX;
            if (_win.y > maxY) _win.y = maxY;
        }

        // ------------- Rows (no TextField) -------------

        private float RowFloatStepper(float y, string label, float value, Action<float> set, float step, float bigStep, float min, float max)
        {
            // label
            GUI.Label(new Rect(0, y, LabelW, LineH), label);

            // value box
            var valRect = new Rect(LabelW + Gap, y, ValW, LineH);
            GUI.Box(valRect, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

            // mouse wheel adjust over value
            var e = Event.current;
            if (e != null && e.type == EventType.ScrollWheel && new Rect(valRect).Contains(e.mousePosition))
            {
                float s = (e.shift || e.control) ? bigStep : step;
                value = Clamp(value + (-e.delta.y) * s, min, max);
                set(value);
                e.Use();
            }

            // step buttons
            float x = valRect.xMax + Gap;
            float s1 = GUI.Button(new Rect(x, y, BtnW, LineH), "-10") ? -bigStep : 0f; x += BtnW + 2;
            float s2 = GUI.Button(new Rect(x, y, BtnW, LineH), " -1") ? -step    : 0f; x += BtnW + 2;
            float s3 = GUI.Button(new Rect(x, y, BtnW, LineH), " +1") ?  step    : 0f; x += BtnW + 2;
            float s4 = GUI.Button(new Rect(x, y, BtnW, LineH), "+10") ?  bigStep : 0f; x += BtnW + 6;

            float delta = s1 + s2 + s3 + s4;
            if (delta != 0f)
            {
                value = Clamp(value + delta, min, max);
                set(value);
            }

            // Set… keypad
            if (GUI.Button(new Rect(x, y, 50f, LineH), "Set…"))
                OpenNumericPopup(label, value.ToString(System.Globalization.CultureInfo.InvariantCulture), true,
                    (Action<float>)(fv => set(Clamp(fv, min, max))));

            return y + LineH + RowGap;
        }

        private float RowIntStepper(float y, string label, int value, Action<int> set, int step, int bigStep, int min, int max)
        {
            GUI.Label(new Rect(0, y, LabelW, LineH), label);
            var valRect = new Rect(LabelW + Gap, y, ValW, LineH);
            GUI.Box(valRect, value.ToString());

            var e = Event.current;
            if (e != null && e.type == EventType.ScrollWheel && new Rect(valRect).Contains(e.mousePosition))
            {
                int s = (e.shift || e.control) ? bigStep : step;
                int nv = Mathf.Clamp(value + (int)(-e.delta.y) * s, min, max);
                if (nv != value) { value = nv; set(value); }
                e.Use();
            }

            float x = valRect.xMax + Gap;
            int d1 = GUI.Button(new Rect(x, y, BtnW, LineH), "-10") ? -bigStep : 0; x += BtnW + 2;
            int d2 = GUI.Button(new Rect(x, y, BtnW, LineH), " -1") ? -step    : 0; x += BtnW + 2;
            int d3 = GUI.Button(new Rect(x, y, BtnW, LineH), " +1") ?  step    : 0; x += BtnW + 2;
            int d4 = GUI.Button(new Rect(x, y, BtnW, LineH), "+10") ?  bigStep : 0; x += BtnW + 6;

            int delta = d1 + d2 + d3 + d4;
            if (delta != 0)
            {
                value = Mathf.Clamp(value + delta, min, max);
                set(value);
            }

            if (GUI.Button(new Rect(x, y, 50f, LineH), "Set…"))
                OpenNumericPopup(label, value.ToString(), false, i => set(Mathf.Clamp(i, min, max)));

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
            GUI.Box(new Rect(x, y, w, 28f), _popupBuffer); y += 34f;

            // Key rows (buttons only)
            float bw = (w - 2 * 6f) / 3f; float bh = 36f; float gx = x; float gy = y;

            // Row helper
            void Key(string k)
            {
                if (GUI.Button(new Rect(gx, gy, bw, bh), k))
                {
                    if (k == "CLR") _popupBuffer = "";
                    else if (k == "DEL" && _popupBuffer.Length > 0) _popupBuffer = _popupBuffer.Substring(0, _popupBuffer.Length - 1);
                    else if (k == "." && !_popupIsFloat) { /* ignore */ }
                    else if (k == "-" )
                    {
                        if (_popupBuffer.StartsWith("-")) _popupBuffer = _popupBuffer.Substring(1);
                        else _popupBuffer = "-" + _popupBuffer;
                    }
                    else _popupBuffer += k;
                }
                gx += bw + 6f;
            }

            // 7 8 9
            gx = x; Key("7"); Key("8"); Key("9"); gy += bh + 6f;
            // 4 5 6
            gx = x; Key("4"); Key("5"); Key("6"); gy += bh + 6f;
            // 1 2 3
            gx = x; Key("1"); Key("2"); Key("3"); gy += bh + 6f;
            // +/- 0 .
            gx = x; Key("-"); Key("0"); Key("."); gy += bh + 6f;
            // CLR  DEL
            gx = x; Key("CLR"); Key("DEL"); gy += bh + 10f;

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
