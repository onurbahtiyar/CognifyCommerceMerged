import { Injectable, ViewContainerRef } from '@angular/core';
import { CustomToastComponent } from '../components/custom-toast/custom-toast.component';
import { NotificationDto } from '../models/notification.dto';

@Injectable({
  providedIn: 'root'
})
export class CustomToastService {
  private viewContainerRef!: ViewContainerRef;

  setRootViewContainerRef(vcRef: ViewContainerRef) {
    this.viewContainerRef = vcRef;
  }

  show(notification: NotificationDto, onClick: (notification: NotificationDto) => void): void {
    if (!this.viewContainerRef) {
      console.error('ViewContainerRef is not set for CustomToastService!');
      return;
    }

    const componentRef = this.viewContainerRef.createComponent(CustomToastComponent);

    componentRef.instance.notification = notification;

    const closeSub = componentRef.instance.close.subscribe(() => {
      closeSub.unsubscribe();
      toastClickSub.unsubscribe();
      componentRef.destroy();
    });

    const toastClickSub = componentRef.instance.toastClick.subscribe((notif) => {
      onClick(notif);
    });
  }
}