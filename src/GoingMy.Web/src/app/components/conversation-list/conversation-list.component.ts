import { Component, computed, inject } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { SkeletonModule } from 'primeng/skeleton';
import { FormsModule } from '@angular/forms';
import { ConversationDto } from '../../models/chat.models';
import { ChatStateService } from '../../services/chat-state.service';
import { AuthService } from '../../services/auth.service';
import { AiChatService } from '../../services/ai-chat.service';

@Component({
  selector: 'app-conversation-list',
  standalone: true,
  imports: [NgClass, DatePipe, SkeletonModule, FormsModule],
  templateUrl: './conversation-list.component.html'
})
export class ConversationListComponent {
  readonly _state = inject(ChatStateService);
  readonly _aiState = inject(AiChatService);
  private readonly _auth = inject(AuthService);

  readonly _aiConv = computed(() => this._aiState.aiConversation());

  _searchTerm = '';

  filteredConversations() {
    const term = this._searchTerm.toLowerCase();
    if (!term) return this._state.conversations();
    return this._state.conversations().filter(c =>
      c.participantUsernames.some(u => u.toLowerCase().includes(term))
    );
  }

  selectConversation(conv: ConversationDto): void {
    this._state.selectConversation(conv.id);
  }

  selectAiConversation(): void {
    this._state.enterAiMode();
  }

  getRecipientName(conv: ConversationDto): string {
    const myId = this._auth.getCurrentUserId();
    const idx = conv.participantIds.findIndex(id => id !== myId);
    return conv.participantUsernames[idx] ?? conv.participantUsernames[0] ?? 'Unknown';
  }

  getInitial(conv: ConversationDto): string {
    return this.getRecipientName(conv).charAt(0).toUpperCase();
  }
}

