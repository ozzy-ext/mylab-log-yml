﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MyLab.Logging;

namespace MyLab.LogYml
{
    class YamlLogger : ILogger
    {
        private readonly ILogMessageQueue _logMessageQueue;
        private readonly List<object> _scopes = new List<object>();

        public string CategoryName { get; set; }

        public YamlLogger(ILogMessageQueue logMessageQueue)
        {
            _logMessageQueue = logMessageQueue;
        }

        public void Log<TState>(
            LogLevel logLevel, 
            EventId eventId, 
            TState state, 
            Exception exception, 
            Func<TState, Exception, string> formatter)
        {
            var msg = LogDtoConverter.Convert(logLevel, eventId, state, exception, formatter);
            var msgToWrite = new LogMessageToWrite(msg, logLevel);

            _logMessageQueue.Push(msgToWrite);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            _scopes.Add(state);
            return new ScopeRollback(_scopes, state);
        }
    }
}