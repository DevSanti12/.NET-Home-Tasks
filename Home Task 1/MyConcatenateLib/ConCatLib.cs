using System;

namespace MyConcatenateLib
{
    public static class ConCatLib
    {
        public static string ConcatenateLogic(string _input)
        {
            DateTime date = DateTime.Now;

            var _output = $"{date} Hello, {_input}!";

            return _output;
        }
    }
}
