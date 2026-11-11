export const environment = {
  production: true,
  // Mismo origen (la API la sirve el propio backend) bajo /api, para no chocar
  // con las rutas del SPA: las llamadas van a /api/auth/login, /api/incomes, etc.
  apiUrl: '/api',
  // Categoría de Mascotas: sus gastos se mandan a PetRecording y el formulario
  // pide a qué mascota. Vale también para sus subcategorías. 0 = desactivado.
  petCategoryId: 27,
};
