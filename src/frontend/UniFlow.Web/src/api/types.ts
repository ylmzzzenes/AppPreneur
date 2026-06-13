export interface ApiError {
  code: string;
  message: string;
}

export interface ApiResult<T> {
  isSuccess: boolean;
  data?: T;
  error?: ApiError;
}

export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  userId: number;
  email: string;
  displayName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  displayName: string;
  password: string;
  major?: string;
}

export type TaskItemStatus = 'Pending' | 'Done' | 'Missed';

export interface TaskItem {
  id: number;
  title: string;
  description?: string;
  dueDate?: string;
  category?: string;
  priorityScore?: number;
  estimatedMinutes?: number;
  status: TaskItemStatus;
  courseId: number;
  courseCode: string;
  courseTitle: string;
  syllabusId: number;
  syllabusTitle: string;
  isAiGenerated: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface DashboardTaskItem {
  id: number;
  title: string;
  dueDate?: string;
  status: TaskItemStatus;
  courseCode: string;
  courseTitle: string;
  priorityScore?: number;
}

export interface DashboardTodayResponse {
  today: string;
  bigThreeTasks: DashboardTaskItem[];
  overdueTasksCount: number;
  completedTodayCount: number;
  pendingTodayCount: number;
  personalityVibe: string;
  aiMood: string;
  dailyMessage: string;
}

export interface Course {
  id: number;
  code: string;
  title: string;
  description?: string;
  color?: string;
  taskCount: number;
  activeTaskCount: number;
}

export interface CreateTaskRequest {
  courseId: number;
  title: string;
  description?: string;
  dueDate?: string;
  estimatedMinutes?: number;
  priorityScore?: number;
  status?: TaskItemStatus;
}

export interface UpdateTaskRequest {
  courseId: number;
  title: string;
  description?: string;
  dueDate?: string;
  estimatedMinutes?: number;
  priorityScore?: number;
  status: TaskItemStatus;
}

export interface SyllabusDetectedItem {
  title: string;
  description?: string;
  dueDate?: string;
  type?: string;
  priorityScore?: number;
}

export interface SyllabusScanResponse {
  scanId: string;
  courseCode: string;
  courseTitle: string;
  detectedItems: SyllabusDetectedItem[];
  sourceSummary: string;
  expiresAt: string;
}

export interface SyllabusConfirmRequest {
  scanId: string;
  courseCode: string;
  courseTitle: string;
  items: SyllabusDetectedItem[];
}

export interface SyllabusIngestionResult {
  courseId: number;
  syllabusId: number;
  taskCount: number;
}

export interface WeeklySummaryResponse {
  summary: string;
  completedCount: number;
  missedCount: number;
  pendingCount: number;
  strongPoint: string;
  improvementPoint: string;
  nextWeekFocus: string;
  isFallback: boolean;
}

export interface StudyPlanResponse {
  title: string;
  summary: string;
  isFallback: boolean;
}
