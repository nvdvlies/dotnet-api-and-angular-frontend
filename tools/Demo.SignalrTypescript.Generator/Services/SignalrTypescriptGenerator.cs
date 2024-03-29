﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using Demo.Application.Shared.Interfaces;
using Demo.SignalrTypescript.Generator.Extensions;
using Demo.SignalrTypescript.Generator.Models;

namespace Demo.SignalrTypescript.Generator.Services;

internal class SignalrTypescriptGenerator
{
    private readonly AppSettings _appSettings;

    public SignalrTypescriptGenerator(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    private static string BaseTemplate => @"
//----------------------
// <auto-generated>
//     Generated using Demo.SignalrTypescript.Generator
// </auto-generated>
//----------------------
import { Injectable, Inject, InjectionToken } from '@angular/core';
import { HubConnection, IHttpConnectionOptions } from '@microsoft/signalr';
import { lastValueFrom, Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from '@auth0/auth0-angular';
import { LoggerService } from '@shared/services/logger.service';

export const SIGNALR_BASE_URL = new InjectionToken<string>('SIGNALR_BASE_URL');

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  public hubConnection: HubConnection;

  constructor(
    @Inject(SIGNALR_BASE_URL) private readonly baseUrl: string,
    private readonly authService: AuthService,
    private readonly loggerService: LoggerService
  ) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.baseUrl + '/hub', {
        accessTokenFactory: () => lastValueFrom(this.authService.getAccessTokenSilently())
      } as IHttpConnectionOptions)
      .withAutomaticReconnect([0, 2000, 5000, ...Array(60).fill(10000), ...Array(60).fill(60000)])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.authService.isAuthenticated$.subscribe((isAuthenticated) =>
      isAuthenticated ? this.connect() : this.disconnect()
    );
  }

  private connect(): void {
    this.hubConnection
      .start()
      .catch((err) => this.loggerService.logError('Error while starting SignalR connection', undefined, err));
  }

  private disconnect(): void {
    if (this.hubConnection) {
      this.hubConnection
        .stop()
        .catch((err) => this.loggerService.logWarning(`Error while terminating SignalR connection: ${err}`));
    }
  }
}

";

    public void Run()
    {
        var eventHubInterface = typeof(IEventHub);
        var eventHubInfo = new EventHubInfo(eventHubInterface);

        var sb = new StringBuilder();
        sb.Append(BaseTemplate);

        foreach (var eventHub in eventHubInfo.EventHubs)
        {
            foreach (var @event in eventHub.Events)
            {
                sb.AppendLine($"export interface {@event.ClassName} {{");
                foreach (var parameter in @event.Parameters)
                {
                    sb.AppendLine($"  {parameter.Name}: {parameter.TypescriptType};");
                }

                sb.AppendLine("}");
                sb.AppendLine("");
            }

            sb.AppendLine("@Injectable({");
            sb.AppendLine("  providedIn: 'root'");
            sb.AppendLine("})");
            sb.AppendLine($"export class {eventHub.ClassName}Service {{");

            foreach (var @event in eventHub.Events)
            {
                sb.AppendLine($"  private {@event.EventName.ToCamelCase()} = new Subject<{@event.ClassName}>();");
            }

            sb.AppendLine("");

            foreach (var @event in eventHub.Events)
            {
                sb.AppendLine(
                    $"  public {@event.EventName.ToCamelCase()}$ = this.{@event.EventName.ToCamelCase()}.asObservable();");
            }

            sb.AppendLine("");

            sb.AppendLine("  constructor(private signalRService: SignalRService) {");
            foreach (var @event in eventHub.Events)
            {
                sb.AppendLine(
                    @$"    this.signalRService.hubConnection.on('{@event.EventName}', ({string.Join(", ", @event.Parameters.Select(x => string.Concat(x.Name, ": ", x.TypescriptType)))}) =>");
                sb.AppendLine(
                    @$"      this.{@event.EventName.ToCamelCase()}.next({{ {string.Join(", ", @event.Parameters.Select(x => x.Name))} }})");
                sb.AppendLine("    );");
            }

            sb.AppendLine("  }");

            sb.AppendLine("}");
            sb.AppendLine("");
        }

        File.WriteAllText(Path.Combine(_appSettings.TargetDirectory, Constants.TargetFileName), sb.ToString());

        Console.WriteLine("Done.");
    }
}
