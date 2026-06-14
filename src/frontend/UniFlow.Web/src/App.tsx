import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from './auth/AuthContext';
import { AppLayout } from './components/AppLayout';
import { ProtectedRoute } from './components/ProtectedRoute';
import { SyllabusScanProvider } from './context/SyllabusScanContext';
import { ToastProvider } from './context/ToastContext';
import { ChatPage } from './pages/ChatPage';
import { CourseFormPage } from './pages/CourseFormPage';
import { CoursesPage } from './pages/CoursesPage';
import { DashboardPage } from './pages/DashboardPage';
import { LoginPage } from './pages/LoginPage';
import { OnboardingPage } from './pages/OnboardingPage';
import { ProfilePage } from './pages/ProfilePage';
import { RegisterPage } from './pages/RegisterPage';
import { StudyPlanPage } from './pages/StudyPlanPage';
import { SyllabusPage } from './pages/SyllabusPage';
import { SyllabusPreviewPage } from './pages/SyllabusPreviewPage';
import { TaskFormPage } from './pages/TaskFormPage';
import { TasksPage } from './pages/TasksPage';

export function App() {
  return (
    <BrowserRouter>
      <ToastProvider>
        <AuthProvider>
          <SyllabusScanProvider>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route element={<ProtectedRoute />}>
                <Route path="/onboarding" element={<OnboardingPage />} />
                <Route element={<AppLayout />}>
                  <Route path="/dashboard" element={<DashboardPage />} />
                  <Route path="/tasks" element={<TasksPage />} />
                  <Route path="/tasks/new" element={<TaskFormPage />} />
                  <Route path="/tasks/:id/edit" element={<TaskFormPage />} />
                  <Route path="/courses" element={<CoursesPage />} />
                  <Route path="/courses/new" element={<CourseFormPage />} />
                  <Route path="/courses/:id/edit" element={<CourseFormPage />} />
                  <Route path="/chat" element={<ChatPage />} />
                  <Route path="/syllabus" element={<SyllabusPage />} />
                  <Route path="/syllabus/preview" element={<SyllabusPreviewPage />} />
                  <Route path="/study-plan" element={<StudyPlanPage />} />
                  <Route path="/profile" element={<ProfilePage />} />
                </Route>
              </Route>
              <Route path="*" element={<Navigate to="/dashboard" replace />} />
            </Routes>
          </SyllabusScanProvider>
        </AuthProvider>
      </ToastProvider>
    </BrowserRouter>
  );
}
