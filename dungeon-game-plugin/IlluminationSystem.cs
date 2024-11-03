using Arch.Core;
using svarog;
using svarog.Algorithms;

namespace dungeon_game_plugin
{
    [Plugin(Priority = 500)]
    internal partial class IlluminationSystem : Plugin
    {
        QueryDescription sightedQuery;
        public IlluminationSystem() 
        {
            sightedQuery = new QueryDescription().WithAll<Player, Position, Sight, Orientation>();    
        }

        public override void Load(Svarog svarog)
        {
            base.Load(svarog);
        }

        public override void Frame(Svarog svarog)
        {
            base.Frame(svarog);
            UpdateFOVSystem(svarog);

            var map = svarog.resources.GetFromBag<BoolMap>("dungeon: has floor");
        }

        void UpdateFOVSystem(Svarog svarog)
        {
            var map = svarog.resources.GetFromBag<BoolMap>("dungeon: has floor");
            var memory = svarog.resources.GetFromBag<BoolMap>("dungeon: memory");
            if (memory == null)
            {
                memory = svarog.resources.Bag<BoolMap>("dungeon: memory", new BoolMap(map.Width, map.Height));
            }

            if (map != null)
            {
                svarog.world.Query(in sightedQuery, (Entity entity, ref Position position, ref Sight sight, ref Orientation orientation) =>
                {
                    if (sight.LastFov != null && sight.LastPosition != null && sight.LastPosition == position.At && orientation.To == orientation.Last) return;
                    if (memory != null && sight.LastFov != null)
                    {
                        svarog.resources.Bag("dungeon: memory", memory.InplaceCombine(sight.LastFov));
                    }

                    var newMap = (BoolMap?)svarog.Invoke("shadowcast", 
                        ("map", map), 
                        ("position", ((int)position.At.X, (int)position.At.Y)), 
                        ("range", sight.Range));

                    var orient = orientation.To;
                    var at = position.At;
                    var range = sight.Range;

                    var fov = newMap.ToIntMap(v => v ? 255 : 0).Filter((i, j, v) =>
                    {
                        var ij = new SFML.System.Vector2f(i, j);
                        var dist = ij.Distance(at);

                        if (ij.Distance(at) <= range / 2)
                        {
                            return v;
                        }
                        else
                        {
                            var dot = orient.Dot(ij - at);
                            if (MathF.Sign(dot) < 0)
                            {
                                return Math.Clamp((int)MathF.Min((255 / (dist + 0.1f)), v), 0, 255);
                            }
                            else
                            {
                                return v;
                            }
                        }
                    }).Blur().FilterByBoolPredicate(newMap);
                    
                    svarog.resources.Bag<IntMap>("lightmap", fov);
                    sight.LastFov = fov.ToBoolMap(p => p > 0);
                    sight.LastPosition = position.At;
                    orientation.Last = orientation.To;
                });
            }
        }
    }
}
