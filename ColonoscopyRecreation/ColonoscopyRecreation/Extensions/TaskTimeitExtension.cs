using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.Extensions
{
    public static class TaskTimeitExtension
    {
        public static async Task<TimeSpan> TimeIt(this Task task)
        {
            DateTime before = DateTime.Now;
            task.Wait();
            return DateTime.Now.Subtract(before);
        }
        public static async Task<(T, TimeSpan)> TimeIt<T>(this Task<T> task) 
        {
            DateTime before = DateTime.Now;
            T ret = task.Result;
            return (ret, DateTime.Now.Subtract(before));
        }
    }
}
