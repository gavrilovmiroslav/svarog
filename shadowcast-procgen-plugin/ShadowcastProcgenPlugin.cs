using SFML.Graphics;
using Stateless;
using svarog.Algorithms;
using svarog.Algorithms.shadowcast;

namespace svarog.Plugins
{
    // uncomment this to make the plugin register:
    [Plugin(Priority = 1100)]
    public class ShadowcastProcgenPlugin : GenerativePlugin
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

        public ShadowcastProcgenPlugin() : base("shadowcast")
        {
        }

        public override void Load(Svarog instance)
        {
            base.Load(instance);

            _windowHeight = instance.window?.Size.Y ?? 0;
            _windowWidth = instance.window?.Size.X ?? 0;

            if (_windowHeight == 0 || _windowWidth == 0)
            {
                Console.WriteLine("ShadowcastProcgenPlugin: Window size is 0 on load!");
                return;
            }
        }

        public override void Generate(Svarog instance, StateMachine<EProcgen, ETrigger> sm)
        {
            Console.WriteLine("Generating...");
            var map = instance.resources.Bag("mapMatrix", IntMap.Noise(_widthUnits, _heightUnits, 0.1f).ToBoolMap(f => f < 150));
            instance.resources.RemoveFromBag("SCmapMatrix");
        }

        public override void Frame(Svarog instance)
        {
            base.Frame(instance);

            var shiftPressed = instance.keyboard.IsDown(SFML.Window.Keyboard.Scancode.LShift);
            var isLeftMouse = instance.mouse.IsJustPressed(SFML.Window.Mouse.Button.Left);
            
            if (isLeftMouse)
            {
                Console.WriteLine("Shadow casting...");
                var x = (int)instance.mouse.Position.Item1 / WidthScaleFactor;
                var y = (int)instance.mouse.Position.Item2 / HeightScaleFactor;
                var map = instance.resources.GetFromBag<BoolMap>("mapMatrix");
                if (map != null)
                {
                    var scMap = Shadowcast.GenerateShadowCast(map, (x, y), _visionUnits);
                    instance.resources?.Bag<BoolMap>("SCmapMatrix", scMap);
                }
            }
        }

        public override void Render(Svarog svarog)
        {
            var map = svarog.resources.GetFromBag<BoolMap>("mapMatrix");
            var s = svarog.resources.GetSprite("Catacombs_skull_wall_top");
            if (s != null)
            {
                sprite.Texture = s.Texture;
                sprite.TextureRect = s.Coords;

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

                            svarog.render?.Draw(sprite);
                        }
                    }
                }
            }

            var scMap = svarog.resources.GetFromBag<BoolMap>("SCmapMatrix");

            s = svarog.resources.GetSprite("White");
            if (s != null)
            {
                shadowSprite.Texture = s.Texture;
                shadowSprite.TextureRect = s.Coords;

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

                            svarog.render?.Draw(shadowSprite);
                        }
                    }
                }
            }
        }
    }
}