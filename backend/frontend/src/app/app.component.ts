import { Component, OnInit, ViewContainerRef, AfterViewInit, ViewChild, ElementRef, HostListener, OnDestroy, isDevMode, ChangeDetectorRef } from '@angular/core'; // ChangeDetectorRef'i import edin
import { CookieService } from 'ngx-cookie-service';
import { NavigationEnd, Router } from '@angular/router';
import { filter, Observable, Subscription } from 'rxjs';
import { AuthService } from './services/auth.service';
import { ThemeService } from './services/theme.service';
import { NotificationService } from './services/notification.service';
import { CustomToastService } from './services/custom-toast.service';
import { NotificationDto } from './models/notification.dto';
import { PwaUpdateService } from './services/pwa-update.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, AfterViewInit, OnDestroy {
  isSidebarOpen = false;
  isLogin = false;
  currentUser: { firstName: string; lastName: string } | null = null;

  isNotificationPanelOpen = false;
  notifications$!: Observable<NotificationDto[]>;

  public unreadCount: number = 0;

  private mainSubscriptions: Subscription[] = [];
  private userSessionSubscriptions: Subscription[] = [];

  @ViewChild('toastContainer', { read: ViewContainerRef })
  private toastContainerRef!: ViewContainerRef;

  isPresentationPage = false;

  constructor(
    private cookieService: CookieService,
    private router: Router,
    public authService: AuthService,
    public themeService: ThemeService,
    public notificationService: NotificationService,
    private toastService: CustomToastService,
    private eRef: ElementRef,
    private pwaUpdateService: PwaUpdateService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {

    const routerSubscription = this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.isPresentationPage = (event.url === '/presentation' || event.urlAfterRedirects === '/presentation');
    });
    this.mainSubscriptions.push(routerSubscription); 

    const authSubscription = this.authService.isLoggedIn$.subscribe((isLoggedIn) => {
      this.isLogin = isLoggedIn;

      this.userSessionSubscriptions.forEach(sub => sub.unsubscribe());
      this.userSessionSubscriptions = [];

      if (isLoggedIn) {
        this.pwaUpdateService.initializeUpdateChecking();
        this.loadUserInfo();
        this.notifications$ = this.notificationService.notifications$;

        const unreadCountSub = this.notificationService.unreadCount$.subscribe(count => {
          this.unreadCount = count;
        });
        this.userSessionSubscriptions.push(unreadCountSub);

        this.listenForNewNotifications();

      } else {
        this.currentUser = null;
        this.isSidebarOpen = false;
      }
      
      this.cdr.detectChanges();
    });

    this.mainSubscriptions.push(authSubscription); 
  }
  
  ngAfterViewInit(): void {
    if (this.toastContainerRef) {
      this.toastService.setRootViewContainerRef(this.toastContainerRef);
    }
  }

  ngOnDestroy(): void {
    this.mainSubscriptions.forEach(sub => sub.unsubscribe());
    this.userSessionSubscriptions.forEach(sub => sub.unsubscribe());
  }

  listenForNewNotifications(): void {
    const newNotifSub = this.notificationService.newNotificationArrived$.subscribe(newNotifications => {
      newNotifications.forEach(notification => {
        this.toastService.show(notification, (notif) => {
          this.notificationService.markAsRead(notif).subscribe();
        });
      });
    });
    this.userSessionSubscriptions.push(newNotifSub);
  }

  loadUserInfo(): void {
    const userInfo = this.cookieService.get('userInfo');
    if (userInfo) {
      try {
        const parsed = JSON.parse(userInfo);
        this.currentUser = { firstName: parsed.FirstName, lastName: parsed.LastName };
      } catch {
        this.currentUser = null;
      }
    }
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  logout(): void {
    this.authService.logout();
  }

  toggleNotificationPanel(event: MouseEvent): void {
    event.stopPropagation();
    this.isNotificationPanelOpen = !this.isNotificationPanelOpen;
  }

  markNotificationAsRead(notification: NotificationDto): void {
    this.isNotificationPanelOpen = false;
    this.notificationService.markAsRead(notification).subscribe();
  }

  markAllNotificationsAsRead(): void {
    if (this.unreadCount === 0) return;

    this.isNotificationPanelOpen = false;
    this.notificationService.markAllAsRead().subscribe();
  }

  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    if (!this.eRef.nativeElement.querySelector('#notification-panel-wrapper')?.contains(event.target)) {
      this.isNotificationPanelOpen = false;
    }
  }
}