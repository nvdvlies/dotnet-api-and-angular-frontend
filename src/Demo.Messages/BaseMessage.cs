﻿using System;

namespace Demo.Messages
{
    public class BaseMessage<T> : IMessage<T> where T : IMessageData
    {
        public string Type { get; private set; }
        public Topics Topic { get; private set; }
        public T Data { get; private set; }
        public DateTime CreatedOn { get; private set; }
        public string Subject { get; private set; }
        public string DataVersion { get; private set; }
        public string CorrelationId { get; private set; }

        public BaseMessage()
        {
        }

        public BaseMessage(Topics topic, T data, string subject, string dataVersion, string correlationId)
        {
            Type = GetType().FullName;
            Topic = topic;
            Data = data;
            CreatedOn = DateTime.UtcNow;
            Subject = subject;
            DataVersion = dataVersion;
            CorrelationId = correlationId;
        }
    }
}