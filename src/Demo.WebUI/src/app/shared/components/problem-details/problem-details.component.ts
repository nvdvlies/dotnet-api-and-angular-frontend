import { Component, ChangeDetectionStrategy, Input, OnInit, Optional } from '@angular/core';
import { ControlContainer, UntypedFormControl, UntypedFormGroup } from '@angular/forms';
import { ApiException, ProblemDetails, ValidationProblemDetails } from '@api/api.generated.clients';
import { PermissionDeniedError } from '@shared/errors/permission-denied.error';
import { LoggerService } from '@shared/services/logger.service';

@Component({
  selector: 'app-problem-details',
  templateUrl: './problem-details.component.html',
  styleUrls: ['./problem-details.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProblemDetailsComponent implements OnInit {
  private _problemDetails:
    | ValidationProblemDetails
    | ProblemDetails
    | ApiException
    | PermissionDeniedError
    | undefined;
  public get problemDetails():
    | ValidationProblemDetails
    | ProblemDetails
    | ApiException
    | PermissionDeniedError
    | undefined {
    return this._problemDetails;
  }
  @Input() public set problemDetails(
    value:
      | ValidationProblemDetails
      | ProblemDetails
      | ApiException
      | PermissionDeniedError
      | undefined
  ) {
    this._problemDetails = value;
    this.setErrorMessage();
  }

  public errorMessages: string[] = [];
  private form: UntypedFormGroup | undefined;

  constructor(
    @Optional() private readonly controlContainer: ControlContainer,
    private readonly loggerService: LoggerService
  ) {}

  ngOnInit(): void {
    this.form = this.controlContainer?.control as UntypedFormGroup;
  }

  private setErrorMessage(): void {
    this.errorMessages = [];

    if (!this.problemDetails) {
      return;
    }

    if (this.problemDetails instanceof ValidationProblemDetails) {
      if (this.problemDetails.errors) {
        for (const key in this.problemDetails.errors) {
          const path = this.getFormControlPath(key);
          const formControl = this.form?.get(path);
          if (formControl && formControl instanceof UntypedFormControl && formControl.enabled) {
            formControl.setErrors({
              serverError: this.problemDetails.errors[key][0]
            });
          } else {
            for (const error of this.problemDetails.errors[key]) {
              this.errorMessages.push(error);
            }
          }
        }
      } else {
        this.errorMessages.push(this.problemDetails.title!);
      }
    } else if (this.problemDetails instanceof ProblemDetails) {
      this.errorMessages.push(this.problemDetails.title!);
    } else if (this.problemDetails instanceof PermissionDeniedError) {
      this.errorMessages.push('Permission denied.');
    } else if (this.problemDetails instanceof ApiException) {
      switch (this.problemDetails.status) {
        case 403:
          this.errorMessages.push('Permission denied error.');
          break;
        default:
          this.errorMessages.push(this.problemDetails.message);
          break;
      }
    } else {
      this.loggerService.logError('Unhandled ProblemDetails', undefined, this.problemDetails);
      this.errorMessages.push('An unknown exception occured.');
    }
  }

  /*
    - InvoiceNumber -> ['invoiceNumber']
    - Address.PostalCode -> ['address', 'postalCode']
    - InvoiceLines[0].SellingPrice -> ['invoiceLines', 0, 'sellingPrice']
  */
  private getFormControlPath(propertyName: string): Array<string | number> {
    const result: Array<string | number> = [];
    if (!propertyName) {
      return result;
    }
    const parts = propertyName.split('.');
    for (let part of parts) {
      part = part.charAt(0).toLowerCase() + part.slice(1);
      const index = /\[(?<index>\d+)\]$/.exec(part)?.groups?.['index'];
      if (index != null) {
        result.push(part.substring(0, part.indexOf('[')));
        result.push(index);
      } else {
        result.push(part);
      }
    }
    return result;
  }
}
