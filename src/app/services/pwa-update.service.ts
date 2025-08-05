import { Injectable, ComponentRef, OnDestroy } from '@angular/core';
import { SwUpdate, VersionReadyEvent } from '@angular/service-worker';
import { filter, takeUntil } from 'rxjs/operators';
import { interval, Subject } from 'rxjs';
import { CustomToastService } from './custom-toast.service';
import { UpdateToastComponent } from '../components/update-toast/update-toast.component';

@Injectable({
  providedIn: 'root'
})
export class PwaUpdateService implements OnDestroy {
  private destroyToast$ = new Subject<void>();
  private toastComponentRef?: ComponentRef<UpdateToastComponent>;

  constructor(
    private swUpdate: SwUpdate,
    private toastService: CustomToastService
  ) { }

  public initializeUpdateChecking(): void {
    if (!this.swUpdate.isEnabled) {
      return;
    }

    this.swUpdate.versionUpdates.pipe(
      filter((evt): evt is VersionReadyEvent => evt.type === 'VERSION_READY')
    ).subscribe(() => {
      if (this.toastComponentRef) {
        return;
      }
      this.promptUserToUpdate();
    });
  }

  public promptUserToUpdate(): void {
    this.toastService.clearAll();

    let remainingSeconds = 15;
    const initialMessage = `Sayfa ${remainingSeconds} saniye içinde yeniden yüklenecek.`;

    this.toastService.showUpdate(initialMessage)
      .pipe(takeUntil(this.destroyToast$))
      .subscribe(componentRef => {
        this.toastComponentRef = componentRef;

        this.toastComponentRef.instance.updateClick.pipe(
          takeUntil(this.destroyToast$)
        ).subscribe(() => {
          this.stopCountdownAndDestroyToast();
          this.activateUpdate();
        });

        this.toastComponentRef.instance.cancelClick.pipe(
          takeUntil(this.destroyToast$)
        ).subscribe(() => {
          this.stopCountdownAndDestroyToast();
        });

        // Geri sayımı başlat.
        interval(1000).pipe(
          takeUntil(this.destroyToast$)
        ).subscribe(() => {
          remainingSeconds--;
          if (remainingSeconds > 0) {
            if (this.toastComponentRef) {
              this.toastComponentRef.instance.message = `Sayfa ${remainingSeconds} saniye içinde yeniden yüklenecek.`;
            }
          } else {
            this.stopCountdownAndDestroyToast();
            this.activateUpdate();
          }
        });
    });
  }

  private activateUpdate(): void {
    if (this.swUpdate.isEnabled) {
      this.swUpdate.activateUpdate().then(() => document.location.reload());
    } else {
      document.location.reload();
    }
  }

  private stopCountdownAndDestroyToast(): void {
    this.destroyToast$.next();
    this.destroyToast$.complete();
    this.destroyToast$ = new Subject<void>();

    this.toastService.clearAll();
    this.toastComponentRef = undefined;
  }
  
  ngOnDestroy(): void {
    this.stopCountdownAndDestroyToast();
  }
}