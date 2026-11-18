/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using QuantConnect.DataProcessing;
using QuantConnect.Logging;

namespace QuantConnect.DataLibrary.Tests
{
    /// <summary>
    /// Tests the downloader's retry log against a stubbed API: the first request fails, the retry
    /// succeeds, and the logged line must say why the attempt failed and how long it took.
    /// </summary>
    [TestFixture]
    public class EIAElectricityDownloaderTests
    {
        private const string ApiKey = "SYNTHETICKEY123";
        private const string Url = "https://api.eia.gov/v2/electricity/rto/daily-region-data/data/?api_key=" + ApiKey;
        private const string Body = "{\"response\":{\"data\":[]}}";

        private ILogHandler _previousLogHandler;
        private TraceCollector _logs;

        [SetUp]
        public void SetUp()
        {
            _previousLogHandler = Log.LogHandler;
            _logs = new TraceCollector();
            Log.LogHandler = _logs;
        }

        [TearDown]
        public void TearDown()
        {
            Log.LogHandler = _previousLogHandler;
        }

        [Test]
        public void LogsStatusCodeOfFailedAttempt()
        {
            var line = FailOnceThenSucceed(TimeSpan.FromSeconds(120),
                (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.GatewayTimeout)));

            StringAssert.Contains("attempt 1 failed after 0s", line);
            StringAssert.Contains("HttpRequestException: ", line);
            StringAssert.Contains("504 (Gateway Timeout)", line);
            StringAssert.Contains("retrying in 4s", line);
        }

        [Test]
        public void LogsInnerExceptionOfRefusedConnection()
        {
            var line = FailOnceThenSucceed(TimeSpan.FromSeconds(120),
                (_, _) => throw new HttpRequestException(HttpRequestError.ConnectionError,
                    "Connection refused (api.eia.gov:443)", new SocketException((int)SocketError.ConnectionRefused)));

            StringAssert.Contains("attempt 1 failed after 0s", line);
            StringAssert.Contains("HttpRequestException: Connection refused (api.eia.gov:443)", line);
            StringAssert.Contains("SocketException: ", line);
            StringAssert.Contains("retrying in 4s", line);
        }

        [Test]
        public void LogsTimeoutAndHowLongTheAttemptTook()
        {
            var line = FailOnceThenSucceed(TimeSpan.FromSeconds(2),
                async (_, cancellationToken) =>
                {
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                    return null;
                });

            StringAssert.Contains("TaskCanceledException: ", line);
            StringAssert.Contains("TimeoutException: ", line);
            var seconds = int.Parse(Regex.Match(line, @"attempt 1 failed after (\d+)s").Groups[1].Value);
            Assert.GreaterOrEqual(seconds, 2);
            StringAssert.Contains("retrying in 4s", line);
        }

        /// <summary>
        /// Runs one request whose first attempt fails with <paramref name="firstAttempt"/> and whose
        /// retry succeeds, and returns the single retry line it logged.
        /// </summary>
        private string FailOnceThenSucceed(TimeSpan timeout,
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> firstAttempt)
        {
            var attempts = 0;
            var handler = new StubHandler((request, cancellationToken) =>
            {
                if (++attempts == 1)
                {
                    return firstAttempt(request, cancellationToken);
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Body) });
            });

            using var downloader = new EIAElectricityDownloader("unused", "unused", ApiKey,
                new HttpClient(handler) { Timeout = timeout });

            Assert.AreEqual(Body, downloader.GetWithRetry(Url));
            Assert.AreEqual(2, attempts);

            var retryLines = _logs.Traces.Where(trace => trace.Contains("GetWithRetry")).ToList();
            Assert.AreEqual(1, retryLines.Count, string.Join(Environment.NewLine, _logs.Traces));
            StringAssert.DoesNotContain(ApiKey, retryLines[0]);
            return retryLines[0];
        }

        private class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _send;

            public StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
            {
                _send = send;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return _send(request, cancellationToken);
            }
        }

        private class TraceCollector : ILogHandler
        {
            private readonly List<string> _traces = new();

            public IReadOnlyList<string> Traces
            {
                get
                {
                    lock (_traces)
                    {
                        return _traces.ToList();
                    }
                }
            }

            public void Trace(string text)
            {
                lock (_traces)
                {
                    _traces.Add(text);
                }
            }

            public void Debug(string text)
            {
            }

            public void Error(string text)
            {
            }

            public void Report(string text)
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
