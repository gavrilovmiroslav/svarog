﻿using SFML.Graphics;
using Stateless;
using svarog;
using svarog.Algorithms;
using static SFML.Window.Keyboard;

namespace svarog.Plugins
{
    // uncomment this to make the plugin register:
    //[Plugin]
    public class EquidistantProcgenPlugin : GenerativePlugin
    {
        private bool[] active = [true, false, false, false, false];

        public EquidistantProcgenPlugin() : base("equidistant") { }

        public override void Generate(Svarog instance, StateMachine<EProcgen, ETrigger> sm)
        {
            Console.WriteLine("GENERATING...");

            instance.resources.Bag("equimap1", BoolMap.EquidistantSampling(40, 25, ESamplingDistance.Minimal));
            instance.resources.Bag("equimap2", BoolMap.EquidistantSampling(40, 25, ESamplingDistance.Low));
            instance.resources.Bag("equimap3", BoolMap.EquidistantSampling(40, 25, ESamplingDistance.Moderate));
            instance.resources.Bag("equimap4", BoolMap.EquidistantSampling(40, 25, ESamplingDistance.High));
        }

        public override void Frame(Svarog instance)
        {
            base.Frame(instance);

            if (instance.keyboard.IsJustReleased(Scancode.Num1))
            {
                active[0] = !active[0];
                var status = active[0] ? "on" : "off";
                Console.WriteLine($"Equidistant [minimal] map: {status}");
            }

            if (instance.keyboard.IsJustReleased(Scancode.Num2))
            {
                active[1] = !active[1];
                var status = active[1] ? "on" : "off";
                Console.WriteLine($"Equidistant [low] map: {status}");
            }

            if (instance.keyboard.IsJustReleased(Scancode.Num3))
            {
                active[2] = !active[2];
                var status = active[2] ? "on" : "off";
                Console.WriteLine($"Equidistant [moderate] map: {status}");
            }

            if (instance.keyboard.IsJustReleased(Scancode.Num4))
            {
                active[3] = !active[3];
                var status = active[3] ? "on" : "off";
                Console.WriteLine($"Equidistant [high] map: {status}");
            }
        }

        public override void Render(Svarog svarog)
        {
            void RenderMap(string name, int r, int g, int b)
            {
                var map = svarog.resources.GetFromBag<BoolMap>(name);

                sprite.Color = Color.White;
                var s = svarog.resources.GetSprite("White");
                if (s != null)
                {
                    sprite.Texture = s.Texture;
                    sprite.TextureRect = s.Coords;
                }

                for (int i = 0; i < 40; i++)
                {
                    for (int j = 0; j < 25; j++)
                    {
                        var p = sprite.Position;
                        p.X = i * 32;
                        p.Y = j * 32;
                        sprite.Position = p;
                        var v = (byte)((map?.Values[i, j] ?? false) ? 255 : 0);
                        sprite.Color = new Color((byte)(v * r), (byte)(v * g), (byte)(v * b), 85);
                        svarog.render?.Draw(sprite, new RenderStates(BlendMode.Add));
                    }
                }
            }

            if (active[0]) RenderMap("equimap1", 0, 1, 0);
            if (active[1]) RenderMap("equimap2", 0, 0, 1);
            if (active[2]) RenderMap("equimap3", 1, 0, 0);
            if (active[3]) RenderMap("equimap4", 1, 0, 1);
        }
    }
}
