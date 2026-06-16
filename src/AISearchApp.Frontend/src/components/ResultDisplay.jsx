import {} from 'react';

export default function ResultDisplay({ results }) {
  if (!results) return null;

  return (
    <div className="bg-white rounded-lg shadow-lg p-6">
      <h2 className="text-2xl font-bold text-indigo-900 mb-4">Resposta</h2>
      <p className="text-gray-700 mb-6 leading-relaxed">{results.answer}</p>

      {results.sources && results.sources.length > 0 && (
        <div>
          <h3 className="text-lg font-semibold text-indigo-900 mb-3">Fontes</h3>
          <ul className="list-disc list-inside space-y-2">
            {results.sources.map((source, index) => (
              <li key={index} className="text-gray-600">
                {source}
              </li>
            ))}
          </ul>
        </div>
      )}

      {results.processingTimeMs && (
        <p className="text-sm text-gray-500 mt-4">
          ⏱️ Tempo de processamento: {results.processingTimeMs}ms
        </p>
      )}
    </div>
  );
}