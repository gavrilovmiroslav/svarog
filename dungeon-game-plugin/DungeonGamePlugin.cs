using SFML.Graphics;
using svarog;

namespace dungeon_game_plugin
{
    // uncomment this to make the plugin register:
    [Plugin(Priority = 499)]
    public class DungeonGamePlugin : Plugin
    {
        private Sprite sprite = new();
        private LevelDesign design;

        public void GenerateLevel(Svarog svarog)
        {
            design = new LevelDesign(svarog);
        }

        public override void Load(Svarog svarog)
        {
            base.Load(svarog);
            GenerateLevel(svarog);
            sprite.Scale = new SFML.System.Vector2f(0.25f, 0.25f);
        }

        public override void Render(Svarog svarog)
        {
            base.Render(svarog);
            //design.DebugRender(svarog);
        }
    }
}
