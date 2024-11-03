using SFML.Graphics;
using svarog;
using svarog.Effects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_game_plugin
{
    [Plugin(Priority = 505)]
    public class GrayscaleShaderPlugin : Plugin
    {
        private RenderStates grayscaleRenderState;
        protected Shader? grayscaleShader;

        public static RenderStates Grayscale => Instance.grayscaleRenderState;

        public static GrayscaleShaderPlugin Instance;

        public override void Load(Svarog instance)
        {
            Instance = this;
            var shader = ShaderUtility.LoadFromName("grayscale");
            if (shader == null)
            {
                Stop();
                return;
            }

            grayscaleShader = shader;

            grayscaleRenderState = new(grayscaleShader)
            {
                BlendMode = BlendMode.Alpha
            };
        }
    }
}
