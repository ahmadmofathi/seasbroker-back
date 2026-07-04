export interface Quote {
  cargo_type: string;
  total_weight: string;
  departure_city: string;
  departure_time: string | Date;
  arrival_time: string | Date;
  delivery_city: string;
  dimensions: string;
  first_name: string;
  last_name: string;
  email: string;
  phone_number: string;
  message: string;
};