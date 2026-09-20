# AfterIdler v1.0.0

AfterIdler is a simple cooldown timer that monitors CPU / GPU temperatures
after high-load PC operation and waits for the system to cool down.
It is a small utility inspired by the turbo timers used in automobiles.

## Features
* CPU / GPU temperature monitoring

* Starts a countdown when the temperature drops below the configured threshold

* Performs a system suspend or shutdown after the countdown, depending on the selected setting

* Uses LibreHardwareMonitor for temperature monitoring

* Temperature monitoring is handled by a dedicated Windows Service

* The main application does not require administrator privileges

* Windows 10 / 11 x64

## Operation

AfterIdler automatically starts temperature monitoring when launched.

### Monitoring

The application waits until both CPU / GPU temperatures reach their configured cooling thresholds.

When the temperatures reach the configured thresholds, the countdown timer starts.

When the countdown finishes, AfterIdler performs the selected end-of-operation action and then exits.

### End Mode

The end-of-operation mode can be selected from the following two options:

* **SUS** — System suspend (sleep)

* **OFF** — Windows shutdown

### CAN

Pressing the **CAN (Cancel)** button during monitoring or countdown cancels the automatic end-of-operation process and puts AfterIdler into the stopped state.


Pressing **CAN** again while in the stopped state exits AfterIdler.

### SET

Pressing the **SET (Setting)** button opens the settings.

The settings are accessed in the following order:

1. CPU cooling threshold

2. GPU cooling threshold

3. Countdown timer duration

4. End-of-operation mode (SUS / OFF)

When leaving the settings, AfterIdler returns to the monitoring state it had before entering the settings.

Pressing and holding the **SET** button for one second while in the stopped state resumes temperature monitoring.

### Status Lamp

The status lamp indicates the current state of AfterIdler.

* 🔴 **Blinking red** — Monitoring

* 🟡 **Yellow** — Stopped

* 🟢 **Green** — Settings

## Included

* `AfterIdler.exe`
* `AfterIdlerSensorService.exe`
* `install.bat`
* `uninstall.bat`
* 
## Requirements

* Windows 10 / 11 64-bit

PawnIO may be required for temperature monitoring depending on the hardware
and sensor configuration.

## Installation

1. Extract the ZIP archive.

2. Run `install.bat` as administrator.

3. Start `AfterIdler.exe`.

## Uninstallation

Run uninstall.bat as administrator.
After the service has been unregistered, you can simply delete the AfterIdler folder.

## Notes
AfterIdler is essentially a "turbo timer for your PC."

Instead of ending the PC session immediately after high-load operation,
it waits for the temperature to drop and then performs the selected action:
system suspend or Windows shutdown.

# AfterIdler v1.0.0

PCの高負荷動作後に、CPU / GPU の温度を監視して冷却を待つための
シンプルなクールダウンタイマーです。

自動車のターボタイマーをイメージした小さなユーティリティです。

## 特徴

* CPU / GPU 温度を監視

* 指定温度まで下がると、設定した時間のカウントダウンを開始

* カウントダウン終了後、設定に応じてサスペンドまたはシャットダウンを実行

* CPU / GPU の温度取得には LibreHardwareMonitor を使用

* 温度取得は専用の Windows Service で実行

* メインアプリは管理者権限を要求しません

* Windows 10 / 11 x64

## 操作

AfterIdlerを起動すると、自動的に温度監視を開始します。

### Monitoring / 温度監視

CPU / GPU の温度が、それぞれ設定した冷却判断温度以下になるまで待機します。

温度が設定値以下になると、設定したカウントダウンタイマーが開始します。

カウントダウンが終了すると、設定された終了モードを実行し、その後AfterIdler自身も終了します。

### End Mode / 終了モード

設定画面では、以下の2種類から選択できます。


* **SUS** — サスペンド（スリープ）

* **OFF** — Windowsを終了（シャットダウン）

### CAN / キャンセル

監視中またはカウントダウン中に **CAN (Cancel)** ボタンを押すと、
自動終了処理をキャンセルして停止状態になります。

停止状態でもう一度 **CAN** ボタンを押すと、AfterIdler自身を終了します。

### SET / 設定

**SET (Setting)** ボタンを押すと設定画面に移行します。

設定項目は以下の順番で切り替わります。

1. CPU 冷却判断温度

2. GPU 冷却判断温度

3. カウントダウンタイマー時間

4. 終了モード（SUS / OFF）

設定画面を抜けると、監視状態は設定画面に入る前の状態に戻ります。

停止状態から **SET ボタンを1秒間長押し**すると、温度監視を再開します。

### Status Lamp / ステータスランプ

ステータスランプは現在の状態を表示します。

* 🔴 **赤点滅** — 監視中
* 
* 🟡 **黄色** — 停止中

* 🟢 **緑色** — 設定中

## 内容

* `AfterIdler.exe`
* `AfterIdlerSensorService.exe`
* `install.bat`
* `uninstall.bat`
* 
## 必要環境

* Windows 10 / 11 64-bit

温度取得に必要な環境によっては PawnIO のインストールが必要です。

## インストール

1. ZIPを展開します。

2. `install.bat` を管理者として実行します。

3. `AfterIdler.exe` を起動します。

## アンインストール

`uninstall.bat` を管理者として実行してください。

レジストリは使わないのでそのままサービス解除後にフォルダを削除してください。

## Notes / 備考

AfterIdler は「PC版ターボタイマー」のようなものです。

高負荷動作を終えた直後にPCを終了するのではなく、
温度が下がるまで少し待ってから、設定された動作
（サスペンドまたはシャットダウン）を実行することを目的としています。
