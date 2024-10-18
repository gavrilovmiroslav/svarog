using MessagePack.Formatters;
using SFML.Graphics;
using svarog;
using svarog.Algorithms;

namespace svarog.Plugins
{
    [Plugin(Priority = 100)]
    public class RenderTilesPlugin : Plugin
    {
        Font? font;
        Text text;
        Sprite sprite;

        public RenderTilesPlugin()
        {
            text = new();
            sprite = new();
        }

        public override void Load(Svarog instance)
        {
            var data = File.ReadAllBytes("Data//Arial.ttf");
            font = new Font(data);
            text.Font = font;
            text.CharacterSize = 14;
            text.FillColor = Color.White;
            text.OutlineColor = Color.White;
        }

        public override void Render(Svarog svarog)
        {
            bool ok = svarog.resources.GetFromBag<bool>("render?");
            if (!ok) return;

            var map = svarog.resources.GetFromBag<BoolMap>("map");
            var tiles = svarog.resources.NamedSprites["tiles"];
            int c = 0;

            sprite.Color = Color.White;

            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    var p = sprite.Position;
                    p.X = i * 32;
                    p.Y = j * 32;
                    sprite.Position = p;
                    var s = svarog.resources.GetSprite(map?.Values[i, j] ?? false ? "Blank_floor" : "Inner_wall");
                    if (s != null)
                    {
                        sprite.Texture = s.Texture;
                        sprite.TextureRect = s.Coords;
                        svarog.render?.Draw(sprite);
                    }
                    c++;
                }
            }

            var flood = svarog.resources.GetFromBag<IntMap>("flood");
            if (flood != null)
            {
                for (int i = 0; i < 40; i++)
                {
                    for (int j = 0; j < 25; j++)
                    {
                        var v = flood.Values[i, j];
                        if (v > 255) {
                            sprite.Color = Color.White;
                            continue; 
                        }

                        var p = sprite.Position;
                        p.X = i * 32;
                        p.Y = j * 32;
                        sprite.Position = p;
                        var s = svarog.resources.GetSprite("White");
                        if (s != null)
                        {
                            var a = 255 - v * 5;
                            if (a < 0) a = 0;
                            sprite.Color = v == 0 ? new Color(255, 0, 0, 255) : new Color(255, 255, 255, (byte)a);
                            sprite.Texture = s.Texture;
                            sprite.TextureRect = s.Coords;
                            svarog.render?.Draw(sprite);
                        }
                        c++;
                    }
                }
            }
        }
    }
}
