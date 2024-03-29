﻿namespace Demo.Infrastructure.Settings;

public class EnvironmentSettings
{
    public string WebApiBaseUrl { get; set; }
    public string DefaultTimeZone { get; set; }
    public string DefaultCulture { get; set; }
    public Postgres Postgres { get; set; } = new();
    public Auth0 Auth0 { get; set; } = new();
    public RabbitMq RabbitMq { get; set; } = new();
    public Redis Redis { get; set; } = new();
    public ElasticSearch ElasticSearch { get; set; } = new();
    public Email Email { get; set; } = new();
}
