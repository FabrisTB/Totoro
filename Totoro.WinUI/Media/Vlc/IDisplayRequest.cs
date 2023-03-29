﻿namespace Totoro.WinUI.Media.Vlc;

/// <summary>
/// Interface for display requests
/// </summary>
internal interface IDisplayRequest
{
    /// <summary>
    /// Activates a display request
    /// </summary>
    void RequestActive();

    /// <summary>
    /// Deactivates a display request
    /// </summary>
    void RequestRelease();
}
