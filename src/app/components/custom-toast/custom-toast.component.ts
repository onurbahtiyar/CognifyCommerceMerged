import { Component, EventEmitter, Input, OnInit, OnDestroy, Output, HostBinding } from '@angular/core';
import { animate, style, transition, trigger } from '@angular/animations';
import { NotificationDto } from '../../models/notification.dto';

@Component({
  selector: 'app-custom-toast',
  templateUrl: './custom-toast.component.html',
  styleUrls: ['./custom-toast.component.scss'],
  animations: [
    trigger('toastAnimation', [
      transition(':enter', [
        style({ transform: 'translateY(100%)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateY(0)', opacity: 1 })),
      ]),
      transition(':leave', [
        animate('300ms ease-in', style({ transform: 'translateY(100%)', opacity: 0 })),
      ]),
    ]),
  ],
})
export class CustomToastComponent implements OnInit, OnDestroy {
  @Input() notification!: NotificationDto;
  @Output() close = new EventEmitter<void>();
  @Output() toastClick = new EventEmitter<NotificationDto>();

  @HostBinding('@toastAnimation')
  public animate = true;

  private timer: any;

  ngOnInit(): void {
    this.timer = setTimeout(() => {
      this.onClose();
    }, 10000);
  }

  ngOnDestroy(): void {
    clearTimeout(this.timer);
  }

  onClose(): void {
    this.close.emit();
  }

  onToastClick(): void {
    this.toastClick.emit(this.notification);
    this.onClose();
  }
}