using Arch.Core;
using svarog;
using svarog.Algorithms;

namespace dungeon_game_plugin
{
    [Plugin(Priority = 500)]
    internal class IlluminationSystem : Plugin
    {
        QueryDescription sightedQuery;

        public IlluminationSystem() 
        {
            sightedQuery = new QueryDescription().WithAll<Player, Position, Sight>();    
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
                svarog.world.Query(in sightedQuery, (Entity entity, ref Position position, ref Sight sight) =>
                {
                    if (sight.LastFov != null && sight.LastPosition != null && sight.LastPosition == position.At) return;
                    if (memory != null && sight.LastFov != null)
                    {
                        svarog.resources.Bag("dungeon: memory", memory.InplaceCombine(sight.LastFov));
                    }

                    var newMap = (BoolMap?)svarog.Invoke("shadowcast", ("map", map), ("position", ((int)position.At.X, (int)position.At.Y)), ("range", sight.Range));
                    sight.LastFov = newMap;
                    sight.LastPosition = position.At;
                });
            }
        }
    }
}
