import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import type { SyllabusScanResponse } from '../api/types';

interface SyllabusScanContextValue {
  scan: SyllabusScanResponse | null;
  setScan: (scan: SyllabusScanResponse | null) => void;
  clearScan: () => void;
}

const SyllabusScanContext = createContext<SyllabusScanContextValue | null>(null);

export function SyllabusScanProvider({ children }: { children: ReactNode }) {
  const [scan, setScan] = useState<SyllabusScanResponse | null>(null);
  const value = useMemo(
    () => ({
      scan,
      setScan,
      clearScan: () => setScan(null),
    }),
    [scan],
  );
  return <SyllabusScanContext.Provider value={value}>{children}</SyllabusScanContext.Provider>;
}

export function useSyllabusScan(): SyllabusScanContextValue {
  const ctx = useContext(SyllabusScanContext);
  if (!ctx) throw new Error('useSyllabusScan must be used within SyllabusScanProvider');
  return ctx;
}
