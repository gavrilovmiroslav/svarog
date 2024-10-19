using MessagePack.Formatters;
using SFML.Graphics;
using svarog;
using svarog.Algorithms;

namespace svarog.Plugins
{
    //[Plugin(Priority = 100)]
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

        
    }
}
