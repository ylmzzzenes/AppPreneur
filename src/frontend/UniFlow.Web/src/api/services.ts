import {
  apiRequest,
  clearStoredToken,
  getErrorMessage,
  getStoredToken,
  setStoredToken,
} from '../api/client';
import type {
  AuthResponse,
  CreateTaskRequest,
  DashboardTodayResponse,
  LoginRequest,
  RegisterRequest,
  StudyPlanResponse,
  SyllabusConfirmRequest,
  SyllabusIngestionResult,
  SyllabusScanResponse,
  TaskItem,
  UpdateTaskRequest,
  WeeklySummaryResponse,
  Course,
} from '../api/types';

export const authApi = {
  async login(request: LoginRequest) {
    const result = await apiRequest<AuthResponse>('api/v1/auth/login', {
      method: 'POST',
      body: JSON.stringify(request),
      auth: false,
    });

    if (result.isSuccess && result.data?.accessToken) {
      setStoredToken(result.data.accessToken);
    }

    return result;
  },

  async register(request: RegisterRequest) {
    const result = await apiRequest<AuthResponse>('api/v1/auth/register', {
      method: 'POST',
      body: JSON.stringify(request),
      auth: false,
    });

    if (result.isSuccess && result.data?.accessToken) {
      setStoredToken(result.data.accessToken);
    }

    return result;
  },

  logout() {
    clearStoredToken();
  },

  isAuthenticated() {
    return Boolean(getStoredToken());
  },
};

export const dashboardApi = {
  today: () => apiRequest<DashboardTodayResponse>('api/v1/dashboard/today'),
};

export const tasksApi = {
  list: () => apiRequest<TaskItem[]>('api/v1/tasks'),
  create: (request: CreateTaskRequest) =>
    apiRequest<TaskItem>('api/v1/tasks', { method: 'POST', body: JSON.stringify(request) }),
  update: (id: number, request: UpdateTaskRequest) =>
    apiRequest<TaskItem>(`api/v1/tasks/${id}`, { method: 'PUT', body: JSON.stringify(request) }),
  remove: (id: number) => apiRequest<boolean>(`api/v1/tasks/${id}`, { method: 'DELETE' }),
  updateStatus: (id: number, status: string) =>
    apiRequest<TaskItem>(`api/v1/tasks/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ status }),
    }),
};

export const coursesApi = {
  list: () => apiRequest<Course[]>('api/v1/courses'),
};

export const syllabusApi = {
  async scan(courseCode: string, courseTitle: string, file: File) {
    const form = new FormData();
    form.append('courseCode', courseCode);
    form.append('courseTitle', courseTitle);
    form.append('file', file);

    return apiRequest<SyllabusScanResponse>('api/v1/syllabus/scan', {
      method: 'POST',
      body: form,
    });
  },

  confirm: (request: SyllabusConfirmRequest) =>
    apiRequest<SyllabusIngestionResult>('api/v1/syllabus/confirm', {
      method: 'POST',
      body: JSON.stringify(request),
    }),
};

export const aiApi = {
  weeklySummary: () => apiRequest<WeeklySummaryResponse>('api/v1/ai/weekly-summary'),
  studyPlan: (courseId?: number, days = 7) =>
    apiRequest<StudyPlanResponse>('api/v1/ai/study-plan', {
      method: 'POST',
      body: JSON.stringify({ courseId, days }),
    }),
  chat: (message: string) =>
    apiRequest<string>('api/v1/Chat', { method: 'POST', body: JSON.stringify({ message }) }),
};

export { getErrorMessage };
