import { Injectable, Renderer2, RendererFactory2 } from '@angular/core';

type Theme = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private renderer: Renderer2;
  private currentTheme: Theme = 'light';
  private readonly themeKey = 'app-theme';

  constructor(rendererFactory: RendererFactory2) {
    // Renderer2'yi güvenli bir şekilde almak için RendererFactory2 kullanılır.
    this.renderer = rendererFactory.createRenderer(null, null);
    this.initializeTheme();
  }

  /**
   * Servis başladığında temayı başlatır.
   * localStorage'dan veya kullanıcının sistem tercihinden temayı okur.
   */
  private initializeTheme(): void {
    const savedTheme = localStorage.getItem(this.themeKey) as Theme;
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    
    // Kayıtlı tema varsa onu kullan, yoksa sistem tercihini kullan, o da yoksa light başla.
    const initialTheme = savedTheme || (prefersDark ? 'dark' : 'light');
    this.setTheme(initialTheme);
  }

  /**
   * Mevcut temayı döndürür.
   */
  public getTheme(): Theme {
    return this.currentTheme;
  }
  
  /**
   * Temayı değiştirir ve uygular.
   * @param theme 'light' veya 'dark'
   */
  public setTheme(theme: Theme): void {
    this.currentTheme = theme;
    localStorage.setItem(this.themeKey, theme);
    
    if (theme === 'dark') {
      this.renderer.addClass(document.documentElement, 'dark');
    } else {
      this.renderer.removeClass(document.documentElement, 'dark');
    }
  }

  /**
   * Mevcut temayı tersine çevirir (light -> dark, dark -> light).
   */
  public toggleTheme(): void {
    this.setTheme(this.currentTheme === 'light' ? 'dark' : 'light');
  }
}