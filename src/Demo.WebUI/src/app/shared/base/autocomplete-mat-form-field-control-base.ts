import {
  Component,
  Optional,
  ElementRef,
  Host,
  SkipSelf,
  Self,
  Output,
  EventEmitter,
  ViewChild,
  OnInit
} from '@angular/core';
import { ControlContainer, UntypedFormControl, NgControl } from '@angular/forms';
import { MatFormFieldControlBase } from '@shared/base/mat-form-field-control-base';
import { FocusMonitor } from '@angular/cdk/a11y';
import {
  BehaviorSubject,
  combineLatest,
  debounceTime,
  distinctUntilChanged,
  filter,
  finalize,
  map,
  Observable,
  takeUntil
} from 'rxjs';
import { MatOptionSelectionChange } from '@angular/material/core';
import { MatInput } from '@angular/material/input';

export interface ViewModel<T> {
  options: T[];
  isLookupOngoing: boolean;
  isSearching: boolean;
}

export interface IAutocompleteOption {
  id: string;
  label: string;
}

export class AutocompleteOption implements IAutocompleteOption {
  constructor(public id: string, public label: string) {}
}

@Component({
  template: ''
})
/* eslint-disable-next-line @angular-eslint/component-class-suffix */
export abstract class AutocompleteMatFormFieldControlBase<T extends IAutocompleteOption>
  extends MatFormFieldControlBase<string>
  implements OnInit
{
  @ViewChild(MatInput) matInput!: MatInput;

  @Output()
  public optionSelected = new EventEmitter<T>();

  @Output()
  public optionCleared = new EventEmitter();

  protected abstract searchFunction: (searchTerm: string | undefined) => Observable<T[]>;
  protected abstract lookupFunction: (id: string) => Observable<T | undefined>;

  private readonly options = new BehaviorSubject<T[]>([]);
  private readonly isLookupOngoing = new BehaviorSubject<boolean>(false);
  private readonly isSearching = new BehaviorSubject<boolean>(false);

  public options$ = this.options.asObservable();
  public isLookupOngoing$ = this.isLookupOngoing.asObservable();
  public isSearching$ = this.isSearching.asObservable();

  public vm$: Observable<ViewModel<T>> = combineLatest([
    this.options$,
    this.isLookupOngoing$,
    this.isSearching$
  ]).pipe(
    debounceTime(0),
    map(([options, isLookupOngoing, isSearching]) => {
      const vm: ViewModel<T> = {
        options,
        isLookupOngoing,
        isSearching
      };
      return vm;
    })
  );

  public searchFormControl = new UntypedFormControl(null);

  private _selectedOption: T | undefined;
  private get selectedOption(): T | undefined {
    return this._selectedOption;
  }
  private set selectedOption(value: T | undefined) {
    this._selectedOption = value;
  }

  constructor(
    @Optional() elementRef: ElementRef<HTMLElement>,
    @Optional() focusMonitor: FocusMonitor,
    @Host() @SkipSelf() controlContainer: ControlContainer,
    @Optional() @Self() ngControl: NgControl
  ) {
    super(elementRef, focusMonitor, controlContainer, ngControl);
  }

  public override ngOnInit(): void {
    super.ngOnInit();

    this.formControl.status === 'DISABLED'
      ? this.searchFormControl.disable()
      : this.searchFormControl.enable();
    this.formControl.statusChanges.subscribe((status) => {
      status === 'DISABLED' ? this.searchFormControl.disable() : this.searchFormControl.enable();
    });

    this.formControl.valueChanges
      .pipe(takeUntil(this.onDestroy$))
      .subscribe((value: string | null) => {
        if (value == null && this.searchFormControl.value != null) {
          this.searchFormControl.setValue(null);
        }
      });

    this.searchFormControl.valueChanges
      .pipe(
        takeUntil(this.onDestroy$),
        filter((value) => value != null),
        filter((value) => this.selectedOption == null || value !== this.selectedOption.label),
        debounceTime(350),
        distinctUntilChanged()
      )
      .subscribe((searchTerm) => {
        this.search(searchTerm);
      });
  }

  public search(searchTerm: string | undefined): void {
    this.isSearching.next(true);
    this.searchFunction(searchTerm)
      .pipe(finalize(() => this.isSearching.next(false)))
      .subscribe((result) => {
        this.options.next(result ?? []);
      });
  }

  private lookup(id: string): void {
    this.isLookupOngoing.next(true);
    this.lookupFunction(id)
      .pipe(finalize(() => this.isLookupOngoing.next(false)))
      .subscribe((option) => {
        if (option && this.value != null) {
          this.selectedOption = option;
          this.searchFormControl.setValue(this.selectedOption.label);
        }
      });
  }

  public onFocus(): void {
    if (!this.searchFormControl.value || this.searchFormControl.value.length === 0) {
      this.search(undefined);
    }
  }

  public onBlur(): void {
    if (this.selectedOption && this.searchFormControl.value !== this.selectedOption.label) {
      if (this.searchFormControl.value && this.searchFormControl.value.length > 0) {
        this.searchFormControl.setValue(this.selectedOption.label);
      } else {
        this.selectedOption = undefined;
        this.value = null;
      }
    }
    if (
      !this.selectedOption &&
      this.searchFormControl.value &&
      this.searchFormControl.value.length > 0
    ) {
      this.searchFormControl.setValue(null);
    }
  }

  public onSelect(event: MatOptionSelectionChange, option: T): void {
    if (event.isUserInput) {
      this.selectedOption = option;
      this.searchFormControl.setValue(option.label, { emitEvent: false });
      this.value = option.id;
      this.optionSelected.emit(this.selectedOption);
    }
  }

  public clearSearchField(): void {
    this.selectedOption = undefined;
    this.value = null;
    this.optionCleared.next(null);
  }

  public focus(): void {
    this.matInput.focus();
  }

  public override get shouldLabelFloat() {
    return super.shouldLabelFloat || this.isLookupOngoing.value;
  }

  public override writeValue(value: any): void {
    super.writeValue(value);
    if (value != null && this.selectedOption == null) {
      this.lookup(value);
    }
  }
}
