using System;

namespace Common.Extensions
{
    public static class TypeExtensions
    {
        public static string GetAssemblyPath(this Type source)
        {
            return new Uri(source.Assembly.EscapedCodeBase).LocalPath;
        }
    }
}