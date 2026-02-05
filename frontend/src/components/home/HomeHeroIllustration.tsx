export function HomeHeroIllustration() {
  return (
    <svg
      viewBox="0 0 560 420"
      className="home-illustration"
      role="img"
      aria-label="FoodAdviser illustration"
    >
      <defs>
        <linearGradient id="bg" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0%" stopColor="rgba(76, 175, 80, 0.18)" />
          <stop offset="45%" stopColor="rgba(245, 230, 211, 0.55)" />
          <stop offset="100%" stopColor="rgba(255, 152, 0, 0.18)" />
        </linearGradient>
        <linearGradient id="card" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0%" stopColor="#FFFFFF" />
          <stop offset="100%" stopColor="#FAFAF8" />
        </linearGradient>
      </defs>

      <rect x="14" y="14" width="532" height="392" rx="22" fill="url(#bg)" stroke="rgba(229, 231, 235, 0.9)" />

      <rect x="52" y="56" width="220" height="90" rx="16" fill="url(#card)" stroke="rgba(229, 231, 235, 1)" />
      <rect x="68" y="76" width="150" height="12" rx="6" fill="rgba(44, 62, 80, 0.20)" />
      <rect x="68" y="100" width="182" height="10" rx="5" fill="rgba(44, 62, 80, 0.12)" />
      <rect x="68" y="122" width="110" height="10" rx="5" fill="rgba(44, 62, 80, 0.12)" />

      <rect x="290" y="56" width="220" height="140" rx="16" fill="url(#card)" stroke="rgba(229, 231, 235, 1)" />
      <circle cx="326" cy="98" r="20" fill="rgba(76, 175, 80, 0.25)" />
      <path d="M317 98l7 7 15-16" fill="none" stroke="#388E3C" strokeWidth="4" strokeLinecap="round" strokeLinejoin="round" />
      <rect x="360" y="84" width="120" height="12" rx="6" fill="rgba(44, 62, 80, 0.18)" />
      <rect x="360" y="108" width="150" height="10" rx="5" fill="rgba(44, 62, 80, 0.12)" />
      <rect x="360" y="128" width="126" height="10" rx="5" fill="rgba(44, 62, 80, 0.12)" />
      <rect x="360" y="148" width="100" height="10" rx="5" fill="rgba(44, 62, 80, 0.12)" />

      <rect x="52" y="174" width="458" height="70" rx="16" fill="url(#card)" stroke="rgba(229, 231, 235, 1)" />
      <rect x="70" y="194" width="180" height="12" rx="6" fill="rgba(44, 62, 80, 0.18)" />
      <rect x="70" y="216" width="260" height="10" rx="5" fill="rgba(44, 62, 80, 0.12)" />
      <rect x="360" y="196" width="130" height="32" rx="16" fill="rgba(255, 152, 0, 0.22)" stroke="rgba(255, 152, 0, 0.40)" />
      <rect x="380" y="208" width="92" height="8" rx="4" fill="rgba(245, 124, 0, 0.55)" />

      <rect x="52" y="270" width="300" height="110" rx="16" fill="url(#card)" stroke="rgba(229, 231, 235, 1)" />
      <rect x="70" y="292" width="220" height="12" rx="6" fill="rgba(44, 62, 80, 0.18)" />
      <rect x="70" y="316" width="260" height="10" rx="5" fill="rgba(44, 62, 80, 0.12)" />
      <rect x="70" y="336" width="240" height="10" rx="5" fill="rgba(44, 62, 80, 0.12)" />
      <rect x="70" y="356" width="190" height="10" rx="5" fill="rgba(44, 62, 80, 0.12)" />

      <rect x="370" y="270" width="140" height="110" rx="16" fill="rgba(76, 175, 80, 0.10)" stroke="rgba(76, 175, 80, 0.30)" />
      <circle cx="440" cy="324" r="34" fill="rgba(76, 175, 80, 0.22)" />
      <path d="M440 304c10 10 10 32-10 44" fill="none" stroke="#388E3C" strokeWidth="5" strokeLinecap="round" />
      <path d="M440 304c-10 10-10 32 10 44" fill="none" stroke="#4CAF50" strokeWidth="5" strokeLinecap="round" />
    </svg>
  );
}
