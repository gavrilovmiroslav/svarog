using Arch.Core;
using Arch.Core.Extensions;
using SFML.Graphics;
using SFML.System;
using svarog;
using svarog.Algorithms;
using svarog.Effects;

namespace dungeon_game_plugin
{
    [Plugin(Priority = 506)]
    public class InWorldRendererSystem : Plugin
    {
        QueryDescription roguesImagesPositionQuery;
        private readonly Sprite sprite = new();
        private RenderStates alphaRenderState;
        private float time;
        
        public override void Load(Svarog instance)
        {
            roguesImagesPositionQuery = new QueryDescription().WithAll<RoguesImage, Position, Orientation>();
            alphaRenderState = new RenderStates(BlendMode.Alpha);
        }

        public override void Render(Svarog svarog)
        {
            time += svarog.clock.ElapsedTime.AsMilliseconds() * 0.01f;
            var lightMap = svarog.resources.GetFromBag<IntMap>("lightmap");
            if (lightMap == null) return;

            var cameraOffset = svarog.resources.GetFromBag<Vector2f>("camera offset");
            var zoom = svarog.resources.GetFromBag<float>("camera zoom");
            var spriteSize = 32 * zoom;
            var dy = MathF.Sin(time) * 0.05f;
            
            svarog.world.Query(in roguesImagesPositionQuery, (Entity entity, ref RoguesImage image, ref Position position, ref Orientation orientation) =>
            {
                var t = svarog.resources.GetSprite(image.Image);

                var p = sprite.Position;
                p.X = cameraOffset.X + (position.At.X + 0.5f) * spriteSize;
                p.Y = cameraOffset.Y + position.At.Y * spriteSize + 3 * (spriteSize / 4);
                sprite.Origin = new Vector2f(16.0f, 32.0f);
                sprite.Position = p;

                sprite.Texture = t.Texture;
                sprite.TextureRect = t.Coords;
                bool useGrayscale = false;
                var alpha = (byte)lightMap.Values[(int)position.At.X, (int)position.At.Y];
                sprite.Color = new Color(255, 255, 255, alpha);

                var s = sprite.Scale;
                s.X = -orientation.Side * zoom;
                s.Y = zoom + dy * (alpha == 0 ? 0 : 1);
                sprite.Scale = s;

                if (alpha == 0)
                {
                    useGrayscale = true;
                    if (entity.Has<LastKnownPosition>() && entity.Get<LastKnownPosition>().At.HasValue)
                    {
                        var lkp = entity.Get<LastKnownPosition>().At.Value;
                        p = sprite.Position;
                        p.X = cameraOffset.X + (lkp.X + 0.5f) * spriteSize;
                        p.Y = cameraOffset.Y + lkp.Y * spriteSize + 3 * (spriteSize / 4);
                        sprite.Position = p;
                        sprite.Color = new Color(45, 40, 40, 255);
                    }
                } 
                else
                {
                    if (entity.Has<LastKnownPosition>())
                    {
                        entity.Get<LastKnownPosition>().At = position.At;
                    }
                }

                svarog.render?.Draw(sprite, useGrayscale ? GrayscaleShaderPlugin.Grayscale : alphaRenderState);
            });
        }
    }
}
