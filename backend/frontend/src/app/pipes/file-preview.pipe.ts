import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'filePreview', pure: true })
export class FilePreviewPipe implements PipeTransform {
  private cache = new WeakMap<File, string>();

  transform(file: File | undefined | null): string | null {
    if (!file) return null;
    if (this.cache.has(file)) {
      return this.cache.get(file)!;
    }
    const url = URL.createObjectURL(file);
    this.cache.set(file, url);
    return url;
  }
}
