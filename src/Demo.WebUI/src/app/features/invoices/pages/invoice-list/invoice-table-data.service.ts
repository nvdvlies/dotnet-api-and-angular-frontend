import { Injectable } from '@angular/core';
import { FormControl } from '@angular/forms';
import { combineLatest, debounceTime, map, Observable, of, switchMap } from 'rxjs';
import {
  ApiInvoicesClient,
  SearchInvoiceDto,
  SearchInvoicesOrderByEnum
} from '@api/api.generated.clients';
import { InvoiceEventsService } from '@api/signalr.generated.services';
import { InvoiceStoreService } from '@domain/invoice/invoice-store.service';
import {
  IEntityUpdatedEvent,
  ITableFilterCriteria,
  TableDataBase,
  TableDataContext,
  TableDataSearchResult,
  TableFilterCriteria
} from '@shared/base/table-data-base';

export declare type InvoiceSortColumn = 'InvoiceNumber' | 'InvoiceDate' | undefined;

export class InvoiceTableFilterCriteria
  extends TableFilterCriteria
  implements ITableFilterCriteria
{
  override sortColumn: InvoiceSortColumn;

  constructor() {
    super();
    this.sortColumn = 'InvoiceNumber';
    this.sortDirection = 'desc';
  }
}
export interface InvoiceTableDataContext extends TableDataContext<SearchInvoiceDto> {
  criteria: InvoiceTableFilterCriteria | undefined;
}

@Injectable()
export class InvoiceTableDataService extends TableDataBase<SearchInvoiceDto> {
  public searchTerm = new FormControl();

  public observe$: Observable<InvoiceTableDataContext> = combineLatest([
    this.observeInternal$
  ]).pipe(
    debounceTime(0),
    map(([baseContext]) => {
      const context: InvoiceTableDataContext = {
        ...baseContext,
        criteria: baseContext.criteria as InvoiceTableFilterCriteria
      };
      return context;
    })
  );

  protected entityUpdatedInStore$ = this.invoiceStoreService.invoiceUpdatedInStore$.pipe(
    map(
      ([event, entity]) => [event, { ...entity, customerName: null }] as [IEntityUpdatedEvent, any]
    )
  );
  protected entityDeletedEvent$ = this.invoiceEventsService.invoiceDeleted$;

  constructor(
    private readonly apiInvoicesClient: ApiInvoicesClient,
    private readonly invoiceStoreService: InvoiceStoreService,
    private readonly invoiceEventsService: InvoiceEventsService
  ) {
    super();
    this.init(new InvoiceTableFilterCriteria());
    this.search();
  }

  protected searchFunction = (criteria: InvoiceTableFilterCriteria) => {
    return this.apiInvoicesClient
      .search(
        this.sortColumn(criteria.sortColumn),
        this.sortbyDescending(criteria.sortDirection),
        criteria.pageIndex,
        criteria.pageSize,
        this.searchTerm.value
      )
      .pipe(
        map((response) => {
          const result: TableDataSearchResult<SearchInvoiceDto> = {
            items: response.invoices,
            pageIndex: response.pageIndex,
            pageSize: response.pageSize,
            totalItems: response.totalItems,
            totalPages: response.totalPages,
            hasPreviousPage: response.hasPreviousPage,
            hasNextPage: response.hasNextPage
          };
          return result;
        })
      );
  };

  private sortColumn(sortColumn: InvoiceSortColumn): SearchInvoicesOrderByEnum | undefined {
    switch (sortColumn) {
      case undefined:
        return undefined;
      case 'InvoiceNumber':
        return SearchInvoicesOrderByEnum.InvoiceNumber;
      case 'InvoiceDate':
        return SearchInvoicesOrderByEnum.InvoiceDate;
      default: {
        const exhaustiveCheck: never = sortColumn; // force compiler error on missing options
        throw new Error(exhaustiveCheck);
      }
    }
  }

  protected getByIdFunction = (id: string) =>
    this.invoiceStoreService.getById(id).pipe(
      map((invoice) => {
        return new SearchInvoiceDto(invoice);
      })
    );
}
