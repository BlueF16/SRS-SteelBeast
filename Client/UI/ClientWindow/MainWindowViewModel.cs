﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using Ciribob.FS3D.SimpleRadio.Standalone.Client.Audio;
using Ciribob.FS3D.SimpleRadio.Standalone.Client.Audio.Managers;
using Ciribob.FS3D.SimpleRadio.Standalone.Client.Settings;
using Ciribob.FS3D.SimpleRadio.Standalone.Client.Singletons;
using Ciribob.FS3D.SimpleRadio.Standalone.Client.UI.ClientWindow.ClientList;
using Ciribob.FS3D.SimpleRadio.Standalone.Client.Utils;
using Ciribob.SRS.Common.Helpers;
using Ciribob.SRS.Common.Network.Client;
using Ciribob.SRS.Common.Network.Models.EventMessages;
using Ciribob.SRS.Common.Network.Singletons;
using NAudio.CoreAudioApi;
using NLog;
using WPFCustomMessageBox;
using ConnectedClientsSingleton = Ciribob.FS3D.SimpleRadio.Standalone.Client.Singletons.ConnectedClientsSingleton;
using LogManager = NLog.LogManager;
using PropertyChangedBase = Ciribob.SRS.Common.Helpers.PropertyChangedBase;

namespace Ciribob.FS3D.SimpleRadio.Standalone.Client.UI.ClientWindow
{
    public class MainWindowViewModel : PropertyChangedBase, IHandle<TCPClientStatusMessage>, IHandle<VOIPStatusMessage>
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();


        private readonly AudioManager _audioManager;

        private readonly GlobalSettingsStore _globalSettings = GlobalSettingsStore.Instance;

        private readonly DispatcherTimer _updateTimer;

        private AudioPreview _audioPreview;
        private AwacsRadioOverlayWindow.RadioOverlayWindow _awacsRadioOverlay;

        private ClientListWindow _clientListWindow;

        private Overlay.RadioOverlayWindow _radioOverlayWindow;

        private ServerSettingsWindow _serverSettingsWindow;

        //used to debounce toggle
        private long _toggleShowHide;
        private TCPClientHandler _client;

        public ClientStateSingleton ClientState { get; } = ClientStateSingleton.Instance;
        public ConnectedClientsSingleton Clients { get; } = ConnectedClientsSingleton.Instance;
        public AudioInputSingleton AudioInput { get; } = AudioInputSingleton.Instance;
        public AudioOutputSingleton AudioOutput { get; } = AudioOutputSingleton.Instance;

        public InputDeviceManager InputManager { get; set; }

        public bool Connected { get; set; }

        public bool IsConnected { get; set; }

        public bool IsVoIPConnected { get; set; }

        public DelegateCommand TrayIconQuitCommand { get; set; }

        public DelegateCommand TrayIconCommand { get; set; }

        public DelegateCommand ConnectCommand { get; set; }

        public ICommand PreviewCommand { get; set; }

        public bool PreviewEnabled
        {
            get => AudioInput.MicrophoneAvailable && !IsConnected;
        }

        public MainWindowViewModel()
        {
            _audioManager = new AudioManager(AudioOutput.WindowsN);

            PreviewCommand = new DelegateCommand(() => PreviewAudio());
                
            ConnectCommand = new DelegateCommand(Connect);

            TrayIconCommand = new DelegateCommand(() =>
            {
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            });

            TrayIconQuitCommand = new DelegateCommand(() =>
            {
                Application.Current.MainWindow.Close();
            });

            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _updateTimer.Tick += UpdatePlayerCountAndVUMeters;
            _updateTimer.Start();

            ClientStateSingleton.Instance.PlayerUnitState.Name = Name;

            EventBus.Instance.SubscribeOnUIThread(this);
        }

        public float SpeakerVU
        {
            get
            {
                if (_audioPreview != null && _audioPreview.IsPreviewing)
                    return _audioPreview.SpeakerMax;
                if (_audioManager != null) return _audioManager.SpeakerMax;

                return -100;
            }
        }

        public float MicVU
        {
            get
            {
                if (_audioPreview != null && _audioPreview.IsPreviewing)
                    return _audioPreview.MicMax;
                if (_audioManager != null) return _audioManager.MicMax;

                return -100;
            }
        }



        public string PreviewText
        {
            get
            {
                if (_audioPreview == null || !_audioPreview.IsPreviewing || IsConnected)
                    return "preview audio";
                return "stop preview";
            }
        }

        public string ConnectText
        {
            get
            {
                if (IsConnected)
                    return "Disconnect";
                return "connect";
            }
        }

        public double SpeakerBoost
        {
            get
            {
                var boost = _globalSettings.GetClientSetting(GlobalSettingsKeys.SpeakerBoost).DoubleValue;
                _audioManager.SpeakerBoost = VolumeConversionHelper.ConvertVolumeSliderToScale((float)boost);
                if (_audioPreview != null) _audioPreview.SpeakerBoost = _audioManager.SpeakerBoost;
                return boost;
            }
            set
            {
                _globalSettings.SetClientSetting(GlobalSettingsKeys.SpeakerBoost,
                    value.ToString(CultureInfo.InvariantCulture));
                _audioManager.SpeakerBoost = VolumeConversionHelper.ConvertVolumeSliderToScale((float)value);

                if (_audioPreview != null) _audioPreview.SpeakerBoost = _audioManager.SpeakerBoost;
                // NotifyPropertyChanged();
                // NotifyPropertyChanged("SpeakerBoostText");
            }
        }

        public string SpeakerBoostText =>
            VolumeConversionHelper.ConvertLinearDiffToDB(
                VolumeConversionHelper.ConvertVolumeSliderToScale((float)SpeakerBoost));

        public string Name
        {
            get
            {
                var name = _globalSettings.GetClientSetting(GlobalSettingsKeys.LastSeenName);

                if (name == null|| name.RawValue == "")
                {
                    return "FS3D Client";
                }
                else
                {
                    return name.RawValue;
                }
            }
            set
            {
                if (value != null)
                {
                    _globalSettings.SetClientSetting(GlobalSettingsKeys.LastSeenName, value);
                    NotifyPropertyChanged();
                }
            }
        }

        public string ServerAddress
        {
            get
            {
                var savedAddress =  _globalSettings.GetClientSetting(GlobalSettingsKeys.LastServer);

                if (savedAddress == null)
                {
                    return "127.0.0.1:5002";
                }
                else
                {
                    return savedAddress.RawValue;
                }
            }
            set
            {
                if (value != null)
                {
                    _globalSettings.SetClientSetting(GlobalSettingsKeys.LastServer, value);
                    NotifyPropertyChanged();
                }
            }
        }

        public bool AudioSettingsEnabled
        {
            get
            {
                if (_audioPreview != null && _audioPreview.IsPreviewing || IsConnected) return false;

                return true;
            }
        }

        private void UpdatePlayerCountAndVUMeters(object sender, EventArgs e)
        {
            NotifyPropertyChanged("SpeakerVU");
            NotifyPropertyChanged("MicVU");
            ConnectedClientsSingleton.Instance.NotifyAll();
        }

        private void Connect()
        {
            if (IsConnected)
            {
                Stop();
            }
            else
            {
                //stop preview
                _audioPreview?.StopEncoding();
                _audioPreview = null;

                IsConnected = true;
                SaveSelectedInputAndOutput();

                try
                {
                    //process hostname
                    var resolvedAddresses = Dns.GetHostAddresses(GetAddressFromTextBox());
                    var ip = resolvedAddresses.FirstOrDefault(xa => xa.AddressFamily == AddressFamily.InterNetwork); // Ensure we get an IPv4 address in case the host resolves to both IPv6 and IPv4

                    if (ip != null)
                    {
                        var resolvedIp = ip;
                        var port = GetPortFromTextBox();

                        _client = new TCPClientHandler(ClientStateSingleton.Instance.GUID, ClientStateSingleton.Instance.PlayerUnitState);
                        _client.TryConnect(new IPEndPoint(resolvedIp, port));
                    }
                    else
                    {
                        //invalid ID
                        MessageBox.Show("Invalid IP or Host Name!", "Host Name Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        IsConnected = false;
                    }
                }
                catch (Exception ex) when (ex is SocketException || ex is ArgumentException)
                {
                    MessageBox.Show("Invalid IP or Host Name!", "Host Name Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    IsConnected = false;
                }
            }
        }

        private void Stop(TCPClientStatusMessage.ErrorCode connectionError = TCPClientStatusMessage.ErrorCode.TIMEOUT)
        {
            if (IsConnected && _globalSettings.GetClientSettingBool(GlobalSettingsKeys.PlayConnectionSounds))
            {
                try
                {
                    Sounds.BeepDisconnected.Play();
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Failed to play disconnect sound");
                }
            }

            IsConnected = false;

            try
            {
                _audioManager.StopEncoding();
            }
            catch (Exception ex)
            {
            }
            
            _client?.Disconnect();
            _client = null;
            
            //TODO
            // ClientState.DcsPlayerRadioInfo.Reset();
            // ClientState.PlayerCoaltionLocationMetadata.Reset();
        }

        private string GetAddressFromTextBox()
        {
            var addr = ServerAddress.Trim();
        
            if (addr.Contains(":"))
            {
                return addr.Split(':')[0];
            }
        
            return addr;
        }
        
        private int GetPortFromTextBox()
        {
            var addr = ServerAddress.Trim();
        
            if (addr.Contains(":"))
            {
                int port;
                if (int.TryParse(addr.Split(':')[1], out port))
                {
                    return port;
                }
                throw new ArgumentException("specified port is  valid");
            }
        
            return 5002;
        }

        
        private void SaveSelectedInputAndOutput()
        {
            //save app settings
            // Only save selected microphone if one is actually available, resulting in a crash otherwise
            if (AudioInput.MicrophoneAvailable)
            {
                if (AudioInput.SelectedAudioInput.Value == null)
                {
                    _globalSettings.SetClientSetting(GlobalSettingsKeys.AudioInputDeviceId, "default");
                }
                else
                {
                    var input = ((MMDevice)AudioInput.SelectedAudioInput.Value).ID;
                    _globalSettings.SetClientSetting(GlobalSettingsKeys.AudioInputDeviceId, input);
                }
            }

            if (AudioOutput.SelectedAudioOutput.Value == null)
            {
                _globalSettings.SetClientSetting(GlobalSettingsKeys.AudioOutputDeviceId, "default");
            }
            else
            {
                var output = (MMDevice)AudioOutput.SelectedAudioOutput.Value;
                _globalSettings.SetClientSetting(GlobalSettingsKeys.AudioOutputDeviceId, output.ID);
            }

            //check if we have optional output
            if (AudioOutput.SelectedMicAudioOutput.Value != null)
            {
                var micOutput = (MMDevice)AudioOutput.SelectedMicAudioOutput.Value;
                _globalSettings.SetClientSetting(GlobalSettingsKeys.MicAudioOutputDeviceId, micOutput.ID);
            }
            else
            {
                _globalSettings.SetClientSetting(GlobalSettingsKeys.MicAudioOutputDeviceId, "");
            }

            ShowMicPassthroughWarning();
        }

        private void ShowMicPassthroughWarning()
        {
            if (_globalSettings.GetClientSetting(GlobalSettingsKeys.MicAudioOutputDeviceId).RawValue
                .Equals(_globalSettings.GetClientSetting(GlobalSettingsKeys.AudioOutputDeviceId).RawValue))
                MessageBox.Show(
                    "Mic Output and Speaker Output should not be set to the same device!\n\nMic Output is just for recording and not for use as a sidetone. You will hear yourself with a small delay!\n\nHit disconnect and change Mic Output / Passthrough",
                    "Warning", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
        }

        private void PreviewAudio()
        {
            if (_audioPreview == null)
            {
                if (!AudioInput.MicrophoneAvailable)
                {
                    Logger.Info("Unable to preview audio, no valid audio input device available or selected");
                    return;
                }

                //get device
                try
                {
                    SaveSelectedInputAndOutput();

                    _audioPreview = new AudioPreview();
                    _audioPreview.SpeakerBoost = VolumeConversionHelper.ConvertVolumeSliderToScale((float)SpeakerBoost);
                    _audioPreview.StartPreview(AudioOutput.WindowsN);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex,
                        "Unable to preview audio - likely output device error - Pick another. Error:" + ex.Message);
                }
            }
            else
            {
                _audioPreview.StopEncoding();
                _audioPreview = null;
            }

            NotifyPropertyChanged("PreviewText");
            NotifyPropertyChanged("AudioSettingsEnabled");
        }

        public void StartAudio(IPEndPoint endPoint)
        {
            //Must be main thread
            Application.Current.Dispatcher.Invoke(delegate {
                try
                {
                    _audioManager.StartEncoding(ClientState.GUID, InputManager, endPoint);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex,
                        "Unable to get audio device - likely output device error - Pick another. Error:" +
                        ex.Message);
                    Stop();

                    var messageBoxResult = CustomMessageBox.ShowOK(
                        "Problem initialising Audio Output!\n\nTry a different Output device and check privacy settings\n\nIf the problem persists, disable ALL other outputs and restart FS3D SRS",
                        "Audio Output Error",
                        "Close",
                        MessageBoxImage.Error);

                }
            });
         

        }


        public async Task HandleAsync(TCPClientStatusMessage obj, CancellationToken cancellationToken)
        {

            if (obj.Connected)
            {
                IsConnected = true;
                //connection sound
                if (_globalSettings.GetClientSettingBool(GlobalSettingsKeys.PlayConnectionSounds))
                {
                    try
                    {
                        Sounds.BeepConnected.Play();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "Failed to play connect sound");
                    }
                }

                StartAudio(obj.Address);

            }
            else
            {
                //disconnect sound
                Stop(obj.Error);
            }
        }

        private void ShowOverlay_OnClick(object sender, RoutedEventArgs e)
        {
            ToggleOverlay(true);
        }

        private void ToggleOverlay(bool uiButton)
        {
            //debounce show hide (1 tick = 100ns, 6000000 ticks = 600ms debounce)
            if (DateTime.Now.Ticks - _toggleShowHide > 6000000 || uiButton)
            {
                _toggleShowHide = DateTime.Now.Ticks;
                if (_radioOverlayWindow == null || !_radioOverlayWindow.IsVisible ||
                    _radioOverlayWindow.WindowState == WindowState.Minimized)
                {
                    //hide awacs panel
                    _awacsRadioOverlay?.Close();
                    _awacsRadioOverlay = null;

                    _radioOverlayWindow?.Close();

                    _radioOverlayWindow = new Overlay.RadioOverlayWindow();


                    _radioOverlayWindow.ShowInTaskbar =
                        !_globalSettings.GetClientSettingBool(GlobalSettingsKeys.RadioOverlayTaskbarHide);
                    _radioOverlayWindow.Show();
                }
                else
                {
                    _radioOverlayWindow?.Close();
                    _radioOverlayWindow = null;
                }
            }
        }

        private void ShowAwacsOverlay_OnClick(object sender, RoutedEventArgs e)
        {
            if (_awacsRadioOverlay == null || !_awacsRadioOverlay.IsVisible ||
                _awacsRadioOverlay.WindowState == WindowState.Minimized)
            {
                //close normal overlay
                _radioOverlayWindow?.Close();
                _radioOverlayWindow = null;

                _awacsRadioOverlay?.Close();

                _awacsRadioOverlay = new AwacsRadioOverlayWindow.RadioOverlayWindow();
                _awacsRadioOverlay.ShowInTaskbar =
                    !_globalSettings.GetClientSettingBool(GlobalSettingsKeys.RadioOverlayTaskbarHide);
                _awacsRadioOverlay.Show();
            }
            else
            {
                _awacsRadioOverlay?.Close();
                _awacsRadioOverlay = null;
            }
        }

        private void ToggleServerSettings_OnClick(object sender, RoutedEventArgs e)
        {
            if (_serverSettingsWindow == null || !_serverSettingsWindow.IsVisible ||
                _serverSettingsWindow.WindowState == WindowState.Minimized)
            {
                _serverSettingsWindow?.Close();

                _serverSettingsWindow = new ServerSettingsWindow();
                _serverSettingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                _serverSettingsWindow.Owner = Application.Current.MainWindow;
                _serverSettingsWindow.Show();
            }
            else
            {
                _serverSettingsWindow?.Close();
                _serverSettingsWindow = null;
            }
        }

        private void ShowClientList_OnClick(object sender, RoutedEventArgs e)
        {
            if (_clientListWindow == null || !_clientListWindow.IsVisible ||
                _clientListWindow.WindowState == WindowState.Minimized)
            {
                _clientListWindow?.Close();

                _clientListWindow = new ClientListWindow();
                _clientListWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                _clientListWindow.Owner = Application.Current.MainWindow;
                _clientListWindow.Show();
            }
            else
            {
                _clientListWindow?.Close();
                _clientListWindow = null;
            }
        }


        public void OnClosing()
        {

            //stop timer
            _updateTimer?.Stop();

            _client?.Disconnect();
            _client = null;

            // Stop();
            //
            // _audioPreview?.StopEncoding();
            // _audioPreview = null;
            //
            // _radioOverlayWindow?.Close();
            // _radioOverlayWindow = null;
            //
            // _awacsRadioOverlay?.Close();
            // _awacsRadioOverlay = null;
        }

        public Task HandleAsync(VOIPStatusMessage message, CancellationToken cancellationToken)
        {
            IsVoIPConnected = message.Connected;
            return Task.CompletedTask;
        }
    }
}