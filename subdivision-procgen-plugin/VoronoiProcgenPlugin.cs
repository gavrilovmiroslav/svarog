using DelaunatorSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SFML.Graphics;
using SFML.System;
using SharpGraph;
using Stateless;
using subdivision_procgen_plugin;
using svarog.Algorithms;
using svarog.Structures;
using static subdivision_procgen_plugin.SubdivisionLevelGenerator;
using static svarog.Plugins.Subdivision;

namespace svarog.Plugins
{
    //[Plugin]
    public class VoronoiProcgenPlugin : GenerativePlugin
    {
        public VoronoiProcgenPlugin() : base("voronoi") { }

        private SubdivisionLevelGenerator.LevelDescriptor? _level;

        public override void Generate(Svarog instance, StateMachine<EProcgen, ETrigger> _)
        {
            _level = instance.Invoke("generate level (subdiv)", 
                ("name", "level1"),
                ("map size", (160, 100)), 
                ("door %", 100)) as LevelDescriptor;
        }

        public override void Render(Svarog svarog)
        {
            if (_level == null) return;

            float scale = 0.25f;
            var (width, height) = _level.MapSize;
            var floorPlan = _level.FloorPlan;
            var tree = floorPlan.GetEdges();
            var rooms = _level.RoomIdMap;
            
            sprite.Color = Color.White;
            var s = svarog.resources.GetSprite("White");
            if (s != null)
            {
                sprite.Texture = s.Texture;
                sprite.TextureRect = s.Coords;
                sprite.Scale = new Vector2f(scale, scale);
            }

            var gray = new Color(128, 128, 128, 255);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var room = rooms.Values[i, j];
                    if (room > 0)
                    {
                        var p = sprite.Position;
                        p.X = i * 32 * scale;
                        p.Y = j * 32 * scale;
                        sprite.Position = p;
                        sprite.Color = _level.Doors.Contains($"{i},{j}") ? Color.Red : gray;
                        svarog.render?.Draw(sprite, new RenderStates(BlendMode.Add));
                    }
                }
            }

            foreach (var edge in tree)
            {
                var (px, py) = SubdivisionLevelGenerator.ParseLabel(edge.From().GetLabel());
                var (qx, qy) = SubdivisionLevelGenerator.ParseLabel(edge.To().GetLabel());

                svarog.render?.Draw(new Vertex[] {
                    new Vertex(new Vector2f((float)px * 32 + 16, (float)py * 32 + 16) * scale, Color.Blue),
                    new Vertex(new Vector2f((float)qx * 32 + 16, (float)qy * 32 + 16) * scale, Color.Blue)
                }, PrimitiveType.Lines);
            }
        }
    }
}
