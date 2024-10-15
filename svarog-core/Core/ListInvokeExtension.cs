using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace svarog
{
    internal static class ListInvokeExtension
    {
        public static void Invoke(this List<RPlugin> list, Svarog instance)
        {
            foreach (var plugin in list.OrderBy(o => o.Priority))
            {
                plugin.Act(instance);
            }
        }

        public static void AddInvocation(this List<RPlugin> list, RPlugin plugin)
        {
            list.Add(plugin);
            list = [.. list.OrderBy(o => o.Priority)];
        }

        public static void RemoveInvocation(this List<RPlugin> list, Action<Svarog> action)
        {
            list.RemoveAll(o => o.Act == action);
            list = [.. list.OrderBy(o => o.Priority)];
        }
    }
}
