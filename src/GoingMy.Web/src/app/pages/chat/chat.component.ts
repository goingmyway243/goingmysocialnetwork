import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { ConversationDto, MessageDto } from '../../models/chat.models';
import { ChatStateService } from '../../services/chat-state.service';
import { ChatSignalRService } from '../../services/chat-signalr.service';
import { AuthService } from '../../services/auth.service';
import { ConversationListComponent } from '../../components/conversation-list/conversation-list.component';
import { MessageThreadComponent } from '../../components/message-thread/message-thread.component';
import { ComposeAreaComponent } from '../../components/compose-area/compose-area.component';

function buildMockData(userId: string): { conversations: ConversationDto[], messages: MessageDto[] } {
const MOCK_CONVERSATIONS: ConversationDto[] = [
  {
    id: 'conv-1',
    participantIds: [userId, 'user-alex'],
    participantUsernames: ['Me', 'Alex Chen'],
    lastMessagePreview: 'Sounds good! See you there 🎉',
    lastMessageAt: new Date(Date.now() - 5 * 60 * 1000).toISOString(),
    createdAt: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
    unreadCount: 0
  },
  {
    id: 'conv-2',
    participantIds: [userId, 'user-sarah'],
    participantUsernames: ['Me', 'Sarah Kim'],
    lastMessagePreview: 'Did you see the new design? It looks amazing!',
    lastMessageAt: new Date(Date.now() - 42 * 60 * 1000).toISOString(),
    createdAt: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
    unreadCount: 3
  },
  {
    id: 'conv-3',
    participantIds: [userId, 'user-james'],
    participantUsernames: ['Me', 'James Rivera'],
    lastMessagePreview: 'Hey, are you free this weekend?',
    lastMessageAt: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
    createdAt: new Date(Date.now() - 10 * 24 * 60 * 60 * 1000).toISOString(),
    unreadCount: 1
  },
  {
    id: 'conv-4',
    participantIds: [userId, 'user-mia'],
    participantUsernames: ['Me', 'Mia Tanaka'],
    lastMessagePreview: 'Thanks for sharing that article 📚',
    lastMessageAt: new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString(),
    createdAt: new Date(Date.now() - 14 * 24 * 60 * 60 * 1000).toISOString(),
    unreadCount: 0
  },
  {
    id: 'conv-5',
    participantIds: [userId, 'user-lucas'],
    participantUsernames: ['Me', 'Lucas Nguyen'],
    lastMessagePreview: 'The PR is ready for review',
    lastMessageAt: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(),
    createdAt: new Date(Date.now() - 20 * 24 * 60 * 60 * 1000).toISOString(),
    unreadCount: 7
  }
];

const MOCK_MESSAGES: MessageDto[] = [
  {
    id: 'msg-1', conversationId: 'conv-1', senderId: 'user-alex',
    senderUsername: 'Alex Chen',
    content: 'Hey! Are you going to the meetup tomorrow?',
    sentAt: new Date(Date.now() - 30 * 60 * 1000).toISOString(),
    isDeleted: false, editedContent: null, editedAt: null
  },
  {
    id: 'msg-2', conversationId: 'conv-1', senderId: userId,
    senderUsername: 'Me',
    content: 'Yeah definitely! What time are you heading there?',
    sentAt: new Date(Date.now() - 28 * 60 * 1000).toISOString(),
    isDeleted: false, editedContent: null, editedAt: null
  },
  {
    id: 'msg-3', conversationId: 'conv-1', senderId: 'user-alex',
    senderUsername: 'Alex Chen',
    content: 'I was thinking around 6:30pm, grab food first maybe?',
    sentAt: new Date(Date.now() - 25 * 60 * 1000).toISOString(),
    isDeleted: false, editedContent: null, editedAt: null
  },
  {
    id: 'msg-4', conversationId: 'conv-1', senderId: userId,
    senderUsername: 'Me',
    content: 'Perfect! There\'s that new ramen place nearby we could try 🍜',
    sentAt: new Date(Date.now() - 20 * 60 * 1000).toISOString(),
    isDeleted: false, editedContent: null, editedAt: null
  },
  {
    id: 'msg-5', conversationId: 'conv-1', senderId: 'user-alex',
    senderUsername: 'Alex Chen',
    content: 'Oh yes! I\'ve been wanting to go there. Let\'s do it!',
    sentAt: new Date(Date.now() - 15 * 60 * 1000).toISOString(),
    isDeleted: false, editedContent: null, editedAt: null
  },
  {
    id: 'msg-6', conversationId: 'conv-1', senderId: userId,
    senderUsername: 'Me',
    content: 'Sounds good! See you there 🎉',
    sentAt: new Date(Date.now() - 5 * 60 * 1000).toISOString(),
    isDeleted: false, editedContent: null, editedAt: null
  }
];
  return { conversations: MOCK_CONVERSATIONS, messages: MOCK_MESSAGES };
}

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
  private readonly _auth = inject(AuthService);

  // ── 2. State ────────────────────────────────────────────────
  readonly _editingMessage = signal<MessageDto | null>(null);

  // ── 3. Lifecycle ─────────────────────────────────────────────
  async ngOnInit(): Promise<void> {
    // Load mock data for development/preview
    this._loadMockData();

    // Optionally connect to SignalR for real-time updates (if backend running)
    try {
      await this._signalR.connect();
      // Don't call loadConversations - mock data is sufficient for UI preview
    } catch {
      // Mock data will be used (dev/preview mode)
    }
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

  // ── 5. Mock data (dev/preview only) ──────────────────────────
  private _loadMockData(): void {
    const { conversations, messages } = buildMockData(this._auth.getCurrentUserId());
    this._state.conversations.set(conversations);

    const msgMap = new Map<string, MessageDto[]>();
    msgMap.set('conv-1', messages);
    this._state.messages.set(msgMap);

    // Pre-select first conversation for visual demo
    this._state.selectedConversationId.set('conv-1');
  }
}
