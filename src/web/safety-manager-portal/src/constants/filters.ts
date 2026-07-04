export const DEPARTMENTS = [
  'All Departments',
  'Machine Shop',
  'Fabrication (Welding & Boilermaking)',
  'Fitting',
  'Stripping',
  'Maintenance',
  'Warehouse',
] as const;

export const OCCUPATIONS = [
  'All Occupations',
  'Artisan',
  'Welder',
  'Boilermaker',
  'Fitter',
  'Electrician',
  'Operator',
  'Storeman',
  'Supervisor',
] as const;

export const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5101/api/v1';
