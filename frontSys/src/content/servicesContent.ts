import { useEffect, useMemo, useState } from 'react';
import { listCollection } from '../api/client';

/**
 * The public text about our services, editable from Admin → Services Content. It's stored as JSON
 * in the `services_content` system setting; anything missing falls back to the defaults below
 * (the original site text), so the site looks the same until someone edits it.
 * Images and the page each card links to stay fixed in ServiceData.
 */
export interface ServiceCardText {
  heading: string;
  para: string;
  button: string;
}

/** One block of the Service Details page. Paragraphs in `body` are separated by a blank line. */
export interface ServiceDetailsSection {
  heading: string;
  body: string;
}

export interface ServicesContent {
  /** Section above the service cards on the home page. */
  homeHeading: string;
  homePara: string;
  /** Section above the service cards on the Services page. */
  pageHeading: string;
  pagePara: string;
  /** In ServiceData order: Cargo Brokerage, Ship Brokerage, Customs Clearance. */
  services: ServiceCardText[];
  /** The Service Details page. */
  details: {
    title: string;
    sections: ServiceDetailsSection[];
  };
}

export const MAX_DETAILS_SECTIONS = 10;

export const SERVICES_SETTING_KEY = 'services_content';

const SHARED_PARA =
  'Solving your supply chain needs from end to end, taking the complexity out of container shipping. We are at the forefront of developing innovation.';

export const DEFAULT_SERVICES_CONTENT: ServicesContent = {
  homeHeading: 'Taking care of you and your business all the way',
  homePara: SHARED_PARA,
  pageHeading: 'We Serve Various Ways',
  pagePara: SHARED_PARA,
  services: [
    {
      heading: 'Cargo Brokerage',
      para: 'We facilitate the delivering of cargos, ensuring smooth transactions and timely deliveries.',
      button: 'Register Cargo',
    },
    {
      heading: 'Ship Brokerage',
      para: 'Our ship brokerage services connect you with reliable vessels and shipping partners for efficient transport.',
      button: 'Register Ship Route',
    },
    {
      heading: 'Customs Clearance',
      para: 'Our customs clearance experts navigate the complexities of regulations to ensure your cargo moves seamlessly.',
      button: 'Register Clearance',
    },
  ],
  // Until someone writes this page, it describes the three services (it used to be placeholder text).
  details: {
    title: 'Service Details',
    sections: [
      {
        heading: 'Cargo Brokerage',
        body: 'We facilitate the delivering of cargos, ensuring smooth transactions and timely deliveries.',
      },
      {
        heading: 'Ship Brokerage',
        body: 'Our ship brokerage services connect you with reliable vessels and shipping partners for efficient transport.',
      },
      {
        heading: 'Customs Clearance',
        body: 'Our customs clearance experts navigate the complexities of regulations to ensure your cargo moves seamlessly.',
      },
    ],
  },
};

/** Splits a section body into paragraphs on blank lines. */
export function paragraphs(body: string): string[] {
  return body
    .split(/\r?\n\s*\r?\n/)
    .map((p) => p.trim())
    .filter(Boolean);
}

const text = (value: unknown, fallback: string): string =>
  typeof value === 'string' && value.trim() ? value : fallback;

/** Reads the stored JSON, keeping the default for anything missing, empty or malformed. */
export function parseServicesContent(raw?: string | null): ServicesContent {
  let stored: Partial<ServicesContent> = {};
  try {
    const parsed: unknown = raw ? JSON.parse(raw) : {};
    if (parsed && typeof parsed === 'object') stored = parsed as Partial<ServicesContent>;
  } catch {
    // malformed value - show the defaults rather than breaking the page
  }

  const d = DEFAULT_SERVICES_CONTENT;
  const services = Array.isArray(stored.services) ? stored.services : [];
  const storedDetails: Partial<ServicesContent['details']> =
    stored.details && typeof stored.details === 'object' ? stored.details : {};
  const storedSections = Array.isArray(storedDetails.sections)
    ? storedDetails.sections
        .map((s: Partial<ServiceDetailsSection>) => ({ heading: text(s.heading, ''), body: text(s.body, '') }))
        .filter((s) => s.heading || s.body)
        .slice(0, MAX_DETAILS_SECTIONS)
    : [];
  return {
    homeHeading: text(stored.homeHeading, d.homeHeading),
    homePara: text(stored.homePara, d.homePara),
    pageHeading: text(stored.pageHeading, d.pageHeading),
    pagePara: text(stored.pagePara, d.pagePara),
    services: d.services.map((fallback, i) => {
      const card: Partial<ServiceCardText> = services[i] ?? {};
      return {
        heading: text(card.heading, fallback.heading),
        para: text(card.para, fallback.para),
        button: text(card.button, fallback.button),
      };
    }),
    details: {
      title: text(storedDetails.title, d.details.title),
      sections: storedSections.length > 0 ? storedSections : d.details.sections,
    },
  };
}

/** Public system settings as key → value; empty until loaded (or if they can't be). */
export function usePublicSettings(): Record<string, string> {
  const [settings, setSettings] = useState<Record<string, string>>({});

  useEffect(() => {
    let active = true;
    listCollection<{ key: string; value: string }>('settings', { page: 1, perPage: 100 })
      .then((res) => {
        if (active) setSettings(Object.fromEntries(res.items.map((s) => [s.key, s.value])));
      })
      .catch(() => {
        // keep the defaults if settings can't be loaded
      });
    return () => {
      active = false;
    };
  }, []);

  return settings;
}

/** Services text from already-loaded settings (defaults until they arrive). */
export function useServicesContentFrom(settings: Record<string, string>): ServicesContent {
  const raw = settings[SERVICES_SETTING_KEY];
  return useMemo(() => parseServicesContent(raw), [raw]);
}

/** Public pages: starts with the defaults and swaps in the saved text once it loads. */
export function useServicesContent(): ServicesContent {
  return useServicesContentFrom(usePublicSettings());
}
