import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'orderStatusColor'
})
export class OrderStatusColorPipe implements PipeTransform {

  private colorMap = new Map<string, string>([
    ['PendingConfirmation', 'bg-amber-500/20 text-amber-600'],
    ['Processing', 'bg-blue-500/20 text-blue-600'],
    ['Shipped', 'bg-sky-500/20 text-sky-600'],
    ['Delivered', 'bg-green-500/20 text-green-600'],
    ['Cancelled', 'bg-red-500/20 text-red-600'],
    ['Returned', 'bg-gray-500/20 text-gray-600']
  ]);

  transform(value: string | undefined | null): string {
    if (!value) {
      return 'bg-gray-200 text-gray-800';
    }
    return this.colorMap.get(value) || 'bg-gray-200 text-gray-800';
  }
}