export const routes = {
  home: "/",
  departments: "/departments",
  locations: "/locations",
  positions: "/positions",
  positionDetails: (id: string) => `/positions/${id}`,
};
