using System;
using DotNetEnv;

namespace Microsoft.AspNetCore.Builder
{
    public static class StaticFileExtensions
    {
        public static IApplicationBuilder UseDotNetEnv(this IApplicationBuilder app) 
        {
            Env.Load();
            return app;
        }

        public static IApplicationBuilder UseDotNetEnv(this IApplicationBuilder app, string path) 
        {
            Env.Load(path);
            return app;
        }
    }
}
