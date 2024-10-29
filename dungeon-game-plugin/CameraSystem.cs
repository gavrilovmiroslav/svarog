using Arch.Core;
using FloodSpill;
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
        QueryDescription roguesImagesPositionQuery;

        Vector2f cameraPosition = new();
        Vector2f cameraTarget = new();

        Vector2f glyphSize;
        float lag = 0.05f;

        BoolMap light;
        private Sprite sprite = new();

        public Vector2f CameraPosition => cameraPosition;

        public CameraSystem()
        {
            cameraTargetQuery = new QueryDescription().WithAll<CameraTarget, Position>();
            playerSightQuery = new QueryDescription().WithAll<Player, Sight>();
            roguesImagesPositionQuery = new QueryDescription().WithAll<RoguesImage, Position>();
        }

        public override void Load(Svarog svarog)
        {
            base.Load(svarog);
            var size = svarog.resources.GetFromBag<Vector2i>("glyphSize");
            glyphSize = size.ToFloats();
            light = new BoolMap(size.X, size.Y);
            cameraPosition = new Vector2f(0, 0); // glyphSize.X / 2, glyphSize.Y / 2);
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
                newTarget += 32 * position.At.ToFloats() * target.Weight;
            });

            if (targetCount == 0) return;

            cameraTarget = (cameraPosition + newTarget / targetCount) / 2;
            cameraPosition = Lerp.Linear(cameraPosition, cameraTarget, lag);
        }

        public override void Render(Svarog svarog)
        {
            light = light.Clear();
            svarog.world.Query(in playerSightQuery, (Entity entity, ref Player player, ref Sight sight) =>
            {
                if (sight.LastFov != null)
                {
                    light = light.InplaceCombine(sight.LastFov);
                }
                else
                {}
            });

            /*
             *                 

             */
            Dictionary<Vector2i, RSprite?> onMap = [];
            Dictionary<Vector2i, Vector2f> realWorldCoord = [];
            svarog.world.Query(in roguesImagesPositionQuery, (Entity entity, ref RoguesImage image, ref Position position) =>
            {
                var t = svarog.resources.GetSprite(image.Image);
                onMap[new Vector2i(position.At.X, position.At.Y)] = t;
            });

            var map = svarog.resources.GetFromBag<BoolMap>("dungeon: has floor");
            var windowCenter = svarog.window.Size.ToFloats() / 2;
            var glyphWindowSize = svarog.window.Size.ToFloats() / 32;
            var leftTop = cameraPosition - windowCenter;
            var leftTopCoord = (leftTop / 32).ToInts();
            if (leftTopCoord.X < 0) leftTopCoord.X = 0;
            if (leftTopCoord.Y < 0) leftTopCoord.Y = 0;

            var bottomDownCoord = leftTopCoord + glyphWindowSize.ToInts();
            if (bottomDownCoord.X >= (int)glyphSize.X) bottomDownCoord.X = (int)glyphSize.X - 1;
            if (bottomDownCoord.Y >= (int)glyphSize.Y) bottomDownCoord.Y = (int)glyphSize.Y - 1;

            var wall = svarog.resources.GetSprite("Dirt_wall_top");
            var side = svarog.resources.GetSprite("Dirt_wall_side");
            var floor1 = svarog.resources.GetSprite("Blank_floor");
            var floor2 = svarog.resources.GetSprite("Blank_floor_dark_purple");

            sprite.Texture = wall.Texture;
            int c = 0;
            for (var i = 0; i < bottomDownCoord.X - leftTopCoord.X; i++)
            {
                for (var j = 0; j < bottomDownCoord.Y - leftTopCoord.Y; j++)
                {
                    c++;

                    var p = sprite.Position;
                    p.X = i * 32;
                    p.Y = j * 32;
                    
                    sprite.Position = p;

                    var (nx, ny) = (leftTopCoord.X + i, leftTopCoord.Y + j);
                    realWorldCoord[new Vector2i(nx, ny)] = p;

                    if (!light.Values[nx, ny]) continue;
                    if (map?.Values[nx, ny] ?? false)
                    {
                        if (j < 100 - 1 && (!map?.Values[nx, ny + 1] ?? false))
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

                    var l = 255;
                    sprite.Color = new Color(255, 255, 255, (byte)l);
                    if (map?.Values[nx, ny] ?? false)
                    {
                        sprite.Color = Color.White;
                    }
                    svarog.render?.Draw(sprite);
                }
            }

            foreach (var e in onMap)
            {
                var ij = e.Key;
                var tt = e.Value;

                if (realWorldCoord.ContainsKey(ij))
                {
                    var xy = realWorldCoord[ij];
                    sprite.Texture = tt.Texture;
                    sprite.TextureRect = tt.Coords;

                    sprite.Position = xy;
                    sprite.Color = Color.White;
                    svarog.render?.Draw(sprite);
                }
            }
        }
    }
}
