using AfterIdler.Hardware;
using AfterIdler.Logic;
using AfterIdler.Models;
using AfterIdler.Services;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AfterIdler
{
    public partial class MainWindow : Window
    {
        [DllImport("PowrProf.dll")]
        private static extern bool SetSuspendState(
        bool hibernate,
        bool forceCritical,
        bool disableWakeEvent);

        //-------------------------------------------------
        // 状態
        //-------------------------------------------------

        private enum AppState
        {
            Monitor,
            Countdown,
            Stop,

            SetCpu,
            SetGpu,
            SetTimer,
            SetMode
        }

        //-------------------------------------------------
        // Services
        //-------------------------------------------------

        //private readonly ConfigService _configService = new();
        private readonly TemperatureService _temperatureService = new();
        private IdleMonitor _idleMonitor = null!;

        //-------------------------------------------------
        // Config
        //-------------------------------------------------

        private AppConfig _config = new();
        private AppConfig _editConfig = new();

        //-------------------------------------------------
        // Temperature
        //-------------------------------------------------

        private TemperatureResult _temperature = new();

        //-------------------------------------------------
        // State
        //-------------------------------------------------

        private AppState _state = AppState.Monitor;
        private AppState _returnState = AppState.Monitor;

        private bool _blinkOn = true;
        private bool _longPressed;

        //-------------------------------------------------
        // Timer
        //-------------------------------------------------

        private DispatcherTimer _mainTimer = null!;
        private DispatcherTimer _blinkTimer = null!;
        private DispatcherTimer _longPressTimer = null!;

        //-------------------------------------------------
        // LED Brush
        //-------------------------------------------------

        private static readonly Brush LedDark =
            new SolidColorBrush(Color.FromRgb(40, 20, 20));

        private static readonly Brush LedRed = Brushes.Red;
        private static readonly Brush LedGreen = Brushes.LimeGreen;
        private static readonly Brush LedOrange = Brushes.Orange;

        //-------------------------------------------------
        // Constructor
        //-------------------------------------------------

        public MainWindow()
        {
            InitializeComponent();

            InitializeServices();
            InitializeTimers();

            ChangeState(AppState.Monitor);

            _mainTimer.Start();
            _blinkTimer.Start();
        }

        //-------------------------------------------------
        // 初期化
        //-------------------------------------------------

        private void InitializeServices()
        {
            LogService.Clear();
            LogService.Write("AfterIdler Started");

            _config = ConfigService.Load();

            _editConfig = _config.Clone();

            if (_config.WindowLeft >= 0)
            {
                Left = _config.WindowLeft;
                Top = _config.WindowTop;
            }

            _temperatureService.Initialize();

            RecreateIdleMonitor();
        }

        private void InitializeTimers()
        {
            _mainTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _mainTimer.Tick += MainTimer_Tick;

            _blinkTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _blinkTimer.Tick += BlinkTimer_Tick;

            _longPressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.5)
            };
            _longPressTimer.Tick += LongPressTimer_Tick;
        }

        //-------------------------------------------------
        // 状態変更
        //-------------------------------------------------

        private void ChangeState(AppState state)
        {
            _state = state;
            _blinkOn = true;

            LogService.Write($"State -> {state}");
        }

        //-------------------------------------------------
        // MainTimer
        //-------------------------------------------------

        private void MainTimer_Tick(object? sender, EventArgs e)
        {
            _temperatureService.Update();
            _temperature = _temperatureService.GetTemperature();

            switch (_state)
            {
                //-------------------------------------------------
                // Monitor
                //-------------------------------------------------
                case AppState.Monitor:

                    _idleMonitor.Update(_temperature);

                    if (_idleMonitor.IsCountdownRunning)
                    {
                        ChangeState(AppState.Countdown);
                    }

                    break;


                //-------------------------------------------------
                // Countdown
                //-------------------------------------------------
                case AppState.Countdown:

                    _idleMonitor.Update(_temperature);

                    if (_idleMonitor.ShouldExecuteEndMode)
                    {
                        ExecuteEndMode();
                        return;
                    }

                    if (!_idleMonitor.IsCountdownRunning)
                    {
                        ChangeState(AppState.Monitor);
                    }

                    break;
            }

            UpdateDisplay();
        }

        //-------------------------------------------------
        // BlinkTimer
        //-------------------------------------------------

        private void BlinkTimer_Tick(object? sender, EventArgs e)
        {
            _blinkOn = !_blinkOn;

            UpdateDisplay();
        }

        //-------------------------------------------------
        // LongPress
        //-------------------------------------------------

        private void LongPressTimer_Tick(object? sender, EventArgs e)
        {
            _longPressTimer.Stop();

            _longPressed = true;

            if (_state == AppState.Stop)
            {
                _idleMonitor.Resume();
                ChangeState(AppState.Monitor);
            }
        }

        //-------------------------------------------------
        // 表示更新
        //-------------------------------------------------

        private void UpdateDisplay()
        {
            switch (_state)
            {
                //-----------------------------------------
                // 温度表示
                //-----------------------------------------

                case AppState.Monitor:
                case AppState.Stop:
                case AppState.SetCpu:
                case AppState.SetGpu:

                    TemperatureDisplayGrid.Visibility = Visibility.Visible;
                    CountdownDisplayGrid.Visibility = Visibility.Collapsed;

                    if (_state == AppState.SetCpu ||
                        _state == AppState.SetGpu)
                    {
                        UpdateSettingDisplay();
                    }
                    else
                    {
                        UpdateTemperatureDisplay();
                    }

                    break;

                //-----------------------------------------
                // タイマー
                //-----------------------------------------

                case AppState.Countdown:
                case AppState.SetTimer:
                case AppState.SetMode:

                    TemperatureDisplayGrid.Visibility = Visibility.Collapsed;
                    CountdownDisplayGrid.Visibility = Visibility.Visible;

                    if (_state == AppState.SetMode)
                    {
                        UpdateModeDisplay();
                    }
                    else if (_state == AppState.SetTimer)
                    {
                        UpdateTimerDisplay(_editConfig.TargetMinutes * 60);
                    }
                    else
                    {
                        UpdateTimerDisplay(_idleMonitor.RemainingSeconds);
                    }

                    break;
            }

            UpdateLed();
        }

        //-------------------------------------------------
        // 温度表示
        //-------------------------------------------------

        private void UpdateTemperatureDisplay()
        {
            CpuTempText.Visibility = Visibility.Visible;
            GpuTempText.Visibility = Visibility.Visible;

            Debug.WriteLine(
                $"CpuAvailable={_temperature.CpuAvailable}  Temp={_temperature.CpuTemperature}");

            CpuTempText.Text =
                _temperature.CpuAvailable
                ? ((int)_temperature.CpuTemperature!).ToString("00")
                : "--";

            GpuTempText.Text =
                _temperature.GpuAvailable
                ? ((int)_temperature.GpuTemperature!).ToString("00")
                : "--";

            Brush cpuBrush =
                (_temperature.CpuAvailable &&
                 _temperature.CpuTemperature <= _config.TargetCpuTemp)
                ? Brushes.LimeGreen
                : Brushes.Orange;

            Brush gpuBrush =
                (_temperature.GpuAvailable &&
                 _temperature.GpuTemperature <= _config.TargetGpuTemp)
                    ? Brushes.LimeGreen
                    : Brushes.Orange;

            CpuLabelText.Foreground = cpuBrush;
            CpuTempText.Foreground = cpuBrush;
            CpuUnitText.Foreground = cpuBrush;
            CpuTempBlank.Foreground = cpuBrush;

            GpuLabelText.Foreground = gpuBrush;
            GpuTempText.Foreground = gpuBrush;
            GpuUnitText.Foreground = gpuBrush;
            GpuTempBlank.Foreground = gpuBrush;

            CpuTempText.ToolTip =
                _temperature.CpuAvailable ? null : "CPU温度を取得できませんでした";
            GpuTempText.ToolTip =
                _temperature.GpuAvailable ? null : "GPU温度を取得できませんでした";
        }

        //-------------------------------------------------
        // 設定表示
        //-------------------------------------------------

        private void UpdateSettingDisplay()
        {
            CpuTempText.Text =
                _editConfig.TargetCpuTemp.ToString("00");

            GpuTempText.Text =
                _editConfig.TargetGpuTemp.ToString("00");

            CpuTempText.Visibility =
                (_state != AppState.SetCpu || _blinkOn)
                ? Visibility.Visible
                : Visibility.Hidden;

            GpuTempText.Visibility =
                (_state != AppState.SetGpu || _blinkOn)
                ? Visibility.Visible
                : Visibility.Hidden;

            Brush cpuBrush =
                (_temperature.CpuAvailable &&
                 _temperature.CpuTemperature <= _editConfig.TargetCpuTemp)
                ? Brushes.LimeGreen
                : Brushes.Orange;

            Brush gpuBrush =
                (_temperature.GpuAvailable &&
                 _temperature.GpuTemperature <= _editConfig.TargetGpuTemp)
                    ? Brushes.LimeGreen
                    : Brushes.Orange;

            CpuLabelText.Foreground = cpuBrush;
            CpuTempText.Foreground = cpuBrush;
            CpuUnitText.Foreground = cpuBrush;
            CpuTempBlank.Foreground = cpuBrush;

            GpuLabelText.Foreground = gpuBrush;
            GpuTempText.Foreground = gpuBrush;
            GpuUnitText.Foreground = gpuBrush;
            GpuTempBlank.Foreground = gpuBrush;
        }

        //-------------------------------------------------
        // Timer表示
        //-------------------------------------------------

        private void UpdateTimerDisplay(int totalSeconds)
        {
            int min = totalSeconds / 60;
            int sec = totalSeconds % 60;

            CountdownText.Text = $"{min:00}:{sec:00}";

            CountdownText.Foreground = Brushes.LimeGreen;

            CountdownText.Visibility =
                (_state != AppState.SetTimer || _blinkOn)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        //-------------------------------------------------
        // OFF / SUS表示
        //-------------------------------------------------

        private void UpdateModeDisplay()
        {
            CountdownText.Text =
                _editConfig.EndMode == "OFF"
                    ? "OFF"
                    : "SUS";

            CountdownText.Foreground =
                _blinkOn
                    ? Brushes.LimeGreen
                    : Brushes.DarkGreen;

            CountdownText.Visibility = Visibility.Visible;
        }

        //-------------------------------------------------
        // LED
        //-------------------------------------------------

        private void UpdateLed()
        {
            switch (_state)
            {
                case AppState.Monitor:
                case AppState.Countdown:

                    StatusLed.Background =
                        _blinkOn ? LedRed : LedDark;

                    break;

                case AppState.Stop:

                    StatusLed.Background = LedOrange;

                    break;

                default:

                    StatusLed.Background = LedGreen;

                    break;
            }
        }

        //-------------------------------------------------
        // 設定開始
        //-------------------------------------------------

        private void BeginSetting()
        {
            if (_state == AppState.Countdown)
            {
                _returnState = AppState.Stop;
                _idleMonitor.CancelCountdown();
            }
            else
                _returnState = _state;

            _editConfig = _config.Clone();
            ChangeState(AppState.SetCpu);
        }

        private bool IsSettingState()
        {
            return _state == AppState.SetCpu ||
                   _state == AppState.SetGpu ||
                   _state == AppState.SetTimer ||
                   _state == AppState.SetMode;
        }

        //-------------------------------------------------
        // SET
        //-------------------------------------------------

        private void SetButton_Click(object sender, RoutedEventArgs e)
        {
            LogService.Write("SET");

            if (_longPressed)
                return;

            switch (_state)
            {
                case AppState.Monitor:
                case AppState.Countdown:

                    BeginSetting();
                    break;

                case AppState.SetCpu:

                    ChangeState(AppState.SetGpu);
                    break;

                case AppState.SetGpu:

                    ChangeState(AppState.SetTimer);
                    break;

                case AppState.SetTimer:

                    ChangeState(AppState.SetMode);
                    break;

                case AppState.SetMode:
                    _config = _editConfig.Clone();
                    _config.WindowLeft = Left;
                    _config.WindowTop = Top;
                    ConfigService.Save(_config);

                    // 新設定でIdleMonitorを作り直す
                    RecreateIdleMonitor();
                    ChangeState(_returnState);
                    break;

                case AppState.Stop:
                    _idleMonitor.ManualCancel();
                    BeginSetting();
                    break;
            }
        }

        //-------------------------------------------------
        // ＋
        //-------------------------------------------------

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            ModifyValue(+1);
        }

        //-------------------------------------------------
        // －
        //-------------------------------------------------

        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            ModifyValue(-1);
        }

        //-------------------------------------------------
        // 値変更
        //-------------------------------------------------

        private void ModifyValue(int delta)
        {
            switch (_state)
            {
                case AppState.SetCpu:

                    _editConfig.TargetCpuTemp =
                        Math.Clamp(
                            _editConfig.TargetCpuTemp + delta,
                            20,
                            99);
                    break;

                case AppState.SetGpu:

                    _editConfig.TargetGpuTemp =
                        Math.Clamp(
                            _editConfig.TargetGpuTemp + delta,
                            20,
                            99);
                    break;

                case AppState.SetTimer:

                    _editConfig.TargetMinutes =
                        Math.Clamp(
                            _editConfig.TargetMinutes + delta,
                            0,
                            99);
                    break;

                case AppState.SetMode:

                    _editConfig.EndMode =
                        _editConfig.EndMode == "OFF"
                        ? "SUS"
                        : "OFF";
                    break;
            }

            UpdateDisplay();
        }

        //-------------------------------------------------
        // CAN
        //-------------------------------------------------

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            LogService.Write("CAN");

            //------------------------------------
            // 設定中
            //------------------------------------

            if (IsSettingState())
            {
                RecreateIdleMonitor();

                // 編集破棄
                _editConfig = _config.Clone();

                ChangeState(_returnState);

                return;
            }

            //------------------------------------
            // Countdown → Stop
            //------------------------------------

            if (_state == AppState.Countdown)
            {
                _idleMonitor.CancelCountdown();

                ChangeState(AppState.Stop);

                return;
            }

            //------------------------------------
            // Monitor
            //------------------------------------

            if (_state == AppState.Monitor)
            {
                ChangeState(AppState.Stop);

                return;
            }

            //------------------------------------
            // Stop
            //------------------------------------

            if (_state == AppState.Stop)
            {
                Close();
            }

        }

        //-------------------------------------------------
        // SET長押し開始
        //-------------------------------------------------

        private void SetButton_PreviewMouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            _longPressed = false;
            _longPressTimer.Start();
        }

        private void SetButton_PreviewMouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            _longPressTimer.Stop();
        }

        //-------------------------------------------------
        // Shutdown / Suspend
        //-------------------------------------------------

        private void ExecuteEndMode()
        {
            _config.WindowLeft = Left;
            _config.WindowTop = Top;

            ConfigService.Save(_config);

            if (_config.EndMode == "OFF")
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "shutdown",
                        Arguments = "/s /t 3",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
            }
            else
            {
                SetSuspendState(false, false, true);
            }

            Close();
        }

        //-------------------------------------------------
        // Window Drag
        //-------------------------------------------------

        private void MainWindow_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.OriginalSource is System.Windows.Controls.Button)
                return;

            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        //-------------------------------------------------
        // Closing
        //-------------------------------------------------

        protected override void OnClosing(CancelEventArgs e)
        {
            _config.WindowLeft = Left;
            _config.WindowTop = Top;

            ConfigService.Save(_config);
            LogService.Write("AfterIdler Closed");

            _temperatureService.Dispose();

            base.OnClosing(e);
        }

        private void RecreateIdleMonitor()
        {
            _idleMonitor = new IdleMonitor(_config);
        }
    }
}