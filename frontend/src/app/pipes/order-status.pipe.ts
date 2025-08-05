import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'orderStatus'
})
export class OrderStatusPipe implements PipeTransform {

  private statusMap = new Map<string, string>([
    ['PendingConfirmation', 'Onay Bekliyor'],
    ['Processing', 'Hazırlanıyor'],
    ['Shipped', 'Kargoya Verildi'],
    ['Delivered', 'Teslim Edildi'],
    ['Cancelled', 'İptal Edildi'],
    ['Returned', 'İade Edildi']
  ]);

  transform(value: string | undefined | null): string {
    if (!value) {
      return 'Bilinmiyor';
    }

    return this.statusMap.get(value) || value;
  }
}