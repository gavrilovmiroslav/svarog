using SFML.Graphics;

namespace svarog_core.Effects
{
    public class ShaderUtility
    {
        public static Shader? LoadFromName(string name)
        {
            if (!Shader.IsAvailable)
            {
                Console.WriteLine($"[{name}] No shaders available.");
                return null;
            }

            var vert = File.ReadAllText($"Data//{name}//{name.ToLower()}.vert");
            var frag = File.ReadAllText($"Data//{name}//{name.ToLower()}.frag");

            return Shader.FromString(vert, null, frag);
        }
    }
}
