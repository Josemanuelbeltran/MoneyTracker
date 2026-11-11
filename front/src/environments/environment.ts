export const environment = {
  production: false,
  // La API (back/) corre en 5138, bajo /api. CORS permite cualquier origen localhost en desarrollo.
  apiUrl: 'http://localhost:5138/api',
  // Categoría de Mascotas: sus gastos se mandan a PetRecording y el formulario
  // pide a qué mascota. Vale también para sus subcategorías. 0 = desactivado.
  petCategoryId: 27,
};
