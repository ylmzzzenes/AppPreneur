import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { getErrorMessage, usersApi } from '../api/services';
import type { PersonalityVibe } from '../api/types';
import { PERSONALITY_OPTIONS } from '../constants/personality';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageLoader } from '../components/PageLoader';
import { useToast } from '../context/ToastContext';

export function ProfilePage() {
  const { profile, refreshProfile } = useAuth();
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [displayName, setDisplayName] = useState('');
  const [major, setMajor] = useState('');
  const [academicGoal, setAcademicGoal] = useState('');
  const [personalityVibe, setPersonalityVibe] = useState<PersonalityVibe>('Friendly');
  const [dailyMinutes, setDailyMinutes] = useState(120);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (profile) {
      setDisplayName(profile.displayName);
      setMajor(profile.major ?? '');
      setAcademicGoal(profile.academicGoal ?? '');
      setPersonalityVibe(profile.personalityVibe);
      setDailyMinutes(profile.dailyStudyTargetMinutes ?? 120);
      setLoading(false);
    }
  }, [profile]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSaving(true);
    setError('');
    setSuccess('');
    const result = await usersApi.updateOnboarding({
      displayName: displayName.trim(),
      major: major.trim() || undefined,
      academicGoal: academicGoal.trim() || undefined,
      personalityVibe,
      dailyStudyTargetMinutes: dailyMinutes,
    });
    if (!result.isSuccess) {
      setError(getErrorMessage(result, 'Profil kaydedilemedi.'));
    } else {
      await refreshProfile();
      setSuccess('Profil güncellendi.');
      showToast('Profil güncellendi.', 'success');
    }
    setSaving(false);
  }

  if (loading) return <PageLoader label="Profil yükleniyor..." />;

  return (
    <div className="page">
      <div className="page-header">
        <h1>Profil</h1>
        <button type="button" className="btn btn-secondary" onClick={() => navigate('/dashboard')}>Geri</button>
      </div>
      <ErrorBanner message={error} />
      {success && <div className="banner banner-success">{success}</div>}
      <form className="card form-grid" onSubmit={handleSubmit}>
        <label>E-posta<input value={profile?.email ?? ''} disabled /></label>
        <label>Görünen ad<input value={displayName} onChange={(e) => setDisplayName(e.target.value)} required /></label>
        <label>Bölüm<input value={major} onChange={(e) => setMajor(e.target.value)} /></label>
        <label>Akademik hedef<textarea value={academicGoal} onChange={(e) => setAcademicGoal(e.target.value)} rows={3} /></label>
        <label>AI kişilik
          <select value={personalityVibe} onChange={(e) => setPersonalityVibe(e.target.value as PersonalityVibe)}>
            {PERSONALITY_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
        </label>
        <label>Günlük hedef (dk)<input type="number" min={15} max={720} value={dailyMinutes} onChange={(e) => setDailyMinutes(Number(e.target.value))} /></label>
        <button type="submit" className="btn btn-primary" disabled={saving}>{saving ? 'Kaydediliyor...' : 'Kaydet'}</button>
      </form>
    </div>
  );
}
