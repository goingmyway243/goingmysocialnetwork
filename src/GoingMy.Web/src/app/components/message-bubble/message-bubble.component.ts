import { Component, computed, input, output } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { MessageDto, ReadReceiptDto } from '../../models/chat.models';

@Component({
  selector: 'app-message-bubble',
  standalone: true,
  imports: [NgClass, DatePipe],
  template: `
    <div class="message-row" [ngClass]="isSent() ? 'sent' : 'received'">
      <!-- Actions (edit/delete) - only for sender, not deleted -->
      @if (isSent() && !message().isDeleted) {
        <div class="message-actions">
          <button class="action-btn" title="Edit" (click)="edit.emit(message())">
            <i class="pi pi-pencil"></i>
          </button>
          <button class="action-btn danger" title="Delete" (click)="delete.emit(message().id)">
            <i class="pi pi-trash"></i>
          </button>
        </div>
      }

      <div class="bubble-wrapper">
        <!-- Sender label for received messages -->
        @if (!isSent()) {
          <span style="font-size:0.7rem;color:var(--text-color-secondary);padding:0 4px">
            {{ message().senderUsername }}
          </span>
        }

        <div class="message-bubble" [ngClass]="{
          sent: isSent(),
          received: !isSent(),
          deleted: message().isDeleted
        }">
          @if (message().isDeleted) {
            <em>This message was deleted</em>
          } @else {
            <!-- Show edited content or original -->
            {{ message().editedContent ?? message().content }}
          }
        </div>

        <div style="display:flex;align-items:center;gap:6px">
          <span class="message-time" [ngClass]="isSent() ? '' : 'received'">
            {{ message().sentAt | date:'shortTime' }}
          </span>
          @if (message().editedAt && !message().isDeleted) {
            <span class="edited-label" [ngClass]="isSent() ? '' : 'received'">(edited)</span>
          }
        </div>

        <!-- Read receipts (only on sent messages) -->
        @if (isSent() && receiptsForMessage().length > 0) {
          <div class="read-receipts">
            <i class="pi pi-check-circle" style="font-size:0.6rem;color:var(--primary-color)"></i>
            Seen by {{ receiptNames() }}
          </div>
        }
      </div>
    </div>
  `
})
export class MessageBubbleComponent {
  // ── 1. Inputs ────────────────────────────────────────────────
  message = input.required<MessageDto>();
  currentUserId = input.required<string>();
  readReceipts = input<ReadReceiptDto[]>([]);

  // ── 2. Outputs ───────────────────────────────────────────────
  edit = output<MessageDto>();
  delete = output<string>();

  // ── 3. Derived ───────────────────────────────────────────────
  isSent = computed(() => this.message().senderId === this.currentUserId());

  receiptsForMessage = computed(() =>
    this.readReceipts().filter(r => r.messageId === this.message().id)
  );

  receiptNames = computed(() =>
    this.receiptsForMessage().map(r => r.readByUsername).join(', ')
  );
}
