import type { PersonalityVibe } from '../api/types';

export const PERSONALITY_OPTIONS: { value: PersonalityVibe; label: string }[] = [
  { value: 'Friendly', label: 'Samimi' },
  { value: 'Strict', label: 'Disiplinli' },
  { value: 'Motivational', label: 'Cesaretlendirici' },
  { value: 'Sarcastic', label: 'Dobra' },
  { value: 'Calm', label: 'Sakin' },
];

export const TASK_STATUS_LABELS: Record<string, string> = {
  Pending: 'Bekliyor',
  Done: 'Tamamlandı',
  Missed: 'Kaçırıldı',
};
