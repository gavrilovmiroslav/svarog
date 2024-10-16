namespace svarog.Plugins
{
    [Plugin]
    public class EmergencyExitPlugin : Plugin
    {
        public override void Frame(Svarog instance)
        {
            if (instance.keyboard.IsJustPressed(SFML.Window.Keyboard.Scancode.F10))
                instance.window?.Close();

            if (instance.keyboard.GetHoldDuration(SFML.Window.Keyboard.Scancode.Escape) > 3000.0)
                instance.window?.Close();

            if (instance.mouse.GetHoldDuration(SFML.Window.Mouse.Button.Middle) > 3000.0)
                instance.window?.Close();
        }
    }
}
