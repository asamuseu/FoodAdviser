import { Link } from 'react-router-dom';
import { useAuth } from '../contexts';
import { HomeHeroIllustration, IconBox, IconReceipt, IconSpark } from '../components/home';

export default function HomePage() {
  const { isAuthenticated, isLoading, user } = useAuth();

  const primaryAction = isAuthenticated
    ? { href: '/inventory', label: 'Open inventory' }
    : { href: '/register', label: 'Get started free' };

  const secondaryAction = isAuthenticated
    ? { href: '/recipes', label: 'Generate recipes' }
    : { href: '/login', label: 'Sign in' };

  return (
    <div className="container">
      <main className="home">
        <section className="home-hero" aria-label="FoodAdviser overview">
          <div className="home-hero__grid">
            <div className="home-hero__content">
              <HeroBadges />

              <h1 className="home-title">Cook smarter with what you already have.</h1>
              <p className="home-subtitle">
                FoodAdviser helps you keep a clean inventory, scan receipts into products, and generate recipes
                based on what's in your kitchen.
              </p>

              <div className="home-cta-row">
                <Link className="home-cta home-cta--primary" to={primaryAction.href}>
                  {primaryAction.label}
                </Link>
                <Link className="home-cta home-cta--secondary" to={secondaryAction.href}>
                  {secondaryAction.label}
                </Link>
              </div>

              <p className="home-microcopy">
                {isLoading ? (
                  <span className="muted">Loading your session…</span>
                ) : isAuthenticated ? (
                  <>
                    <span className="muted">Signed in as </span>
                    <span className="home-strong">{user?.email}</span>
                    <span className="muted"> · Jump into your next action.</span>
                  </>
                ) : (
                  <>
                    <span className="muted">New here? Start with </span>
                    <Link to="/register">a quick account</Link>
                    <span className="muted"> (takes a minute).</span>
                  </>
                )}
              </p>

              <HeroMetrics />
            </div>

            <div className="home-hero__visual" aria-hidden="true">
              <HomeHeroIllustration />
            </div>
          </div>
        </section>

        <section className="home-section" aria-label="Core features">
          <div className="home-section__header">
            <h2>Everything you need to stay on top of your kitchen</h2>
            <p className="muted">
              Built around the three daily actions that matter: manage inventory, scan receipts, and pick recipes.
            </p>
          </div>

          <FeaturesGrid />
        </section>

        <section className="home-section" aria-label="How it works">
          <div className="home-section__header">
            <h2>How it works</h2>
            <p className="muted">A simple flow designed for everyday use.</p>
          </div>

          <HowItWorksSteps />
        </section>

        <section className="home-cta-banner" aria-label="Call to action">
          <div>
            <h2>Ready to plan tonight's meal?</h2>
            <p className="muted">
              Start from your inventory and let FoodAdviser suggest ideas that fit what you already have.
            </p>
          </div>
          <div className="home-cta-row home-cta-row--tight">
            <Link className="home-cta home-cta--primary" to={primaryAction.href}>
              {primaryAction.label}
            </Link>
            <Link className="home-cta home-cta--secondary" to="/recipes">
              Explore recipes
            </Link>
          </div>
        </section>
      </main>
    </div>
  );
}

function HeroBadges() {
  const badges = ['Inventory', 'Receipts', 'Recipes'];

  return (
    <div className="home-badges" aria-hidden="true">
      {badges.map((badge) => (
        <span key={badge} className="home-badge">
          {badge}
        </span>
      ))}
    </div>
  );
}

function HeroMetrics() {
  const metrics = [
    { value: 'Less waste', label: 'Track what expires' },
    { value: 'Faster planning', label: 'Recipes from inventory' },
    { value: 'Cleaner data', label: 'Receipt-to-products' },
  ];

  return (
    <div className="home-metrics" aria-label="Key benefits">
      {metrics.map((metric) => (
        <div key={metric.value} className="home-metric">
          <div className="home-metric__value">{metric.value}</div>
          <div className="home-metric__label">{metric.label}</div>
        </div>
      ))}
    </div>
  );
}

function FeaturesGrid() {
  const features = [
    {
      title: 'Inventory that stays tidy',
      description: 'Add, update, and review products with quantities and optional expiry dates.',
      to: '/inventory',
      icon: <IconBox />,
      cta: 'Manage inventory',
    },
    {
      title: 'Scan receipts into products',
      description: 'Upload a receipt image and extract line items—keep the image visible while you review.',
      to: '/receipts',
      icon: <IconReceipt />,
      cta: 'Upload a receipt',
    },
    {
      title: 'Generate recipes from what you have',
      description: 'Choose a dish type and number of persons, then confirm cooking to update inventory.',
      to: '/recipes',
      icon: <IconSpark />,
      cta: 'Generate recipes',
    },
  ];

  return (
    <div className="home-feature-grid">
      {features.map((feature) => (
        <FeatureCard key={feature.to} {...feature} />
      ))}
    </div>
  );
}

interface FeatureCardProps {
  title: string;
  description: string;
  to: string;
  icon: React.ReactNode;
  cta: string;
}

function FeatureCard({ title, description, to, icon, cta }: FeatureCardProps) {
  return (
    <div className="home-card">
      <div className="home-card__icon" aria-hidden="true">
        {icon}
      </div>
      <h3 className="home-card__title">{title}</h3>
      <p className="home-card__desc muted">{description}</p>
      <Link className="home-card__link" to={to}>
        {cta} <span aria-hidden="true">→</span>
      </Link>
    </div>
  );
}

function HowItWorksSteps() {
  const steps = [
    {
      num: 1,
      title: 'Stock your inventory',
      description: 'Add products manually or edit quantities anytime.',
    },
    {
      num: 2,
      title: 'Scan receipts',
      description: 'Extract purchased items from a photo, then review the results.',
    },
    {
      num: 3,
      title: 'Cook with confidence',
      description: 'Generate recipes and confirm cooking to keep inventory accurate.',
    },
  ];

  return (
    <ol className="home-steps">
      {steps.map((step) => (
        <li key={step.num} className="home-step">
          <div className="home-step__top">
            <span className="home-step__num">{step.num}</span>
            <h3>{step.title}</h3>
          </div>
          <p className="muted">{step.description}</p>
        </li>
      ))}
    </ol>
  );
}
