using SFML.Graphics;
using svarog_core;

namespace svarog.Plugins
{
    [Plugin]
    public class RenderTilesPlugin : Plugin
    {
        public override void Render(Svarog svarog)
        {
            Sprite sprite = new();

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
                    var s = svarog.resources.Sprites[c % 2 == 0 ? "Blank_floor" : "Inner_wall"];
                    sprite.Texture = s.Item1;
                    sprite.TextureRect = s.Item2;
                    svarog.render?.Draw(sprite);
                    c++;
                }
            }
        }
    }
}
