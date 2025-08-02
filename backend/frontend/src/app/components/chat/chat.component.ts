import { Component, ElementRef, ViewChild, ChangeDetectorRef, AfterViewInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { marked } from 'marked';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

// Servisler
import { ChatService } from '../../services/chat.service';
import { CustomerService } from 'src/app/services/customer.service';
import { ProductService, ProductDto } from 'src/app/services/product.service';
import { OrderService } from 'src/app/services/order.service';

// Modeller
import { CustomerDto } from 'src/app/models/customer.model';
import { OrderDto } from 'src/app/models/order.model';

// =======================================================================
// ARAYÜZ / MODEL TANIMLAMALARI
// =======================================================================
interface ChatMessage {
  text: string;
  sender: 'user' | 'bot';
  cachedHtml?: Promise<SafeHtml>;
  data?: { headers: string[]; rows: any[]; } | null;
}

interface Mention {
  id: number | string;
  type: 'customer' | 'product' | 'order';
  displayText: string;
  backendText: string;
}

interface PromptPart {
  type: 'text' | 'mention';
  content: string | Mention;
}

interface MentionPopupState {
  isOpen: boolean;
  type: 'kullanıcı' | 'ürün' | 'sipariş' | null;
  searchTerm: string;
  suggestions: any[];
  activeTrigger: string | null;
}

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
  ) { }

  ngAfterViewInit(): void {
    const welcomeMessage: ChatMessage = {
      sender: 'bot',
      text: "Merhaba! Veritabanınızla ilgili sorular sorabilir veya sohbet edebilirsiniz. \n\nÖrnekler:\n- `@k` veya `@kullanıcı` yazarak müşteri arayabilirsiniz.\n- `@ü` veya `@ürün` yazarak ürün arayabilirsiniz.\n- `@s` veya `@sipariş` yazarak sipariş arayabilirsiniz."
    };
    welcomeMessage.cachedHtml = this.markdownToHtml(welcomeMessage.text);
    this.messages.push(welcomeMessage);
    this.loadMentionData();
  }

  ngOnDestroy(): void {
    if (this.chatSubscription) {
      this.chatSubscription.unsubscribe();
    }
  }

  private loadMentionData(): void {
    Promise.all([
      this.customerService.getAllCustomers().toPromise(),
      this.productService.getAllProducts().toPromise(),
      this.orderService.getAllOrders().toPromise()
    ]).then(([customerRes, productRes, orderRes]) => {
      this.allCustomers = customerRes?.data || [];
      this.allProducts = productRes?.data || [];
      this.allOrders = orderRes?.data || [];
    }).catch(err => {
      console.error("Etiketleme verisi yüklenirken hata oluştu:", err);
    });
  }

  public async markdownToHtml(text: string): Promise<SafeHtml> {
    const rawHtml = await marked.parse(text || '');
    return this.sanitizer.bypassSecurityTrustHtml(rawHtml);
  }

  public handleInput(event: Event): void {
    const element = event.target as HTMLDivElement;
    this.updateModelFromView(element);

    const selection = window.getSelection();
    if (!selection || selection.rangeCount === 0) return;

    const range = selection.getRangeAt(0);
    const textNode = range.startContainer;
    if (textNode.nodeType !== Node.TEXT_NODE) {
      this.mentionPopup.isOpen = false;
      return;
    }

    const textContent = textNode.textContent || '';
    const cursorPosition = range.startOffset;
    const textBeforeCursor = textContent.substring(0, cursorPosition);

    // Kelimeyi boşluğa göre değil, doğrudan @'den sonraki metne göre bul
    const lastAtSymbolIndex = textBeforeCursor.lastIndexOf('@');
    if (lastAtSymbolIndex === -1 || /\s/.test(textBeforeCursor.substring(lastAtSymbolIndex))) {
      this.mentionPopup.isOpen = false;
      return;
    }

    const currentWord = textBeforeCursor.substring(lastAtSymbolIndex);
    const triggers = { '@kullanıcı': 'kullanıcı', '@k': 'kullanıcı', '@ürün': 'ürün', '@ü': 'ürün', '@sipariş': 'sipariş', '@s': 'sipariş' };

    let triggered = false;
    for (const [key, type] of Object.entries(triggers)) {
      if (currentWord.toLowerCase().startsWith(key)) {
        this.mentionPopup = {
          isOpen: true,
          type: type as any,
          searchTerm: currentWord.substring(key.length),
          suggestions: [],
          activeTrigger: key
        };
        this.filterMentionSuggestions();
        triggered = true;
        break;
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
          const mentionData = JSON.parse(el.dataset['mention'] || '{}');
          this.promptParts.push({ type: 'mention', content: mentionData });
        }
      }
    });
    this.isInputEmpty = this.buildPromptForBackend().trim() === '';
  }

  private updateViewFromModel(): void {
    const element = this.promptInput.nativeElement;
    element.innerHTML = '';
    this.promptParts.forEach(part => {
      if (part.type === 'text') {
        element.appendChild(document.createTextNode(part.content as string));
      } else if (part.type === 'mention') {
        const mention = part.content as Mention;
        const span = document.createElement('span');
        span.className = 'mention-badge';
        span.contentEditable = 'false';
        span.dataset['mention'] = JSON.stringify(mention);

        const textNode = document.createTextNode(mention.displayText);
        span.appendChild(textNode);

        const removeButton = document.createElement('button');
        removeButton.innerHTML = '×'; // 'x' ikonu
        removeButton.className = 'mention-remove-button';
        removeButton.onclick = () => this.removeMention(mention);

        span.appendChild(removeButton);
        element.appendChild(span);
        element.appendChild(document.createTextNode('\u00A0')); // Rozetten sonraki boşluk
      }
    });
    this.setCursorToEnd(element);
    this.isInputEmpty = this.buildPromptForBackend().trim() === '';
  }

  public removeMention(mentionToRemove: Mention): void {
    this.promptParts = this.promptParts.filter(p => {
      if (p.type === 'mention') {
        return p.content !== mentionToRemove;
      }
      return true;
    });
    this.updateViewFromModel();
  }

  public filterMentionSuggestions(): void {
    const term = this.mentionPopup.searchTerm.toLowerCase();
    switch (this.mentionPopup.type) {
      case 'kullanıcı':
        this.mentionPopup.suggestions = this.allCustomers.filter(c => `${c.firstName} ${c.lastName} ${c.email}`.toLowerCase().includes(term)).slice(0, 5);
        break;
      case 'ürün':
        this.mentionPopup.suggestions = this.allProducts.filter(p => p.name.toLowerCase().includes(term)).slice(0, 5);
        break;
      case 'sipariş':
        this.mentionPopup.suggestions = this.allOrders.filter(o => o.orderId.toString().includes(term) || o.customerFullName.toLowerCase().includes(term)).slice(0, 5);
        break;
    }
  }

  public selectMention(item: any, type: 'customer' | 'product' | 'order'): void {
    let mention: Mention;
    if (type === 'customer') {
      mention = { id: item.customerId, type, displayText: `@${item.firstName} ${item.lastName}`, backendText: `(müşteri ID: ${item.customerId})` };
    } else if (type === 'product') {
      mention = { id: item.productId, type, displayText: `@${item.name}`, backendText: `(ürün ID: ${item.productId})` };
    } else {
      mention = { id: item.orderId, type, displayText: `@Sipariş #${item.orderId}`, backendText: `(sipariş ID: ${item.orderId})` };
    }

    // Modeli mevcut görünümden güncelle
    this.updateModelFromView(this.promptInput.nativeElement);

    // Son metin parçasını bul ve tetikleyiciyi değiştir
    let lastPartIndex = -1;
    for (let i = this.promptParts.length - 1; i >= 0; i--) {
      if (this.promptParts[i].type === 'text') {
        lastPartIndex = i;
        break;
      }
    }

    if (lastPartIndex !== -1) {
      const lastPart = this.promptParts[lastPartIndex];
      let content = lastPart.content as string;

      const trigger = this.mentionPopup.activeTrigger;
      if (trigger) {
        const triggerWithSearchTerm = trigger + this.mentionPopup.searchTerm;
        const startIndex = content.lastIndexOf(triggerWithSearchTerm);

        if (startIndex !== -1) {
          // Tetikleyiciyi metinden kaldır
          const before = content.substring(0, startIndex);
          const after = content.substring(startIndex + triggerWithSearchTerm.length);

          // Eski metin parçasını güncelle/böl ve araya mention'ı ekle
          this.promptParts.splice(lastPartIndex, 1,
            { type: 'text', content: before },
            { type: 'mention', content: mention },
            { type: 'text', content: after }
          );

          // Boş metin parçalarını temizle
          this.promptParts = this.promptParts.filter(p => p.content !== '');
        } else {
          this.promptParts.push({ type: 'mention', content: mention });
        }
      }
    } else {
      this.promptParts.push({ type: 'mention', content: mention });
    }

    this.updateViewFromModel();
    this.mentionPopup.isOpen = false;
  }

  public send(): void {
    const processedPrompt = this.buildPromptForBackend().trim();
    if (!processedPrompt || this.loading) return;

    const userText = this.buildDisplayText();
    this.messages.push({ text: userText, sender: 'user' });

    this.promptParts = [];
    this.updateViewFromModel();

    this.loading = true;
    this.scrollToBottom();

    const botMessage: ChatMessage = { text: '', sender: 'bot', data: null };
    this.messages.push(botMessage);
    this.isFirstChunk = true;

    this.chatSubscription = this.chatService.chatStream(processedPrompt).subscribe({
      next: (chunk) => this.processStreamChunk(chunk, botMessage),
      error: (err) => {
        botMessage.text = 'Üzgünüm, bir hata oluştu: ' + (err.message || 'Lütfen tekrar deneyin.');
        botMessage.cachedHtml = this.markdownToHtml(botMessage.text);
        this.loading = false;
        this.cdRef.detectChanges();
      },
      complete: () => {
        this.loading = false;
        this.cdRef.detectChanges();
        this.scrollToBottom();
      }
    });
  }

  public handleKeyDown(event: KeyboardEvent): void {
    if (this.mentionPopup.isOpen) {
      if (event.key === 'Escape') {
        this.mentionPopup.isOpen = false;
        event.preventDefault();
      }
      return;
    }
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  private buildPromptForBackend(): string {
    return this.promptParts.map(part => (part.type === 'text' ? part.content : (part.content as Mention).backendText)).join('').replace(/\s+/g, ' ').trim();
  }

  private buildDisplayText(): string {
    return this.promptParts.map(part => (part.type === 'text' ? part.content : (part.content as Mention).displayText)).join('');
  }

  private setCursorToEnd(element: HTMLElement): void {
    const range = document.createRange();
    const selection = window.getSelection();
    range.selectNodeContents(element);
    range.collapse(false);
    if (selection) {
      selection.removeAllRanges();
      selection.addRange(range);
    }
    element.focus();
  }

  private processStreamChunk(chunk: string, botMessage: ChatMessage): void {
    try {
      const parsedData = JSON.parse(chunk);
      if (parsedData.type === 'stream_end') {
        if (this.chatSubscription) {
          this.chatSubscription.unsubscribe();
        }
        this.loading = false;
        return;
      }
      if (this.isFirstChunk) {
        this.isFirstChunk = false;
        if (parsedData.type === 'full_response') {
          this.handleParsedData(parsedData.payload, botMessage);
          this.loading = false;
          if (this.chatSubscription) {
            this.chatSubscription.unsubscribe();
          }
          return;
        }
        if (parsedData.type === 'stream_start') {
          return;
        }
      }
      this.handleParsedData(parsedData, botMessage);
    } catch (e) {
      console.error("Gelen chunk parse edilemedi:", chunk, e);
    }
    this.cdRef.detectChanges();
  }

  private handleParsedData(parsedData: any, botMessage: ChatMessage): void {
    switch (parsedData.type) {
      case 'data_response':
        botMessage.text = parsedData.explanation;
        botMessage.cachedHtml = this.markdownToHtml(botMessage.text);
        if (parsedData.data && parsedData.data.length > 0) {
          botMessage.data = { headers: Object.keys(parsedData.data[0]), rows: parsedData.data };
        }
        break;
      case 'text_response':
        botMessage.text += parsedData.text;
        botMessage.cachedHtml = this.markdownToHtml(botMessage.text);
        break;
      case 'error_response':
        botMessage.text = `${parsedData.explanation}\n\n*Hata Detayı: ${parsedData.error}*`;
        botMessage.cachedHtml = this.markdownToHtml(botMessage.text);
        this.loading = false;
        break;
    }
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      try {
        this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
      } catch (err) {
        // ignore error
      }
    }, 50);
  }
}