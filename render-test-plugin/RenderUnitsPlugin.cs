using SFML.Graphics;
using svarog_core;

namespace svarog.Plugins
{
    [Plugin(Priority = 101)]
    public class RenderUnitPlugin : Plugin
    {
        public override void Render(Svarog svarog)
        {
            Sprite sprite = new();

            var rogues = svarog.resources.NamedSprites["rogues"];
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    var p = sprite.Position;
                    p.X = i * 32;
                    p.Y = j * 32;
                    sprite.Position = p;
                    var s = svarog.resources.Sprites[rogues[Random.Shared.Next(0, rogues.Count())]];
                    sprite.Texture = s.Item1;
                    sprite.TextureRect = s.Item2;
                    svarog.render?.Draw(sprite);
                }
            }
        }
    }
}
