using System;

namespace UCExample
{
    public class ExampleReactingObject
    {
        public string Name { get; set; }
        public void ExampleMethod(string message)
        {
            Console.WriteLine($"{Name} - Example method called with message {message}.");
        }
    }
}
