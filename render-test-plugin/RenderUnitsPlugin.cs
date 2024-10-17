using SFML.Graphics;
using svarog;
using svarog.Algorithms;

namespace svarog.Plugins
{
    //[Plugin(Priority = 101)]
    public class RenderUnitPlugin : Plugin
    {
        public override void Render(Svarog svarog)
        {
            Sprite sprite = new();

            var map = svarog.resources.GetFromBag<BoolMap>("map");
            var rogues = svarog.resources.NamedSprites["rogues"];
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (map?.Values[i, j] ?? false)
                    {
                        var p = sprite.Position;
                        p.X = i * 32;
                        p.Y = j * 32;
                        sprite.Position = p;
                        var s = svarog.resources.Sprites[rogues[Random.Shared.Next(0, rogues.Count())]];
                        sprite.Texture = s.Texture;
                        sprite.TextureRect = s.Coords;
                        svarog.render?.Draw(sprite);
                    }
                }
            }
        }
    }
}
