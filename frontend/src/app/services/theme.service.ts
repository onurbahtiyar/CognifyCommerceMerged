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
    this.renderer = rendererFactory.createRenderer(null, null);
    this.initializeTheme();
  }

  private initializeTheme(): void {
    const savedTheme = localStorage.getItem(this.themeKey) as Theme;
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    
    const initialTheme = savedTheme || (prefersDark ? 'dark' : 'light');
    this.setTheme(initialTheme);
  }

  public getTheme(): Theme {
    return this.currentTheme;
  }
  
  public setTheme(theme: Theme): void {
    this.currentTheme = theme;
    localStorage.setItem(this.themeKey, theme);
    
    if (theme === 'dark') {
      this.renderer.addClass(document.documentElement, 'dark');
    } else {
      this.renderer.removeClass(document.documentElement, 'dark');
    }
  }

  public toggleTheme(): void {
    this.setTheme(this.currentTheme === 'light' ? 'dark' : 'light');
  }
}