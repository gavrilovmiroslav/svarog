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

        public override void Load(Svarog instance)
        {
            var data = File.ReadAllBytes("Data//Arial.ttf");
            font = new Font(data);
        }

        public override void Render(Svarog svarog)
        {
            Sprite sprite = new();

            var map = svarog.resources.GetFromBag<BoolMap>("map");
            var tiles = svarog.resources.NamedSprites["tiles"];
            int c = 0;
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
                Text text = new();
                text.Font = font;
                text.CharacterSize = 14;
                text.FillColor = Color.White;
                text.OutlineColor = Color.White;

                for (int i = 0; i < 40; i++)
                {
                    for (int j = 0; j < 25; j++)
                    {
                        if (flood.Values[i, j] > 100) { continue; }

                        text.DisplayedString = flood.Values[i, j].ToString();
                        text.Position = new SFML.System.Vector2f(i * 32 + 16 - text.DisplayedString.Length * 4, j * 32);
                        svarog.render?.Draw(text);
                    }
                }
            }
        }
    }
}
