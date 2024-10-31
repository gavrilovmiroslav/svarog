using SFML.Graphics;
using SFML.System;
using SharpGraph;
using svarog;
using svarog.Algorithms;
using svarog.Structures;
using System.Diagnostics;
using System.Threading;

namespace dungeon_game_plugin
{
    internal class LevelDesign
    {
        internal MultiMap<int, int> roomDegrees = new();
        internal MultiMap<int, Vector2i> roomTiles = new();
        internal Graph floorPlan;
        internal IntMap roomIdMap;
        internal BoolMap hasFloor;

        Sprite sprite = new();

        internal static (int, int) ParseLabel(string label)
        {
            var parts = label.Split(",");
            var x = parts[0].Trim();
            var y = parts[1].Trim();
            return (int.Parse(x), int.Parse(y));
        }

        public LevelDesign(Svarog svarog) 
        {
            svarog.world.Clear();
            var size = svarog.resources.Bag("glyphSize", new Vector2i(160, 100));

            svarog.Invoke(
                "generate level (subdiv)",
                ("name", "dungeon"),
                ("map size", size.AsTuple()),
                ("door %", 99));

            var floorPlan = svarog.resources.GetFromBag<Graph>("dungeon: floor plan");
            var roomIdMap = svarog.resources.GetFromBag<IntMap>("dungeon: room id map");

            Debug.Assert(roomIdMap != null);
            Debug.Assert(floorPlan != null);

            for (int i = 0; i < roomIdMap.Width; i++)
            {
                for (int j = 0; j < roomIdMap.Height; j++)
                {
                    if (roomIdMap.Values[i, j] > 0)
                    {
                        roomTiles.Add(roomIdMap.Values[i, j], new Vector2i(i, j));
                    }
                }
            }

            var center = new Vector2f(size.X, size.Y * 3.0f / 4.0f);
            var minDist = 1000000.0f;
            Node? bestRoom = null;
            foreach (var room in floorPlan.GetNodes())
            {
                var edgeCount = floorPlan.GetIncidentEdges(room).Count;
                var (x, y) = ParseLabel(room.GetLabel());
                var id = roomIdMap.Values[x, y];
                var d = new Vector2f(x, y).Distance(center);
                if (d < minDist * edgeCount)
                {
                    minDist = d;
                    bestRoom = room;
                }
                roomDegrees.Add(edgeCount, id);
            }

            hasFloor = svarog.resources.Bag<BoolMap>("dungeon: has floor", roomIdMap.ToBoolMap(x => x == 0));

            Debug.Assert(bestRoom != null);
            var (bx, by) = ParseLabel(bestRoom.Value.GetLabel());
            svarog.world.Create(
                new Player(), 
                new Position(new Vector2f(bx, by)), 
                new CameraTarget(1.0f), 
                new Sight(10, null, null), 
                new RoguesImage("Rogue"));

            svarog.world.Create(
                new Monster(),
                new Position(new Vector2f(bx - 2, by - 2)),
                new RoguesImage("Goblin_archer"));

            svarog.world.Create(
                new Monster(),
                new Position(new Vector2f(bx - 3, by - 1)),
                new RoguesImage("Goblin_mage"));

            svarog.world.Create(
                new Monster(),
                new Position(new Vector2f(bx - 5, by - 5)),
                new RoguesImage("Orc_warchief"));
        }

        public void DebugRender(Svarog svarog)
        {
            var size = svarog.resources.GetFromBag<Vector2i>("glyphSize");
            var useLight = svarog.resources.GetFromBag<bool>("use light?");
            var map = hasFloor;

            var wall = svarog.resources.GetSprite("Dirt_wall_top");
            var side = svarog.resources.GetSprite("Dirt_wall_side");
            var floor1 = svarog.resources.GetSprite("Blank_floor");
            var floor2 = svarog.resources.GetSprite("Blank_floor_dark_purple");

            if (wall != null && side != null && floor1 != null && floor2 != null)
            {
                sprite.Texture = wall.Texture;
                sprite.Color = new Color(255, 128, 128, 255);
                int c = 0;
                for (int i = 0; i < size.X; i++)
                {
                    for (int j = 0; j < size.Y; j++)
                    {
                        c++;
                        
                        var p = sprite.Position;
                        p.X = i * 32 * 0.25f;
                        p.Y = j * 32 * 0.25f;
                        sprite.Position = p;

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

                        svarog.render?.Draw(sprite);
                    }
                    c++;
                }
            }

        }
    }
}
