import {
  apiRequest,
  clearStoredToken,
  getErrorMessage,
  getStoredToken,
  setStoredToken,
} from './client';
import type {
  AuthResponse,
  Course,
  CreateCourseRequest,
  CreateTaskRequest,
  DashboardTodayResponse,
  LoginRequest,
  RegisterRequest,
  StudyPlanRequest,
  StudyPlanResponse,
  SyllabusConfirmRequest,
  SyllabusIngestionResult,
  SyllabusScanResponse,
  TaskFeedbackRequest,
  TaskFeedbackResponse,
  TaskItem,
  TaskItemStatus,
  TaskListResponse,
  UpdateCourseRequest,
  UpdateOnboardingRequest,
  UpdateTaskRequest,
  UserProfile,
  WeeklySummaryResponse,
} from './types';

export { getErrorMessage };

export const authApi = {
  async login(request: LoginRequest) {
    const result = await apiRequest<AuthResponse>('api/v1/auth/login', {
      method: 'POST',
      body: JSON.stringify(request),
      auth: false,
    });
    if (result.isSuccess && result.data?.accessToken) setStoredToken(result.data.accessToken);
    return result;
  },
  async register(request: RegisterRequest) {
    const result = await apiRequest<AuthResponse>('api/v1/auth/register', {
      method: 'POST',
      body: JSON.stringify(request),
      auth: false,
    });
    if (result.isSuccess && result.data?.accessToken) setStoredToken(result.data.accessToken);
    return result;
  },
  logout() {
    clearStoredToken();
  },
  isAuthenticated() {
    return Boolean(getStoredToken());
  },
};

export const usersApi = {
  me: () => apiRequest<UserProfile>('api/v1/users/me'),
  updateOnboarding: (request: UpdateOnboardingRequest) =>
    apiRequest<UserProfile>('api/v1/users/me/onboarding', {
      method: 'PATCH',
      body: JSON.stringify(request),
    }),
};

export const dashboardApi = {
  today: () => apiRequest<DashboardTodayResponse>('api/v1/dashboard/today'),
};

export const tasksApi = {
  list: () => apiRequest<TaskItem[]>('api/v1/tasks'),
  today: () => apiRequest<TaskListResponse>('api/v1/tasks/today'),
  upcoming: (days = 14, status?: TaskItemStatus) => {
    let url = `api/v1/tasks/upcoming?days=${days}`;
    if (status) url += `&status=${status}`;
    return apiRequest<TaskItem[]>(url);
  },
  get: (id: number) => apiRequest<TaskItem>(`api/v1/tasks/${id}`),
  create: (request: CreateTaskRequest) =>
    apiRequest<TaskItem>('api/v1/tasks', { method: 'POST', body: JSON.stringify(request) }),
  update: (id: number, request: UpdateTaskRequest) =>
    apiRequest<TaskItem>(`api/v1/tasks/${id}`, { method: 'PUT', body: JSON.stringify(request) }),
  remove: (id: number) => apiRequest<boolean>(`api/v1/tasks/${id}`, { method: 'DELETE' }),
  updateStatus: (id: number, status: TaskItemStatus) =>
    apiRequest<TaskItem>(`api/v1/tasks/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ status }),
    }),
};

export const coursesApi = {
  list: () => apiRequest<Course[]>('api/v1/courses'),
  get: (id: number) => apiRequest<Course>(`api/v1/courses/${id}`),
  create: (request: CreateCourseRequest) =>
    apiRequest<Course>('api/v1/courses', { method: 'POST', body: JSON.stringify(request) }),
  update: (id: number, request: UpdateCourseRequest) =>
    apiRequest<Course>(`api/v1/courses/${id}`, { method: 'PUT', body: JSON.stringify(request) }),
  remove: (id: number) => apiRequest<boolean>(`api/v1/courses/${id}`, { method: 'DELETE' }),
};

export const syllabusApi = {
  scan: async (courseCode: string, courseTitle: string, file: File) => {
    const form = new FormData();
    form.append('courseCode', courseCode);
    form.append('courseTitle', courseTitle);
    form.append('file', file);
    return apiRequest<SyllabusScanResponse>('api/v1/syllabus/scan', { method: 'POST', body: form });
  },
  confirm: (request: SyllabusConfirmRequest) =>
    apiRequest<SyllabusIngestionResult>('api/v1/syllabus/confirm', {
      method: 'POST',
      body: JSON.stringify(request),
    }),
};

export const aiApi = {
  weeklySummary: () => apiRequest<WeeklySummaryResponse>('api/v1/ai/weekly-summary'),
  studyPlan: (request: StudyPlanRequest) =>
    apiRequest<StudyPlanResponse>('api/v1/ai/study-plan', {
      method: 'POST',
      body: JSON.stringify(request),
    }),
  taskFeedback: (request: TaskFeedbackRequest) =>
    apiRequest<TaskFeedbackResponse>('api/v1/ai/task-feedback', {
      method: 'POST',
      body: JSON.stringify(request),
    }),
  chat: (message: string) =>
    apiRequest<string>('api/v1/Chat', { method: 'POST', body: JSON.stringify({ message }) }),
};

export async function sendChatMessage(message: string) {
  return aiApi.chat(message);
}
