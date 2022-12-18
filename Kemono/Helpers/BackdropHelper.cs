using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using WinRT;

namespace Kemono.Helpers;

public class BackdropHelper
{
    public BackdropHelper(Window window)
    {
        _cssb = window.As<ICompositionSupportsSystemBackdrop>();
        _window = window;

        _mica = MicaController.IsSupported() ? new MicaController() : null;
        _micaAlt = MicaController.IsSupported() ? new MicaController {Kind = MicaKind.BaseAlt} : null;
        _acrylic = DesktopAcrylicController.IsSupported() ? new DesktopAcrylicController() : null;

        _wsdqHelper = new WindowsSystemDispatcherQueueHelper();
        _wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
    }

    public enum BackdropType
    {
        DefaultColor,
        Mica,
        MicaAlt,
        DesktopAcrylic,
    }

    private readonly ICompositionSupportsSystemBackdrop _cssb;
    private readonly Window _window;

    private WindowsSystemDispatcherQueueHelper _wsdqHelper;

    private BackdropType _currentBackdrop;
    private MicaController? _mica;
    private MicaController? _micaAlt;
    private DesktopAcrylicController? _acrylic;
    private SystemBackdropConfiguration? _configurationSource;

    public bool SetBackdrop(BackdropType type)
    {
        // Reset to default color. If the requested type is supported, we'll update to that.
        // Note: This sample completely removes any previous controller to reset to the default
        //       state. This is done so this sample can show what is expected to be the most
        //       common pattern of an app simply choosing one controller type which it sets at
        //       startup. If an app wants to toggle between Mica and Acrylic it could simply
        //       call RemoveSystemBackdropTarget() on the old controller and then setup the new
        //       controller, reusing any existing m_configurationSource and Activated/Closed
        //       event handlers.
        // _currentBackdrop = BackdropType.DefaultColor;

        _window.Activated -= Window_Activated;
        _window.Closed -= Window_Closed;
        ((FrameworkElement)_window.Content).ActualThemeChanged -= Window_ThemeChanged;

        _configurationSource = null;

        switch (_currentBackdrop)
        {
            case BackdropType.Mica when _mica != null:
                _mica.RemoveSystemBackdropTarget(_cssb);
                break;
            case BackdropType.MicaAlt when _micaAlt != null:
                _micaAlt.RemoveSystemBackdropTarget(_cssb);
                break;
            case BackdropType.DesktopAcrylic when _acrylic != null:
                _acrylic.RemoveSystemBackdropTarget(_cssb);
                break;
            case BackdropType.DefaultColor:
                break;
            default:
                throw new ArgumentOutOfRangeException($"{nameof(_currentBackdrop)}{_currentBackdrop}");
        }

        switch (type)
        {
            case BackdropType.Mica when TrySetMicaBackdrop():
            case BackdropType.DesktopAcrylic when TrySetAcrylicBackdrop():
            case BackdropType.MicaAlt when TrySetMicaAltBackdrop():
            case BackdropType.DefaultColor:
                _currentBackdrop = type;
                return true;
            default:
                return false;
        }
    }

    private void Configurations()
    {
        // Hooking up the policy object
        _configurationSource = new SystemBackdropConfiguration();
        _window.Activated += Window_Activated;
        _window.Closed += Window_Closed;
        ((FrameworkElement)_window.Content).ActualThemeChanged += Window_ThemeChanged;

        // Initial configuration state.
        _configurationSource.IsInputActive = true;
        SetConfigurationSourceTheme();
    }

    private bool TrySetMicaAltBackdrop()
    {
        if (_micaAlt == null)
        {
            return false; // Mica is not supported on this system
        }

        Configurations();

        // Enable the system backdrop.
        // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
        _micaAlt.AddSystemBackdropTarget(_cssb);
        _micaAlt.SetSystemBackdropConfiguration(_configurationSource);
        return true; // succeeded
    }

    private bool TrySetMicaBackdrop()
    {
        if (_mica == null)
        {
            return false; // Mica is not supported on this system
        }

        Configurations();

        // Enable the system backdrop.
        // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
        _mica.AddSystemBackdropTarget(_cssb);
        _mica.SetSystemBackdropConfiguration(_configurationSource);
        return true; // succeeded
    }

    private bool TrySetAcrylicBackdrop()
    {
        if (_acrylic == null)
        {
            return false; // Acrylic is not supported on this system
        }

        Configurations();

        // Enable the system backdrop.
        // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
        _acrylic.AddSystemBackdropTarget(_cssb);
        _acrylic.SetSystemBackdropConfiguration(_configurationSource);
        return true; // succeeded
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        _configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
        // use this closed window.
        if (_mica != null)
        {
            _mica.Dispose();
            _mica = null;
        }

        if (_acrylic != null)
        {
            _acrylic.Dispose();
            _acrylic = null;
        }

        _window.Activated -= Window_Activated;
        _configurationSource = null;
    }

    private void Window_ThemeChanged(FrameworkElement sender, object args)
    {
        if (_configurationSource != null)
        {
            SetConfigurationSourceTheme();
        }
    }

    private void SetConfigurationSourceTheme()
    {
        _configurationSource.Theme = ((FrameworkElement)_window.Content).ActualTheme switch
        {
            ElementTheme.Dark => SystemBackdropTheme.Dark,
            ElementTheme.Light => SystemBackdropTheme.Light,
            ElementTheme.Default => SystemBackdropTheme.Default,
            _ => _configurationSource.Theme
        };
    }
}