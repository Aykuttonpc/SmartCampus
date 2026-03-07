using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        string hash = "$2a$11$X4KkljUL4yRrD1wxG8hxTeB9yBFoPhhElqMUTsfacZCcwGi5E7HNu";
        bool result = BCrypt.Net.BCrypt.Verify("Campus123!", hash);
        Console.WriteLine($"Verification result: {result}");
    }
}
