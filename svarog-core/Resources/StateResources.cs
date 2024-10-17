
using Stateless;

namespace svarog
{
    public partial class Resources
    {
        private Dictionary<string, object> StateMachines = [];
        private Dictionary<string, (string, string)> MachineDescriptors = [];

        public StateMachine<K, V> CreateStateMachine<K, V>(string name, K defaultState)
        {
            StateMachine<K, V> machine = new(defaultState);
            StateMachines[name] = machine;
            MachineDescriptors[name] = (typeof(K).Name, typeof(V).Name);
            return machine;
        }

        public StateMachine<K, V>? GetStateMachine<K, V>(string name)
        {
            var kt = typeof(K).Name;
            var vt = typeof(V).Name;

            if (MachineDescriptors.TryGetValue(name, out (string, string) kv))
            {
                if (kt != kv.Item1 && vt != kv.Item2)
                {
                    Console.WriteLine($"Error: Trying to fetch statemachine \"{name}\" ({kv.Item1} => {kv.Item2}) as ({kt} => {vt}).");
                    return null;
                }
                else
                {
                    return StateMachines[name] as StateMachine<K, V>;
                }
            }
            else
            {
                return null;
            }
        }
    }
}