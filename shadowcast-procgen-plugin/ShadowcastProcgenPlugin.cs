using SFML.Graphics;
using SFML.System;
using Stateless;
using svarog.Algorithms;

namespace svarog.Plugins
{
    // uncomment this to make the plugin register:
    // [Plugin]
    public class ShadowcastProcgenPlugin : GenerativePlugin
    {
        private readonly int _widthUnits = 80;
        private readonly int _heightUnits = 50;
        private readonly int _visionUnits = 10;

        private uint _windowWidth = 0;
        private uint _windowHeight = 0;

        public int WidthScaleFactor => (int)_windowWidth / _widthUnits;
        public int HeightScaleFactor => (int)_windowHeight / _heightUnits;

        public Sprite shadowSprite = new();

        public ShadowcastProcgenPlugin() : base("shadowcast")
        {
        }

        public override void Load(Svarog instance)
        {
            base.Load(instance);

            sprite.Scale = new SFML.System.Vector2f(0.5f, 0.5f);
            shadowSprite.Scale = new SFML.System.Vector2f(0.5f, 0.5f);

            _windowHeight = instance.window?.Size.Y ?? 0;
            _windowWidth = instance.window?.Size.X ?? 0;

            if (_windowHeight == 0 || _windowWidth == 0)
            {
                Console.WriteLine("ShadowcastProcgenPlugin: Window size is 0 on load!");
                return;
            }

            instance.resources.Bag<bool>("use light?", true);
        }

        public override void Generate(Svarog instance, StateMachine<EProcgen, ETrigger> sm)
        {
            Console.WriteLine("Generating...");
            instance.Invoke("generate level (subdiv)",
                ("name", "level1"),
                ("map size", (_widthUnits * 2, _heightUnits * 2)),
                ("door %", 100));
            var levelMap = instance.resources.GetFromBag<IntMap>("level1: room id map");
            if (levelMap != null)
            {
                var map = instance.resources.Bag<BoolMap>("map", levelMap.ToBoolMap(p => p == 0));
                instance.resources.RemoveFromBag("shadowcast map");

                var rand = new Random();
                while (true)
                {
                    var x = rand.Next(0, _widthUnits);
                    var y = rand.Next(0, _heightUnits);
                    if (levelMap.Values[x, y] > 0)
                    {
                        var shadowcastMap = (BoolMap?)instance.Invoke("shadowcast", ("map", map), ("position", (x, y)), ("range", _visionUnits));
                        var shadowcastBlurMap = shadowcastMap.ToIntMap(i => i ? 255 : 0).Blur().Blur().Copy(shadowcastMap);
                        instance.resources?.Bag<IntMap>("shadowcast map", shadowcastBlurMap);
                        break;
                    }
                }
            }
        }

        public override void Frame(Svarog instance)
        {
            base.Frame(instance);

            var shiftPressed = instance.keyboard.IsDown(SFML.Window.Keyboard.Scancode.LShift);
            var isLeftMouse = instance.mouse.IsDown(SFML.Window.Mouse.Button.Left);
            
            if (isLeftMouse)
            {
                var x = (int)(instance.mouse.Position.Item1 / WidthScaleFactor);
                var y = (int)(instance.mouse.Position.Item2 / HeightScaleFactor);
                var map = instance.resources.GetFromBag<BoolMap>("map");
                if (map != null)
                {
                    var shadowcastMap = (BoolMap?)instance.Invoke("shadowcast", ("map", map), ("position", (x, y)), ("range", _visionUnits));
                    var shadowcastBlurMap = shadowcastMap.ToIntMap(i => i ? 255 : 0).Blur().Blur().Blur().Copy(shadowcastMap);
                    instance.resources?.Bag<IntMap>("shadowcast map", shadowcastBlurMap);
                }
            }

            instance.resources.Bag("use light?", instance.keyboard.IsDown(SFML.Window.Keyboard.Scancode.Tab));
            
        }

        public override void Render(Svarog svarog)
        {
            var useLight = svarog.resources.GetFromBag<bool>("use light?");
            var map = svarog.resources.GetFromBag<BoolMap>("map");
            var wall = svarog.resources.GetSprite("Dirt_wall_top");
            var side = svarog.resources.GetSprite("Dirt_wall_side");
            var floor1  = svarog.resources.GetSprite("Blank_floor");
            var floor2 = svarog.resources.GetSprite("Blank_floor_dark_purple");
            var light = svarog.resources.GetFromBag<IntMap>("shadowcast map");

            if (light == null) return;

            if (wall != null && side != null && floor1 != null && floor2 != null)
            {
                sprite.Texture = wall.Texture;

                int c = 0;
                for (int i = 0; i < _widthUnits; i++)
                {
                    for (int j = 0; j < _heightUnits; j++)
                    {
                        c++;
                        if (!useLight && light.Values[i, j] == 0) continue;

                        var p = sprite.Position;
                        p.X = i * WidthScaleFactor;
                        p.Y = j * HeightScaleFactor;
                        sprite.Position = p;

                        if (map?.Values[i, j] ?? false)
                        {
                            if (j < _heightUnits - 1 && (!map?.Values[i, j + 1] ?? false))
                            {
                                sprite.TextureRect = side.Coords;
                            }
                            else
                            {
                                sprite.TextureRect = wall.Coords;
                            }
                        }
                        else
                        {
                            sprite.TextureRect = (c % 2 == 0) ? floor1.Coords : floor2.Coords;
                        }

                        sprite.Color = !useLight ? new Color(255, 255, 255, (byte)light.Values[i, j]) : Color.White;
                        if (map?.Values[i, j] ?? false)
                        {
                            sprite.Color = Color.White;
                        }
                        svarog.render?.Draw(sprite);   
                    }
                    c++;
                }
            }
        }
    }
}