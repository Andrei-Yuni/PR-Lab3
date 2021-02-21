using System;
using System.Threading.Tasks;

namespace Lab3
{
    class MenuAction
    {
        public string Name { get; set; }
        public Func<ValueTask> Method { get; set; }
    }
}
