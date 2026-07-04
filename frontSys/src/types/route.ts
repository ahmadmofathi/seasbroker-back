export interface Route {
  vessel_name: string;
  vessel_type: string;
  vessel_capacity: string;
  imo: string;
  departure_time: string | Date;
  arrival_time: string | Date;
  departure_port: string;
  arrival_port: string;
  passing_ports: string[]; // Array of passing ports
  broker_name: string;
  broker_email: string;
  broker_phone_number: string;
  cargo_type: string[]; // Array of cargo types
  total_cargo_weight: string;
};