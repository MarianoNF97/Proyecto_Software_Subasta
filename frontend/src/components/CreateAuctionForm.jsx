import React, { useState } from 'react';
import { toast } from 'sonner';
import apiClient from '../apiClient';

const CreateAuctionForm = () => {
  const [formData, setFormData] = useState({
    titulo: '',
    descripcion: '',
    imagen: '',
    categoria: 'Tecnología',
    precio_base: 0,
    incremento_minimo: 0,
    fecha_inicio: '',
    fecha_fin: ''
  });

  const [isLoading, setIsLoading] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: (name === 'precio_base' || name === 'incremento_minimo') ? Number(value) : value
    }));
  };

  // Validaciones de UX
  const isBasePriceInvalid = formData.precio_base < 0;
  const isIncrementInvalid = formData.incremento_minimo < 0;
  const hasNegativeEconomics = isBasePriceInvalid || isIncrementInvalid;

  const isDateInvalid = formData.fecha_inicio && formData.fecha_fin && 
                        new Date(formData.fecha_fin) <= new Date(formData.fecha_inicio);
                        
  const isFormIncomplete = !formData.titulo || !formData.fecha_inicio || !formData.fecha_fin || !formData.categoria;

  // Bloqueo preventivo
  const isSubmitDisabled = hasNegativeEconomics || isDateInvalid || isFormIncomplete || isLoading;

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (isSubmitDisabled) return;

    setIsLoading(true); // Mostrar spinner y deshabilitar

    try {
      // POST a la API
      await apiClient.post('/auctions', formData);
      toast.success('Subasta publicada con éxito');
      
      // Limpiar formulario tras éxito
      setFormData({
        titulo: '',
        descripcion: '',
        imagen: '',
        categoria: 'Tecnología',
        precio_base: 0,
        incremento_minimo: 0,
        fecha_inicio: '',
        fecha_fin: ''
      });
    } catch (error) {
      console.error('Error al crear subasta:', error);
    } finally {
      setIsLoading(false); // Ocultar spinner
    }
  };

  return (
    <div className="bg-white p-6 rounded-2xl shadow-sm border border-gray-100 max-w-2xl w-full">
      <h2 className="text-2xl font-bold mb-6 text-gray-800">Publicar Nueva Subasta</h2>
      
      <form onSubmit={handleSubmit} className="flex flex-col gap-8">
        
        {/* Panel 1: Producto */}
        <section className="flex flex-col gap-4">
          <h3 className="text-lg font-semibold text-gray-700 border-b pb-2">1. Producto</h3>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Título</label>
            <input 
              type="text" name="titulo" value={formData.titulo} onChange={handleChange} required disabled={isLoading}
              className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none bg-gray-50 text-gray-900"
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Descripción</label>
            <textarea 
              name="descripcion" value={formData.descripcion} onChange={handleChange} disabled={isLoading} rows="3"
              className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none bg-gray-50 text-gray-900 resize-none"
            ></textarea>
          </div>

          <div className="flex flex-col sm:flex-row gap-4">
            <div className="flex-1">
              <label className="block text-sm font-medium text-gray-700 mb-1">Categoría</label>
              <select 
                name="categoria" value={formData.categoria} onChange={handleChange} disabled={isLoading}
                className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none bg-gray-50 text-gray-900"
              >
                <option value="Tecnología">Tecnología</option>
                <option value="Vehículos">Vehículos</option>
                <option value="Inmuebles">Inmuebles</option>
                <option value="Arte">Arte</option>
                <option value="Otros">Otros</option>
              </select>
            </div>
            
            <div className="flex-1">
              <label className="block text-sm font-medium text-gray-700 mb-1">URL de Imagen</label>
              <input 
                type="url" name="imagen" value={formData.imagen} onChange={handleChange} disabled={isLoading} placeholder="https://..."
                className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none bg-gray-50 text-gray-900"
              />
            </div>
          </div>
        </section>

        {/* Panel 2: Economía */}
        <section className="flex flex-col gap-4">
          <h3 className="text-lg font-semibold text-gray-700 border-b pb-2">2. Economía</h3>
          
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="flex-1">
              <label className="block text-sm font-medium text-gray-700 mb-1">Precio Base ($)</label>
              <input 
                type="number" name="precio_base" value={formData.precio_base} onChange={handleChange} required disabled={isLoading}
                className={`w-full px-3 py-2 border rounded-lg outline-none bg-gray-50 text-gray-900 transition-colors ${isBasePriceInvalid ? 'border-red-500 focus:ring-red-500' : 'border-gray-200 focus:ring-blue-500'}`}
              />
              {isBasePriceInvalid && <p className="text-red-500 text-xs mt-1">No puede ser negativo.</p>}
            </div>

            <div className="flex-1">
              <label className="block text-sm font-medium text-gray-700 mb-1">Incremento Mínimo ($)</label>
              <input 
                type="number" name="incremento_minimo" value={formData.incremento_minimo} onChange={handleChange} required disabled={isLoading}
                className={`w-full px-3 py-2 border rounded-lg outline-none bg-gray-50 text-gray-900 transition-colors ${isIncrementInvalid ? 'border-red-500 focus:ring-red-500' : 'border-gray-200 focus:ring-blue-500'}`}
              />
              {isIncrementInvalid && <p className="text-red-500 text-xs mt-1">No puede ser negativo.</p>}
            </div>
          </div>
        </section>

        {/* Panel 3: Tiempos */}
        <section className="flex flex-col gap-4">
          <h3 className="text-lg font-semibold text-gray-700 border-b pb-2">3. Tiempos</h3>
          
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="flex-1">
              <label className="block text-sm font-medium text-gray-700 mb-1">Inicio</label>
              <input 
                type="datetime-local" name="fecha_inicio" value={formData.fecha_inicio} onChange={handleChange} required disabled={isLoading}
                className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none bg-gray-50 text-gray-900"
              />
            </div>

            <div className="flex-1">
              <label className="block text-sm font-medium text-gray-700 mb-1">Fin</label>
              <input 
                type="datetime-local" name="fecha_fin" value={formData.fecha_fin} onChange={handleChange} required disabled={isLoading}
                className={`w-full px-3 py-2 border rounded-lg outline-none bg-gray-50 text-gray-900 transition-colors ${isDateInvalid ? 'border-red-500 focus:ring-red-500' : 'border-gray-200 focus:ring-blue-500'}`}
              />
              {isDateInvalid && <p className="text-red-500 text-xs mt-1">Debe ser posterior al inicio.</p>}
            </div>
          </div>
        </section>

        {/* Botón Guardar */}
        <button 
          type="submit" 
          disabled={isSubmitDisabled}
          className={`mt-2 py-3 px-4 rounded-xl font-bold text-white flex items-center justify-center transition-all
            ${isSubmitDisabled ? 'bg-gray-400 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700 active:scale-[0.99]'}
          `}
        >
          {isLoading ? (
            <>
              <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Publicando...
            </>
          ) : (
            'Publicar Subasta'
          )}
        </button>

      </form>
    </div>
  );
};

export default CreateAuctionForm;

