import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-update-toast',
  template: `
    <div class="pointer-events-auto w-full max-w-sm overflow-hidden rounded-lg bg-primary text-primary-foreground shadow-lg ring-1 ring-black ring-opacity-5">
      <div class="p-4">
        <div class="flex items-start">
          <div class="flex-shrink-0">
            <i class="bi bi-arrow-down-circle-fill text-xl"></i>
          </div>

          <div class="ml-3 flex-1 pt-0.5">
            <p class="text-sm font-bold">Yeni sürüm hazır!</p>
            <p class="mt-1 text-sm">{{ message }}</p>
            <div class="mt-4 flex gap-x-3">
              <button
                (click)="updateClick.emit()"
                class="inline-flex items-center rounded-md border border-transparent bg-primary-foreground px-3 py-2 text-sm font-medium leading-4 text-primary shadow-sm transition-colors hover:bg-primary-foreground/90 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-primary-foreground"
              >
                Şimdi Güncelle
              </button>
              <button
                (click)="cancelClick.emit()"
                class="inline-flex items-center rounded-md border border-transparent bg-transparent px-3 py-2 text-sm font-medium leading-4 text-primary-foreground hover:bg-white/10 focus:outline-none"
              >
                Daha Sonra
              </button>
            </div>
          </div>
          
          <div class="ml-4 flex flex-shrink-0">
             <button (click)="cancelClick.emit()" class="inline-flex rounded-md text-primary-foreground/70 hover:text-primary-foreground focus:outline-none">
                <span class="sr-only">Kapat</span>
                <i class="bi bi-x-lg"></i>
             </button>
          </div>
        </div>
      </div>
    </div>
  `,
  standalone: true,
  imports: [CommonModule]
})
export class UpdateToastComponent {
  @Input() message: string = '';
  @Output() updateClick = new EventEmitter<void>();
  @Output() cancelClick = new EventEmitter<void>();
}