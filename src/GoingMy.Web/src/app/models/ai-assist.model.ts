export type AiAction = 'suggest' | 'improve' | 'grammar' | 'tone' | 'shorten' | 'lengthen';

export interface AiAssistRequest {
  action: AiAction;
  content?: string;
  tone?: string;
}

export interface AiAssistResponse {
  suggestion: string;
}
