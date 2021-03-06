﻿using BunqDownloader.Bunq;
using Microsoft.AspNetCore.Http;
using Requestr.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Requestr.Middleware
{
    public class RestoreContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly BunqInitializer initializer;

        private static readonly object _lock = new object();
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public RestoreContextMiddleware(BunqInitializer initializer, RequestDelegate next)
        {
            _next = next;
            this.initializer = initializer;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Unfortunately the Bunq SDK uses a singleton context.
            // Therefore only one request at a time can be processed.
            await semaphoreSlim.WaitAsync();
            try
            {
                var userId = context.User.Claims.Where(c => c.Type == TokenService.UserIdClaim).SingleOrDefault()?.Value;
                if (userId != null)
                {
                    try
                    {
                        this.initializer.RestoreApiContext(Guid.Parse(userId));
                    }
                    catch(ArgumentException)
                    {
                        var exemptPaths = new string[]
                        {
                            "users/apikey",
                            "users/token"
                        };
                        if (!exemptPaths.Any(context.Request.Path.Value.EndsWith))
                            throw;
                    }
                }

                await _next(context);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
