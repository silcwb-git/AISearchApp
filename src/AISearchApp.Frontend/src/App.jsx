import { useState } from 'react';
import SearchForm from './components/SearchForm';
import ResultDisplay from './components/ResultDisplay';
import { searchQuery } from './services/api';
import './App.css';

function App() {
  const [results, setResults] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleSearch = async (query) => {
    setLoading(true);
    setError(null);
    try {
      const data = await searchQuery(query);
      setResults(data);
    } catch (err) {
      setError(err.message);
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-8">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-4xl font-bold text-center text-indigo-900 mb-2">
          🤖 AI Search Assistant
        </h1>
        <p className="text-center text-gray-600 mb-8">
          Busca semântica com IA generativa
        </p>

        <SearchForm onSearch={handleSearch} loading={loading} />

        {error && (
          <div className="mt-6 p-4 bg-red-100 border border-red-400 text-red-700 rounded">
            Erro: {error}
          </div>
        )}

        {results && <ResultDisplay results={results} />}
      </div>
    </div>
  );
}

export default App;