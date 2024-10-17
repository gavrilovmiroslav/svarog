using SFML.Graphics;
using svarog.Structures;

namespace svarog
{
    public record RSprite(Texture Texture, IntRect Coords);

    public partial class Resources
    {
        public Dictionary<string, Texture> Textures = [];
        public Dictionary<string, RSprite> Sprites = [];
        public MultiMap<string, string> NamedSprites = new();

        public RSprite? GetSprite(string name)
        {
            if (Sprites.TryGetValue(name, out var sprite))
                return sprite;
            else
                return null;
        }
    }
}
