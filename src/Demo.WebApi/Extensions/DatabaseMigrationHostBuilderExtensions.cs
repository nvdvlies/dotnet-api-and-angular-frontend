﻿using System;
using System.Linq;
using Demo.Domain.Role.Seed;
using Demo.Domain.User.Seed;
using Demo.Infrastructure.Persistence;
using Demo.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Demo.WebApi.Extensions;

public static class DatabaseMigrationHostBuilderExtensions
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DatabaseMigrationHostBuilderExtensions));
        try
        {
            var databaseExistedBeforeMigrate =
                (dbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator)!.Exists();

            logger.LogInformation("Retrieving pending database migrations.");

            var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();

            if (pendingMigrations.Any())
            {
                logger.LogInformation("Pending database migrations are available.");

                var lastAppliedMigration = dbContext.Database.GetAppliedMigrations().LastOrDefault();
                if (!string.IsNullOrEmpty(lastAppliedMigration))
                {
                    logger.LogInformation("Database currently on migration '{lastAppliedMigration}'.",
                        lastAppliedMigration);
                }

                logger.LogInformation("Applying {count} migration(s) ({names}).", pendingMigrations.Count,
                    string.Join(",", pendingMigrations));

                dbContext.Database.Migrate();

                lastAppliedMigration = dbContext.Database.GetAppliedMigrations().Last();

                logger.LogInformation("Database migrated to '{lastAppliedMigration}'.", lastAppliedMigration);

                if (!databaseExistedBeforeMigrate)
                {
                    logger.LogInformation("Database was newly created. Seeding with data.");

                    if (!dbContext.Roles.Any())
                    {
                        logger.LogInformation("Seeding default roles.");
                        dbContext.Roles.AddRange(AdministratorRole.Create(), UserRole.Create());
                    }

                    if (!dbContext.Users.Any())
                    {
                        logger.LogInformation("Seeding default user(s).");
                        dbContext.Users.Add(DefaultAdministratorUser.Create());
                    }

                    dbContext.SaveChanges();

                    logger.LogInformation("Finished seeding.");
                }

                var environmentSettings = scope.ServiceProvider.GetRequiredService<EnvironmentSettings>();
                if (!string.IsNullOrEmpty(environmentSettings.Redis.Host))
                {
                    logger.LogInformation("Clearing Redis cache.");
                    using var redisConnection = ConnectionMultiplexer.Connect(new ConfigurationOptions
                    {
                        EndPoints = { { environmentSettings.Redis.Host, environmentSettings.Redis.Port } },
                        AllowAdmin = true,
                        Password = environmentSettings.Redis.Password
                    });
                    foreach (var endPoint in redisConnection.GetEndPoints())
                    {
                        redisConnection.GetServer(endPoint).FlushDatabase();
                    }

                    logger.LogInformation("Finished clearing Redis cache.");
                }
            }
            else
            {
                logger.LogInformation("No pending database migrations available.");
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Database migration failed with error: {message}.", ex.Message);
            throw;
        }

        return host;
    }
}
