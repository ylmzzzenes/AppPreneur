interface StepIndicatorProps {
  steps: string[];
  current: number;
}

export function StepIndicator({ steps, current }: StepIndicatorProps) {
  return (
    <ol className="step-indicator">
      {steps.map((label, i) => (
        <li key={label} className={i < current ? 'done' : i === current ? 'active' : ''}>
          <span className="step-dot">{i + 1}</span>
          <span>{label}</span>
        </li>
      ))}
    </ol>
  );
}
