using SFML.Graphics;
using svarog;

namespace dungeon_game_plugin
{
    // uncomment this to make the plugin register:
    [Plugin(Priority = 1)]
    public class DungeonGamePlugin : Plugin
    {
        private Sprite sprite = new();
        private LevelDesign design;
        public static event EventHandler OnLevelGenerated;

        public void GenerateLevel(Svarog svarog)
        {
            design = new LevelDesign(svarog);
            OnLevelGenerated?.Invoke(this, new EventArgs());
        }

        public override void Load(Svarog svarog)
        {
            base.Load(svarog);
            GenerateLevel(svarog);
            sprite.Scale = new SFML.System.Vector2f(0.25f, 0.25f);
        }

        public override void Frame(Svarog svarog)
        {
            if (svarog.keyboard.IsJustReleased(SFML.Window.Keyboard.Scancode.Tab))
            {
                GenerateLevel(svarog);
            }
        }

        public override void Render(Svarog svarog)
        {
            base.Render(svarog);
            //design.DebugRender(svarog);
        }
    }
}
