using System;

namespace MongoTest2
{
    public class EndMethodHelper
    {
        public static void EndMethod(string currentMethod)
        {
            Console.WriteLine("\n=============== Method: {0}() ====================\n", currentMethod);
        }
    }
}
