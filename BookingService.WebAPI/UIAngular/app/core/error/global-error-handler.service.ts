import { ErrorHandler, Injectable, Injector } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { LoggingService } from './service/logging-service.service';
import { ErrorService } from './service/error-service.service';
import { ToastService } from './service/notification-service.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {

  constructor(private injector: Injector) { }

  handleError(error: Error | HttpErrorResponse) {
    const errorService = this.injector.get(ErrorService);
    const logger = this.injector.get(LoggingService);
    const notifier = this.injector.get(ToastService);

    let message;

    if (error instanceof HttpErrorResponse) {
      // Server error
      message = errorService.getServerErrorMessage(error);

      notifier.showError(message,'Error','top-center');
    } else {
      // Client Error
      message = errorService.getClientErrorMessage(error);
      //todo
     // notifier.showError(message,'Error','top-center');
    }
    // Always log errors
    logger.logError(message);
    console.error(error);
  }
}
