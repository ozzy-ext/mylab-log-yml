﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.Logging;
using Xunit;
using Xunit.Abstractions;

namespace MyLab.LogYml.Tests
{
    public class AsyncLoggingBehavior
    {
        private readonly ITestOutputHelper _output;

        public AsyncLoggingBehavior(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ShouldStoreAndWriteLogMessage()
        {
            //Arrange

            LogMessageToWrite origin = new LogMessageToWrite(null, LogLevel.None);
            LogMessageToWrite got = null;

            var logWriterMock = new Mock<ILogMessageWriter>();
            logWriterMock.Setup(w => w.WriteMessageAsync(It.IsAny<IEnumerable<LogMessageToWrite>>(), It.IsAny<CancellationToken>()))
                .Returns((Func<IEnumerable<LogMessageToWrite>, CancellationToken, Task>) ((e, t) =>
                {
                    return Task.Run(() =>
                    {
                        got = e?.FirstOrDefault();
                    }, t);
                }));

            var al = new AsyncLogging(logWriterMock.Object, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));

            //Act
            try
            {
                al.Push(origin);
                al.Start();

                Thread.Sleep(200);
            }
            finally 
            {
                al.Dispose();
            }

            //Assert
            Assert.Equal(origin, got);
        }

        [Fact]
        public void ShouldWriteLogMessagesWhenDispose()
        {
            //Arrange

            LogMessageToWrite origin = new LogMessageToWrite(null, LogLevel.None);
            LogMessageToWrite got = null;

            var logWriterMock = new Mock<ILogMessageWriter>();
            logWriterMock.Setup(w => w.WriteMessageAsync(It.IsAny<IEnumerable<LogMessageToWrite>>(), It.IsAny<CancellationToken>()))
                .Returns((Func<IEnumerable<LogMessageToWrite>, CancellationToken, Task>)((e, t) =>
                {
                    return Task.Run(() =>
                    {
                        got = e?.FirstOrDefault();
                    }, t);
                }));

            var al = new AsyncLogging(logWriterMock.Object, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));

            //Act
            try
            {
                al.Start();
                Thread.Sleep(150);
                al.Push(origin);
            }
            finally
            {
                al.Dispose();
            }

            //Assert
            Assert.Equal(origin, got);
        }
    }
}
