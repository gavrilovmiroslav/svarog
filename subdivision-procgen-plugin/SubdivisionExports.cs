using SFML.System;
using SharpGraph;
using subdivision_procgen_plugin;
using svarog.Algorithms;

namespace svarog.Plugins
{
    [Plugin(Priority = 1)]
    public class SubdivisionExports : Plugin
    {
        public override void Register(Svarog instance)
        {
            Export("triangulate")
                .With<List<Vector2f>>("points")
                .Returning((svarog, args) =>
                {
                    return Subdivision.Triangulate((List<Vector2f>)args["points"]);
                });

            Export("voronoi")
                .With<BoolMap>("points")
                .Returning((svarog, args) =>
                {
                    return Subdivision.VoronoiSubdivide((BoolMap)args["points"]);
                });

            Export("min spanning tree")
                .With<Graph>("graph")
                .Returning((svarog, args) =>
                {
                    return new Graph(((Graph)args["graph"]).GenerateMinimumSpanningTree(SpanningTreeAlgorithm.Kruskal, false));
                });

            //public static LevelDescriptor Generate(Svarog instance, (int, int) mapSize, int doorProbability, Func<int, int, int> corridorConnections)
            Export("generate level (subdiv)")
                .With<string>("name")
                .With<(int, int)>("map size")
                .With<int>("door %")
                .With<Func<int, int, int>>("corridor distribution")
                .Returning((svarog, args) =>
                {
                    var level = SubdivisionLevelGenerator.Generate(svarog, 
                        ((int, int))args["map size"], 
                        (int)args["door %"], 
                        (Func<int, int, int>)args["corridor distribution"]);

                    var name = (string)args["name"];
                    svarog.resources.Bag($"{name}: floor plan", level.FloorPlan);
                    svarog.resources.Bag($"{name}: map size", level.MapSize);
                    svarog.resources.Bag($"{name}: door set", level.Doors);
                    svarog.resources.Bag($"{name}: room id map", level.RoomIdMap);
                    svarog.resources.Bag(name, new[] { 
                        $"{name}: floor plan", 
                        $"{name}: map size", 
                        $"{name}: door set", 
                        $"{name}: room id map"});
                    return level;
                });
        }
    }
}
