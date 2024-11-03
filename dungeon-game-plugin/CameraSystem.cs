using Arch.Core;
using Arch.Core.Extensions;
using FloodSpill;
using MathNet.Numerics.RootFinding;
using SFML.Graphics;
using SFML.System;
using svarog;
using svarog.Algorithms;

namespace dungeon_game_plugin
{
    [Plugin(Priority = 501)]
    internal class CameraSystem : Plugin
    {
        QueryDescription cameraTargetQuery;
        QueryDescription playerSightQuery;

        Vector2f cameraPosition = new();
        Vector2f cameraTarget = new();

        Vector2f glyphSize;
        float zoom = 2.0f;
        float lag = 0.05f;

        BoolMap light;
        private Sprite sprite = new();

        public Vector2f CameraPosition => cameraPosition;

        public CameraSystem()
        {
            cameraTargetQuery = new QueryDescription().WithAll<CameraTarget, Position>();
            playerSightQuery = new QueryDescription().WithAll<Player, Sight, Position, Orientation>();
        }

        public override void Load(Svarog svarog)
        {
            base.Load(svarog);
            var size = svarog.resources.GetFromBag<Vector2i>("glyphSize");
            glyphSize = size.ToFloats();
            light = new BoolMap(size.X, size.Y);
            cameraPosition = glyphSize * 16 * zoom;
            
        }

        public override void Frame(Svarog svarog)
        {
            UpdateCamera(svarog);
        }

        void UpdateCamera(Svarog svarog)
        {
            int targetCount = 0;
            Vector2f newTarget = new Vector2f(0, 0);
            svarog.world.Query(in cameraTargetQuery, (Entity entity, ref CameraTarget target, ref Position position) => 
            {
                targetCount++;
                newTarget += (32 * zoom) * position.At * target.Weight;
            });

            if (targetCount == 0) return;

            cameraTarget = (cameraPosition + newTarget / targetCount) / 2;
            cameraPosition = Lerp.Linear(cameraPosition, cameraTarget, lag);
            
            var windowCenter = svarog.window.Size.ToFloats() / 2;
            svarog.resources.Bag<Vector2f>("camera offset", windowCenter - cameraPosition);
            svarog.resources.Bag<float>("camera zoom", zoom);
        }

        public override void Render(Svarog svarog)
        {
            var memory = svarog.resources.GetFromBag<BoolMap>("dungeon: memory");
            var s = sprite.Scale;
            s.X = zoom;
            s.Y = zoom;
            sprite.Scale = s;

            light = light.Clear();
            svarog.world.Query(in playerSightQuery, (Entity entity, ref Player player, ref Sight sight, ref Orientation orientation, ref Position position) =>
            {
                if (sight.LastFov != null)
                {
                    light = light.InplaceCombine(sight.LastFov);
                }

                var lightMap = svarog.resources.GetFromBag<IntMap>("lightmap");
                if (lightMap == null) return;
                var spriteSize = 32 * zoom;
                var map = svarog.resources.GetFromBag<BoolMap>("dungeon: has floor");
                var windowCenter = svarog.window.Size.ToFloats() / 2;
                var glyphWindowSize = svarog.window.Size.ToFloats() / spriteSize;
                var leftTop = cameraPosition - windowCenter;
                var leftTopCoord = leftTop / spriteSize;

                var wall = svarog.resources.GetSprite("Dirt_wall_top");
                var side = svarog.resources.GetSprite("Dirt_wall_side");
                var floor1 = svarog.resources.GetSprite("Blank_floor");
                var floor2 = svarog.resources.GetSprite("Blank_floor_dark_purple");

                sprite.Texture = wall.Texture;
                int c = 0;
                for (int i = 0; i < map.Width; i++)
                {
                    c++;
                    for (int j = 0; j < map.Height; j++)
                    {
                        c++;

                        var p = sprite.Position;
                        p.X = windowCenter.X - cameraPosition.X + i * spriteSize;
                        p.Y = windowCenter.Y - cameraPosition.Y + j * spriteSize;

                        if (p.X + 2 * spriteSize < 0 || p.Y + 2 * spriteSize < 0) continue;
                        if (p.X - 2 * spriteSize >= svarog.window.Size.X || p.Y - 2 * spriteSize >= svarog.window.Size.Y) continue;
                        sprite.Position = p;

                        if (i < 0) continue;
                        if (j < 0) continue;
                        if (i >= (int)glyphSize.X) continue;
                        if (j >= (int)glyphSize.Y) continue;

                        if (map?.Values[i, j] ?? false)
                        {
                            if (j < 100 - 1 && (!map?.Values[i, j + 1] ?? false))
                            {
                                sprite.TextureRect = side.Coords;
                            }
                            else
                            {
                                sprite.TextureRect = wall.Coords;
                            }
                        }
                        else
                        {
                            sprite.TextureRect = (c % 2 == 0) ? floor1.Coords : floor2.Coords;
                        }

                        var l = lightMap.Values[i, j];
                        var color = new Color(255, 255, 255, (byte)l);
                        if (!light.Values[i, j])
                        {
                            if (memory?.Values[i, j] ?? false)
                            {
                                sprite.Color = new Color(25, 25, 25, 255);
                                svarog.render?.Draw(sprite, GrayscaleShaderPlugin.Grayscale);
                            }
                            continue;
                        }

                        sprite.Color = color;
                        svarog.render?.Draw(sprite, new RenderStates(BlendMode.Alpha));
                    }
                }
            });
        }
    }
}
