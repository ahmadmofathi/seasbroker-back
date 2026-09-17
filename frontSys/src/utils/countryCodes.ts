export interface CountryCode {
  iso: string;
  name: string;
  dialCode: string;
  flag: string;
}

/** Kept in sync with the country list used across the public forms (FormSeedData.cs). */
export const COUNTRY_CODES: CountryCode[] = [
  { iso: 'EG', name: 'Egypt', dialCode: '+20', flag: '🇪🇬' },
  { iso: 'SA', name: 'Saudi Arabia', dialCode: '+966', flag: '🇸🇦' },
  { iso: 'AE', name: 'United Arab Emirates', dialCode: '+971', flag: '🇦🇪' },
  { iso: 'KW', name: 'Kuwait', dialCode: '+965', flag: '🇰🇼' },
  { iso: 'QA', name: 'Qatar', dialCode: '+974', flag: '🇶🇦' },
  { iso: 'OM', name: 'Oman', dialCode: '+968', flag: '🇴🇲' },
  { iso: 'BH', name: 'Bahrain', dialCode: '+973', flag: '🇧🇭' },
  { iso: 'JO', name: 'Jordan', dialCode: '+962', flag: '🇯🇴' },
  { iso: 'LB', name: 'Lebanon', dialCode: '+961', flag: '🇱🇧' },
  { iso: 'PA', name: 'Panama', dialCode: '+507', flag: '🇵🇦' },
  { iso: 'LR', name: 'Liberia', dialCode: '+231', flag: '🇱🇷' },
  { iso: 'MH', name: 'Marshall Islands', dialCode: '+692', flag: '🇲🇭' },
  { iso: 'MT', name: 'Malta', dialCode: '+356', flag: '🇲🇹' },
  { iso: 'BS', name: 'Bahamas', dialCode: '+1242', flag: '🇧🇸' },
  { iso: 'SG', name: 'Singapore', dialCode: '+65', flag: '🇸🇬' },
  { iso: 'HK', name: 'Hong Kong', dialCode: '+852', flag: '🇭🇰' },
  { iso: 'CY', name: 'Cyprus', dialCode: '+357', flag: '🇨🇾' },
  { iso: 'GR', name: 'Greece', dialCode: '+30', flag: '🇬🇷' },
  { iso: 'TR', name: 'Turkey', dialCode: '+90', flag: '🇹🇷' },
  { iso: 'IT', name: 'Italy', dialCode: '+39', flag: '🇮🇹' },
  { iso: 'ES', name: 'Spain', dialCode: '+34', flag: '🇪🇸' },
  { iso: 'FR', name: 'France', dialCode: '+33', flag: '🇫🇷' },
  { iso: 'DE', name: 'Germany', dialCode: '+49', flag: '🇩🇪' },
  { iso: 'NL', name: 'Netherlands', dialCode: '+31', flag: '🇳🇱' },
  { iso: 'BE', name: 'Belgium', dialCode: '+32', flag: '🇧🇪' },
  { iso: 'GB', name: 'United Kingdom', dialCode: '+44', flag: '🇬🇧' },
  { iso: 'NO', name: 'Norway', dialCode: '+47', flag: '🇳🇴' },
  { iso: 'DK', name: 'Denmark', dialCode: '+45', flag: '🇩🇰' },
  { iso: 'PT', name: 'Portugal', dialCode: '+351', flag: '🇵🇹' },
  { iso: 'CN', name: 'China', dialCode: '+86', flag: '🇨🇳' },
  { iso: 'JP', name: 'Japan', dialCode: '+81', flag: '🇯🇵' },
  { iso: 'KR', name: 'South Korea', dialCode: '+82', flag: '🇰🇷' },
  { iso: 'IN', name: 'India', dialCode: '+91', flag: '🇮🇳' },
  { iso: 'PK', name: 'Pakistan', dialCode: '+92', flag: '🇵🇰' },
  { iso: 'BD', name: 'Bangladesh', dialCode: '+880', flag: '🇧🇩' },
  { iso: 'LK', name: 'Sri Lanka', dialCode: '+94', flag: '🇱🇰' },
  { iso: 'ID', name: 'Indonesia', dialCode: '+62', flag: '🇮🇩' },
  { iso: 'MY', name: 'Malaysia', dialCode: '+60', flag: '🇲🇾' },
  { iso: 'TH', name: 'Thailand', dialCode: '+66', flag: '🇹🇭' },
  { iso: 'VN', name: 'Vietnam', dialCode: '+84', flag: '🇻🇳' },
  { iso: 'PH', name: 'Philippines', dialCode: '+63', flag: '🇵🇭' },
  { iso: 'AU', name: 'Australia', dialCode: '+61', flag: '🇦🇺' },
  { iso: 'US', name: 'United States', dialCode: '+1', flag: '🇺🇸' },
  { iso: 'CA', name: 'Canada', dialCode: '+1', flag: '🇨🇦' },
  { iso: 'BR', name: 'Brazil', dialCode: '+55', flag: '🇧🇷' },
  { iso: 'AR', name: 'Argentina', dialCode: '+54', flag: '🇦🇷' },
  { iso: 'CL', name: 'Chile', dialCode: '+56', flag: '🇨🇱' },
  { iso: 'MX', name: 'Mexico', dialCode: '+52', flag: '🇲🇽' },
  { iso: 'ZA', name: 'South Africa', dialCode: '+27', flag: '🇿🇦' },
  { iso: 'NG', name: 'Nigeria', dialCode: '+234', flag: '🇳🇬' },
  { iso: 'MA', name: 'Morocco', dialCode: '+212', flag: '🇲🇦' },
  { iso: 'DZ', name: 'Algeria', dialCode: '+213', flag: '🇩🇿' },
  { iso: 'TN', name: 'Tunisia', dialCode: '+216', flag: '🇹🇳' },
  { iso: 'LY', name: 'Libya', dialCode: '+218', flag: '🇱🇾' },
  { iso: 'RU', name: 'Russia', dialCode: '+7', flag: '🇷🇺' },
  { iso: 'UA', name: 'Ukraine', dialCode: '+380', flag: '🇺🇦' },
];

export const DEFAULT_DIAL_CODE = '+20';

/** Splits a combined phone string (e.g. "+20 1012345678") into dial code + local number,
 * matching the longest known dial code prefix so e.g. +1242 (Bahamas) isn't mistaken for +1. */
export function splitPhone(value?: string | null): { dialCode: string; number: string } {
  const trimmed = (value ?? '').trim();
  if (!trimmed) {
    return { dialCode: DEFAULT_DIAL_CODE, number: '' };
  }

  const match = [...COUNTRY_CODES]
    .sort((a, b) => b.dialCode.length - a.dialCode.length)
    .find((c) => trimmed.startsWith(c.dialCode));

  if (match) {
    return { dialCode: match.dialCode, number: trimmed.slice(match.dialCode.length).trim() };
  }

  return { dialCode: DEFAULT_DIAL_CODE, number: trimmed };
}
