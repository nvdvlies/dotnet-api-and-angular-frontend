﻿import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiRolesClient } from '@api/api.generated.clients';
import { AuditlogBase, AuditlogContext, IAuditlog } from '@shared/base/auditlog-base';
import { UserLookupService } from '@shared/services/user-lookup.service';
import { BehaviorSubject, combineLatest, debounceTime, map, Observable, tap } from 'rxjs';

class ViewModel extends AuditlogContext {
  id: string | undefined;
}

@Component({
  selector: 'app-role-auditlog',
  templateUrl: './role-auditlog.component.html',
  styleUrls: ['./role-auditlog.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RoleAuditlogComponent extends AuditlogBase implements OnInit {
  public entityName = 'Role';
  protected searchFunction = (pageIndex?: number) => {
    return this.apiRolesClient.getRoleAuditlog(this.id.value!, pageIndex, this.pageSize).pipe(
      map((response) => {
        const auditlog: IAuditlog = {
          items: response?.auditlogs ?? [],
          ...response
        };
        return auditlog;
      })
    );
  };

  private readonly id = new BehaviorSubject<string | undefined>(undefined);

  protected id$ = this.id.asObservable();

  private vm: Readonly<ViewModel> | undefined;

  public vm$ = combineLatest([this.id$, this.context$]).pipe(
    debounceTime(0),
    map(([id, context]) => {
      const vm: ViewModel = {
        id,
        ...context
      };
      return vm;
    }),
    tap((vm) => (this.vm = vm))
  ) as Observable<ViewModel>;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly apiRolesClient: ApiRolesClient,
    userLookupService: UserLookupService
  ) {
    super(userLookupService);
  }

  public ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id == null) {
      throw new Error(`Couldn't find parameter 'id' in route parameters.`);
    }
    this.id.next(id);

    this.search();
  }
}
