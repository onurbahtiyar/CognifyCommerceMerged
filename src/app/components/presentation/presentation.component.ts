import { Component, ViewChild, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';
import { ThemeService } from 'src/app/services/theme.service';

@Component({
  selector: 'app-presentation',
  templateUrl: './presentation.component.html',
  styleUrls: ['./presentation.component.scss']
})
export class PresentationComponent implements AfterViewInit, OnDestroy {
  // HTML'deki #videoPlayer elementine erişim sağlıyoruz
  @ViewChild('videoPlayer') videoPlayer!: ElementRef<HTMLVideoElement>;
  @ViewChild('playerContainer') playerContainer!: ElementRef<HTMLDivElement>;

  isPlaying = false;
  progress = 0;
  currentTime = '00:00';
  duration = '00:00';
  
  // Event listener'ları temizlemek için
  private videoListeners: { [key: string]: () => void } = {};

  constructor(public themeService: ThemeService) {}

  ngAfterViewInit(): void {
    // Event listener'ları burada bağlıyoruz
    this.addVideoListeners();
  }

  ngOnDestroy(): void {
    // Component yok olduğunda listener'ları kaldırıyoruz
    this.removeVideoListeners();
  }
  
  private get video(): HTMLVideoElement {
    return this.videoPlayer.nativeElement;
  }

  addVideoListeners(): void {
    this.videoListeners['timeupdate'] = () => {
      this.progress = (this.video.currentTime / this.video.duration) * 100;
      this.currentTime = this.formatTime(this.video.currentTime);
    };
    this.videoListeners['loadedmetadata'] = () => {
      this.duration = this.formatTime(this.video.duration);
    };
    this.videoListeners['play'] = () => this.isPlaying = true;
    this.videoListeners['pause'] = () => this.isPlaying = false;
    this.videoListeners['ended'] = () => this.isPlaying = false;

    Object.keys(this.videoListeners).forEach(key => {
      this.video.addEventListener(key, this.videoListeners[key]);
    });
  }

  removeVideoListeners(): void {
     Object.keys(this.videoListeners).forEach(key => {
      this.video.removeEventListener(key, this.videoListeners[key]);
    });
  }

  togglePlayPause(): void {
    this.video.paused ? this.video.play() : this.video.pause();
  }

  seek(seconds: number): void {
    this.video.currentTime += seconds;
  }

  scrub(event: MouseEvent): void {
    const scrubTime = (event.offsetX / (event.target as HTMLElement).offsetWidth) * this.video.duration;
    this.video.currentTime = scrubTime;
  }

  toggleFullscreen(): void {
    const container = this.playerContainer.nativeElement;
    if (!document.fullscreenElement) {
      container.requestFullscreen().catch(err => {
        alert(`Tam ekran modu başlatılamadı: ${err.message}`);
      });
    } else {
      document.exitFullscreen();
    }
  }

  formatTime(time: number): string {
    const minutes = Math.floor(time / 60);
    const seconds = Math.floor(time % 60);
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }
}