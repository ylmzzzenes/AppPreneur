import type { TaskItemStatus } from '../../api/types';
import { TASK_STATUS_LABELS } from '../../constants/personality';

const STATUS_CLASS: Record<TaskItemStatus, string> = {
  Pending: 'badge-pending',
  Done: 'badge-done',
  Missed: 'badge-missed',
};

export function StatusBadge({ status }: { status: TaskItemStatus }) {
  return (
    <span className={`badge ${STATUS_CLASS[status]}`}>
      {TASK_STATUS_LABELS[status] ?? status}
    </span>
  );
}

export function AiBadge({ fallback }: { fallback?: boolean }) {
  if (!fallback) return null;
  return <span className="badge badge-ai">Yedek AI</span>;
}
