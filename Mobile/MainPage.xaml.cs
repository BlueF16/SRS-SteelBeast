﻿using System.Net;
using Ciribob.SRS.Mobile.Client;

namespace Mobile;

public partial class MainPage : ContentPage
{
    private SRSAudioManager _srsAudioManager;
    private int count = 0;

    public MainPage()
    {
        InitializeComponent();

        var status = CheckAndRequestMicrophonePermission();
    }

    public async Task<PermissionStatus> CheckAndRequestMicrophonePermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

        if (status == PermissionStatus.Granted)
            return status;

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            // Prompt the user to turn on in settings
            // On iOS once a permission has been denied it may not be requested again from the application
            return status;

        if (Permissions.ShouldShowRationale<Permissions.Microphone>())
        {
            // Prompt the user with additional information as to why the permission is needed
        }

        status = await Permissions.RequestAsync<Permissions.Microphone>();

        return status;
    }

    private void OnStopClicked(object sender, EventArgs e)
    {
        _srsAudioManager?.StopEncoding();
    }

    private void OnStartClicked(object sender, EventArgs e)
    {
        _srsAudioManager?.StopEncoding();

        if (IPEndPoint.TryParse(Address.Text, out var result))
        {
            _srsAudioManager = new SRSAudioManager();
            _srsAudioManager.StartPreview(result);
        }
        else
        {
            DisplayAlert("Error", "Invalid IP and port", "OK");
        }
    }
}