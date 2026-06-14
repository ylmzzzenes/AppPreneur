import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { getErrorMessage, usersApi } from '../api/services';
import type { PersonalityVibe } from '../api/types';
import { PERSONALITY_OPTIONS } from '../constants/personality';
import { ErrorBanner } from '../components/ErrorBanner';
import { useToast } from '../context/ToastContext';

export function OnboardingPage() {
  const { profile, refreshProfile } = useAuth();
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [displayName, setDisplayName] = useState(profile?.displayName ?? '');
  const [major, setMajor] = useState(profile?.major ?? '');
  const [academicGoal, setAcademicGoal] = useState(profile?.academicGoal ?? '');
  const [personalityVibe, setPersonalityVibe] = useState<PersonalityVibe>(profile?.personalityVibe ?? 'Friendly');
  const [dailyMinutes, setDailyMinutes] = useState(profile?.dailyStudyTargetMinutes ?? 120);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      const result = await usersApi.updateOnboarding({
        displayName: displayName.trim(),
        major: major.trim() || undefined,
        academicGoal: academicGoal.trim() || undefined,
        personalityVibe,
        dailyStudyTargetMinutes: dailyMinutes,
      });
      if (!result.isSuccess) {
        setError(getErrorMessage(result, 'Profil kaydedilemedi.'));
        return;
      }
      await refreshProfile();
      showToast('Hoş geldiniz! Planlamaya başlayabilirsiniz.', 'success');
      navigate('/dashboard', { replace: true });
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-shell">
      <form className="auth-card card onboarding-card" onSubmit={handleSubmit}>
        <h1>Hoş geldiniz</h1>
        <p className="muted">Birkaç bilgiyle UniFlow deneyimini kişiselleştirelim.</p>
        <ErrorBanner message={error} />
        <label>Görünen ad<input value={displayName} onChange={(e) => setDisplayName(e.target.value)} required /></label>
        <label>Bölüm<input value={major} onChange={(e) => setMajor(e.target.value)} /></label>
        <label>Akademik hedef<textarea value={academicGoal} onChange={(e) => setAcademicGoal(e.target.value)} rows={3} /></label>
        <label>
          AI kişilik tonu
          <select value={personalityVibe} onChange={(e) => setPersonalityVibe(e.target.value as PersonalityVibe)}>
            {PERSONALITY_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>{o.label}</option>
            ))}
          </select>
        </label>
        <label>Günlük çalışma hedefi (dk)<input type="number" min={15} max={720} value={dailyMinutes} onChange={(e) => setDailyMinutes(Number(e.target.value))} /></label>
        <button type="submit" className="btn btn-primary btn-block" disabled={loading}>
          {loading ? 'Kaydediliyor...' : 'Başla'}
        </button>
      </form>
    </div>
  );
}
