using UnityEngine;
using System;
using ConquestDarkCheatMods.Constants;
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

        private int _idCounter;

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

        // Tracking the active button between down/up
        private bool _btnArmed;
        private Rect _btnArmedScreenRect;
        private Rect _btnArmedRect;
        private int _btnArmedId = -1;

        public GodModeUI(MelonLogger.Instance logger)
        {
            _logger = logger;
        }

        public void SyncStringsFromValues(ModSettings m)
        {
            
        }

        public void DrawWindow(ModSettings m, bool canApply, bool godMode)
        {
            try
            {
                _idCounter = 0;
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

                // Compute total content height (rows)
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

                // inner group's screen-space origin
                var groupOrigin = new Vector2(areaRect.x, areaRect.y - _scrollY);

                GUI.Label(new Rect(0, y, areaRect.width - 16f, LineH),
                    "Tip: toggle 'Live Apply' to push changes continuously. Otherwise click 'Apply to Player'.");
                y += LineH + RowGap;

                float a = RowIntStepper(
                    y, "Target Health", m.TargetHealth, v => m.TargetHealth = v,
                    CheatUiConstants.TargetHealth_Step, CheatUiConstants.TargetHealth_BigStep,
                    CheatUiConstants.TargetHealth_Min, CheatUiConstants.TargetHealth_Max, groupOrigin);

                float b = RowFloatStepper(
                    a, "Attack Speed", m.AttackSpeedBoost, v => m.AttackSpeedBoost = v,
                    CheatUiConstants.AttackSpeed_Step, CheatUiConstants.AttackSpeed_BigStep,
                    CheatUiConstants.AttackSpeed_Min, CheatUiConstants.AttackSpeed_Max, groupOrigin);

                float c = RowFloatStepper(
                    b, "Block Chance", m.BlockChance, v => m.BlockChance = v,
                    CheatUiConstants.BlockChance_Step, CheatUiConstants.BlockChance_BigStep,
                    CheatUiConstants.BlockChance_Min, CheatUiConstants.BlockChance_Max, groupOrigin);

                float d = RowFloatStepper(
                    c, "Rare Find", m.RareFind, v => m.RareFind = v,
                    CheatUiConstants.RareFind_Step, CheatUiConstants.RareFind_BigStep,
                    CheatUiConstants.RareFind_Min, CheatUiConstants.RareFind_Max, groupOrigin);

                float f = RowFloatStepper(
                    d, "Ability Cooldown", m.AutoAttackCoolDown, v => m.AutoAttackCoolDown = v,
                    CheatUiConstants.AbilityCooldown_Step, CheatUiConstants.AbilityCooldown_Big,
                    CheatUiConstants.AbilityCooldown_Min, CheatUiConstants.AbilityCooldown_Max, groupOrigin);

                float g = RowFloatStepper(
                    f, "Base Movement Speed", m.BaseMovementSpeed, v => m.BaseMovementSpeed = v,
                    CheatUiConstants.BaseMoveSpeed_Step, CheatUiConstants.BaseMoveSpeed_BigStep,
                    CheatUiConstants.BaseMoveSpeed_Min, CheatUiConstants.BaseMoveSpeed_Max, groupOrigin);

                // Extras
                GUI.Label(new Rect(0, g, areaRect.width - 16f, LineH), "<b>Extras</b>");
                g += LineH + RowGap;

                float h = RowFloatStepper(
                    g, "Crit Chance", m.CritChance, v => m.CritChance = v,
                    CheatUiConstants.CritChance_Step, CheatUiConstants.CritChance_BigStep,
                    CheatUiConstants.CritChance_Min, CheatUiConstants.CritChance_Max, groupOrigin);

                float i = RowFloatStepper(
                    h, "Crit Damage", m.CritDamage, v => m.CritDamage = v,
                    CheatUiConstants.CritDamage_Step, CheatUiConstants.CritDamage_BigStep,
                    CheatUiConstants.CritDamage_Min, CheatUiConstants.CritDamage_Max, groupOrigin);

                float j = RowIntStepper(
                    i, "Projectile Amount", m.ProjAmount, v => m.ProjAmount = v,
                    CheatUiConstants.ProjAmount_Step, CheatUiConstants.ProjAmount_BigStep,
                    CheatUiConstants.ProjAmount_Min, CheatUiConstants.ProjAmount_Max, groupOrigin);

                float k = RowIntStepper(
                    j, "Pierce Amount", m.PierceAmount, v => m.PierceAmount = v,
                    CheatUiConstants.PierceAmount_Step, CheatUiConstants.PierceAmount_BigStep,
                    CheatUiConstants.PierceAmount_Min, CheatUiConstants.PierceAmount_Max, groupOrigin);

                float l = RowIntStepper(
                    k, "Target Amount", m.TargetAmount, v => m.TargetAmount = v,
                    CheatUiConstants.TargetAmount_Step, CheatUiConstants.TargetAmount_BigStep,
                    CheatUiConstants.TargetAmount_Min, CheatUiConstants.TargetAmount_Max, groupOrigin);

                float n = RowIntStepper(
                    l, "Chain Targets", m.ChainTargets, v => m.ChainTargets = v,
                    CheatUiConstants.ChainTargets_Step, CheatUiConstants.ChainTargets_BigStep,
                    CheatUiConstants.ChainTargets_Min, CheatUiConstants.ChainTargets_Max, groupOrigin);

                GUI.EndGroup();

                // Vertical scrollbar
                if (maxScroll > 0f)
                {
                    float step = 24f;
                    if (Btn(new Rect(areaRect.width - 20f, 0f, 18f, 18f), "▲", new Vector2(areaRect.x, areaRect.y),
                            unchecked(0x70000001)))
                        _scrollY -= step;
                    if (Btn(new Rect(areaRect.width - 20f, areaRect.height - 18f, 18f, 18f), "▼",
                            new Vector2(areaRect.x, areaRect.y), unchecked(0x70000002)))
                        _scrollY += step;
                    _scrollY = Mathf.Clamp(_scrollY, 0f, maxScroll);
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

                // if (_popupOpen)
                //     DrawNumericPopup();

                var ev = Event.current;
                if (ev != null && ev.rawType == EventType.MouseUp && _btnArmed)
                {
                    _btnArmed = false;
                    _btnArmedId = -1;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public void ClampToScreen()
        {
            try
            {
                float maxX = Screen.width - _win.width;
                float maxY = Screen.height - _win.height;
                if (_win.x < 0) _win.x = 0;
                if (_win.y < 0) _win.y = 0;
                if (_win.x > maxX) _win.x = maxX;
                if (_win.y > maxY) _win.y = maxY;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private int NextId() => _idCounter++;

        private bool Btn(Rect localRect, string text, Vector2 originScreen, int id)
        {
            try
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
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }


            return false;
        }

        // ------------- Rows -------------
        private float RowFloatStepper(float y, string label, float value,
            Action<float> set, float step, float bigStep,
            float min, float max, Vector2 originScreen)
        {
            GUI.Label(new Rect(0, y, LabelW, LineH), label);

            int idBase = NextId();  // stable per row, per frame

            float x = LabelW + Gap;
            float applied = 0f;

            // small minus
            if (Btn(new Rect(x, y, BtnW, LineH), "-",  originScreen, idBase * 4 + 0))
                applied -= step;
            x += BtnW + Gap;

            // BIG minus
            if (Btn(new Rect(x, y, BtnW, LineH), "--", originScreen, idBase * 4 + 1))
                applied -= bigStep;
            x += BtnW + Gap;

            // value box
            var valRect = new Rect(x, y, ValW, LineH);
            GUI.Box(valRect, GUIContent.none, GUI.skin.textField);
            string valStr = value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            GUI.Label(valRect, valStr);
            x += ValW + Gap;

            // small plus
            if (Btn(new Rect(x, y, BtnW, LineH), "+",  originScreen, idBase * 4 + 2))
                applied += step;
            x += BtnW + Gap;

            // BIG plus
            if (Btn(new Rect(x, y, BtnW, LineH), "++", originScreen, idBase * 4 + 3))
                applied += bigStep;
            x += BtnW + Gap;

            // mouse wheel over value uses small step
            var e = Event.current;
            if (e != null && e.type == EventType.ScrollWheel && valRect.Contains(e.mousePosition))
            {
                applied += (-e.delta.y) * step;
                e.Use();
            }

            if (applied != 0f)
            {
                value = Mathf.Clamp(value + applied, min, max);
                if (float.IsNaN(value) || float.IsInfinity(value)) value = min;
                set(value);
                // refresh display in same frame
                valStr = value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
                GUI.Label(valRect, valStr);
            }

            return y + LineH + RowGap;
        }


        private float RowIntStepper(float y, string label, int value,
            Action<int> set, int step, int bigStep,
            int min, int max, Vector2 originScreen)
        {
            GUI.Label(new Rect(0, y, LabelW, LineH), label);

            int idBase = NextId();

            float x = LabelW + Gap;
            int delta = 0;

            if (Btn(new Rect(x, y, BtnW, LineH), "--", originScreen, idBase * 4 + 1))
                delta -= bigStep;
            x += BtnW + Gap;

            if (Btn(new Rect(x, y, BtnW, LineH), "-",  originScreen, idBase * 4 + 0))
                delta -= step;
            x += BtnW + Gap;

            var valRect = new Rect(x, y, ValW, LineH);
            GUI.Box(valRect, GUIContent.none, GUI.skin.textField);
            GUI.Label(valRect, value.ToString());
            x += ValW + Gap;

            if (Btn(new Rect(x, y, BtnW, LineH), "+",  originScreen, idBase * 4 + 2))
                delta += step;
            x += BtnW + Gap;

            if (Btn(new Rect(x, y, BtnW, LineH), "++", originScreen, idBase * 4 + 3))
                delta += bigStep;
            x += BtnW + Gap;

            var e = Event.current;
            if (e != null && e.type == EventType.ScrollWheel && valRect.Contains(e.mousePosition))
            {
                delta += (int)(-e.delta.y) * step;
                e.Use();
            }

            if (delta != 0)
            {
                long temp = (long)value + delta;               
                if (temp < min) temp = min;
                if (temp > max) temp = max;
                set((int)temp);
                GUI.Label(valRect, ((int)temp).ToString());
            }

            return y + LineH + RowGap;
        }


        private bool ValidateInput(float value, float min, float max)
        {
            try
            {
                if (float.IsNaN(value) || float.IsInfinity(value)) return false;
                return value >= min && value <= max;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return false;
        }

        private bool ValidateInput(int value, float min, float max)
        {
            try
            {
                if (float.IsNaN(value) || float.IsInfinity(value)) return false;
                return value >= min && value <= max;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return false;
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

            if (e.type == EventType.MouseDrag && _btnArmed)
            {
                _btnArmed = false;
                _btnArmedId = -1;
            }

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