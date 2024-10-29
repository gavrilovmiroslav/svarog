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

        public override void Frame(Svarog svarog)
        {
            base.Frame(svarog);
            UpdateFOVSystem(svarog);
        }

        void UpdateFOVSystem(Svarog svarog)
        {
            var map = svarog.resources.GetFromBag<BoolMap>("dungeon: has floor");
            if (map != null)
            {
                svarog.world.Query(in sightedQuery, (Entity entity, ref Position position, ref Sight sight) =>
                {
                    if (sight.LastFov != null && sight.LastPosition != null && sight.LastPosition == position.At) return;
                    if (map.Values[position.At.X, position.At.Y]) Console.WriteLine("STANDING IN CONCRETE!");
                    sight.LastFov = (BoolMap?)svarog.Invoke("shadowcast", ("map", map), ("position", (position.At.X, position.At.Y)), ("range", sight.Range));
                    sight.LastPosition = position.At;
                });
            }
        }
    }
}
