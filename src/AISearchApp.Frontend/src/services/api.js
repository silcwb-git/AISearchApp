import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const searchQuery = async (query) => {
  try {
    const response = await api.post('/search', {
      query: query,
      topResults: 5,
    });
    return response.data;
  } catch (error) {
    console.error('Erro ao buscar:', error);
    throw error;
  }
};

export default api;
