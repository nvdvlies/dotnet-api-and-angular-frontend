﻿using System.Collections.Generic;
using Demo.Scaffold.Tool.Changes;
using Demo.Scaffold.Tool.Helpers;
using Demo.Scaffold.Tool.Interfaces;

namespace Demo.Scaffold.Tool.Scaffolders.OutputCollectors.DomainEntity.OutputCollectors;

internal class CreateDomainEntityOutputCollector : IOutputCollector
{
    public IEnumerable<IChange> CollectChanges(ScaffolderContext context)
    {
        var changes = new List<IChange>();

        var entityName = context.Variables.Get<string>(Constants.EntityName);

        changes.Add(new CreateNewClass(
            context.GetEntityDirectory(entityName),
            $"{entityName}DomainEntity.cs",
            GetTemplate(entityName)
        ));

        return changes;
    }

    private static string GetTemplate(string entityName)
    {
        var code = @"
using System;
using Demo.Common.Interfaces;
using Demo.Domain.Shared.DomainEntity;
using Demo.Domain.%ENTITY%.Interfaces;
using Demo.Domain.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Demo.Domain.%ENTITY%
{
    internal class %ENTITY%DomainEntity : DomainEntity<%ENTITY%>, I%ENTITY%DomainEntity
    {
        public %ENTITY%DomainEntity(
            ILogger<%ENTITY%DomainEntity> logger,
            ICurrentUserIdProvider currentUserIdProvider,
            IDateTime dateTime,
            IDbCommand<%ENTITY%> dbCommand, 
            Lazy<IEnumerable<IDefaultValuesSetter<%ENTITY%>>> defaultValuesSetters, 
            Lazy<IEnumerable<IValidator<%ENTITY%>>> validators, 
            Lazy<IEnumerable<IBeforeCreate<%ENTITY%>>> beforeCreateHooks, 
            Lazy<IEnumerable<IAfterCreate<%ENTITY%>>> afterCreateHooks,
            Lazy<IEnumerable<IBeforeUpdate<%ENTITY%>>> beforeUpdateHooks,
            Lazy<IEnumerable<IAfterUpdate<%ENTITY%>>> afterUpdateHooks,
            Lazy<IEnumerable<IBeforeDelete<%ENTITY%>>> beforeDeleteHooks,
            Lazy<IEnumerable<IAfterDelete<%ENTITY%>>> afterDeleteHooks,
            Lazy<IOutboxEventCreator> outboxEventCreator,
            Lazy<IOutboxMessageCreator> outboxMessageCreator,
            Lazy<IJsonService<%ENTITY%>> jsonService,
            Lazy<IAuditlogger<%ENTITY%>> auditlogger
        ) 
            : base(logger, currentUserIdProvider, dateTime, dbCommand, defaultValuesSetters, validators, beforeCreateHooks, afterCreateHooks, beforeUpdateHooks, afterUpdateHooks, beforeDeleteHooks, afterDeleteHooks, outboxEventCreator, outboxMessageCreator, jsonService, auditlogger)
        {
        }
    }
}
";
        code = code.Replace("%ENTITY%", entityName);
        return code;
    }
}
