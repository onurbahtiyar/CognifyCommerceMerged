import { Component, ElementRef, ViewChild, ChangeDetectorRef, AfterViewInit, OnDestroy } from '@angular/core';
import { Subscription, firstValueFrom } from 'rxjs';
import { marked } from 'marked';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { Chart, registerables } from 'chart.js';

// Servisler ve Modeller
import { ChatService } from '../../services/chat.service';
import { CustomerService } from 'src/app/services/customer.service';
import { ProductService, ProductDto } from 'src/app/services/product.service';
import { OrderService } from 'src/app/services/order.service';
import { CustomerDto } from 'src/app/models/customer.model';
import { OrderDto } from 'src/app/models/order.model';
import { ChatMessageDto, ChatSessionDto } from 'src/app/models/chat.model';

// Component içi Arayüzler
interface ChatMessage {
  text: string;
  sender: 'user' | 'assistant';
  cachedHtml?: Promise<SafeHtml>;
  data?: { headers: string[]; rows: any[]; } | null;
  chartData?: any;
}

interface Mention { id: number | string; type: 'customer' | 'product' | 'order'; displayText: string; backendText: string; }
interface PromptPart { type: 'text' | 'mention'; content: string | Mention; }
interface MentionPopupState { isOpen: boolean; type: 'kullanıcı' | 'ürün' | 'sipariş' | null; searchTerm: string; suggestions: any[]; activeTrigger: string | null; }

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})
export class ChatComponent implements AfterViewInit, OnDestroy {
  @ViewChild('scrollContainer') private scrollContainer!: ElementRef;
  @ViewChild('promptInput') private promptInput!: ElementRef<HTMLDivElement>;

  public messages: ChatMessage[] = [];
  public loading: boolean = false;
  private chatSubscription: Subscription | null = null;
  private isFirstChunk: boolean = true;
  private promptParts: PromptPart[] = [];
  public isInputEmpty = true;
  public mentionPopup: MentionPopupState = { isOpen: false, type: null, searchTerm: '', suggestions: [], activeTrigger: null };
  
  public sessions: ChatSessionDto[] = [];
  public activeSessionId: string | null = null;
  public isSidebarOpen = false;
  
  private allCustomers: CustomerDto[] = [];
  private allProducts: ProductDto[] = [];
  private allOrders: OrderDto[] = [];

  constructor(
    private chatService: ChatService,
    private cdRef: ChangeDetectorRef,
    private sanitizer: DomSanitizer,
    private customerService: CustomerService,
    private productService: ProductService,
    private orderService: OrderService
  ) {
    Chart.register(...registerables);
  }
  
  public get activeSessionTitle(): string {
    if (!this.activeSessionId) return 'Yeni Sohbet';
    const activeSession = this.sessions.find(s => s.sessionId === this.activeSessionId);
    return activeSession ? activeSession.title : 'Sohbet';
  }

  ngAfterViewInit(): void {
    this.startNewChat();
    this.loadMentionData();
    this.loadAllSessions();
  }

  ngOnDestroy(): void {
    if (this.chatSubscription) {
      this.chatSubscription.unsubscribe();
    }
  }

  public setPromptAndSend(prompt: string): void {
    this.promptParts = [{ type: 'text', content: prompt }];
    this.updateViewFromModel();
    // Gönder
    this.send();
  }

  public loadAllSessions(): void {
    this.chatService.getAllSessions().subscribe({
      next: result => {
        if (result.success) { this.sessions = result.data; this.cdRef.detectChanges(); }
        else { console.error("Sohbet geçmişi yüklenemedi:", result.message); }
      },
      error: err => console.error("Sohbet geçmişi yüklenirken API hatası:", err)
    });
  }

  public selectSession(sessionId: string): void {
    if (this.loading || sessionId === this.activeSessionId) return;
    this.isSidebarOpen = false; this.loading = true; this.activeSessionId = sessionId; this.messages = []; this.scrollToBottom();
    this.chatService.getMessagesBySessionId(sessionId).subscribe({
      next: result => {
        if (result.success) { this.messages = result.data.map(backendMsg => this.mapBackendMessageToFrontend(backendMsg)); }
        else {
          const errorMsg: ChatMessage = { sender: 'assistant', text: `Sohbet yüklenemedi: ${result.message}` };
          errorMsg.cachedHtml = this.markdownToHtml(errorMsg.text); this.messages = [errorMsg];
        }
        this.loading = false; this.scrollToBottom(); this.cdRef.detectChanges();
      },
      error: err => {
        const errorMsg: ChatMessage = { sender: 'assistant', text: `Bir hata oluştu: ${err.message}` };
        errorMsg.cachedHtml = this.markdownToHtml(errorMsg.text); this.messages = [errorMsg]; this.loading = false;
      }
    });
  }

  public startNewChat(): void {
    this.isSidebarOpen = false; this.activeSessionId = null; this.messages = []; this.promptParts = []; this.updateViewFromModel();
    const welcomeMessage: ChatMessage = {
      sender: 'assistant',
      text: "Merhaba! Veritabanınızla ilgili sorular sorabilir veya sohbet edebilirsiniz. \n\n**Örnekler:**\n- `@k` veya `@kullanıcı` yazarak müşteri arayabilirsiniz.\n- `@ü` veya `@ürün` yazarak ürün arayabilirsiniz.\n- `@s` veya `@sipariş` yazarak sipariş arayabilirsiniz."
    };
    welcomeMessage.cachedHtml = this.markdownToHtml(welcomeMessage.text);
    this.messages.push(welcomeMessage); this.cdRef.detectChanges();
  }

  public deleteSession(sessionId: string, event: MouseEvent): void {
    event.stopPropagation();
    if (confirm("Bu sohbeti ve tüm mesajlarını kalıcı olarak silmek istediğinizden emin misiniz?")) {
      this.chatService.deleteSession(sessionId).subscribe({
        next: result => {
          if (result.success) {
            this.sessions = this.sessions.filter(s => s.sessionId !== sessionId);
            if (this.activeSessionId === sessionId) { this.startNewChat(); }
            this.cdRef.detectChanges();
          } else { alert(`Sohbet silinemedi: ${result.message}`); }
        },
        error: err => alert(`Bir hata oluştu: ${err.message}`)
      });
    }
  }

  private mapBackendMessageToFrontend(backendMsg: ChatMessageDto): ChatMessage {
    const frontendMsg: ChatMessage = { sender: backendMsg.role, text: '' };
    try {
      const parsedContent = JSON.parse(backendMsg.content);
      if (parsedContent.type && (parsedContent.type === 'data_response' || parsedContent.type === 'chart_response' || parsedContent.type === 'error_response')) {
        this.handleParsedData(parsedContent, frontendMsg, -1);
      } else {
        frontendMsg.text = backendMsg.content;
      }
    } catch (e) {
      frontendMsg.text = backendMsg.content;
    }
    frontendMsg.cachedHtml = this.markdownToHtml(frontendMsg.text);
    return frontendMsg;
  }

  private async loadMentionData(): Promise<void> {
    try {
      const [customerRes, productRes, orderRes] = await Promise.all([
        firstValueFrom(this.customerService.getAllCustomers()),
        firstValueFrom(this.productService.getAllProducts()),
        firstValueFrom(this.orderService.getAllOrders())
      ]);
      this.allCustomers = customerRes?.data || []; this.allProducts = productRes?.data || []; this.allOrders = orderRes?.data || [];
    } catch (err) {
      console.error("Etiketleme verisi yüklenirken hata oluştu:", err);
    }
  }

  public async markdownToHtml(text: string): Promise<SafeHtml> {
    const rawHtml = await marked.parse(text || '');
    return this.sanitizer.bypassSecurityTrustHtml(rawHtml);
  }

  public handleInput(event: Event): void {
    const element = event.target as HTMLDivElement;
    this.updateModelFromView(element); const selection = window.getSelection();
    if (!selection || selection.rangeCount === 0) return;
    const range = selection.getRangeAt(0); const textNode = range.startContainer;
    if (textNode.nodeType !== Node.TEXT_NODE) { this.mentionPopup.isOpen = false; return; }
    const textContent = textNode.textContent || ''; const cursorPosition = range.startOffset;
    const textBeforeCursor = textContent.substring(0, cursorPosition);
    const lastAtSymbolIndex = textBeforeCursor.lastIndexOf('@');
    if (lastAtSymbolIndex === -1 || /\s/.test(textBeforeCursor.substring(lastAtSymbolIndex))) { this.mentionPopup.isOpen = false; return; }
    const currentWord = textBeforeCursor.substring(lastAtSymbolIndex);
    const triggers = { '@kullanıcı': 'kullanıcı', '@k': 'kullanıcı', '@ürün': 'ürün', '@ü': 'ürün', '@sipariş': 'sipariş', '@s': 'sipariş' };
    let triggered = false;
    for (const [key, type] of Object.entries(triggers)) {
      if (currentWord.toLowerCase().startsWith(key)) {
        this.mentionPopup = { isOpen: true, type: type as any, searchTerm: currentWord.substring(key.length), suggestions: [], activeTrigger: key };
        this.filterMentionSuggestions(); triggered = true; break;
      }
    }
    if (!triggered) this.mentionPopup.isOpen = false;
  }

  private updateModelFromView(element: HTMLDivElement): void {
    this.promptParts = [];
    element.childNodes.forEach(node => {
      if (node.nodeType === Node.TEXT_NODE) {
        if (node.textContent) this.promptParts.push({ type: 'text', content: node.textContent.replace(/\u00A0/g, ' ') });
      } else if (node.nodeType === Node.ELEMENT_NODE) {
        const el = node as HTMLElement;
        if (el.tagName === 'SPAN' && el.classList.contains('mention-badge')) {
          const mentionData = JSON.parse(el.dataset['mention'] || '{}'); this.promptParts.push({ type: 'mention', content: mentionData });
        }
      }
    });
    this.isInputEmpty = this.buildPromptForBackend().trim() === '';
  }

  private updateViewFromModel(): void {
    const element = this.promptInput.nativeElement;
    element.innerHTML = '';
    this.promptParts.forEach(part => {
      if (part.type === 'text') { element.appendChild(document.createTextNode(part.content as string)); }
      else if (part.type === 'mention') {
        const mention = part.content as Mention;
        const span = document.createElement('span'); span.className = 'mention-badge'; span.contentEditable = 'false';
        span.dataset['mention'] = JSON.stringify(mention); const textNode = document.createTextNode(mention.displayText);
        span.appendChild(textNode); const removeButton = document.createElement('button');
        removeButton.innerHTML = '×'; removeButton.className = 'mention-remove-button';
        removeButton.onclick = () => this.removeMention(mention); span.appendChild(removeButton);
        element.appendChild(span); element.appendChild(document.createTextNode('\u00A0'));
      }
    });
    this.setCursorToEnd(element);
    this.isInputEmpty = this.buildPromptForBackend().trim() === '';
  }

  public removeMention(mentionToRemove: Mention): void {
    this.promptParts = this.promptParts.filter(p => p.type === 'mention' ? p.content !== mentionToRemove : true);
    this.updateViewFromModel();
  }
  
  public filterMentionSuggestions(): void {
    const term = this.mentionPopup.searchTerm.toLowerCase();
    switch (this.mentionPopup.type) {
      case 'kullanıcı': this.mentionPopup.suggestions = this.allCustomers.filter(c => `${c.firstName} ${c.lastName} ${c.email}`.toLowerCase().includes(term)).slice(0, 5); break;
      case 'ürün': this.mentionPopup.suggestions = this.allProducts.filter(p => p.name.toLowerCase().includes(term)).slice(0, 5); break;
      case 'sipariş': this.mentionPopup.suggestions = this.allOrders.filter(o => o.orderId.toString().includes(term) || o.customerFullName.toLowerCase().includes(term)).slice(0, 5); break;
    }
  }

  public selectMention(item: any, type: 'customer' | 'product' | 'order'): void {
    let mention: Mention;
    if (type === 'customer') { mention = { id: item.customerId, type, displayText: `@${item.firstName} ${item.lastName}`, backendText: `(müşteri ID: ${item.customerId})` }; }
    else if (type === 'product') { mention = { id: item.productId, type, displayText: `@${item.name}`, backendText: `(ürün ID: ${item.productId})` }; }
    else { mention = { id: item.orderId, type, displayText: `@Sipariş #${item.orderId}`, backendText: `(sipariş ID: ${item.orderId})` }; }
    this.updateModelFromView(this.promptInput.nativeElement);
    let lastPartIndex = -1;
    for (let i = this.promptParts.length - 1; i >= 0; i--) {
        if (this.promptParts[i].type === 'text') { lastPartIndex = i; break; }
    }
    if (lastPartIndex !== -1) {
      const lastPart = this.promptParts[lastPartIndex];
      let content = lastPart.content as string;
      const trigger = this.mentionPopup.activeTrigger;
      if (trigger) {
        const triggerWithSearchTerm = trigger + this.mentionPopup.searchTerm;
        const startIndex = content.lastIndexOf(triggerWithSearchTerm);
        if (startIndex !== -1) {
          const before = content.substring(0, startIndex);
          const after = content.substring(startIndex + triggerWithSearchTerm.length);
          this.promptParts.splice(lastPartIndex, 1, { type: 'text', content: before }, { type: 'mention', content: mention }, { type: 'text', content: after });
          this.promptParts = this.promptParts.filter(p => p.content !== '');
        } else { this.promptParts.push({ type: 'mention', content: mention }); }
      }
    } else { this.promptParts.push({ type: 'mention', content: mention }); }
    this.updateViewFromModel(); this.mentionPopup.isOpen = false;
  }

  public send(): void {
    const processedPrompt = this.buildPromptForBackend().trim();
    if (!processedPrompt || this.loading) return;
    if (this.messages.length === 1 && this.messages[0].text.startsWith("Merhaba!")) {
      this.messages = [];
    }
    const userText = this.buildDisplayText();
    this.messages.push({ text: userText, sender: 'user' });
    this.promptParts = []; this.updateViewFromModel(); this.loading = true; this.scrollToBottom();
    const botMessage: ChatMessage = { text: '', sender: 'assistant', data: null };
    this.messages.push(botMessage); this.isFirstChunk = true;
    this.chatSubscription = this.chatService.chatStream(processedPrompt, this.activeSessionId || undefined).subscribe({
      next: (chunk) => this.processStreamChunk(chunk, botMessage, userText),
      error: (err) => {
        botMessage.text = 'Üzgünüm, bir hata oluştu: ' + (err.message || 'Lütfen tekrar deneyin.');
        botMessage.cachedHtml = this.markdownToHtml(botMessage.text);
        this.loading = false; this.cdRef.detectChanges();
      },
      complete: () => {
        this.loading = false; this.cdRef.detectChanges(); this.scrollToBottom();
      }
    });
  }

  private processStreamChunk(chunk: string, botMessage: ChatMessage, userPromptForTitle: string): void {
    const messageIndex = this.messages.indexOf(botMessage);
    try {
      const parsedData = JSON.parse(chunk);
      if (parsedData.type === 'session_info') {
        if (!this.activeSessionId) {
          this.activeSessionId = parsedData.sessionId;
          this.sessions.unshift({
            sessionId: parsedData.sessionId,
            title: userPromptForTitle.length > 35 ? userPromptForTitle.substring(0, 35) + '...' : userPromptForTitle,
            createdAt: new Date().toISOString()
          });
        }
        return;
      }
      if (parsedData.type === 'stream_end') { if (this.chatSubscription) this.chatSubscription.unsubscribe(); this.loading = false; return; }
      if (this.isFirstChunk) {
        this.isFirstChunk = false;
        if (parsedData.type === 'full_response') {
          this.handleParsedData(parsedData.payload, botMessage, messageIndex);
          this.loading = false; if (this.chatSubscription) this.chatSubscription.unsubscribe(); return;
        }
        if (parsedData.type === 'stream_start') return;
      }
      this.handleParsedData(parsedData, botMessage, messageIndex);
    } catch (e) {
      console.error("Gelen chunk parse edilemedi:", chunk, e);
    }
    this.cdRef.detectChanges();
  }

  public handleKeyDown(event: KeyboardEvent): void {
    if (this.mentionPopup.isOpen) {
      if (event.key === 'Escape') { this.mentionPopup.isOpen = false; event.preventDefault(); }
      return;
    }
    if (event.key === 'Enter' && !event.shiftKey) { event.preventDefault(); this.send(); }
  }

  private buildPromptForBackend(): string {
    return this.promptParts.map(part => (part.type === 'text' ? part.content : (part.content as Mention).backendText)).join('').replace(/\s+/g, ' ').trim();
  } 
  private buildDisplayText(): string {
    return this.promptParts.map(part => (part.type === 'text' ? part.content : (part.content as Mention).displayText)).join('');
  } 
  private setCursorToEnd(element: HTMLElement): void {
    const range = document.createRange(); const selection = window.getSelection();
    range.selectNodeContents(element); range.collapse(false);
    if (selection) { selection.removeAllRanges(); selection.addRange(range); }
    element.focus();
  } 
  private handleParsedData(parsedData: any, botMessage: ChatMessage, messageIndex: number): void {
    switch (parsedData.type) {
      case 'data_response':
        botMessage.text = parsedData.explanation;
        if (parsedData.data && parsedData.data.length > 0) { botMessage.data = { headers: Object.keys(parsedData.data[0]), rows: parsedData.data }; }
        break;
      case 'chart_response':
        botMessage.text = parsedData.explanation; botMessage.chartData = parsedData.payload;
        setTimeout(() => { if (messageIndex >= 0) { this.renderChart(`chart-${messageIndex}`, botMessage.chartData); } }, 0);
        break;
      case 'text_response':
        botMessage.text += parsedData.text;
        break;
      case 'error_response':
        botMessage.text = `${parsedData.explanation}\n\n*Hata Detayı: ${parsedData.error}*`;
        this.loading = false;
        break;
    }
    botMessage.cachedHtml = this.markdownToHtml(botMessage.text);
  } 
  private renderChart(canvasId: string, chartData: any): void {
    const canvas = document.getElementById(canvasId) as HTMLCanvasElement;
    if (!canvas) return;
    const existingChart = Chart.getChart(canvas);
    if (existingChart) existingChart.destroy();
    new Chart(canvas, { type: chartData.type, data: chartData.data, options: chartData.options || {} });
  } 
  private scrollToBottom(): void {
    setTimeout(() => {
      try { this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight; } catch (err) {}
    }, 50);
  }
  
}