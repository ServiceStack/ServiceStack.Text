using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceStack.Text.TestsConsole
{
    public class T
    {
        public Guid X { get; set; }
        public char Y { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var violations = 0;
            const int count = 1000 * 1000;
            var json = new List<string>();
            var serializer = new JsonSerializer<T>();
            for (int i = 0; i < count; i++)
            {
                var t = new T
                {
                    X = Guid.NewGuid(),
                    Y = i % 2 == 0 ? 'C' : 'P',
                };
                json.Add(serializer.SerializeToString(t));
            }

            var tasks = new List<Task>();
            var tasksCount = args.Length > 0 ? int.Parse(args[0]) : 3;
            for (int jj = 0; jj < tasksCount; jj++)
            {
                int j = jj;
                tasks.Add(Task.Run(() => {
                    for (int i = 0; i < count; i++)
                    {
                        string s = json[i];
                        var t = serializer.DeserializeFromString(s);
                        if (t.Y != (i % 2 == 0 ? 'C' : 'P'))
                        {
                            violations++;
                            Console.WriteLine("Constraint violation index {0} thread {1} expected: {2} received: {3} json: {4}",
                                i, j, i % 2 == 0 ? 'C' : 'P', t.Y, s);
                        }
                    }
                }));
            }
            tasks.ForEach(task => task.Wait());

            Console.WriteLine($"There were {violations} viloations, running {tasksCount} Tasks");
        }
    }
}
