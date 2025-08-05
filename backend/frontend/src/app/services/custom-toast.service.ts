import { ComponentRef, Injectable, ViewContainerRef } from '@angular/core';
import { BehaviorSubject, EMPTY, Observable } from 'rxjs';
import { filter, switchMap, take } from 'rxjs/operators';
import { CustomToastComponent } from '../components/custom-toast/custom-toast.component';
import { NotificationDto } from '../models/notification.dto';
import { UpdateToastComponent } from '../components/update-toast/update-toast.component';

@Injectable({
  providedIn: 'root'
})
export class CustomToastService {
  private viewContainerRef$ = new BehaviorSubject<ViewContainerRef | null>(null);

  setRootViewContainerRef(vcRef: ViewContainerRef) {
    this.viewContainerRef$.next(vcRef);
  }
  
  show(notification: NotificationDto, onClick: (notification: NotificationDto) => void): void {
    this.viewContainerRef$.pipe(
      filter((ref): ref is ViewContainerRef => ref !== null),
      take(1)
    ).subscribe(viewContainerRef => {
      const componentRef = viewContainerRef.createComponent(CustomToastComponent);
      componentRef.instance.notification = notification;

      const closeSub = componentRef.instance.close.subscribe(() => {
        closeSub.unsubscribe();
        toastClickSub.unsubscribe();
        componentRef.destroy();
      });

      const toastClickSub = componentRef.instance.toastClick.subscribe((notif) => {
        onClick(notif);
      });
    });
  }

  showUpdate(initialMessage: string): Observable<ComponentRef<UpdateToastComponent>> {
    return this.viewContainerRef$.pipe(
      filter((ref): ref is ViewContainerRef => ref !== null),
      take(1),
      switchMap(viewContainerRef => {
        const componentRef = viewContainerRef.createComponent(UpdateToastComponent);
        componentRef.instance.message = initialMessage;
        return [componentRef]; 
      })
    );
  }

  clearAll(): void {
    const vcRef = this.viewContainerRef$.getValue();
    if (vcRef) {
      vcRef.clear();
    }
  }
}