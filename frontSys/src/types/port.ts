export interface Port {
  name: string;
  city: string;
  province?: string;
  country?: string;
  alias?: string[];
  regions?: string[];
  coordinates?: [number, number];
  timezone: string;
  unlocs?: string[];
  code?: string;
};

export type Ports = Record<string, Port>;