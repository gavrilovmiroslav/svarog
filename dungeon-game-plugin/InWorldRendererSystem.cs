﻿using Arch.Core;
using Arch.Core.Extensions;
using SFML.Graphics;
using SFML.System;
using svarog;
using svarog.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_game_plugin
{
    [Plugin(Priority = 506)]
    public class InWorldRendererSystem : Plugin
    {
        QueryDescription roguesImagesPositionQuery;
        private Sprite sprite = new();
        private float t;

        public override void Load(Svarog svarog)
        {
            roguesImagesPositionQuery = new QueryDescription().WithAll<RoguesImage, Position>();
        }

        public override void Render(Svarog svarog)
        {
            t += 0.1f;
            var lightMap = svarog.resources.GetFromBag<IntMap>("lightmap");
            var cameraOffset = svarog.resources.GetFromBag<Vector2f>("camera offset");
            var zoom = svarog.resources.GetFromBag<float>("camera zoom");
            var spriteSize = 32 * zoom;
            var dy = MathF.Sin(t) * 0.05f;

            var s = sprite.Scale;
            s.X = zoom;
            s.Y = zoom + dy;
            sprite.Scale = s;

            svarog.world.Query(in roguesImagesPositionQuery, (Entity entity, ref RoguesImage image, ref Position position) =>
            {
                var t = svarog.resources.GetSprite(image.Image);

                var p = sprite.Position;
                p.X = cameraOffset.X + (position.At.X + 0.5f) * spriteSize;
                p.Y = cameraOffset.Y + position.At.Y * spriteSize + 3 * (spriteSize / 4);
                sprite.Origin = new Vector2f(16.0f, 32.0f);
                sprite.Position = p;

                sprite.Texture = t.Texture;
                sprite.TextureRect = t.Coords;

                sprite.Color = new Color(255, 255, 255, (byte)lightMap.Values[(int)position.At.X, (int)position.At.Y]);

                var s = sprite.Scale;
                s.X = (entity.Has<Monster>() ? -1 : 1) * zoom;
                sprite.Scale = s;

                svarog.render?.Draw(sprite, new RenderStates(BlendMode.Alpha));
            });
        }
    }
}
