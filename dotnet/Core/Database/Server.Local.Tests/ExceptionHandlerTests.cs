// <copyright file="ExceptionHandlerTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Allors.Server;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class ExceptionHandlerTests
    {
        [Fact]
        public async Task UnhandledExceptionReachesTheApplicationLogger()
        {
            var loggerProvider = new RecordingLoggerProvider();
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddProvider(loggerProvider));
            services.AddMetrics();
            services.AddSingleton(new DiagnosticListener("Tests"));
            using var provider = services.BuildServiceProvider();

            var app = new ApplicationBuilder(provider);
            app.ConfigureExceptionHandler(new StubWebHostEnvironment());
            app.Run(_ => throw new InvalidOperationException("reaches-the-application-logger"));
            var pipeline = app.Build();

            var context = new DefaultHttpContext { RequestServices = provider };
            context.Response.Body = new MemoryStream();
            await pipeline(context);

            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
            Assert.Contains(loggerProvider.Errors, v => v.Message == "reaches-the-application-logger");
        }

        [Fact]
        public void ProductionMessageDoesNotLeakExceptionDetail()
        {
            var error = new Exception("secret-internal-detail");

            var message = ExceptionHandler.ErrorMessage(error, isDevelopment: false);

            Assert.DoesNotContain("secret-internal-detail", message);
        }

        [Fact]
        public void DevelopmentMessageIncludesExceptionDetail()
        {
            var error = new Exception("dev-visible-detail");

            var message = ExceptionHandler.ErrorMessage(error, isDevelopment: true);

            Assert.Contains("dev-visible-detail", message);
        }

        private sealed class RecordingLoggerProvider : ILoggerProvider
        {
            public ConcurrentQueue<Exception> Errors { get; } = new();

            public ILogger CreateLogger(string categoryName) => new RecordingLogger(this.Errors);

            public void Dispose()
            {
            }

            private sealed class RecordingLogger : ILogger
            {
                private readonly ConcurrentQueue<Exception> errors;

                public RecordingLogger(ConcurrentQueue<Exception> errors) => this.errors = errors;

                public IDisposable BeginScope<TState>(TState state) => null;

                public bool IsEnabled(LogLevel logLevel) => true;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                {
                    if (logLevel >= LogLevel.Error && exception != null)
                    {
                        this.errors.Enqueue(exception);
                    }
                }
            }
        }

        private sealed class StubWebHostEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get; set; }

            public IFileProvider WebRootFileProvider { get; set; }

            public string ApplicationName { get; set; } = "Allors.Tests";

            public IFileProvider ContentRootFileProvider { get; set; }

            public string ContentRootPath { get; set; } = Path.GetTempPath();

            public string EnvironmentName { get; set; } = "Production";
        }
    }
}
