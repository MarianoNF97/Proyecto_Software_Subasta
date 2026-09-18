import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import apiClient from '../apiClient';
import { toast } from 'sonner';

const UserDashboard = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('compras');  
  const [purchases, setPurchases] = useState([]);
  const [publications, setPublications] = useState([]);
  const [isLoading, setIsLoading] = useState(false);

  
  useEffect(() => {
    const fetchData = async () => {
      setIsLoading(true);
      try {
        if (activeTab === 'compras') {
          const response = await apiClient.get('/users/my-purchases');
          setPurchases(response.data);
        } else {
          const response = await apiClient.get('/users/my-auctions');
          setPublications(response.data);
        }
      } catch (error) {
        console.error('Error fetching dashboard data:', error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, [activeTab]);

  return (
    <div className="max-w-6xl mx-auto flex flex-col gap-6">
      <h2 className="text-3xl font-bold text-gray-800">Mis Actividades</h2>
      
      {}
      <div className="flex border-b border-gray-200">
        <button
          className={`py-2 px-4 text-sm font-medium border-b-2 transition-colors ${
            activeTab === 'compras'
              ? 'border-blue-600 text-blue-600'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
          }`}
          onClick={() => setActiveTab('compras')}
        >
          Mis Compras / Pujas
        </button>
        <button
          className={`py-2 px-4 text-sm font-medium border-b-2 transition-colors ${
            activeTab === 'publicaciones'
              ? 'border-blue-600 text-blue-600'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
          }`}
          onClick={() => setActiveTab('publicaciones')}
        >
          Mis Publicaciones
        </button>
      </div>

      {}
      <div className="mt-4">
        {isLoading ? (
          <div className="text-center py-20">
            <div className="animate-spin h-8 w-8 border-4 border-blue-600 border-t-transparent rounded-full mx-auto" />
            <p className="text-gray-500 mt-4">Cargando...</p>
          </div>
        ) : activeTab === 'compras' ? (
          
          purchases.length === 0 ? (
            <div className="text-center py-20 bg-white rounded-lg shadow-sm border border-gray-100">
              <p className="text-gray-500 text-lg mb-4">No has participado en ninguna subasta aún.</p>
              <Link
                to="/"
                className="bg-blue-600 text-white px-6 py-2 rounded-lg font-semibold hover:bg-blue-700 transition-colors"
              >
                Ir al Catálogo
              </Link>
            </div>
          ) : (
            <div className="overflow-x-auto bg-white rounded-lg shadow-sm border border-gray-100">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="bg-gray-50 text-gray-700 text-sm">
                    <th className="p-4 border-b">Subasta</th>
                    <th className="p-4 border-b">Precio Actual</th>
                    <th className="p-4 border-b">Estado</th>
                    <th className="p-4 border-b">Resultado</th>
                  </tr>
                </thead>
                <tbody>
                  {purchases.map((auction) => {
                    const isWinner = auction.winningBidderId === user?.id;
                    return (
                      <tr key={auction.id} className="hover:bg-gray-50 transition-colors">
                        <td className="p-4 border-b">
                          <Link to={`/subasta/${auction.id}`} className="text-blue-600 font-medium hover:underline">
                            {auction.title}
                          </Link>
                        </td>
                        <td className="p-4 border-b font-semibold text-gray-800">
                          ${auction.currentPrice.toLocaleString()}
                        </td>
                        <td className="p-4 border-b">
                          <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                            auction.status === 'ACTIVA' ? 'bg-green-100 text-green-800' : 
                            auction.status === 'FINALIZADA' ? 'bg-gray-200 text-gray-800' :
                            'bg-yellow-100 text-yellow-800'
                          }`}>
                            {auction.status}
                          </span>
                        </td>
                        <td className="p-4 border-b">
                          {auction.status === 'ACTIVA' ? (
                            <span className="text-sm text-blue-600 font-medium">Subasta Abierta</span>
                          ) : auction.status === 'FINALIZADA' ? (
                            isWinner ? (
                              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                                ¡Ganaste el producto!
                              </span>
                            ) : (
                              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                                Superado/Perdida
                              </span>
                            )
                          ) : (
                            <span className="text-sm text-gray-500">-</span>
                          )}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )
        ) : (
          
          publications.length === 0 ? (
            <div className="text-center py-20 bg-white rounded-lg shadow-sm border border-gray-100">
              <p className="text-gray-500 text-lg mb-4">No tienes publicaciones activas.</p>
              <Link
                to="/publicar"
                className="bg-green-600 text-white px-6 py-2 rounded-lg font-semibold hover:bg-green-700 transition-colors"
              >
                Crear Subasta
              </Link>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {publications.map((auction) => {
                const hasWinner = auction.winningBidderId != null;
                return (
                  <div key={auction.id} className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 flex flex-col justify-between">
                    <div>
                      <h3 className="text-xl font-bold text-gray-800 mb-2 truncate" title={auction.title}>
                        <Link to={`/subasta/${auction.id}`} className="hover:text-blue-600">
                          {auction.title}
                        </Link>
                      </h3>
                      <p className="text-sm text-gray-500 mb-4 flex justify-between">
                        <span>Estado:</span>
                        <span className="font-semibold">{auction.status}</span>
                      </p>
                      
                      <div className="bg-gray-50 p-3 rounded-lg mb-4">
                        <p className="text-xs text-gray-500">Recaudación / Precio Final</p>
                        <p className="text-lg font-bold text-blue-600">
                          ${auction.currentPrice.toLocaleString()}
                        </p>
                      </div>
                    </div>

                    <div className="mt-auto border-t pt-4">
                      {auction.status === 'ACTIVA' ? (
                        <p className="text-sm text-blue-600 font-medium text-center">Subasta Abierta</p>
                      ) : auction.status === 'FINALIZADA' ? (
                        hasWinner ? (
                          <p className="text-sm text-green-700 font-medium text-center bg-green-50 p-2 rounded-md">
                            Adjudicada a ID {auction.winningBidderId}
                          </p>
                        ) : (
                          <p className="text-sm text-gray-600 font-medium text-center bg-gray-50 p-2 rounded-md">
                            Declarada Desierta
                          </p>
                        )
                      ) : (
                        <p className="text-sm text-yellow-600 font-medium text-center">
                          Programada
                        </p>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          )
        )}
      </div>
    </div>
  );
};

export default UserDashboard;

