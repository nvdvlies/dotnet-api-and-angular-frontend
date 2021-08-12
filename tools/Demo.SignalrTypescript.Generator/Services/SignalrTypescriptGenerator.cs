﻿using Demo.Application.Shared.Interfaces;
using Demo.SignalrTypescript.Generator.Models;
using System;

namespace Demo.SignalrTypescript.Generator.Services
{
    internal class SignalrTypescriptGenerator
    {
        private readonly AppSettings _appSettings;

        public SignalrTypescriptGenerator(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public void Run()
        {
            var eventHubInterface = typeof(IEventHub);
            var eventHubInfo = new EventHubInfo(eventHubInterface);
            // convert to template
            // save to file

            foreach (var eventHub in eventHubInfo.EventHubs)
            {
                Console.WriteLine($"{eventHub.ClassName}");
                foreach (var @event in eventHub.Events)
                {
                    Console.WriteLine($"  {@event.ClassName}");
                    Console.WriteLine($"  {@event.EventName}");
                    foreach (var parameter in @event.Parameters)
                    {
                        Console.WriteLine($"    {parameter.Name}");
                        Console.WriteLine($"    {parameter.CSharpType}");
                        Console.WriteLine($"    {parameter.TypescriptType}");
                    }
                }
            }
            Console.ReadLine();
        }

        public static string BaseTemplate => @"
//----------------------
// <auto-generated>
//     Generated using Demo.SignalrTypescript.Generator
// </auto-generated>
//----------------------

import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';

import { Subject } from 'rxjs';
import { HubConnection } from '@microsoft/signalr';
import * as signalR from '@microsoft/signalr';

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  public hubConnection: HubConnection;

  constructor(@Inject(API_BASE_URL) baseUrl: string) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(baseUrl + '/hub')
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection
      .start()
      .catch(err => console.log('Error while starting connection: ' + err));
  }
}
";

        public static string EventHubTemplate => @"
export class CustomerUpdatedEvent {
  id: string;
  updatedBy: string;
}

export class CustomerDeletedEvent {
  id: string;
  deletedBy: string;
}

@Injectable({
  providedIn: 'root'
})
export class CustomerEvents {
  private customerUpdated = new Subject<CustomerUpdatedEvent>();
  private customerDeleted = new Subject<CustomerDeletedEvent>();

  public customerUpdated$ = this.customerUpdated.asObservable();
  public customerDeleted$ = this.customerDeleted.asObservable();

  constructor(private signalRService: SignalRService) {
    this.signalRService.hubConnection.on('CustomerUpdated', (id: string, updatedBy: string) => this.customerUpdated.next({ id, updatedBy }));
	this.signalRService.hubConnection.on('CustomerDeleted', (id: string, deletedBy: string) => this.customerDeleted.next({ id, deletedBy }));
  }
}
";
    }
}