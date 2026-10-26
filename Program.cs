using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

class Program
{
    static void Main()
    {
        var path = @"C:\Users\dell 5490\.nuget\packages\microsoft.entityframeworkcore\8.0.26\lib\net8.0\Microsoft.EntityFrameworkCore.dll";
        var asm = Assembly.LoadFrom(path);
        var type = asm.GetType("Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder");
        Console.WriteLine(type == null ? "Type missing" : type.FullName);
        var methods = type?.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (methods == null)
            return;
        foreach (var method in methods.Where(m => m.Name == "HasTrigger"))
        {
            Console.WriteLine(method);
            Console.WriteLine(string.Join(", ", method.GetParameters().Select(p => p.ParameterType + " " + p.Name)));
        }
    }
}
