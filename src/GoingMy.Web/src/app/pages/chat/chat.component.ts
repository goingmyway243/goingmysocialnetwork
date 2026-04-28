import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { ConversationDto, MessageDto } from '../../models/chat.models';
import { ChatStateService } from '../../services/chat-state.service';
import { ChatSignalRService } from '../../services/chat-signalr.service';
import { ConversationListComponent } from '../../components/conversation-list/conversation-list.component';
import { MessageThreadComponent } from '../../components/message-thread/message-thread.component';
import { ComposeAreaComponent } from '../../components/compose-area/compose-area.component';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [ConversationListComponent, MessageThreadComponent, ComposeAreaComponent],
  styleUrl: './chat.component.css',
  template: `
    <div class="chat-container">
      @if (_state.selectedConversationId()) {
        <div class="message-wrapper">
          <app-message-thread
            (editMessageRequested)="onEditRequested($event)"
            (editCancel)="onEditCancel()"
          />
          <app-compose-area
            [editingMessage]="_editingMessage()"
            (editSubmitted)="onEditSubmitted()"
            (editCancelled)="onEditCancel()"
          />
        </div>
      } @else {
        <div class="chat-empty-state">
          <div class="empty-icon-wrapper">
            <i class="pi pi-comments"></i>
          </div>
          <h3 class="empty-title">No conversation selected</h3>
          <p class="empty-subtitle">Choose a conversation from the right panel to start chatting</p>
        </div>
      }

      <app-conversation-list />
    </div>
  `
})
export class ChatComponent implements OnInit, OnDestroy {

  // ── 1. Dependencies ─────────────────────────────────────────
  readonly _state = inject(ChatStateService);
  private readonly _signalR = inject(ChatSignalRService);

  // ── 2. State ────────────────────────────────────────────────
  readonly _editingMessage = signal<MessageDto | null>(null);

  // ── 3. Lifecycle ─────────────────────────────────────────────
  async ngOnInit(): Promise<void> {
    await this._signalR.connect();
    await this._state.loadConversations();
  }

  async ngOnDestroy(): Promise<void> {
    await this._signalR.disconnect();
  }

  // ── 4. Actions ───────────────────────────────────────────────
  onEditRequested(msg: MessageDto): void {
    this._editingMessage.set(msg);
  }

  onEditCancel(): void {
    this._editingMessage.set(null);
  }

  onEditSubmitted(): void {
    this._editingMessage.set(null);
  }
}
