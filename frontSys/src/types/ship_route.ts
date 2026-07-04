export interface ShipRoute {
  vessel_name: string;
  vessel_type: string;
  vessel_capacity: string;
  imo: string;
  departure_time: string; // or Date, depending on your data
  arrival_time: string;   // or Date, depending on your data
  departure_port: string;
  passing_ports: string; // comma-separated string or string[]
  arrival_port: string;
  broker_name: string;
  broker_email: string;
  broker_phone_number: string;
  cargo_type: string; // comma-separated string or string[]
  total_cargo_weight: string;
};