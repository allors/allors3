// <copyright file="MailKitMailerTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Buffers;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Database.Configuration;
    using MimeKit;
    using SmtpServer;
    using SmtpServer.ComponentModel;
    using SmtpServer.Protocol;
    using SmtpServer.Storage;
    using Xunit;

    public class MailKitMailerTests : DomainTest, IClassFixture<Fixture>
    {
        public MailKitMailerTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ParseSmtpDefaultsToPort25()
        {
            var (host, port) = MailKitMailer.ParseSmtp("mail.example.com");
            Assert.Equal("mail.example.com", host);
            Assert.Equal(25, port);
        }

        [Fact]
        public void ParseSmtpReadsHostAndPort()
        {
            var (host, port) = MailKitMailer.ParseSmtp("localhost:1025");
            Assert.Equal("localhost", host);
            Assert.Equal(1025, port);
        }

        [Fact]
        public void SendDeliversTheMessageToTheSmtpServer()
        {
            var port = FreeTcpPort();
            var store = new CapturingMessageStore();

            var options = new SmtpServerOptionsBuilder()
                .ServerName("localhost")
                .Endpoint(endpoint => endpoint.Port(port).AllowUnsecureAuthentication(true))
                .Build();

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(store);

            var server = new global::SmtpServer.SmtpServer(options, serviceProvider);
            _ = server.StartAsync(CancellationToken.None);

            try
            {
                WaitUntil(() => CanConnect(port));

                var message = new EmailMessageBuilder(this.Transaction)
                    .WithRecipientEmailAddress("jane@example.com")
                    .WithSubject("Reset your password")
                    .WithBody("<a href=\"https://example.com/reset?code=abc\">Reset</a>")
                    .Build();
                this.Transaction.Derive();
                this.Transaction.Commit();

                var mailer = new MailKitMailer { Smtp = $"127.0.0.1:{port}" };
                mailer.Send(message, "noreply@example.com");

                WaitUntil(() => store.Received.Count >= 1);

                var captured = Assert.Single(store.Received);
                Assert.Equal("Reset your password", captured.Subject);
                Assert.Contains("jane@example.com", captured.To.ToString());
                Assert.Contains("reset?code=abc", captured.HtmlBody);
            }
            finally
            {
                server.Shutdown();
            }
        }

        private static int FreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static bool CanConnect(int port)
        {
            try
            {
                using var client = new TcpClient();
                client.Connect(IPAddress.Loopback, port);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        private static void WaitUntil(Func<bool> condition)
        {
            for (var i = 0; i < 200; i++)
            {
                if (condition())
                {
                    return;
                }

                Thread.Sleep(25);
            }

            throw new TimeoutException("Timed out waiting for the SMTP server.");
        }

        private class CapturingMessageStore : MessageStore
        {
            public ConcurrentQueue<MimeMessage> Received { get; } = new ConcurrentQueue<MimeMessage>();

            public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
            {
                using var stream = new MemoryStream();
                foreach (var segment in buffer)
                {
                    await stream.WriteAsync(segment, cancellationToken);
                }

                stream.Position = 0;
                this.Received.Enqueue(await MimeMessage.LoadAsync(stream, cancellationToken));

                return SmtpResponse.Ok;
            }
        }
    }
}
