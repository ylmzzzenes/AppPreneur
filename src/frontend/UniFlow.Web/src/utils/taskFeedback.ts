import { aiApi } from '../api/services';
import type { TaskItemStatus } from '../api/types';

export async function tryShowTaskFeedback(
  taskId: number,
  newStatus: TaskItemStatus,
  onShow: (message: string, nextAction: string, isFallback: boolean) => void,
): Promise<void> {
  const result = await aiApi.taskFeedback({ taskId, newStatus });
  if (!result.isSuccess || !result.data) return;
  onShow(result.data.message, result.data.nextAction, result.data.isFallback);
}
