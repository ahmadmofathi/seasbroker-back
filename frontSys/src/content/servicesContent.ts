import { useEffect, useState } from 'react';
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

export interface ServicesContent {
  /** Section above the service cards on the home page. */
  homeHeading: string;
  homePara: string;
  /** Section above the service cards on the Services page. */
  pageHeading: string;
  pagePara: string;
  /** In ServiceData order: Cargo Brokerage, Ship Brokerage, Customs Clearance. */
  services: ServiceCardText[];
}

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
};

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
  };
}

/** Public pages: starts with the defaults and swaps in the saved text once it loads. */
export function useServicesContent(): ServicesContent {
  const [content, setContent] = useState<ServicesContent>(DEFAULT_SERVICES_CONTENT);

  useEffect(() => {
    let active = true;
    listCollection<{ key: string; value: string }>('settings', { page: 1, perPage: 100 })
      .then((res) => {
        const item = res.items.find((s) => s.key === SERVICES_SETTING_KEY);
        if (active && item) setContent(parseServicesContent(item.value));
      })
      .catch(() => {
        // keep the defaults if settings can't be loaded
      });
    return () => {
      active = false;
    };
  }, []);

  return content;
}
