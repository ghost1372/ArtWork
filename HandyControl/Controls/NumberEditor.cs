﻿using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace HandyControl.Controls
{
    public enum NumberEditorMode
    {
        Simple,
        WideRange
    }
    public class NumberEditor : FrameworkElement
    {
        #region IsReadOnly
        internal static object Bool(bool i) => i ? true : false;

        public bool IsReadOnly
        {
            get => _IsReadOnly;
            set
            {
                if (value != _IsReadOnly)
                    SetValue(IsReadOnlyProperty, Bool(value));
            }
        }

        private bool _IsReadOnly;

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    ValueBoxes.FalseBox,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._IsReadOnly = (bool)e.NewValue;
                    }));

        #endregion

        #region BorderColor

        public Color BorderColor
        {
            get => _BorderColor;
            set
            {
                if (value != _BorderColor)
                    SetValue(BorderColorProperty, value);
            }
        }

        private Color _BorderColor = Colors.Red;

        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register(nameof(BorderColor), typeof(Color), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    Colors.Red,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._BorderColor = (Color)e.NewValue;
                    }));

        #endregion

        #region SliderBrush

        public Brush SliderBrush
        {
            get => _SliderBrush;
            set
            {
                if (value != _SliderBrush)
                    SetValue(SliderBrushProperty, value);
            }
        }

        private Brush _SliderBrush = Brushes.GreenYellow;

        public static readonly DependencyProperty SliderBrushProperty =
            DependencyProperty.Register(nameof(SliderBrush), typeof(Brush), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    Brushes.GreenYellow,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._SliderBrush = (Brush)e.NewValue;
                    }));

        #endregion

        #region Value

        public double Value
        {
            get => _Value;
            set
            {
                if (NumberHelper.AreClose(value, _Value) == false)
                    SetValue(ValueProperty, value);
            }
        }

        private double _Value;

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    ValueBoxes.Double0Box,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        // 最小値・最大値でクランプして保存する

                        var self = (NumberEditor)s;
                        self._Value = self.ClampValue((double)e.NewValue);
                    }));

        #endregion

        #region Caption

        public string Caption
        {
            get => _Caption;
            set
            {
                if (value != _Caption)
                    SetValue(CaptionProperty, value);
            }
        }

        private string _Caption;

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register(nameof(Caption), typeof(string), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    default(string),
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._Caption = (string)e.NewValue;
                    }));

        #endregion

        #region SliderMinimum

        public double SliderMinimum
        {
            get => _SliderMinimum;
            set
            {
                if (NumberHelper.AreClose(value, _SliderMinimum) == false)
                    SetValue(SliderMinimumProperty, value);
            }
        }

        private double _SliderMinimum;

        public static readonly DependencyProperty SliderMinimumProperty =
            DependencyProperty.Register(nameof(SliderMinimum), typeof(double), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    ValueBoxes.Double0Box,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._SliderMinimum = (double)e.NewValue;
                    }));

        #endregion

        #region SliderMaximum

        public double SliderMaximum
        {
            get => _SliderMaximum;
            set
            {
                if (NumberHelper.AreClose(value, _SliderMaximum) == false)
                    SetValue(SliderMaximumProperty, value);
            }
        }

        private double _SliderMaximum = 100.0;

        public static readonly DependencyProperty SliderMaximumProperty =
            DependencyProperty.Register(nameof(SliderMaximum), typeof(double), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    ValueBoxes.Double100Box,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._SliderMaximum = (double)e.NewValue;
                    }));

        #endregion

        #region Minimum

        public double Minimum
        {
            get => _Minimum;
            set
            {
                if (NumberHelper.AreClose(value, _Minimum) == false)
                    SetValue(MinimumProperty, value);
            }
        }

        private double _Minimum;

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    ValueBoxes.Double0Box,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        // 変更後の最小値でValueをクランプする

                        var self = (NumberEditor)s;
                        self._Minimum = (double)e.NewValue;

                        self.Value = self.ClampValue(self.Value);
                    }));

        #endregion

        #region Maximum

        public double Maximum
        {
            get => _Maximum;
            set
            {
                if (NumberHelper.AreClose(value, _Maximum) == false)
                    SetValue(MaximumProperty, value);
            }
        }

        private double _Maximum = 100.0;

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    ValueBoxes.Double100Box,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        // 変更後の最大値でValueをクランプする

                        var self = (NumberEditor)s;
                        self._Maximum = (double)e.NewValue;

                        self.Value = self.ClampValue(self.Value);
                    }));

        #endregion

        #region DisplayFormat

        public string DisplayFormat
        {
            get => _DisplayFormat;
            set
            {
                if (value != _DisplayFormat)
                    SetValue(DisplayFormatProperty, value);
            }
        }

        private string _DisplayFormat = "F3";

        public static readonly DependencyProperty DisplayFormatProperty =
            DependencyProperty.Register(nameof(DisplayFormat), typeof(string), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    "F3",
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._DisplayFormat = (string)e.NewValue;
                    }));

        #endregion

        #region UnitString

        public string UnitString
        {
            get => _UnitString;
            set
            {
                if (value != _UnitString)
                    SetValue(UnitStringProperty, value);
            }
        }

        private string _UnitString = "";

        public static readonly DependencyProperty UnitStringProperty =
            DependencyProperty.Register(nameof(UnitString), typeof(string), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    "",
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._UnitString = (string)e.NewValue;
                    }));

        #endregion

        #region Background

        public Brush Background
        {
            get => _Background;
            set
            {
                if (value != _Background)
                    SetValue(BackgroundProperty, value);
            }
        }

        private Brush _Background;

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    default(Brush),
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._Background = (Brush)e.NewValue;
                    }));

        #endregion

        #region Foreground

        public Brush Foreground
        {
            get => _Foreground;
            set
            {
                if (value != _Foreground)
                    SetValue(ForegroundProperty, value);
            }
        }

        private Brush _Foreground;

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    default(Brush),
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._Foreground = (Brush)e.NewValue;
                    }));

        #endregion

        #region Padding

        public Thickness Padding
        {
            get => _Padding;
            set
            {
                if (value != _Padding)
                    SetValue(PaddingProperty, value);
            }
        }

        private Thickness _Padding;

        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    Thickness,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._Padding = (Thickness)e.NewValue;
                    }));

        #endregion

        #region Mode

        public NumberEditorMode Mode
        {
            get => _Mode;
            set
            {
                if (value != _Mode)
                    SetValue(ModeProperty, value);
            }
        }

        private NumberEditorMode _Mode = NumberEditorMode.Simple;

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(nameof(Mode), typeof(NumberEditorMode), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    Boxes.BiaNumberModeSimple,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._Mode = (NumberEditorMode)e.NewValue;
                    }));

        #endregion

        #region Increment

        public double Increment
        {
            get => _Increment;
            set
            {
                if (NumberHelper.AreClose(value, _Increment) == false)
                    SetValue(IncrementProperty, value);
            }
        }

        private double _Increment = 1.0;

        public static readonly DependencyProperty IncrementProperty =
            DependencyProperty.Register(nameof(Increment), typeof(double), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    Boxes.Double1,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._Increment = (double)e.NewValue;
                    }));

        #endregion

        #region CornerRadius

        public double CornerRadius
        {
            get => _CornerRadius;
            set
            {
                if (NumberHelper.AreClose(value, _CornerRadius) == false)
                    SetValue(CornerRadiusProperty, value);
            }
        }

        private double _CornerRadius = Constants.BasicCornerRadiusPrim;

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(double), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    Boxes.ConstantsBasicCornerRadiusPrim,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._CornerRadius = (double)e.NewValue;
                    }));

        #endregion

        #region IsVisibleBorder

        public bool IsVisibleBorder
        {
            get => _IsVisibleBorder;
            set
            {
                if (value != _IsVisibleBorder)
                    SetValue(IsVisibleBorderProperty, Boxes.Bool(value));
            }
        }

        private bool _IsVisibleBorder = true;

        public static readonly DependencyProperty IsVisibleBorderProperty =
            DependencyProperty.Register(nameof(IsVisibleBorder), typeof(bool), typeof(NumberEditor),
                new FrameworkPropertyMetadata(
                    Boxes.BoolTrue,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._IsVisibleBorder = (bool)e.NewValue;
                    }));

        #endregion

        #region StartedContinuousEditingCommand

        public ICommand StartedContinuousEditingCommand
        {
            get => _StartedContinuousEditingCommand;
            set
            {
                if (value != _StartedContinuousEditingCommand)
                    SetValue(StartedContinuousEditingCommandProperty, value);
            }
        }

        private ICommand _StartedContinuousEditingCommand;

        public static readonly DependencyProperty StartedContinuousEditingCommandProperty =
            DependencyProperty.Register(
                nameof(StartedContinuousEditingCommand),
                typeof(ICommand),
                typeof(NumberEditor),
                new PropertyMetadata(
                    default(ICommand),
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._StartedContinuousEditingCommand = (ICommand)e.NewValue;
                    }));

        #endregion

        #region EndContinuousEditingCommand

        public ICommand EndContinuousEditingCommand
        {
            get => _EndContinuousEditingCommand;
            set
            {
                if (value != _EndContinuousEditingCommand)
                    SetValue(EndContinuousEditingCommandProperty, value);
            }
        }

        private ICommand _EndContinuousEditingCommand;

        public static readonly DependencyProperty EndContinuousEditingCommandProperty =
            DependencyProperty.Register(
                nameof(EndContinuousEditingCommand),
                typeof(ICommand),
                typeof(NumberEditor),
                new PropertyMetadata(
                    default(ICommand),
                    (s, e) =>
                    {
                        var self = (NumberEditor)s;
                        self._EndContinuousEditingCommand = (ICommand)e.NewValue;
                    }));

        #endregion

        static NumberEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberEditor),
                new FrameworkPropertyMetadata(typeof(NumberEditor)));

            SetupSpinGeom();
        }

        public NumberEditor()
        {
            IsEnabledChanged += (_, __) => InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (ActualWidth <= 1 ||
                ActualHeight <= 1)
                return;

            DrawBackground(dc);

            var isCornerRadiusZero = NumberHelper.AreCloseZero(CornerRadius);

            if (isCornerRadiusZero == false)
                dc.PushClip(
                    Caches.GetClipGeom(ActualWidth, ActualHeight, CornerRadius, IsVisibleBorder));
            {
                if (Mode == NumberEditorMode.Simple)
                    DrawSlider(dc);

                DrawText(dc);

                if (IsMouseOver &&
                    IsEnabled &&
                    IsReadOnly == false &&
                    NumberHelper.AreCloseZero(Increment) == false)
                    DrawSpin(dc);
            }
            if (isCornerRadiusZero == false)
                dc.Pop();

            if (IsVisibleBorder)
                DrawBorder(dc);
        }

        private void DrawBackground(DrawingContext dc)
        {
            var brush = _isEditing
                ? _textBox.Background
                : Background;

            if (NumberHelper.AreCloseZero(CornerRadius))
                dc.DrawRectangle(
                    brush,
                    null,
                    this.RoundLayoutActualRectangle(IsVisibleBorder));
            else
                dc.DrawRoundedRectangle(
                    brush,
                    null,
                    this.RoundLayoutActualRectangle(IsVisibleBorder),
                    CornerRadius,
                    CornerRadius);
        }

        private void DrawBorder(DrawingContext dc)
        {
            if (NumberHelper.AreCloseZero(CornerRadius))
                dc.DrawRectangle(
                    Brushes.Transparent,
                    this.GetBorderPen(BorderColor),
                    this.RoundLayoutActualRectangle(IsVisibleBorder)
                );
            else
                dc.DrawRoundedRectangle(
                    Brushes.Transparent,
                    this.GetBorderPen(BorderColor),
                    this.RoundLayoutActualRectangle(IsVisibleBorder),
                    CornerRadius,
                    CornerRadius);
        }

        private void DrawSlider(DrawingContext dc)
        {
            if (SliderWidth <= 0.0f)
                return;

            var w = (UiValue - ActualSliderMinimum) * this.RoundLayoutActualWidth(IsVisibleBorder) / SliderWidth;
            var brush = _isEditing
                ? _textBox.Background
                : SliderBrush;

            var r = this.RoundLayoutActualRectangle(IsVisibleBorder);
            r.Width = FrameworkElementHelper.RoundLayoutValue(w);

            dc.DrawRectangle(brush, null, r);
        }

        private const double SpinWidth = 14.0;

        private void DrawText(DrawingContext dc)
        {
            TextRenderer.Default.Draw(
                Caption,
                Padding.Left + SpinWidth,
                Padding.Top,
                Foreground,
                dc,
                ActualWidth - Padding.Left - Padding.Right,
                TextAlignment.Left
            );

            TextRenderer.Default.Draw(
                UiValueString,
                Padding.Left,
                Padding.Top,
                Foreground,
                dc,
                ActualWidth - Padding.Left - Padding.Right - SpinWidth,
                TextAlignment.Right
            );
        }

        private static readonly Brush _moBrush = Application.Current.FindResource("AccentBrushKey") as Brush;

        private void DrawSpin(DrawingContext dc)
        {
            {
                var offsetX = 5.0;
                var offsetY = 8.0;

                if (_mouseOverType == MouseOverType.DecSpin)
                    dc.DrawRectangle(_SpinBackground, null, new Rect(0, 0, SpinWidth, ActualHeight));

                var key = (offsetX, offsetY);

                if (_translateTransformCache.TryGetValue(key, out var tt) == false)
                {
                    tt = new TranslateTransform(offsetX, offsetY);
                    _translateTransformCache.Add(key, tt);
                }

                dc.PushTransform(tt);
                dc.DrawGeometry(
                    _mouseOverType == MouseOverType.DecSpin
                        ? _moBrush
                        : Foreground, null, _DecSpinGeom);
                dc.Pop();
            }

            {
                var offsetX = ActualWidth - 5.0 * 2 + 1;
                var offsetY = 8.0;

                if (_mouseOverType == MouseOverType.IncSpin)
                    dc.DrawRectangle(_SpinBackground, null,
                        new Rect(ActualWidth - SpinWidth, 0, SpinWidth, ActualHeight));

                var key = (offsetX, offsetY);

                if (_translateTransformCache.TryGetValue(key, out var tt) == false)
                {
                    tt = new TranslateTransform(offsetX, offsetY);
                    _translateTransformCache.Add(key, tt);
                }

                dc.PushTransform(tt);
                dc.DrawGeometry(
                    _mouseOverType == MouseOverType.IncSpin
                        ? _moBrush
                        : Foreground, null, _IncSpinGeom);
                dc.Pop();
            }
        }

        private enum MouseOverType
        {
            None,
            DecSpin,
            IncSpin,
            Slider
        }

        private bool _isMouseDown;
        private bool _isMouseMoved;
        private Point _oldPos;
        private Point _mouseDownPos;
        private MouseOverType _mouseOverTypeOnMouseDown;
        private MouseOverType _mouseOverType;

        private bool _isContinuousEdited;
        private double _ContinuousEditingStartValue;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            _oldPos = e.GetPosition(this);
            _mouseDownPos = _oldPos;

            if (IsReadOnly)
                return;

            _isMouseDown = true;
            _isMouseMoved = false;
            _mouseOverTypeOnMouseDown = MakeMouseOverType(e);
            _isContinuousEdited = false;
            _ContinuousEditingStartValue = Value;

            if (_mouseOverTypeOnMouseDown == MouseOverType.Slider)
                CaptureMouse();

            e.Handled = true;
        }

        private const double ClickPlayWidth = 4;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var newMouseOverType = MakeMouseOverType(e);

            if (_mouseOverType != newMouseOverType)
            {
                _mouseOverType = newMouseOverType;
                InvalidateVisual();
            }

            if (_isMouseDown == false)
                return;

            if (IsReadOnly)
                return;

            // Down直後マウス移動ない場合でもOnMouseMoveが呼ばれることがあるため、判定する。
            var currentPos = e.GetPosition(this);
            if (currentPos == _oldPos)
                return;

            if (_isMouseMoved == false)
                if (Math.Abs(currentPos.X - _mouseDownPos.X) <= ClickPlayWidth)
                    return;

            if (_mouseOverTypeOnMouseDown != MouseOverType.Slider)
                return;

            var oldValue = Value;
            var newValue = Value;

            switch (Mode)
            {
                case NumberEditorMode.Simple:
                    {
                        // 0から1
                        var xr = (currentPos.X, 0.0, ActualWidth).Clamp() / ActualWidth;
                        newValue = SliderWidth * xr + ActualSliderMinimum;
                        break;
                    }

                case NumberEditorMode.WideRange:
                    {
                        // Ctrl押下中は５倍速い
                        var s = KeyboardHelper.IsPressControl
                            ? 5.0
                            : 1.0;
                        var w = currentPos.X - _oldPos.X;
                        var v = oldValue + s * w * Increment;

                        newValue = (v, ActualSliderMinimum, ActualSliderMaximum).Clamp();

                        break;
                    }
            }

            if (NumberHelper.AreClose(newValue, oldValue) == false)
            {
                if (_isContinuousEdited == false)
                {
                    _isContinuousEdited = true;

                    if (StartedContinuousEditingCommand != null)
                        if (StartedContinuousEditingCommand.CanExecute(null))
                            StartedContinuousEditingCommand.Execute(null);
                }

                Value = newValue;
            }

            _mouseOverType = MouseOverType.Slider;
            _isMouseMoved = true;
            _oldPos = currentPos;

            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (IsReadOnly == false)
            {
                if (_mouseOverTypeOnMouseDown == MouseOverType.Slider)
                    ReleaseMouseCapture();
            }

            _isMouseDown = false;

            if (_isMouseMoved)
            {
                if (_isContinuousEdited)
                {
                    if (EndContinuousEditingCommand != null)
                    {
                        if (EndContinuousEditingCommand.CanExecute(null))
                        {

                            var changedValue = Value;
                            Value = _ContinuousEditingStartValue;

                            EndContinuousEditingCommand.Execute(null);

                            Value = changedValue;
                        }
                    }

                    _isContinuousEdited = false;
                }
            }
            else
            {
                var p = e.GetPosition(this);

                if (Math.Abs(p.X - _mouseDownPos.X) <= ClickPlayWidth)
                {
                    // Ctrl押下中は５倍速い
                    var inc = KeyboardHelper.IsPressControl
                        ? Increment * 5
                        : Increment;

                    if (p.X <= SpinWidth && IsReadOnly == false)
                        AddValue(-inc);
                    else if (p.X >= ActualWidth - SpinWidth && IsReadOnly == false)
                        AddValue(inc);
                    else
                        ShowEditBox();
                }
            }

            _mouseOverType = MakeMouseOverType(e);

            e.Handled = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (_isMouseDown)
            {
                _isMouseDown = false;
                ReleaseMouseCapture();
                this.ResetMouseClipping();
            }

            _mouseOverType = MouseOverType.None;

            e.Handled = true;
        }

        private MouseOverType MakeMouseOverType(MouseEventArgs e)
        {
            var x = e.GetPosition(this).X;

            if (x < 0)
                return MouseOverType.None;

            if (x >= ActualWidth)
                return MouseOverType.None;

            if (x <= SpinWidth)
                return MouseOverType.DecSpin;

            if (x >= ActualWidth - SpinWidth)
                return MouseOverType.IncSpin;

            return MouseOverType.Slider;
        }

        private TextBox _textBox;
        private bool _isEditing;

        private Window _parentWindow;

        private void AddValue(double i)
        {
            var v = Value + i;

            Value = ClampValue(v);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (IsEnabled == false)
                return;

            if (e.Key == Key.Return)
            {
                ShowEditBox();
                e.Handled = true;
            }
        }

        private void ShowEditBox()
        {
            if (_isEditing)
                return;

            if (_textBox == null)
            {
                _textBox = new TextBox
                {
                    IsTabStop = false,
                    IsUndoEnabled = false,
                    FocusVisualStyle = null,
                    SelectionLength = 0
                };

                _textBox.TextChanged += TextBox_OnTextChanged;
                _textBox.LostFocus += TextBox_OnLostFocus;
                _textBox.PreviewKeyDown += TextBox_OnPreviewKeyDown;
            }

            _textBox.Width = ActualWidth;
            _textBox.Height = ActualHeight;
            _textBox.IsReadOnly = IsReadOnly;
            _textBox.Text = FormattedValueString;

            AddVisualChild(_textBox);
            InvalidateMeasure();

            _textBox.SelectAll();
            _textBox.Focus();

            _isEditing = true;

            if (_parentWindow == null)
                _parentWindow = this.GetParent<Window>();

            if (_parentWindow != null)
                _parentWindow.PreviewMouseDown += ParentWindow_PreviewMouseDown;
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            InvalidateVisual();
        }

        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
                FinishEditing(true);
        }

        private void TextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Tab:
                    {
                        var v = MakeValueFromString(_textBox.Text);

                        if (v.Result == MakeValueResult.Continue)
                            _textBox.Text = v.Value.ToString(DisplayFormat);
                        else
                            FinishEditing(v.Result == MakeValueResult.Ok);

                        var t = Keyboard.Modifiers == ModifierKeys.Shift
                            ? Caches.PreviousTraversalRequest
                            : Caches.NextTraversalRequest;
                        MoveFocus(t);

                        e.Handled = true;
                        break;
                    }

                case Key.Return:
                    {
                        var v = MakeValueFromString(_textBox.Text);

                        if (v.Result == MakeValueResult.Continue)
                            _textBox.Text = v.Value.ToString(DisplayFormat);
                        else
                        {
                            FinishEditing(v.Result == MakeValueResult.Ok);
                            Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)(() => Focus()));
                        }

                        e.Handled = true;
                        break;
                    }

                case Key.Escape:
                    {
                        _textBox.Text = FormattedValueString;
                        FinishEditing(false);

                        Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)(() => Focus()));

                        e.Handled = true;
                        break;
                    }
            }
        }

        private void ParentWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 自コントロール上であれば、終了させない
            var pos = e.GetPosition(this);
            var rect = this.RoundLayoutActualRectangle(false);
            if (rect.Contains(pos))
                return;

            if (_isEditing)
                FinishEditing(true);
        }

        private void FinishEditing(bool isEdit)
        {
            if (isEdit)
            {
                var v = MakeValueFromString(_textBox.Text);
                if (v.Result == MakeValueResult.Ok ||
                    v.Result == MakeValueResult.Continue)
                    Value = v.Value;
            }

            RemoveVisualChild(_textBox);
            _isEditing = false;
            InvalidateVisual();

            if (_parentWindow != null)
                _parentWindow.PreviewMouseDown -= ParentWindow_PreviewMouseDown;
        }

        protected override int VisualChildrenCount
            => _isEditing
                ? 1
                : 0;

        protected override Visual GetVisualChild(int index)
            => _textBox;

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_isEditing)
                _textBox.Arrange(new Rect(new Point(0, 0), _textBox.DesiredSize));

            return base.ArrangeOverride(finalSize);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_isEditing)
                _textBox.Measure(new Size(ActualWidth, ActualHeight));

            return base.MeasureOverride(availableSize);
        }

        private enum MakeValueResult
        {
            Ok,
            Cancel,
            Continue
        }

        private static Regex _evalRegex;

        private (MakeValueResult Result, double Value) MakeValueFromString(string src)
        {
            if (double.TryParse(src, out var v))
                return (MakeValueResult.Ok, ClampValue(v));

            if (double.TryParse(Evaluator.Eval(src), out v))
                return (MakeValueResult.Continue, ClampValue(v));

            // Math.を補間
            if (_evalRegex == null)
                _evalRegex = new Regex("[A-Za-z_]+");

            var rs = _evalRegex.Replace(src, "Math.$0");
            if (double.TryParse(Evaluator.Eval(rs), out v))
                return (MakeValueResult.Continue, ClampValue(v));

            return (MakeValueResult.Cancel, default);
        }

        private static void SetupSpinGeom()
        {
            {
                var start = new Point(0, 4);

                var segments = new[]
                {
                    new LineSegment(new Point(4, 0), true),
                    new LineSegment(new Point(4, 8), true)
                };

                segments[0].Freeze();
                segments[1].Freeze();

                var figure = new PathFigure(start, segments, true);
                figure.Freeze();

                _DecSpinGeom = new PathGeometry(new[]
                {
                    figure
                });
                _DecSpinGeom.Freeze();
            }

            {
                var start = new Point(4, 4);

                var segments = new[]
                {
                    new LineSegment(new Point(0, 0), true),
                    new LineSegment(new Point(0, 8), true)
                };

                segments[0].Freeze();
                segments[1].Freeze();

                var figure = new PathFigure(start, segments, true);
                figure.Freeze();

                _IncSpinGeom = new PathGeometry(new[]
                {
                    figure
                });
                _IncSpinGeom.Freeze();
            }

            _SpinBackground = new SolidColorBrush(Color.FromArgb(0x40, 0x00, 0x00, 0x00));
        }

        private double ClampValue(double v)
            => (v, ActualMinimum, ActualMaximum).Clamp();

        private string FormattedValueString => Value.ToString(DisplayFormat);

        private double SliderWidth => Math.Abs(SliderMaximum - SliderMinimum);

        private double ActualSliderMinimum => (SliderMinimum, SliderMaximum).Min();

        private double ActualSliderMaximum => (SliderMinimum, SliderMaximum).Max();

        private double ActualMinimum => (Minimum, Maximum).Min();

        private double ActualMaximum => (Minimum, Maximum).Max();

        private string UiValueString
        {
            get
            {
                if (_isEditing == false)
                    return Concat(FormattedValueString, UnitString);

                var v = MakeValueFromString(_textBox.Text);

                return
                    v.Result == MakeValueResult.Ok || v.Result == MakeValueResult.Continue
                        ? Concat(v.Value.ToString(DisplayFormat), UnitString)
                        : Concat(FormattedValueString, UnitString);

                string Concat(string a, string b)
                    => string.IsNullOrEmpty(b)
                        ? a
                        : a + b;
            }
        }

        private double UiValue
        {
            get
            {
                if (_isEditing == false)
                    return Value;

                var v = MakeValueFromString(_textBox.Text);

                return
                    v.Result == MakeValueResult.Ok || v.Result == MakeValueResult.Continue
                        ? v.Value
                        : Value;
            }
        }

        private static Geometry _DecSpinGeom;
        private static Geometry _IncSpinGeom;
        private static Brush _SpinBackground;

        private static readonly Dictionary<(double X, double Y), TranslateTransform> _translateTransformCache =
            new Dictionary<(double X, double Y), TranslateTransform>();
    }
}
