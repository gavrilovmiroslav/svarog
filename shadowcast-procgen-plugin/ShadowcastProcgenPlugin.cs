using SFML.Graphics;
using svarog.Algorithms;
using svarog.Algorithms.shadowcast;

namespace svarog.Plugins
{
    // uncomment this to make the plugin register:
    //[Plugin(Priority = 1100)]
    public class ShadowcastProcgenPlugin : Plugin
    {
        private readonly int _widthUnits = 40;
        private readonly int _heightUnits = 25;
        private readonly int _visionUnits = int.MaxValue;

        private uint _windowWidth = 0;
        private uint _windowHeight = 0;

        public int WidthScaleFactor => (int)_windowWidth / _widthUnits;
        public int HeightScaleFactor => (int)_windowHeight / _heightUnits;

        public Sprite sprite = new();
        public Sprite shadowSprite = new();

        public enum EProcgen
        {
            Generation,
            Playback,
            Casting
        }

        public enum ETrigger
        {
            Done,
            Restart,
        }

        public override void Load(Svarog instance)
        {
            _windowHeight = instance.window?.Size.Y ?? 0;
            _windowWidth = instance.window?.Size.X ?? 0;

            if (_windowHeight == 0 || _windowWidth == 0)
            {
                Console.WriteLine("ShadowcastProcgenPlugin: Window size is 0 on load!");
                return;
            }

            CreateStateMachine(instance);
        }
        public void CreateStateMachine(Svarog instance)
        {
            var sm = instance.resources.CreateStateMachine<EProcgen, ETrigger>("shadowcast-procgen", EProcgen.Generation);

            var generate = () =>
            {
                Console.WriteLine("Generating...");
                var map = instance.resources.Bag("mapMatrix", IntMap.Noise(_widthUnits, _heightUnits, 0.1f).ToBoolMap(f => f < 150));
                instance.resources.RemoveFromBag("SCmapMatrix");
                sm.Fire(ETrigger.Done);
            };

            sm.Configure(EProcgen.Generation)
                .OnEntry(generate)
                .OnActivate(generate)
                .Permit(ETrigger.Done, EProcgen.Playback)
                .Ignore(ETrigger.Restart);

            sm.Configure(EProcgen.Playback)
                .OnEntry(() =>
                {
                    Console.WriteLine("DONE!");
                })
                .Permit(ETrigger.Restart, EProcgen.Casting)
                .Ignore(ETrigger.Done);

            sm.Configure(EProcgen.Casting)
                .OnEntry(e =>
                {
                    Console.WriteLine("Shadow casting...");
                    var x = (int)e.Parameters[0] / WidthScaleFactor;
                    var y = (int)e.Parameters[1] / HeightScaleFactor;
                    var map = instance.resources.GetFromBag<BoolMap>("mapMatrix");
                    if (map != null)
                    {
                        var scMap = Shadowcast.GenerateShadowCast(map, (x, y), _visionUnits);
                        instance.resources?.Bag<BoolMap>("SCmapMatrix", scMap);
                    }

                    sm.Fire(ETrigger.Done);

                })
                .Permit(ETrigger.Done, EProcgen.Playback);

            sm.Activate();
        }
        public override void Frame(Svarog instance)
        {
            var shiftPressed = instance.keyboard.IsDown(SFML.Window.Keyboard.Scancode.LShift);
            var isLeftMouse = instance.mouse.IsJustPressed(SFML.Window.Mouse.Button.Left);
            if (shiftPressed && isLeftMouse)
            {
                var sm = instance.resources.GetStateMachine<EProcgen, ETrigger>("shadowcast-procgen");
                sm?.Fire<int, int>(new Stateless.StateMachine<EProcgen, ETrigger>.TriggerWithParameters<int, int>(ETrigger.Restart), instance.mouse.Position.Item1, instance.mouse.Position.Item2);
            }
        }
        public override void Render(Svarog svarog)
        {
            var map = svarog.resources.GetFromBag<BoolMap>("mapMatrix");
            for (int i = 0; i < _widthUnits; i++)
            {
                for (int j = 0; j < _heightUnits; j++)
                {
                    if (map?.Values[i, j] ?? false)
                    {
                        var p = sprite.Position;
                        p.X = i * WidthScaleFactor;
                        p.Y = j * HeightScaleFactor;
                        sprite.Position = p;

                        var s = svarog.resources.GetSprite("Catacombs_skull_wall_top");
                        if (s != null)
                        {
                            sprite.Texture = s.Texture;
                            sprite.TextureRect = s.Coords;
                            svarog.render?.Draw(sprite);
                        }
                    }
                }
            }

            var scMap = svarog.resources.GetFromBag<BoolMap>("SCmapMatrix");
            for (int i = 0; i < _widthUnits; i++)
            {
                for (int j = 0; j < _heightUnits; j++)
                {
                    if (scMap?.Values[i, j] ?? false)
                    {
                        shadowSprite.Color = map?.Values[i, j] ?? false ? shadowSprite.Color = Color.Cyan : shadowSprite.Color = Color.Red;

                        var p = shadowSprite.Position;
                        p.X = i * WidthScaleFactor;
                        p.Y = j * HeightScaleFactor;
                        shadowSprite.Position = p;

                        var s = svarog.resources.GetSprite("White");
                        if (s != null)
                        {
                            shadowSprite.Texture = s.Texture;
                            shadowSprite.TextureRect = s.Coords;
                            svarog.render?.Draw(shadowSprite);
                        }
                    }
                }
            }
        }
    }
}