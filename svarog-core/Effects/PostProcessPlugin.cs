using SFML.Graphics;
using svarog_core;

namespace svarog.Effects
{
    public class PostprocessPlugin(string name) : Plugin
    {
        protected Shader? postprocessShader;
        Sprite? screenSprite;
        RenderTexture? postprocessTexture;

        public override void Load(Svarog instance)
        {
            if (!Shader.IsAvailable)
            {
                Console.WriteLine($"[{this.GetType().Name}] No shaders available.");
                Stop();
                return;
            }

            var vert = File.OpenRead($"Data//{name}//{name.ToLower()}.vert");
            var frag = File.OpenRead($"Data//{name}//{name.ToLower()}.frag");
            postprocessShader = new Shader(vert, null, frag);
            postprocessTexture = new RenderTexture(instance.window?.Size.X ?? 1280, instance.window?.Size.Y ?? 800);
            screenSprite = new()
            {
                Texture = postprocessTexture.Texture
            };
        }

        public override void Render(Svarog instance)
        {
            if (screenSprite != null && instance.render != null && postprocessTexture != null)
            {
                instance.render.Display();

                // screen -> texture
                screenSprite.Texture = instance.render.Texture;
                postprocessTexture.Draw(screenSprite, new RenderStates(postprocessShader));
                postprocessTexture.Display();

                // texture (post processing) -> (blanked) screen
                instance.render.Clear();
                screenSprite.Texture = postprocessTexture.Texture;
                instance.render.Draw(screenSprite);
            }
        }
    }

}
